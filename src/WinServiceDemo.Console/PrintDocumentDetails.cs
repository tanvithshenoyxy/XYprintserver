using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPrinterWinSvc
{
    public class PrinterQueueResponse
    {
        public DocumentOutput DocumentOutput { get; set; }
        public string AccountURI { get; set; }
        public int Status { get; set; }
    }

    public class DocumentOutput
    {
        public DocumentResponse DocumentResponse { get; set; }
    }

    public class DocumentResponse
    {
        public DocumentValue DocumentValue { get; set; }
    }

    public class DocumentValue
    {
        public List<Document> Documents { get; set; }
    }

    public class Document
    {
        public string PrinterId { get; set; }
        public string DocumentId { get; set; }
        public string Content { get; set; }
    }
}
