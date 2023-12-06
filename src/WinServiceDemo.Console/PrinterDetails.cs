using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPrinterWinSvc
{


    public class PrinterResponse
    {
        public Output Output { get; set; }
        public string AccountURI { get; set; }
        public int Status { get; set; }
    }

    public class Output
    {
        public Response Response { get; set; }
    }

    public class Response
    {
        public Value Value { get; set; }
    }

    public class Value
    {
        public List<Printer> Printers { get; set; }
    }

    public class Printer
    {
        public string HostName { get; set; }
        public string Protocol { get; set; }
        public string Name { get; set; }
        public string PrinterId { get; set; }
        public string Model { get; set; }
    }

}
