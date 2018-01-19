using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.Models.DataAccess
{
    public interface IDataConnection
    {
        void CreateStore(StoreInfo model);
        void SaveStores(List<StoreInfo> models);
        void CreateStorePorts(StorePort model);
        void SaveStorePorts(List<StorePort> models);
        void CreateInvoice(Invoice model, StoreInfo store, int year, int month);
        void UpdateInvoice(Invoice model, StoreInfo store, int year, int month);
        List<StoreInfo> GetAllStore();
        List<StorePort> GetAllStorePort();
        List<Invoice> GetAllInvoice(StoreInfo store, int year, int month);
    }
}
