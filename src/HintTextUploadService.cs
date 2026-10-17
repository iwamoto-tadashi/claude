using CommonUtils.Extensions;
using CommonUtils.Utility;
using EDMCore.Models.Entities;
using manager.Models;
using manager.Models.HintTextUpload;
using manager.Repositories;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using static CommonUtils.Extensions.StringExtensions;

namespace manager.Services
{
    /// <summary>
    /// HintTextアップロード機能のビジネスロジックを提供します。
    /// </summary>
    public class HintTextUploadService
    {
        private readonly IHintTextBulkMergeRepository _bulkMergeRepository;

        /// <summary>
        /// 既定のRepository実装を使用してServiceを生成します。
        /// </summary>
        public HintTextUploadService() : this(new HintTextBulkMergeRepository())
        {
        }

        /// <summary>
        /// Repositoryを指定してServiceを生成します（テスト時のモック差し替え用）。
        /// </summary>
        /// <param name="bulkMergeRepository"></param>
        public HintTextUploadService(IHintTextBulkMergeRepository bulkMergeRepository)
        {
            _bulkMergeRepository = bulkMergeRepository;
        }

        /// <summary>
        /// 年度コンボボックスの値一覧（5年前～来年）を取得します。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<SelectListItem> GetYearList()
        {
            try
            {
                const int Range = 5;
                return Enumerable.Range(DateTime.Now.Year - Range, Range + 2)
                    .Select(y => new SelectListItem { Value = y.ToString(), Text = $"{y}年" })
                    .ToList();
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// ヒントテキスト一覧を取得します。
        /// </summary>
        /// <param name="companyCd"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private IEnumerable<HintTexts> GetHintTextList(int companyCd, int year)
        {
            try
            {
                using (var db = new dbModel())
                {
                    return db.HintText.AsNoTracking()
                        .Where(h => h.CompanyCd == companyCd && h.Year == year)
                        .OrderBy(h => h.QuestionCd)
                        .ToList()
                        ;
                }
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// HintTextUploadViewModelの共通処理を集約します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private HintTextUploadViewModel SetViewModel(HintTextUploadViewModel viewModel)
        {
            try
            {
                viewModel.YearList = GetYearList();
                viewModel.HintTextList = GetHintTextList(viewModel.CompanyCd, viewModel.Year);

                return viewModel;
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// 初期表示用のViewModelを生成します
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public HintTextUploadViewModel CreateViewModel(int companyCd, int year) 
            => SetViewModel(new HintTextUploadViewModel { CompanyCd = companyCd, Year = year });
        
        /// <summary>
        /// ファイルのアップロード
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public HintTextUploadViewModel Upload(HintTextUploadViewModel viewModel)
        {
            try
            {
                if (viewModel.CsvFile == null || viewModel.CsvFile.ContentLength <= 0)
                {
                    viewModel.ErrorMessage = "CSVファイルが選択されていません。";
                    return SetViewModel(viewModel);
                }

                // CSVをHintTextテーブルにマージ
                var count = MergeHintCSV(viewModel);
                if (count == 0)
                {
                    viewModel.ErrorMessage = "取込データが0件でした";
                }
                else
                {
                    viewModel.SuccessMessage = $"{count}件CSVデータを取込みました";
                }
                    
                return SetViewModel(viewModel);
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// 編集を開始します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public HintTextUploadViewModel StartEdit(HintTextUploadViewModel viewModel, EditMode mode)
        {
            try
            {
                if (mode == EditMode.Add)
                {
                    viewModel.EditHintText = new HintTexts
                    {
                        CompanyCd = viewModel.CompanyCd,
                        Year = viewModel.Year
                    };
                }
                else
                {
                    using (var db = new dbModel())
                    {
                        viewModel.EditHintText = db.HintText
                            .FirstOrDefault(h =>
                                h.CompanyCd == viewModel.CompanyCd
                                && h.Year == viewModel.Year
                                && h.QuestionCd == viewModel.SelectQuestionCd
                                )
                            ?? new HintTexts
                            {
                                CompanyCd = viewModel.CompanyCd,
                                Year = viewModel.Year
                            };
                        ;
                    }
                }

                viewModel.EditMode = mode;

                return SetViewModel(viewModel);
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// QustionIdを指定してHintTextを削除します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public HintTextUploadViewModel Delete(HintTextUploadViewModel viewModel)
        {
            try
            {
                using (var db = new dbModel())
                {
                    var hintText = db.HintText
                        .FirstOrDefault(h =>
                            h.CompanyCd == viewModel.CompanyCd
                            && h.Year == viewModel.Year
                            && h.QuestionCd == viewModel.SelectQuestionCd
                            );
                        
                    if (hintText != null)
                    {
                        db.HintText.Remove(hintText);
                    }

                    db.SaveChanges();                    
                }

                return SetViewModel(viewModel);
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// 企業ID、年度を指定してHintTextを削除します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public HintTextUploadViewModel Reset(HintTextUploadViewModel viewModel)
        {
            try
            {
                using (var db = new dbModel())
                {
                    var hintText = db.HintText
                        .Where(h =>
                            h.CompanyCd == viewModel.CompanyCd
                            && h.Year == viewModel.Year
                            );

                    if (hintText.Any())
                    {
                        db.HintText.RemoveRange(hintText);
                    }

                    db.SaveChanges();
                }

                return SetViewModel(viewModel);
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// HintTextをCSV形式のファイルに変換します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public FileContentResult Download(HintTextUploadViewModel viewModel)
        {
            try
            {
                using (var db = new dbModel())
                {
                    var hintTexts = db.HintText
                        .Where(h =>
                            h.CompanyCd == viewModel.CompanyCd &&
                            h.Year == viewModel.Year)
                        .OrderBy(h => h.QuestionCd)
                        .ToList();

                    var sb = new StringBuilder();

                    // ヘッダー
                    sb.AppendLine("QuestionCd,HintText");

                    foreach (var item in hintTexts)
                    {
                        sb.AppendLine(string.Join(",", item.QuestionCd, item.HintText.EscapeCsv(CrLfHandling.Keep)));
                    }

                    // UTF-8(BOM付き)
                    var encoding = new UTF8Encoding(true);
                    var bom = encoding.GetPreamble();
                    var body = encoding.GetBytes(sb.ToString());

                    var bytes = new byte[bom.Length + body.Length];
                    Buffer.BlockCopy(bom, 0, bytes, 0, bom.Length);
                    Buffer.BlockCopy(body, 0, bytes, bom.Length, body.Length);

                    return new FileContentResult(bytes, "text/csv; charset=utf-8")
                    {
                        FileDownloadName = $"HintText_{viewModel.Year}.csv"
                    };
                }
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// CSVファイルをHintTextテーブルにマージします。
        /// CSVのパース（DataTableへの変換）まではビジネスロジックとしてここで行い、
        /// DB固有の一括更新処理は <see cref="IHintTextBulkMergeRepository"/> に委譲します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private int MergeHintCSV(HintTextUploadViewModel viewModel)
        {
            try
            {
                var dt = BuildHintTextDataTable(viewModel);

                if (dt.Rows.Count == 0)
                    return 0;

                return _bulkMergeRepository.Merge(dt);
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// アップロードされたCSVファイルを読み取り、HintTextテーブルにマージ可能な
        /// DataTable（CompanyCd, Year, QuestionCd, HintText, LastUpdateTime）を組み立てます。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private DataTable BuildHintTextDataTable(HintTextUploadViewModel viewModel)
        {
            try
            {
                var now = DateTime.Now;
                var dt = new DataTable();
                dt.Columns.Add("CompanyCd", typeof(int));
                dt.Columns.Add("Year", typeof(int));
                dt.Columns.Add("QuestionCd", typeof(int));
                dt.Columns.Add("HintText", typeof(string));
                dt.Columns.Add("LastUpdateTime", typeof(DateTime));

                using (var sr = ReadCsvFile(viewModel))
                {
                    var headerFields = sr.ReadFields(); // ヘッダー行を読み飛ばす
                    while (!sr.EndOfData)
                    {
                        var fields = sr.ReadFields();
                        if (fields.Length < 2 || !int.TryParse(fields[0], out var questionCd)) continue;

                        var row = dt.NewRow();
                        row["CompanyCd"] = viewModel.CompanyCd;
                        row["Year"] = viewModel.Year;
                        row["QuestionCd"] = questionCd;
                        row["HintText"] = fields[1];
                        row["LastUpdateTime"] = now;
                        dt.Rows.Add(row);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// CSV読み込み
        /// </summary>
        /// <returns></returns>
        private TextFieldParser ReadCsvFile(HintTextUploadViewModel viewModel)
        {
            try
            {
                var encoding = EncodingDetector.DetectEncoding(viewModel.CsvFile.InputStream);
                var sr = new TextFieldParser(viewModel.CsvFile.InputStream, encoding) 
                {
                    TextFieldType = FieldType.Delimited,    // カンマ区切りの指定
                    HasFieldsEnclosedInQuotes = true,       // フィールドが引用符で囲まれている
                    TrimWhiteSpace = false                  // フィールドの空白トリム設定
                };

                sr.SetDelimiters(",");

                return sr;
            }
            catch (Exception ex)
            {
                errorProc err = new errorProc();
                err.ErrLog(this.ThisName(), ex);
                throw;
            }
        }

        /// <summary>
        /// HintTextを更新します。
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public HintTextUploadViewModel Update(HintTextUploadViewModel viewModel)
        {
            try
            {
                var editHintText = viewModel.EditHintText;
                
                if (editHintText == null)  
                    return SetViewModel(viewModel);

                using (var db = new dbModel())
                {
                    var hintText = db.HintText
                        .FirstOrDefault(h =>
                            h.CompanyCd == editHintText.CompanyCd
                            && h.Year == editHintText.Year
                            && h.QuestionCd == editHintText.QuestionCd
                            );

                    if (hintText == null) {
                        hintText = new HintTexts 
                        { 
                            CompanyCd = editHintText.CompanyCd, 
                            Year = editHintText.Year, 
                            QuestionCd = editHintText.QuestionCd 
                        };
                        db.HintText.Add(hintText);
                    }

                    hintText.HintText = editHintText.HintText;
                    hintText.LastUpdateTime = DateTime.Now;

                    db.SaveChanges();
                }
                
                return SetViewModel(viewModel);
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
