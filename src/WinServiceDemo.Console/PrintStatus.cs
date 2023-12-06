using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPrinterWinSvc
{
    public class PrintStatus
    {
        //public string AccountURI { get; set; }
        public string printServerId { get; set; }
        public string documentId { get; set; }
        public string printerId { get; set; }
        public string content { get; set; }
    }
}
