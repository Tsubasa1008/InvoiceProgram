using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceProgram.Models.DataAccess.TextHelper;

namespace InvoiceProgram.Models.DataAccess
{
    public class TextConnector : IDataConnection
    {
        public void CreateStore(StoreInfo model)
        {
            List<StoreInfo> stores = GetAllStore();

            stores.Add(model);

            stores.SaveToStoreFile();
        }

        public void CreateStorePorts(StorePort model)
        {
            List<StorePort> storeports = GetAllStorePort();

            storeports.Add(model);

            storeports.SaveToStorePortFile();
        }

        public void SaveStorePorts(List<StorePort> models)
        {
            models.SaveToStorePortFile();
        }

        public List<StoreInfo> GetAllStore()
        {
            return GlobalConfig.StoreFileName.FullFilePath().LoadFile().ConvertToStoreInfo();
        }

        public List<StorePort> GetAllStorePort()
        {
            return GlobalConfig.StorePortFileName.FullFilePath().LoadFile().ConvertToStorePort();
        }

        public List<Invoice> GetAllInvoice(StoreInfo store, int year, int month)
        {
            return GlobalConfig.InvoiceFileName(store, year, month).FullFilePath().LoadFile().ConvertToInvoice();
        }

        public void SaveStores(List<StoreInfo> models)
        {
            models.SaveToStoreFile();
        }

        public void CreateInvoice(Invoice model, StoreInfo store, int year, int month)
        {
            List<Invoice> invoices = GetAllInvoice(store, year, month);

            invoices.Add(model);

            invoices.SaveToInvoiceFile(store, year, month);
        }

        public void UpdateInvoice(Invoice model, StoreInfo store, int year, int month)
        {
            List<Invoice> invoices = GetAllInvoice(store, year, month);

            invoices.Where(x => x.Number == model.Number).First().InValid = model.InValid;

            invoices.SaveToInvoiceFile(store, year, month);
        }
    }
}
