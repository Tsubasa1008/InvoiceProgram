using Caliburn.Micro;
using InvoiceProgram.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace InvoiceProgram.ViewModels
{
    public class ManageStoreViewModel : Screen, IDataErrorInfo
    {
        public ManageStoreViewModel(StoreInfo store)
        {
            if (store == null)
            {
                Store = null;
                IsDeleteVisible = Visibility.Collapsed;
            }
            else
            {
                Store = store;
                IsDeleteVisible = Visibility.Visible;
            }
        }

        #region -- Properties --

        private StoreInfo Store;

        private string _uniformNumber = "";
        private string _storeName = "";
        private string _phone = "";
        private string _taxId = "";
        private string _cashierNumber = "";
        private string _itemString = "";
        private Visibility _isDeleteVisible = Visibility.Collapsed;

        public Visibility IsDeleteVisible
        {
            get { return _isDeleteVisible; }
            set
            {
                _isDeleteVisible = value;
                NotifyOfPropertyChange(() => IsDeleteVisible);
            }
        }

        public string UniformNumber
        {
            get { return _uniformNumber; }
            set
            {
                _uniformNumber = value;
                IsUniformNumberValid(value);
                NotifyOfPropertyChange(() => UniformNumber);
                NotifyOfPropertyChange(() => CanSaveStore);
            }
        }

        public string StoreName
        {
            get { return _storeName; }
            set
            {
                _storeName = value;
                IsStoreNameValid(value);
                NotifyOfPropertyChange(() => StoreName);
                NotifyOfPropertyChange(() => CanSaveStore);
            }
        }

        public string Phone
        {
            get { return _phone; }
            set
            {
                _phone = value;
                IsPhoneValid(value);
                NotifyOfPropertyChange(() => Phone);
                NotifyOfPropertyChange(() => CanSaveStore);
            }
        }

        public string TaxId
        {
            get { return _taxId; }
            set
            {
                _taxId = value;
                IsTaxIdValid(value);
                NotifyOfPropertyChange(() => TaxId);
                NotifyOfPropertyChange(() => CanSaveStore);
            }
        }

        public string CashierNumber
        {
            get { return _cashierNumber; }
            set
            {
                _cashierNumber = value;
                IsCashierNumberValid(value);
                NotifyOfPropertyChange(() => CashierNumber);
                NotifyOfPropertyChange(() => CanSaveStore);
            }
        }

        public string ItemString
        {
            get { return _itemString; }
            set
            {
                _itemString = value;
                IsItemStringValid(value);
                NotifyOfPropertyChange(() => ItemString);
                NotifyOfPropertyChange(() => CanSaveStore);
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
                return (!errors.ContainsKey(propertyName) ? null : String.Join(Environment.NewLine, errors[propertyName]));
            }
        }

        #endregion

        #region -- Methods --

        public bool CanSaveStore
        {
            get
            {
                return ValidateForm();
            }
        }

        public void SaveStore()
        {
            StoreInfo s = new StoreInfo
            {
                UniformNumber = UniformNumber,
                Name = StoreName,
                TaxId = TaxId,
                Phone = Phone,
                CashierNumber = CashierNumber,
                ItemString = ItemString
            };

            GlobalConfig.Connection.CreateStore(s);
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(s);
            TryClose();
        }

        public void DeleteStore()
        {

        }

        public void Cancel()
        {
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(new StoreInfo());
            TryClose();
        }

        #endregion

        #region -- Validations --

        Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>();

        private const string ItemStringNullOrWhiteSpaceError = "發票品項名稱不可為空, 請重新確認.";

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
    
        public bool IsItemStringValid(string value)
        {
            bool isValid = true;

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("ItemString", ItemStringNullOrWhiteSpaceError);
                isValid = false;
            }
            else
            {
                RemoveError("ItemString", ItemStringNullOrWhiteSpaceError);
            }

            return isValid;
        }

        public bool ValidateForm()
        {
            return IsUniformNumberValid(UniformNumber) &&
                IsStoreNameValid(StoreName) &&
                IsTaxIdValid(TaxId) &&
                IsPhoneValid(Phone) &&
                IsCashierNumberValid(CashierNumber) &&
                IsItemStringValid(ItemString);
        }

        private const string UniformNumberEmptyErro = "必須輸入統一編號.";
        private const string UniformNumberContainsNonDigitError = "不可包含非數字字元.";
        private const string UniformNumberLengthError = "統一編號長度必須為8個字元.";

        public bool IsUniformNumberValid(string value)
        {
            bool isValid = true;
            Regex rgx = new Regex(@"[^0-9]");

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("UniformNumber", UniformNumberEmptyErro);
                isValid = false;
            }
            else
            {
                RemoveError("UniformNumber", UniformNumberEmptyErro);
            }

            if (rgx.IsMatch(value))
            {
                AddError("UniformNumber", UniformNumberContainsNonDigitError);
                isValid = false;
            }
            else
            {
                RemoveError("UniformNumber", UniformNumberContainsNonDigitError);
            }

            if (value.Length != 8)
            {
                AddError("UniformNumber", UniformNumberLengthError);
                isValid = false;
            }
            else
            {
                RemoveError("UniformNumber", UniformNumberLengthError);
            }

            return isValid;
        }

        private const string NameEmptyError = "必須輸入店舖名稱.";
        private const string NameContainsSpaceError = "名稱不可包含空白字元.";

        public bool IsStoreNameValid(string value)
        {
            bool isValid = true;

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("StoreName", NameEmptyError);
            }
            else
            {
                RemoveError("StoreName", NameEmptyError);
            }

            if (value.Contains(' '))
            {
                AddError("StoreName", NameContainsSpaceError);
            }
            else
            {
                RemoveError("StoreName", NameContainsSpaceError);
            }

            return isValid;
        }

        private const string TaxIdEmptyError = "必須輸入稅籍編號.";
        private const string TaxIdContainsNonDigitError = "不可包含非數字字元.";

        public bool IsTaxIdValid(string value)
        {
            bool isValid = true;
            Regex rgx = new Regex(@"[^0-9]");

            if (String.IsNullOrEmpty(value) || String.IsNullOrEmpty(value))
            {
                AddError("TaxId", TaxIdEmptyError);
            }
            else
            {
                RemoveError("TaxId", TaxIdEmptyError);
            }

            if (rgx.IsMatch(value))
            {
                AddError("TaxId", TaxIdContainsNonDigitError);
            }
            else
            {
                RemoveError("TaxId", TaxIdContainsNonDigitError);
            }

            return isValid;
        }

        private const string PhoneEmptyError = "必須輸入店舖電話.";
        private const string PhoneContainsNonValidCharacterError = "電話不可包含非數字, -, (, )字元.";

        public bool IsPhoneValid(string value)
        {
            bool isValid = true;
            Regex rgx = new Regex(@"[^0-9()-]");

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("Phone", PhoneEmptyError);
            }
            else
            {
                RemoveError("Phone", PhoneEmptyError);
            }

            if (rgx.IsMatch(value))
            {
                AddError("Phone", PhoneContainsNonValidCharacterError);
            }
            else
            {
                RemoveError("Phone", PhoneContainsNonValidCharacterError);
            }

            return isValid;
        }

        private const string CashierNumberEmptyError = "必須輸入收銀機編號.";
        private const string CashierNumberContainsNonValidCharacterError = "收銀機編號必須為英數字元.";

        public bool IsCashierNumberValid(string value)
        {
            bool isValid = true;

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("CashierNumber", CashierNumberEmptyError);
            }
            else
            {
                RemoveError("CashierNumber", CashierNumberEmptyError);
            }

            if (String.IsNullOrEmpty(value) || String.IsNullOrWhiteSpace(value))
            {
                AddError("CashierNumber", CashierNumberContainsNonValidCharacterError);
            }
            else
            {
                RemoveError("CashierNumber", CashierNumberContainsNonValidCharacterError);
            }

            return isValid;
        }

        #endregion
    }
}
