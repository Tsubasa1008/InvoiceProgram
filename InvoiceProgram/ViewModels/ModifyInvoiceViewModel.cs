using Caliburn.Micro;
using InvoiceProgram.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.ViewModels
{
    public class ModifyInvoiceViewModel : Screen
    {
        public ModifyInvoiceViewModel(Invoice invoice, StoreInfo store)
        {
            Invoice = invoice;
            Store = store;
        }

        #region -- Properties --

        private Invoice invoice;
        private StoreInfo Store;

        public Invoice Invoice
        {
            get { return invoice; }
            set
            {
                invoice = value;
                NotifyOfPropertyChange(() => Invoice);
            }
        }

        #endregion

        #region -- Methods --

        public void Commit()
        {
            GlobalConfig.Connection.UpdateInvoice(Invoice, Store, Invoice.CreateTime.Year, Invoice.CreateTime.Month);
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(Invoice);
            TryClose();
        }

        public void Cancel()
        {
            EventAggregationProvider.InvoiceProgramEventAggregator.PublishOnUIThread(new Invoice());
            TryClose();
        }

        #endregion
    }
}
