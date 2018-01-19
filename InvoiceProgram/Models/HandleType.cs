using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceProgram.Models
{
    public class HandleType
    {
        public Cancel Type { get; set; }
    }

    public enum Cancel
    {
        Store,
        StorePort
    }
}
