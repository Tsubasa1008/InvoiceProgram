using Caliburn.Micro;
using ClosedXML.Excel;
using InvoiceProgram.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace InvoiceProgram.ViewModels
{
    public class InvoiceDetailViewModel : Conductor<object>, IHandle<Invoice>, IHandle<DateTime>
    {
        public InvoiceDetailViewModel(StoreInfo store)
        {
            Store = store;
            Initialize();
        }

        #region -- Properties --

        private StoreInfo _store;
        private Invoice _selectedInvoice;
        private BindableCollection<Invoice> _invoices = new BindableCollection<Invoice>();
        private bool _modifyInvoiceViewIsVisible = false;
        private BindableCollection<int> _month = new BindableCollection<int>();
        private string _year = "";
        private string _yearErrors = "";
        private int _selectedMonth;
        private string _monthErrors = "";
        private string _track = "";
        private string _trackErrors = "";
        private BindableCollection<Filter> _filters = new BindableCollection<Filter>(
            new List<Filter>() {
                new Filter() { DisplayName = "發票號碼", Type = Kind.Number },
                new Filter() { DisplayName = "訂單編號", Type = Kind.OrderNumber }
            });
        private Filter _selectedFilter;
        private string _filterString = "";

        public StoreInfo Store
        {
            get { return _store; }
            set
            {
                _store = value;
                NotifyOfPropertyChange(() => Store);
            }
        }

        public BindableCollection<Filter> Filters
        {
            get { return _filters; }
            set
            {
                _filters = value;
                NotifyOfPropertyChange(() => Filters);
            }
        }

        public Filter SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                _selectedFilter = value;
                NotifyOfPropertyChange(() => SelectedFilter);
                if (LoadValidateForm())
                {
                    LoadInvoices();
                }
            }
        }

        public string FilterString
        {
            get { return _filterString; }
            set
            {
                _filterString = value;
                NotifyOfPropertyChange(() => FilterString);
                if (LoadValidateForm())
                {
                    LoadInvoices();
                }
            }
        }

        public string Year
        {
            get { return _year; }
            set
            {
                _year = value;
                NotifyOfPropertyChange(() => Year);
                NotifyOfPropertyChange(() => CanLoadInvoices);
                NotifyOfPropertyChange(() => CanCreateMReport);
            }
        }

        public string YearErrors
        {
            get { return _yearErrors; }
            set
            {
                _yearErrors = value;
                NotifyOfPropertyChange(() => YearErrors);
                NotifyOfPropertyChange(() => IsYearErrorsVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsYearErrorsVisible
        {
            get
            {
                return !String.IsNullOrEmpty(YearErrors);
            }
        }

        public BindableCollection<int> Month
        {
            get { return _month; }
            set
            {
                _month = value;
                NotifyOfPropertyChange(() => Month);
            }
        }

        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                _selectedMonth = value;
                NotifyOfPropertyChange(() => SelectedMonth);
                NotifyOfPropertyChange(() => CanLoadInvoices);
                NotifyOfPropertyChange(() => CanCreateMReport);
            }
        }

        public string TotalAmount
        {
            get
            {
                return Invoices.Where(x => x.InValid == false).Select(x => x.Amount).Sum().ToString("$ ###,##0");
            }
        }

        public int InValidCount
        {
            get
            {
                return Invoices.Where(x => x.InValid == true).Count();
            }
        }

        public Invoice SelectedInvoice
        {
            get { return _selectedInvoice; }
            set
            {
                _selectedInvoice = value;
                NotifyOfPropertyChange(() => SelectedInvoice);
                LoadModifyInvoiceView();
            }
        }

        public BindableCollection<Invoice> Invoices
        {
            get { return _invoices; }
            set
            {
                _invoices = value;
                NotifyOfPropertyChange(() => Invoices);
                NotifyOfPropertyChange(() => TotalAmount);
                NotifyOfPropertyChange(() => InValidCount);
            }
        }

        public bool ModifyInvoiceViewIsVisible
        {
            get { return _modifyInvoiceViewIsVisible; }
            set
            {
                _modifyInvoiceViewIsVisible = value;
                NotifyOfPropertyChange(() => ModifyInvoiceViewIsVisible);
            }
        }

        public string MonthErrors
        {
            get { return _monthErrors; }
            set
            {
                _monthErrors = value;
                NotifyOfPropertyChange(() => MonthErrors);
                NotifyOfPropertyChange(() => IsMonthErrorsVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsMonthErrorsVisible
        {
            get
            {
                return !String.IsNullOrEmpty(MonthErrors);
            }
        }

        public string Track
        {
            get { return _track; }
            set
            {
                _track = value;
                NotifyOfPropertyChange(() => Track);
                NotifyOfPropertyChange(() => CanCreateMReport);
            }
        }

        public string TrackErrors
        {
            get { return _trackErrors; }
            set
            {
                _trackErrors = value;
                NotifyOfPropertyChange(() => TrackErrors);
                NotifyOfPropertyChange(() => IsTrackErrorsVisible);
                NotifyOfPropertyChange(() => IsErrorsVisible);
            }
        }

        public bool IsTrackErrorsVisible
        {
            get
            {
                return !String.IsNullOrEmpty(TrackErrors);
            }
        }

        public bool IsErrorsVisible
        {
            get
            {
                return !String.IsNullOrEmpty(YearErrors) ||
                    !String.IsNullOrEmpty(MonthErrors) ||
                    !String.IsNullOrEmpty(TrackErrors);
            }
        }

        #endregion

        #region -- Validations --

        public bool LoadValidateForm()
        {
            YearValidator();

            MonthValidator();

            return String.IsNullOrEmpty(YearErrors) &&
                String.IsNullOrEmpty(MonthErrors);
        }

        public bool MReportValidateForm()
        {
            YearValidator();

            MonthValidator();

            TrackValidator();

            return String.IsNullOrEmpty(YearErrors) &&
                String.IsNullOrEmpty(MonthErrors) &&
                String.IsNullOrEmpty(TrackErrors);
        }

        public void YearValidator()
        {
            string output = "";
            Regex rgx = new Regex(@"[^0-9]");

            if (String.IsNullOrEmpty(Year))
            {
                output = "年份不可為空.";
            }
            else if (rgx.IsMatch(Year))
            {
                output = "年份不可包含非數字字元.";
            }
            else if (Year.Length > 4)
            {
                output = "年份長度不可大於4個字元.";
            }
            else if (Year.Length < 4)
            {
                output = "年份長度不可小於4個字元.";
            }

            YearErrors = output;
        }

        public void MonthValidator()
        {
            string output = "";

            if (SelectedMonth < 1)
            {
                output = "必須選擇一個月份.";
            }

            MonthErrors = output;
        }

        public void TrackValidator()
        {
            string output = "";
            Regex rgx = new Regex(@"[^A-Z]");

            if (String.IsNullOrEmpty(Track))
            {
                output = "字軌不可為空.";
            }
            else if (rgx.IsMatch(Track))
            {
                output = "字軌不可包含非英文字元.";
            }
            else if (Track.Length > 2)
            {
                output = "字軌長度不可大於2個字元.";
            }
            else if (Track.Length < 2)
            {
                output = "字軌長度不可小於2個字元.";
            }

            TrackErrors = output;
        }

        #endregion

        #region -- Methods --

        public void ScrollIntoView(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (SelectedInvoice != null)
            {
                dataGrid.ScrollIntoView(SelectedInvoice);
            }
        }

        public bool CanLoadInvoices
        {
            get
            {
                return LoadValidateForm();
            }
        }

        public void LoadInvoices()
        {
            if (String.IsNullOrEmpty(FilterString))
            {
                Invoices = new BindableCollection<Invoice>(GlobalConfig.Connection.GetAllInvoice(Store, int.Parse(Year), SelectedMonth));
            }
            else
            {
                switch (SelectedFilter.Type)
                {
                    case Kind.Number:
                        Invoices = new BindableCollection<Invoice>(GlobalConfig.Connection.GetAllInvoice(Store, int.Parse(Year), SelectedMonth).Where(x => x.Number.Contains(FilterString)));
                        break;
                    case Kind.OrderNumber:
                        Invoices = new BindableCollection<Invoice>(GlobalConfig.Connection.GetAllInvoice(Store, int.Parse(Year), SelectedMonth).Where(x => x.OrderNumber.Contains(FilterString)));
                        break;
                    default:
                        break;
                }
            }
        }

        public bool CanCreateMReport
        {
            get
            {
                return MReportValidateForm();
            }
        }

        public void CreateMReport()
        {
            // 取得儲存檔案名稱, 並刪除舊有資料
            string filepath = GlobalConfig.MReportFileName(Store, int.Parse(Year), SelectedMonth);
            if (File.Exists(filepath))
            {
                try
                {
                    File.Delete(filepath);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show($"刪除原有檔案失敗，請確認檔案是否開啟中.( { e.Message } )");
                    return;
                }
            }

            // 匯入所選年月發票資料
            List<Invoice> monthInvoices = GlobalConfig.Connection.GetAllInvoice(Store, int.Parse(Year), SelectedMonth);

            // 產生月報資料
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Sheet1");

            ws.Style.Font.FontName = "微軟正黑體";
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Rows().AdjustToContents();
            ws.Style.Font.SetFontSize(10);
            ws.Row(4).Height = 5;
            ws.Row(8).Height = 5;

            using (var row = ws.Range("B2:M2"))
            {
                row.Value = "營業人使用二聯式收銀機統一發票明細表";
                row.Style.Font.SetFontSize(16);
                row.Merge();
            }

            using (var row = ws.Range("B3:M3"))
            {
                row.SetValue<string>($"中華民國{ (int.Parse(Year) - 1911).ToString() }年{ SelectedMonth.ToString() }月");
                row.Style.Font.SetFontSize(16);
                row.Merge();
            }

            using (var block = ws.Range("B5:E7"))
            {
                using (var row = ws.Range("B5:D5"))
                {
                    row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row.Value = "統一編號";
                    row.Merge();
                }

                using (var row = ws.Range("B6:D6"))
                {
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row.Value = "營業人名稱";
                    row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    row.Merge();
                }

                using (var row = ws.Range("B7:D7"))
                {
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row.Value = "稅籍編號";
                    row.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    row.Merge();
                }

                using (var row = ws.Range("E5:E7"))
                {
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // 統一編號
                ws.Range("E5").SetValue(Store.UniformNumber);
                // 營業人名稱
                ws.Range("E6").SetValue(Store.Name);
                ws.Range("E6").Style.Font.FontSize = 6;
                // 稅籍編號
                ws.Range("E7").SetValue(Store.TaxId);
            }

            using (var row = ws.Range("F7:I7"))
            {
                row.Value = "所屬年月及發票字軌請營業人填註";
                row.Merge();
            }

            using (var block = ws.Range("J5:J7"))
            {
                using (var row = ws.Range("J6:J7"))
                {
                    row.Merge();
                    // 收銀機編號
                    row.SetValue(Store.CashierNumber);
                    row.Style.Font.FontSize = 9;
                }

                ws.Cell("J5").Value = "收銀機編號";
                ws.Cell("J5").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range("L5:M7"))
            {
                using (var col = ws.Range("L5:L7"))
                {
                    col.Value = "發票字軌";
                    col.Style.Alignment.WrapText = true;
                    col.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col.Merge();
                }

                using (var col = ws.Range("M5:M7"))
                {
                    col.SetValue(Track);
                    col.Style.Font.Bold = true;
                    col.Merge();
                }

                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range("B9:C10"))
            {
                block.Value = "開立日期";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range("D9:E10"))
            {
                block.Value = "開立發票起訖號碼";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range("F9:G9"))
            {
                block.Value = "應　　　　　　　稅";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range("F10:G10"))
            {
                block.Value = "發　票　總　金　額";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range("H9:H10"))
            {
                block.Value = "免　　　稅銷　售　額";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range("I9:M10"))
            {
                block.Value = "誤開作廢發票號碼";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            int rowIndex = 11;

            for (var i = 1; i <= DateTime.DaysInMonth(int.Parse(Year), SelectedMonth); i++)
            {
                List<Invoice> dayInovices = monthInvoices.Where(x => x.CreateTime.Day == i).ToList();

                using (var block = ws.Range(rowIndex, 2, rowIndex + 1, 3))
                {
                    block.Value = i;
                    block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    block.Merge();
                }

                using (var block = ws.Range(rowIndex, 4, rowIndex + 1, 5))
                {
                    List<int> invoiceNumber = new List<int>();

                    foreach (Invoice invoice in dayInovices)
                    {
                        invoiceNumber.Add(int.Parse(invoice.Number));
                    }

                    var result = String.Join(",", invoiceNumber
                        .Distinct()
                        .OrderBy(x => x)
                        .GroupAdjacentBy((x, y) => x + 1 == y)
                        .Select(g => new int[] { g.First(), g.Last() }.Distinct())
                        .Select(g => String.Join("-", g)));

                    string[] cols = result.Split(',');

                    using (var row = ws.Range(rowIndex, 4, rowIndex, 5))
                    {
                        // 開立發票起訖號碼
                        // if (invoice[i].Count() > 0)
                        if (cols.Length > 0)
                        {
                            // row.SetValue<string>(invoice[i].First() + " - " + invoice[i].Last());
                            row.SetValue(cols[0]);
                        }
                        row.Style.Font.FontSize = 9;
                        row.Merge();
                    }

                    using (var row = ws.Range(rowIndex + 1, 4, rowIndex + 1, 5))
                    {
                        // 開立發票起訖號碼
                        if (cols.Length > 1)
                        {
                            row.SetValue(cols[1]);
                        }
                        row.Style.Font.FontSize = 9;
                        row.Merge();
                    }
                    block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }

                using (var block = ws.Range(rowIndex, 6, rowIndex + 1, 7))
                {

                    if (dayInovices.Count > 0)
                    {
                        // 發票總金額
                        block.SetValue(dayInovices.Where(x => !x.InValid).Select(x => x.Amount).Sum().ToString("###,##0"));
                    }
                    block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    block.Merge();
                }

                using (var block = ws.Range(rowIndex, 8, rowIndex + 1, 8))
                {
                    block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    block.Merge();
                }

                using (var block = ws.Range(rowIndex, 9, rowIndex + 1, 13))
                {
                    // 作廢發票號碼
                    if (dayInovices.Where(x => x.InValid == true).Count() > 0)
                    {
                        string invalid = String.Join(",", dayInovices.Where(x => x.InValid).Select(x => x.Number).ToList());

                        block.SetValue(invalid);
                    }
                    block.Style.Alignment.WrapText = true;
                    block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    block.Merge();
                }

                rowIndex += 2;
            }

            using (var block = ws.Range(rowIndex, 2, rowIndex + 10, 2))
            {
                block.Value = "申報　　　　　　　　　　　　單位";
                block.Style.Alignment.WrapText = true;
                block.Style.Font.SetFontSize(5);
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex, 3, rowIndex + 10, 5))
            {
                using (var row = ws.Range(rowIndex, 3, rowIndex + 8, 5))
                {
                    row.Merge();
                }

                using (var row = ws.Range(rowIndex + 9, 3, rowIndex + 9, 5))
                {
                    row.Value = "(請蓋用統一發票專用章)";
                    row.Merge();
                }

                using (var row = ws.Range(rowIndex + 10, 3, rowIndex + 10, 5))
                {
                    row.Value = "申報日期：　年　月　日";
                    row.Merge();
                }

                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            using (var row = ws.Range(rowIndex, 6, rowIndex, 7))
            {
                row.Value = "作　　 廢　　 份　　 數";
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Merge();
            }

            using (var row = ws.Range(rowIndex, 8, rowIndex, 13))
            {
                // 作廢份數
                row.SetValue(monthInvoices.Where(x => x.InValid == true).Count().ToString("###,##0"));
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Merge();
            }

            using (var row = ws.Range(rowIndex + 1, 6, rowIndex + 1, 7))
            {
                row.Value = "空  白  發  票  起  訖  號  碼";
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Merge();
            }

            using (var row = ws.Range(rowIndex + 1, 8, rowIndex + 1, 13))
            {
                row.Value = "　　　　　　號　　　　　　號共　　　份";
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Merge();
            }

            using (var row = ws.Range(rowIndex + 2, 6, rowIndex + 3, 13))
            {
                row.Value = "銷　　售　　額　　及　　稅　　額　　計　　算";
                row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Merge();
            }

            using (var block = ws.Range(rowIndex + 4, 6, rowIndex + 6, 6))
            {
                block.Value = "          區分項目";
                block.Style.Alignment.WrapText = true;
                block.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                block.Merge();
                block.Style.Border.DiagonalDown = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range(rowIndex + 4, 7, rowIndex + 4, 11))
            {
                block.Value = "應　　　　　　　　　　　　稅";
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 5, 7, rowIndex + 6, 7))
            {
                ws.Cell(rowIndex + 5, 7).Value = "發票總金額";
                ws.Cell(rowIndex + 6, 7).Value = "(1)";
                ws.Cell(rowIndex + 6, 7).Style.Font.SetFontSize(6);
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range(rowIndex + 5, 8, rowIndex + 6, 9))
            {
                using (var row = ws.Range(rowIndex + 5, 8, rowIndex + 5, 9))
                {
                    row.Value = "銷售額";
                    row.Merge();
                }

                using (var row = ws.Range(rowIndex + 6, 8, rowIndex + 6, 9))
                {
                    row.Value = "(2) = (1) x (100/105)";
                    row.Style.Font.SetFontSize(6);
                    row.Merge();
                }
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range(rowIndex + 5, 10, rowIndex + 6, 11))
            {
                using (var row = ws.Range(rowIndex + 5, 10, rowIndex + 5, 11))
                {
                    row.Value = "稅額";
                    row.Merge();
                }

                using (var row = ws.Range(rowIndex + 6, 10, rowIndex + 6, 11))
                {
                    row.Value = "(3) = (2) x 5%";
                    row.Style.Font.SetFontSize(6);
                    row.Merge();
                }
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            using (var block = ws.Range(rowIndex + 4, 12, rowIndex + 6, 13))
            {
                block.Value = "免          稅銷   售   額";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 7, 6, rowIndex + 8, 6))
            {
                block.Value = "本　　表合　　計";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 7, 7, rowIndex + 8, 7))
            {
                // 本表合計
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 7, 8, rowIndex + 8, 9))
            {
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.DiagonalUp = true;
                block.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 7, 10, rowIndex + 8, 11))
            {
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Style.Border.DiagonalUp = true;
                block.Style.Border.DiagonalBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 7, 12, rowIndex + 8, 13))
            {
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 9, 6, rowIndex + 10, 6))
            {
                block.Value = "本期(月) 總　　計";
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            // 本月發票總金額
            int total = monthInvoices.Where(x => x.InValid == false).Select(x => x.Amount).Sum();
            double taxFree = 0;

            using (var block = ws.Range(rowIndex + 9, 7, rowIndex + 10, 7))
            {
                // 本期(月)總計
                block.SetValue(total.ToString("###,##0"));
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 9, 8, rowIndex + 10, 9))
            {
                taxFree = total / 1.05;
                // 銷售額
                block.SetValue(Math.Round(taxFree).ToString("###,##0"));
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 9, 10, rowIndex + 10, 11))
            {
                double tax = taxFree * 0.05;
                // 稅額
                block.SetValue(Math.Round(tax).ToString("###,##0"));
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            using (var block = ws.Range(rowIndex + 9, 12, rowIndex + 10, 13))
            {
                block.Style.Alignment.WrapText = true;
                block.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                block.Merge();
            }

            ws.Column(1).Width = 1;
            ws.Column(2).Width = 2;
            ws.Column(3).Width = 2;
            ws.Column(4).Width = 6;
            ws.Column(5).Width = 12;
            ws.Column(6).Width = 8;
            ws.Column(7).Width = 12;
            ws.Column(8).Width = 10;
            ws.Column(9).Width = 2;
            ws.Column(10).Width = 10;
            ws.Column(11).Width = 2;
            ws.Column(12).Width = 4;
            ws.Column(13).Width = 4;
            wb.SaveAs(filepath);

            System.Windows.MessageBox.Show($"月報資料已產生至 { filepath } .");
        }

        private void Initialize()
        {
            // Subscribe
            EventAggregationProvider.InvoiceProgramEventAggregator.Subscribe(this);

            for (int i = 1; i <= 12; i++)
            {
                Month.Add(i);
            }

            Year = DateTime.Now.Year.ToString();
            SelectedMonth = Month.Where(x => x == DateTime.Now.Month).First();
            SelectedFilter = Filters.First();
            LoadInvoices();
        }

        private void LoadModifyInvoiceView()
        {
            if (SelectedInvoice != null)
            {
                ActivateItem(new ModifyInvoiceViewModel(SelectedInvoice, Store));
                ModifyInvoiceViewIsVisible = true;
            }
        }

        public void Handle(Invoice message)
        {
            LoadInvoices();
            ModifyInvoiceViewIsVisible = false;
            SelectedInvoice = null;
        }

        public void Handle(DateTime message)
        {
            Year = message.Year.ToString();
            SelectedMonth = Month.Where(x => x == message.Month).First();
            LoadInvoices();
            SelectedInvoice = Invoices.Last();
        }

        #endregion
    }
}
