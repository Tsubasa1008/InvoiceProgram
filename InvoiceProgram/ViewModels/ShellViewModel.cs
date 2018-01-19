using Caliburn.Micro;
using InvoiceProgram.Models;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.ViewModels
{
    public class ShellViewModel : Conductor<object>.Collection.AllActive, IHandle<StoreInfo>, IHandle<HandleType>
    {
        public ShellViewModel()
        {
            Initialize();
        }

        #region -- Properties --

        private Screen _activeManageStoreView;
        private Screen _activeCreateInvoiceView;
        private Screen _activeInvoiceDetailView;
        private StoreInfo _selectedStore;
        private BindableCollection<StoreInfo> _stores = new BindableCollection<StoreInfo>();
        private string _selectedSerialPort;
        private BindableCollection<string> _serialPorts = new BindableCollection<string>();
        private StorePort _selectedStorePort;
        private BindableCollection<StorePort> _storePorts = new BindableCollection<StorePort>();
        private bool _selectedStoreIsVisible = true;
        private bool _createStoreIsVisible = false;
        private bool _createInvoiceIsVisible = false;
        private bool _invoiceDetailIsVisible = false;

        public Screen ActiveManageStoreView
        {
            get { return _activeManageStoreView; }
            set
            {
                _activeManageStoreView = value;
                NotifyOfPropertyChange(() => ActiveManageStoreView);
            }
        }

        public Screen ActiveCreateInvoiceView
        {
            get { return _activeCreateInvoiceView; }
            set
            {
                _activeCreateInvoiceView = value;
                NotifyOfPropertyChange(() => ActiveCreateInvoiceView);
            }
        }

        public Screen ActiveInvoiceDetailView
        {
            get { return _activeInvoiceDetailView; }
            set
            {
                _activeInvoiceDetailView = value;
                NotifyOfPropertyChange(() => ActiveInvoiceDetailView);
            }
        }

        public StorePort SelectedStorePort
        {
            get { return _selectedStorePort; }
            set
            {
                _selectedStorePort = value;
                NotifyOfPropertyChange(() => SelectedStorePort);
                NotifyOfPropertyChange(() => CanDeleteStorePort);
                LoadInvoiceDetailView();
                LoadCreateInvoiceView();
            }
        }

        public BindableCollection<StorePort> StorePorts
        {
            get { return _storePorts; }
            set
            {
                _storePorts = value;
                NotifyOfPropertyChange(() => StorePorts);
                
            }
        }

        public string SelectedSerialPort
        {
            get { return _selectedSerialPort; }
            set
            {
                _selectedSerialPort = value;
                NotifyOfPropertyChange(() => SelectedSerialPort);
                NotifyOfPropertyChange(() => CanCreateStorePort);
            }
        }

        public BindableCollection<string> SerialPorts
        {
            get { return _serialPorts; }
            set
            {
                _serialPorts = value;
                NotifyOfPropertyChange(() => SerialPorts);
            }
        }

        public StoreInfo SelectedStore
        {
            get { return _selectedStore; }
            set
            {
                _selectedStore = value;
                NotifyOfPropertyChange(() => SelectedStore);
                NotifyOfPropertyChange(() => CanDeleteStore);
                NotifyOfPropertyChange(() => CanCreateStorePort);
            }
        }

        public BindableCollection<StoreInfo> Stores
        {
            get { return _stores; }
            set
            {
                _stores = value;
                NotifyOfPropertyChange(() => Stores);
            }
        }

        public bool SelectedStoreIsVisible
        {
            get { return _selectedStoreIsVisible; }
            set
            {
                _selectedStoreIsVisible = value;
                NotifyOfPropertyChange(() => SelectedStoreIsVisible);
            }
        }

        public bool CreateStoreIsVisible
        {
            get { return _createStoreIsVisible; }
            set
            {
                _createStoreIsVisible = value;
                NotifyOfPropertyChange(() => CreateStoreIsVisible);
            }
        }

        public bool CreateInvoiceIsVisible
        {
            get { return _createInvoiceIsVisible; }
            set
            {
                _createInvoiceIsVisible = value;
                NotifyOfPropertyChange(() => CreateInvoiceIsVisible);
            }
        }

        public bool InvoiceDetailIsVisible
        {
            get { return _invoiceDetailIsVisible; }
            set
            {
                _invoiceDetailIsVisible = value;
                NotifyOfPropertyChange(() => InvoiceDetailIsVisible);
            }
        }

        #endregion

        #region -- Methods --

        private void Initialize()
        {
            Stores = new BindableCollection<StoreInfo>(GlobalConfig.Connection.GetAllStore());
            SerialPorts = new BindableCollection<string>(SerialPort.GetPortNames().ToList());
            StorePorts = new BindableCollection<StorePort>(GlobalConfig.Connection.GetAllStorePort());

            if (Stores.Count > 0)
            {
                SelectedStore = Stores.First();
            }
            SelectedSerialPort = SerialPorts.First();

            EventAggregationProvider.InvoiceProgramEventAggregator.Subscribe(this);
        }

        public void Handle(StoreInfo message)
        {
            if (!String.IsNullOrEmpty(message.UniformNumber))
            {
                Stores.Add(message);
            }
            SelectedStoreIsVisible = true;
            CreateStoreIsVisible = false;
        } 

        public void CreateStore()
        {
            ActiveManageStoreView = new ManageStoreViewModel(null);
            Items.Add(ActiveManageStoreView);

            SelectedStoreIsVisible = false;
            CreateStoreIsVisible = true;
        }

        public bool CanDeleteStore
        {
            get
            {
                if (SelectedStore != null && !StorePorts.Any(x => x.Store.UniformNumber == SelectedStore.UniformNumber))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void DeleteStore()
        {
            Stores.Remove(SelectedStore);
            GlobalConfig.Connection.SaveStores(Stores.ToList());
        }

        public bool CanCreateStorePort
        {
            get
            {
                if (SelectedStore != null && SelectedSerialPort != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void CreateStorePort()
        {
            if (StorePorts.Any(x => x.Store.UniformNumber == SelectedStore.UniformNumber))
            {
                System.Windows.MessageBox.Show("該店舖已設定連接埠對應，請先刪除該資料。");
            }
            else if (StorePorts.Any(x => x.Port == SelectedSerialPort))
            {
                System.Windows.MessageBox.Show("該連接埠已被使用，請重新確認。");
            }
            else
            {
                StorePort sp = new StorePort
                {
                    Store = SelectedStore,
                    Port = SelectedSerialPort
                };
                GlobalConfig.Connection.CreateStorePorts(sp);
                StorePorts.Add(sp);
                NotifyOfPropertyChange(() => CanDeleteStore);
            }
        }

        public bool CanDeleteStorePort
        {
            get
            {
                if (SelectedStorePort != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void DeleteStorePort()
        {
            StorePorts.Remove(SelectedStorePort);
            GlobalConfig.Connection.SaveStorePorts(StorePorts.ToList());
            NotifyOfPropertyChange(() => CanDeleteStore);
        }

        public void LoadCreateInvoiceView()
        {
            if (SelectedStorePort != null)
            {
                ActiveCreateInvoiceView = new CreateInvoiceViewModel(SelectedStorePort);
                Items.Add(ActiveCreateInvoiceView);

                CreateInvoiceIsVisible = true;
            }
            else
            {
                CreateInvoiceIsVisible = false;
            }
        }

        public void LoadInvoiceDetailView()
        {
            if (SelectedStorePort != null)
            {
                ActiveInvoiceDetailView = new InvoiceDetailViewModel(SelectedStorePort.Store);
                Items.Add(ActiveInvoiceDetailView);

                InvoiceDetailIsVisible = true;
            }
            else
            {
                InvoiceDetailIsVisible = false;
            }
        }

        public void Handle(HandleType message)
        {
            switch (message.Type)
            {
                case Cancel.Store:
                    break;
                case Cancel.StorePort:
                    SelectedStorePort = null;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
