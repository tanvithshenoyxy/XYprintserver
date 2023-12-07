using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Configuration;
using XYPrinterWinSvc;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace WinServiceDemo.Console
{
    /// <summary>
    /// Service manager containing service actions and managing processing.
    /// </summary>
    public class ServiceManager
    {
        private double TimerInterval; //10 seconds
        private Timer _timer;

        //private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(5);
        static HttpClient _httpClient;
        private static Dictionary<string, Printer> printerCache;// = new Dictionary<string, Printer>();

        static string GetPrinterRelativeURI;// = ConfigurationManager.AppSettings["Get_Printer_Relative_URI"];
        static string PrinterServerID;// = ConfigurationManager.AppSettings["print_server_id"];
        static string AuthKey;// = ConfigurationManager.AppSettings["auth_key"];
        static string AuthValue;//= ConfigurationManager.AppSettings["auth_value"];
        static string baseURI;// = ConfigurationManager.AppSettings["ApiBaseUrl"];
        static string GetPrinterQueueRelativeURI;
        static string UpdatePrinterURI;
        static string Interval;
        static bool iProcessed = false;

        /// <summary>
        /// Starts the service
        /// </summary>
        public void Start()
        {


            AuthKey = ConfigurationManager.AppSettings["auth_key"];

            AuthValue = ConfigurationManager.AppSettings["auth_value"];

            baseURI = ConfigurationManager.AppSettings["ApiBaseUrl"];

            GetPrinterRelativeURI = ConfigurationManager.AppSettings["Get_Printer_Relative_URI"];

            PrinterServerID = ConfigurationManager.AppSettings["print_server_id"];

            GetPrinterQueueRelativeURI = ConfigurationManager.AppSettings["Get_Printer_Queue_Relative_URI"];

            UpdatePrinterURI = ConfigurationManager.AppSettings["Update_Printer_URI"];

            Interval = ConfigurationManager.AppSettings["Interval"];

            TimerInterval = String.IsNullOrEmpty(Interval) ? 5000 : Convert.ToInt32(Interval);

            GetPrinterDetails();

            _timer = new Timer(TimerInterval);
            _timer.Elapsed += Process;
            _timer.Start();
        }

        /// <summary>
        /// Stops the service
        /// </summary>
        public void Stop()
        {
            //  LogHelper.LogInfo("Printing Process Stopped at :" + DateTime.Now.ToString());
        }

        /// <summary>
        /// Process action for each tick
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void Process(object sender, ElapsedEventArgs eventArgs)
        {
            _timer.Enabled = false;

            try
            {
                //Task.Run(async () => await GetPrinterDetails()).Wait();
                Task.Run(async () => await GetQueueDocuments()).Wait();
            }
            catch (Exception ex)
            {
                //     LogHelper.LogInfo($"Exception: {ex.Message}");
            }


            _timer.Interval = TimerInterval;
            _timer.Enabled = true;
        }



        public static async Task GetPrinterDetails()
        {

            _httpClient = new HttpClient();
            Uri baseUrI = new Uri(baseURI, UriKind.Absolute);
            Uri relativeUri = new Uri(GetPrinterRelativeURI, UriKind.Relative);

            Uri fullUri = new Uri(baseUrI, relativeUri);

            _httpClient.DefaultRequestHeaders.Add(AuthKey, AuthValue);
            try
            {

                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler)) //see update below
                {
                    client.DefaultRequestHeaders.Add(AuthKey, AuthValue);
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fullUri);


                    var httpResponse = await client.SendAsync(httpRequestMessage);

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string responseBody = await httpResponse.Content.ReadAsStringAsync();
                        await ProcessPrintQueue(responseBody);
                    }
                }

            }
            catch (Exception ex)
            {
                //   LogHelper.LogError($"An error occurred: {ex.Message}");
            }

        }


        private static async Task ProcessPrintQueue(string responseBody)
        {
            try
            {

                JObject json = JObject.Parse(responseBody);

                string printersJson = json["output"]["response"]["$value"]["printers"].ToString();

                List<Printer> printers = JsonConvert.DeserializeObject<List<Printer>>(printersJson);


                //List<Printer> printers = printerResponse.printers;



                if (printerCache == null || printerCache.Count <= 0)
                {
                    printerCache = new Dictionary<string, Printer>();

                    foreach (var printer in printers)
                    {
                        printerCache[printer.printerId] = printer;
                    }
                }

                #region temp
                //_httpClient = new HttpClient();

                //Uri baseUrI = new Uri(baseURI, UriKind.Absolute);


                //Uri relativeUri = new Uri(GetPrinterQueueRelativeURI, UriKind.Relative);

                //Uri fullUri = new Uri(baseUrI, relativeUri);


                //HttpClientHandler handler = new HttpClientHandler()
                //{
                //    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                //};

                //using (var client = new HttpClient(handler)) //see update below
                //{
                //    client.DefaultRequestHeaders.Add(AuthKey, AuthValue);
                //    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fullUri);


                //    var httpResponse = await client.SendAsync(httpRequestMessage);

                //    if (httpResponse.IsSuccessStatusCode)
                //    {


                //        string printerUri = ReturnPrinrtURI(printers[0].model, printers[0].hostName);

                //        string responseBody2 = await httpResponse.Content.ReadAsStringAsync();

                //        JObject json2 = JObject.Parse(responseBody2);

                //        JToken documentsNode = json2["output"]["response"]["$value"]["documents"];

                //        string documentsJsonResponse = documentsNode.ToString();

                //        List<Document> documents = JsonConvert.DeserializeObject<List<Document>>(documentsJsonResponse);

                //        foreach (var _document in documents)
                //        {
                //            PrintDoc(printerUri, _document.Content, printers[0].printerId, _document.DocumentId);
                //        }

                //    }
                //} 
                #endregion
            }
            catch (Exception ex)
            {
                // LogHelper.LogError($"ProcessPrintQueue - An error occurred: {ex.Message}");
            }
        }


        public static async Task GetQueueDocuments()
        {
            try
            {
                _httpClient = new HttpClient();

                Uri baseUrI = new Uri(baseURI, UriKind.Absolute);


                Uri relativeUri = new Uri(GetPrinterQueueRelativeURI, UriKind.Relative);

                Uri fullUri = new Uri(baseUrI, relativeUri);


                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler)) //see update below
                {
                    client.DefaultRequestHeaders.Add(AuthKey, AuthValue);
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fullUri);


                    var httpResponse = await client.SendAsync(httpRequestMessage);

                    if (httpResponse.IsSuccessStatusCode)
                    {

                        if (printerCache == null || printerCache.Count <= 0)
                        {
                            await GetPrinterDetails();
                        }

                        string responseBody2 = await httpResponse.Content.ReadAsStringAsync();

                        JObject json2 = JObject.Parse(responseBody2);

                        JToken documentsNode = json2["output"]["response"]["$value"]["documents"];

                        string documentsJsonResponse = documentsNode.ToString();

                        List<Document> documents = JsonConvert.DeserializeObject<List<Document>>(documentsJsonResponse);





                        foreach (var _document in documents)
                        {
                            Printer foundPrinter = GetPrinterByID(_document.PrinterId);

                            if (foundPrinter == null)
                                return;

                            string printerUri = ReturnPrinrtURI(foundPrinter.model, foundPrinter.hostName);

                            PrintDoc(printerUri, _document.Content, foundPrinter.printerId, _document.DocumentId);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // LogHelper.LogError($"ProcessPrintQueue - An error occurred: {ex.Message}");
            }
        }

        private static Printer GetPrinterByID(string id)
        {
            if (printerCache.ContainsKey(id))
            {
                return printerCache[id];
            }
            else
                return null;
        }


        private async static void PrintDoc(string printerUri, string message, string printerid, string documentID)
        {

            try
            {
                //  LogHelper.LogError($"PrintDoc started for printerid: " + printerid + " and for documentID: " + documentID);
                _httpClient = new HttpClient();

                StringContent content = new StringContent(message, Encoding.UTF8, "text/xml");

                HttpResponseMessage res = await _httpClient.PostAsync(printerUri, content);

                PrintStatus _status = new PrintStatus()
                {
                    id = PrinterServerID,
                    printerId = printerid,
                    documentId = documentID,
                    content = res.Content.ReadAsStringAsync().Result
                };

                Task.Run(async () => await UpdatePrintStatus(_status)).Wait();


                //   LogHelper.LogError($"PrintDoc completed for printerid: " + printerid + " and for documentID: " + documentID);
            }
            catch (Exception ex)
            {
                //  LogHelper.LogError($"PrintDoc - An error occurred: {ex.Message}");
            }
        }


        private async static Task UpdatePrintStatus(PrintStatus status)
        {

            try
            {

                Uri baseUrI = new Uri(baseURI, UriKind.Absolute);

                Uri relativeUri = new Uri(UpdatePrinterURI, UriKind.Relative);

                Uri fullUri = new Uri(baseUrI, relativeUri);

                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                using (var client = new HttpClient(handler))
                {

                    var request = new UpdateRequest
                    {
                        Value = new RequestValue
                        {
                            id = status.id,
                            documentId = status.documentId,
                            printerId = status.printerId,
                            content = status.content,
                        }
                    };


                    Request req = new Request() { request = request };

                    string jsonString2 = JsonConvert.SerializeObject(req, Formatting.None);
                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, fullUri);
                    httpRequest.Headers.Add(AuthKey, AuthValue);
                    httpRequest.Content = new StringContent(jsonString2, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.SendAsync(httpRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                //    LogHelper.LogError($"UpdatePrintStatus - An error occurred: {ex.Message}");
            }
        }


        private static string ReturnPrinrtURI(String printerName, string hostname)
        {

            string firstTwoCharacters = printerName.Substring(0, 2);
            // string ipAddress = null;

            if (firstTwoCharacters == "TM")
            {
                // ipAddress = ConfigurationManager.AppSettings["Fisical_Printer_URI"];
                //      LogHelper.LogInfo("Fisical_Printer_URI :" + ipAddress);

                return $"http://{hostname}/cgi-bin/epos/service.cgi";
            }
            else
            {
                // ipAddress = ConfigurationManager.AppSettings["NON_Fisical_Printer_URI"];
                //     LogHelper.LogInfo("NON_Fisical_Printer_URI :" + ipAddress);

                return $"http://{hostname}/cgi-bin/fpmate.cgi?timeout=20000";
            }
        }
    }
}
