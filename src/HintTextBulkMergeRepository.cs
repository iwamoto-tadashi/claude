using CommonUtils.Extensions;
using manager.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace manager.Repositories
{
    /// <inheritdoc cref="IHintTextBulkMergeRepository"/>
    public class HintTextBulkMergeRepository : IHintTextBulkMergeRepository
    {
        /// <inheritdoc/>
        /// <remarks>テーブルの構成が変われば BulkCopy、MergeSQLをメンテナンスしてください。</remarks>
        public int Merge(DataTable dt)
        {
            try
            {
                if (dt == null || dt.Rows.Count == 0)
                    return 0;

                using (var db = new dbModel())
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 一時テーブルに追加後にマージする
                        var sql = @"
                            drop table if exists #HintText;
                            select top 0 * into #HintText from HintText;
                            ";
                        db.Database.ExecuteSqlCommand(sql);

                        using (var bulk = new SqlBulkCopy(
                            db.Database.Connection as SqlConnection,
                            SqlBulkCopyOptions.Default,
                            db.Database.CurrentTransaction.UnderlyingTransaction as SqlTransaction))
                        {
                            bulk.DestinationTableName = "#HintText";
                            foreach (DataColumn col in dt.Columns)
                            {
                                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
                            }
                            bulk.WriteToServer(dt);
                        }

                        sql = @"
                            merge into HintText as target
                            using #HintText as source
                            on
                            (
                                target.CompanyCd = source.CompanyCd and target.Year = source.Year and target.QuestionCd = source.QuestionCd
                            )
                            when matched then
                                update set
                                    target.HintText = source.HintText,
                                    target.LastUpdateTime = source.LastUpdateTime
                            when not matched by target then
                                insert
                                (
                                    CompanyCd,
                                    Year,
                                    QuestionCd,
                                    HintText,
                                    LastUpdateTime
                                )
                                values
                                (
                                    source.CompanyCd,
                                    source.Year,
                                    source.QuestionCd,
                                    source.HintText,
                                    source.LastUpdateTime
                                );
                            ";
                        db.Database.ExecuteSqlCommand(sql);

                        sql = @"drop table if exists #HintText;";
                        db.Database.ExecuteSqlCommand(sql);

                        tran.Commit();
                    }
                    catch
                    {
                        tran?.Rollback();
                        throw;
                    }
                }

                return dt.Rows.Count;
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }
    }
}
