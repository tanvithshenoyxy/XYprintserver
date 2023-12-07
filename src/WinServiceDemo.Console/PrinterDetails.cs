using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using XYPrinterWinSvc;

namespace XYPrinterWinSvc
{


    //public class PrinterResponse
    //{
    //    public Output Output { get; set; }
    //    public string AccountURI { get; set; }
    //    public int Status { get; set; }
    //}

    //public class Output
    //{
    //    public Response Response { get; set; }
    //}

    //public class Response
    //{
    //    public Value Value { get; set; }
    //}

    //public class Value
    //{
    //    public List<Printer> Printers { get; set; }
    //}

    //public class Printer
    //{
    //    public string HostName { get; set; }
    //    public string Protocol { get; set; }
    //    public string Name { get; set; }
    //    public string PrinterId { get; set; }
    //    public string Model { get; set; }
    //}



    public class PrinterDetails
    {
        public string hostName { get; set; }
        public string protocol { get; set; }
        public string name { get; set; }
        public string printerId { get; set; }
        public string model { get; set; }
    }

    public class PrinterResponse
    {
        public List<Printer> printers { get; set; }
    }

    public class Printer
    {
        public string hostName { get; set; }
        public string protocol { get; set; }
        public string name { get; set; }
        public string printerId { get; set; }
        public string model { get; set; }
    }

    public class Response
    {
        public List<Printer> printers { get; set; }
        public string id { get; set; }
        public int status { get; set; }
    }

    public class Output
    {
        public string nodeType { get; set; }
        public Response response { get; set; }
        public string type { get; set; }
        public bool draft { get; set; }
        public string uri { get; set; }
        public int version { get; set; }
    }

    public class RootObject
    {
        public Output output { get; set; }
        public string accountURI { get; set; }
        public int status { get; set; }
    }

}
