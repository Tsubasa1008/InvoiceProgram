using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.Models
{
    public class Filter
    {
        public string DisplayName { get; set; }

        public Kind Type { get; set; }
    }

    public enum Kind
    {
        Number,
        OrderNumber
    }
}
