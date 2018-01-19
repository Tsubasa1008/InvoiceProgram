using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.Models
{
    public class Invoice
    {
        public string Number { get; set; }

        public int Amount { get; set; }

        public string OrderNumber { get; set; }

        public bool InValid { get; set; } = false;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public string AmountFormat
        {
            get
            {
                return Amount.ToString("$ ###,##0");
            }
        }

        public string CreateTimeFormat
        {
            get
            {
                return CreateTime.ToString("yyyy/MM/dd HH:mm:ss");
            }
        }
    }
}
