using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.Models.DataAccess.TextHelper
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string filename)
        {
            string filepath = $@"{ Directory.GetCurrentDirectory() }\db\{ filename }";
            FileInfo fp = new FileInfo(filepath);
            fp.Directory.Create();

            return filepath;
        }

        public static List<string> LoadFile(this string file)
        {
            if (!File.Exists(file))
            {
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<StoreInfo> ConvertToStoreInfo(this List<string> lines)
        {
            List<StoreInfo> output = new List<StoreInfo>();

            foreach (string line in lines)
            {
                string[] cols = line.Split('|');
                if (cols.Length < 6)
                {
                    output.Add(new StoreInfo
                    {
                        UniformNumber = cols[0],
                        Name = cols[1],
                        TaxId = cols[2],
                        Phone = cols[3],
                        CashierNumber = cols[4]
                    });
                }
                else
                {
                    output.Add(new StoreInfo
                    {
                        UniformNumber = cols[0],
                        Name = cols[1],
                        TaxId = cols[2],
                        Phone = cols[3],
                        CashierNumber = cols[4],
                        ItemString = cols[5]
                    });
                }
            }

            return output;
        }

        public static void SaveToStoreFile(this List<StoreInfo> stores)
        {
            List<string> lines = new List<string>();

            // UniformNumber, Name, TaxId, Phone, CashierNumber
            foreach (StoreInfo store in stores)
            {
                lines.Add($"{ store.UniformNumber }|{ store.Name }|{ store.TaxId }|{ store.Phone }|{ store.CashierNumber }|{ store.ItemString }");
            }

            File.WriteAllLines(GlobalConfig.StoreFileName.FullFilePath(), lines);
        }

        public static List<StorePort> ConvertToStorePort(this List<string> lines)
        {
            List<StorePort> output = new List<StorePort>();

            foreach (string line in lines)
            {
                string[] cols = line.Split('|');

                output.Add(new StorePort
                {
                    Store = GlobalConfig.Connection.GetAllStore().Where(x => x.UniformNumber == cols[0]).FirstOrDefault(),
                    Port = cols[1]
                });
            }

            return output;
        }

        public static void SaveToStorePortFile(this List<StorePort> models)
        {
            List<string> lines = new List<string>();

            // UniformNumber, Port
            foreach (StorePort sp in models)
            {
                lines.Add($"{ sp.Store.UniformNumber }|{ sp.Port }");
            }

            File.WriteAllLines(GlobalConfig.StorePortFileName.FullFilePath(), lines);
        }

        public static List<Invoice> ConvertToInvoice(this List<string> lines)
        {
            List<Invoice> output = new List<Invoice>();

            foreach (string line in lines)
            {
                string[] cols = line.Split('|');

                output.Add(new Invoice
                {
                    Number = cols[0],
                    Amount = int.Parse(cols[1]),
                    OrderNumber = cols[2],
                    InValid = bool.Parse(cols[3]),
                    CreateTime = DateTime.Parse(cols[4])
                });
            }

            return output;
        }

        public static void SaveToInvoiceFile(this List<Invoice> models, StoreInfo store, int year, int month)
        {
            List<string> lines = new List<string>();

            // InvoiceNumber, InvoiceAmount, OrderNumber, IsValid, CreateTime
            foreach (Invoice i in models)
            {
                lines.Add($"{ i.Number }|{ i.Amount }|{ i.OrderNumber }|{ i.InValid.ToString() }|{ i.CreateTime }");
            }

            File.WriteAllLines(GlobalConfig.InvoiceFileName(store, year, month).FullFilePath(), lines);
        }
    }
}
