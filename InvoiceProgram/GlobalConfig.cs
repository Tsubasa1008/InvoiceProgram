using InvoiceProgram.Models;
using InvoiceProgram.Models.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram
{
    public static class GlobalConfig
    {
        public const string StoreFileName = "StoreDB.txt";

        public const string StorePortFileName = "StorePortDB.txt";

        public static IDataConnection Connection { get; set; } = new TextConnector();

        public static string InvoiceFileName(StoreInfo store, int year, int month)
        {
            return $"{ store.Name }_{ year.ToString().PadLeft(4, '0') }{ month.ToString().PadLeft(2, '0') }_InvoiceDB.txt";
        }

        public static string MReportFileName(StoreInfo store, int year, int month)
        {
            string path = $@"D:\月結表\{ store.Name }\{ year - 1911 }{ month.ToString("00") }.xlsx";
            FileInfo fp = new FileInfo(path);
            fp.Directory.Create();

            return path;
        }
    }
}
