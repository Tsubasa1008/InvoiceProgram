using Caliburn.Micro;
using ClosedXML.Excel;
using InvoiceProgram.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InvoiceProgram.ViewModels
{
    public class CreateInvoiceViewModel : Screen, IDataErrorInfo
    {
        public CreateInvoiceViewModel(StorePort model)
        {
            StorePort = model;
            invoices = GlobalConfig.Connection.GetAllInvoice(StorePort.Store, DateTime.Now.Year, DateTime.Now.Month);
            Initialize();  
        }

        #region -- Properties --

        private List<Invoice> invoices = new List<Invoice>();
        private StorePort _storePort;
        private string _orderNumber = "";
        private string _invoiceAmount = "";
        private string _invoiceNumber = "";
        private bool _isInvoiceNumberEditable = true;
        private bool _isResetInvoiceEnabled = false;
        private bool _isConfirmInvoiceNumberEnabled = true;
        private bool _isOrderNumberFocused = false;
        private bool _isInvoiceAmountFocused = false;

        public StorePort StorePort
        {
            get { return _storePort; }
            set
            {
                _storePort = value;
                NotifyOfPropertyChange(() => StorePort);
            }
        }

        public string OrderNumber
        {
            get { return _orderNumber; }
            set
            {
                _orderNumber = value;
                IsOrderNumberValid(value);
                NotifyOfPropertyChange(() => OrderNumber);
                NotifyOfPropertyChange(() => CanCreateInvoice);
            }
        }

        public string InvoiceAmount
        {
            get { return _invoiceAmount; }
            set
            {
                _invoiceAmount = value;
                IsInvoiceAmountValid(value);
                NotifyOfPropertyChange(() => InvoiceAmount);
                NotifyOfPropertyChange(() => CanCreateInvoice);
            }
        }

        public string InvoiceNumber
        {
            get { return _invoiceNumber; }
            set
            {
                _invoiceNumber = value;
                IsInvoiceNumberValid(value);
                NotifyOfPropertyChange(() => InvoiceNumber);
                NotifyOfPropertyChange(() => CanCreateInvoice);
            }
        }

        public bool IsInvoiceNumberEditable
        {
            get { return _isInvoiceNumberEditable; }
            set
            {
                _isInvoiceNumberEditable = value;
                NotifyOfPropertyChange(() => IsInvoiceNumberEditable);
            }
        }

        public bool IsResetInvoiceEnabled
        {
            get { return _isResetInvoiceEnabled; }
            set
            {
                _isResetInvoiceEnabled = value;
                NotifyOfPropertyChange(() => IsResetInvoiceEnabled);
            }
        }

        public bool IsConfirmInvoiceNumberEnabled
        {
            get { return _isConfirmInvoiceNumberEnabled; }
            set
            {
                _isConfirmInvoiceNumberEnabled = value;
                NotifyOfPropertyChange(() => IsConfirmInvoiceNumberEnabled);
            }
        }

        public bool IsOrderNumberFocused
        {
            get { return _isOrderNumberFocused; }
            set
            {
                _isOrderNumberFocused = value;
                NotifyOfPropertyChange(() => IsOrderNumberFocused);
            }
        }

        public bool IsInvoiceAmountFocused
        {
            get { return _isInvoiceAmountFocused; }
            set
            {
                _isInvoiceAmountFocused = value;
                NotifyOfPropertyChange(() => IsInvoiceAmountFocused);
            }
        }

        #endregion

        #region -- IDataErrorInfo --

        public string Error
        {
            get { return null; }
        }

        public string this[string propertyName]
        {
            get
            {
                return  (!errors.ContainsKey(propertyName) ? null : String.Join(Environment.NewLine, errors[propertyName]));
            }
        }

        #endregion

        #region -- Methods --

        public void Initialize()
        {
            if (invoices.Count > 0)
            {
                InvoiceNumber = (int.Parse(invoices.Last().Number) + 1).ToString().PadLeft(8, '0');

                IsInvoiceNumberEditable = false;
                IsConfirmInvoiceNumberEnabled = false;
                IsResetInvoiceEnabled = true;
            }
            else
            {
                InvoiceNumber = "";

                IsInvoiceNumberEditable = true;
                IsConfirmInvoiceNumberEnabled = true;
                IsResetInvoiceEnabled = false;
            }

            OrderNumber = "";
            InvoiceAmount = "";
        }

        public bool CanCreateInvoice
        {
            get
            {
                return ValidateForm();
            }
        }

        public void CreateInvoice()
        {
            Invoice i = new Invoice {
                Number = InvoiceNumber,
                Amount = int.Parse(InvoiceAmount),
                OrderNumber = OrderNumber
            };

            GlobalConfig.Connection.CreateInvoice(i, StorePort.Store, i.CreateTime.Year,i.CreateTime.Month);
            PrintInvoice(i);
            invoices.Add(i);
            Initialize();
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(i.CreateTime);
        }

        public void OrderNumberKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                IsInvoiceAmountFocused = true;
                IsInvoiceAmountFocused = false;
            }
        }

        public void EnterInvoiceAmount(KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (CanCreateInvoice)
                {
                    CreateInvoice();
                    IsOrderNumberFocused = true;
                    IsOrderNumberFocused = false;
                }
            }
        }

        public void Cancel()
        {
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(new HandleType { Type = Models.Cancel.StorePort });
            TryClose();
        }

        public void ResetInvoice()
        {
            IsInvoiceNumberEditable = true;
            IsResetInvoiceEnabled = false;
            IsConfirmInvoiceNumberEnabled = true;
        }

        public void ConfirmInvoiceNumber()
        {
            if (IsInvoiceNumberValid(InvoiceNumber))
            {
                IsInvoiceNumberEditable = false;
                IsResetInvoiceEnabled = true;
                IsConfirmInvoiceNumberEnabled = false;
            }
        }

        private void PrintInvoice(Invoice invoice)
        {
            SerialPort comport = new SerialPort(StorePort.Port, 9600, Parity.None, 8, StopBits.One);

            if (!comport.IsOpen)
            {
                comport.Open();
            }

            // 初始化發票機
            string line = $"{ Convert.ToChar(27) }@";
            ComportWrite(comport, line);
            // 跳 2 行
            line = $"{ Convert.ToChar(27) }d{ Convert.ToChar(2) }";
            ComportWrite(comport, line);
            // 店舖名稱
            line = $"{ StorePort.Store.Name }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 店舖電話
            line = $"TEL: { StorePort.Store.Phone }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 店舖統一編號
            line = $"NO: { StorePort.Store.UniformNumber }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 跳 1 行
            line = $"{ Convert.ToChar(27) }d{ Convert.ToChar(1) }";
            ComportWrite(comport, line);
            // 發票開立時間
            line = $"{ invoice.CreateTime.ToString("yyyy/MM/dd HH:mm:ss") }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 訂單編號
            line = $"{ invoice.OrderNumber }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 跳 1 行
            line = $"{ Convert.ToChar(27) }d{ Convert.ToChar(1) }";
            ComportWrite(comport, line);
            // 品項金額
            line = $"{ StorePort.Store.ItemString }{ invoice.Amount.ToString("$ ###,##0").PadLeft(24 - Encoding.GetEncoding("big5").GetByteCount(StorePort.Store.ItemString)) }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 跳 3 行
            line = $"{ Convert.ToChar(27) }d{ Convert.ToChar(3) }";
            ComportWrite(comport, line);
            // 合計
            line = $"合計：{ invoice.Amount.ToString("$ ###,##0").PadLeft(18) }{ Convert.ToChar(13) }{ Convert.ToChar(10) }";
            ComportWrite(comport, line);
            // 跳 19 行
            line = $"{ Convert.ToChar(27) }d{ Convert.ToChar(19) }";
            ComportWrite(comport, line);
            // 蓋店章
            line = $"{ Convert.ToChar(27) }o";
            ComportWrite(comport, line);
            // 蓋店章
            line = $"{ Convert.ToChar(12) }";
            ComportWrite(comport, line);

            comport.Close();
        }

        private void ComportWrite(SerialPort comport, string line)
        {
            comport.Write(Encoding.GetEncoding("big5").GetBytes(line), 0, Encoding.GetEncoding("big5").GetByteCount(line));
        }

        #endregion

        #region -- Validations --

        private Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        private const string InvoiceNumberEmptyError = "必須輸入發票號碼.";
        private const string InvoiceNumberContainsNonDigitError = "發票號碼不可包含非數字字元.";
        private const string InvoiceNumberLengthError = "發票號碼長度必須為8個字元.";
        private const string InvoiceAmountInvalidInput = "請輸入正確的發票金額.";
        private const string InvoiceAmountLessThanZero = "發票金額不可小於0, 請重新確認.";
        private const string OrderNumberIsNotUniqueError = "訂單編號不可重複, 請重新確認.";

        public void AddError(string propertyName, string error)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }

            if (!errors[propertyName].Contains(error))
            {
                errors[propertyName].Add(error);
            }
        }

        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName) && errors[propertyName].Contains(error))
            {
                errors[propertyName].Remove(error);
                if (errors[propertyName].Count == 0)
                {
                    errors.Remove(propertyName);
                }
            }
        }

        public bool IsInvoiceAmountValid(string value)
        {
            bool isValid = true;
            int amount = 0;

            if (!int.TryParse(value, out amount))
            {
                AddError("InvoiceAmount", InvoiceAmountInvalidInput);
                isValid = false;
            }
            else
            {
                RemoveError("InvoiceAmount", InvoiceAmountInvalidInput);
            }

            if (amount <= 0)
            {
                AddError("InvoiceAmount", InvoiceAmountLessThanZero);
                isValid = false;
            }
            else
            {
                RemoveError("InvoiceAmount", InvoiceAmountLessThanZero);
            }

            return isValid;
        }

        public bool IsOrderNumberValid(string value)
        {
            bool isValid = true;

            if (!String.IsNullOrEmpty(value) && invoices.Any(x => x.OrderNumber == value))
            {
                AddError("OrderNumber", OrderNumberIsNotUniqueError);
                isValid= false;
            }
            else
            {
                RemoveError("OrderNumber", OrderNumberIsNotUniqueError);
            }

            return isValid;
        }

        public bool IsInvoiceNumberValid(string value)
        {
            bool isValid = true;
            Regex rgx = new Regex(@"[^0-9]");

            if (String.IsNullOrEmpty(value))
            {
                AddError("InvoiceNumber", InvoiceNumberEmptyError);
                isValid = false;
            }
            else
            {
                RemoveError("InvoiceNumber", InvoiceNumberEmptyError);
            }

            if (rgx.IsMatch(value))
            {
                AddError("InvoiceNumber", InvoiceNumberContainsNonDigitError);
                isValid = false;
            }
            else
            {
                RemoveError("InvoiceNumber", InvoiceNumberContainsNonDigitError);
            }

            if (value.Length != 8)
            {
                AddError("InvoiceNumber", InvoiceNumberLengthError);
                isValid = false;
            }
            else
            {
                RemoveError("InvoiceNumber", InvoiceNumberLengthError);
            }

            return isValid;
        }

        public bool ValidateForm()
        {
            return IsInvoiceAmountValid(InvoiceAmount) &&
                IsInvoiceNumberValid(InvoiceNumber);
        }

        #endregion
    }
}
