using System.Data;

namespace manager.Repositories
{
    /// <summary>
    /// HintTextテーブルへの一括マージ処理を提供します。
    /// SqlBulkCopy・一時テーブル・MERGE文といったSQL Server固有の実装を担当します。
    /// </summary>
    public interface IHintTextBulkMergeRepository
    {
        /// <summary>
        /// DataTableの内容をHintTextテーブルにマージ（Upsert）します。
        /// </summary>
        /// <param name="dt">
        /// CompanyCd, Year, QuestionCd, HintText, LastUpdateTime の列を持つDataTable
        /// </param>
        /// <returns>マージ対象となった行数</returns>
        /// <remarks>テーブルの構成が変われば BulkCopy、MergeSQLをメンテナンスしてください。</remarks>
        int Merge(DataTable dt);
    }
}
