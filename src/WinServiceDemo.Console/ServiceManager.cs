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

namespace WinServiceDemo.Console
{
    /// <summary>
    /// Service manager containing service actions and managing processing.
    /// </summary>
    public class ServiceManager
    {
        private static readonly double TimerInterval = 5000;
        private Timer _timer;

        static HttpClient _httpClient;
        private static Dictionary<string, Printer> printerCache;// = new Dictionary<string, Printer>();

        static string GetPrinterRelativeURI;// = ConfigurationManager.AppSettings["Get_Printer_Relative_URI"];
        static string PrinterServerID;// = ConfigurationManager.AppSettings["print_server_id"];
        static string AuthKey;// = ConfigurationManager.AppSettings["auth_key"];
        static string AuthValue;//= ConfigurationManager.AppSettings["auth_value"];
        static string baseURI;// = ConfigurationManager.AppSettings["ApiBaseUrl"];
        static string GetPrinterQueueRelativeURI;
        static string UpdatePrinterURI;

        /// <summary>
        /// Starts the service
        /// </summary>
        public void Start()
        {

            //LogHelper.LogInfo("Printing Process Started");

            AuthKey = ConfigurationManager.AppSettings["auth_key"];
            //  LogHelper.LogInfo("AuthKey :" + AuthKey);


            AuthValue = ConfigurationManager.AppSettings["auth_value"];


            baseURI = ConfigurationManager.AppSettings["ApiBaseUrl"];
            //  LogHelper.LogInfo("baseURI :" + baseURI);


            GetPrinterRelativeURI = ConfigurationManager.AppSettings["Get_Printer_Relative_URI"];
            // LogHelper.LogInfo("GetPrinterRelativeURI :" + GetPrinterRelativeURI);


            PrinterServerID = ConfigurationManager.AppSettings["print_server_id"];
            // LogHelper.LogInfo("PrinterServerID :" + PrinterServerID);


            GetPrinterQueueRelativeURI = ConfigurationManager.AppSettings["Get_Printer_Queue_Relative_URI"];
            // LogHelper.LogInfo("GetPrinterQueueRelativeURI :" + GetPrinterQueueRelativeURI);

            UpdatePrinterURI = ConfigurationManager.AppSettings["Update_Printer_URI"];
            // LogHelper.LogInfo("UpdatePrinterURI :" + UpdatePrinterURI);


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

            // LogHelper.LogInfo("Processing...");

            try
            {
                Task.Run(async () => await GetPrinterDetails()).Wait();
            }
            catch (Exception ex)
            {
                //     LogHelper.LogInfo($"Exception: {ex.Message}");
            }

            LogHelper.LogInfo("Processing... DONE!");

            _timer.Interval = TimerInterval;
            _timer.Enabled = true;
        }



        public static async Task GetPrinterDetails()
        {
            _httpClient = new HttpClient();
            Uri baseUrI = new Uri(baseURI, UriKind.Absolute);
            //  string relativeUriString = "xypop.api.queryPrinters?id={{print_server_id}}";
            Uri relativeUri = new Uri(GetPrinterRelativeURI, UriKind.Relative);

            Uri fullUri = new Uri(baseUrI, relativeUri);

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthKey, AuthValue);

            // LogHelper.LogInfo("Continuous polling started");

            //while (true)
            //{
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(fullUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    ProcessPrintQueue(responseBody);
                }
                else
                {
                    //     LogHelper.LogError($"API request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                //   LogHelper.LogError($"An error occurred: {ex.Message}");
            }

           // LogHelper.LogInfo("Service Polling Interval");
            //  await Task.Delay(PollingInterval);
            //}
        }


        private static async void ProcessPrintQueue(string responseBody)
        {
            try
            {
                //   LogHelper.LogInfo($"ProcessPrintQueue: Received API response  : {responseBody}");

                PrinterResponse printerResponse = JsonConvert.DeserializeObject<PrinterResponse>(responseBody);

                List<Printer> printers = printerResponse?.Output?.Response?.Value?.Printers;


                if (printerCache == null || printerCache.Count <= 0)
                {
                    printerCache = new Dictionary<string, Printer>();

                    foreach (var printer in printers)
                    {
                        printerCache[printer.PrinterId] = printer;
                    }
                }

                _httpClient = new HttpClient();

                Uri baseUrI = new Uri(baseURI, UriKind.Absolute);

                string relativeUriString = "xypop.api.queryPrinterQueue?id={{print_server_id}}";

                Uri relativeUri = new Uri(relativeUriString, UriKind.Relative);

                Uri fullUri = new Uri(baseUrI, relativeUri);

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthKey, AuthValue);


                HttpResponseMessage printerQueueRresponse = await _httpClient.GetAsync(fullUri);

                if (printerQueueRresponse.IsSuccessStatusCode)
                {

                    PrinterQueueResponse response = JsonConvert.DeserializeObject<PrinterQueueResponse>(printerQueueRresponse.Content.ReadAsStringAsync().Result);

                    List<Document> documents = response?.DocumentOutput?.DocumentResponse?.DocumentValue?.Documents;

                    string printerUri = ReturnPrinrtURI(printers[0].Model);

                    foreach (var _document in documents)
                    {
                        PrintDoc(printerUri, _document.Content, printers[0].PrinterId, _document.DocumentId);
                    }

                }
            }
            catch (Exception ex)
            {
                // LogHelper.LogError($"ProcessPrintQueue - An error occurred: {ex.Message}");
            }
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
                    printServerId = PrinterServerID,
                    printerId = printerid,
                    documentId = documentID,
                    content = res.Content.ReadAsStringAsync().Result
                };

                UpdatePrintStatus(_status);

                //   LogHelper.LogError($"PrintDoc completed for printerid: " + printerid + " and for documentID: " + documentID);
            }
            catch (Exception ex)
            {
                //  LogHelper.LogError($"PrintDoc - An error occurred: {ex.Message}");
            }
        }


        private async static void UpdatePrintStatus(PrintStatus status)
        {

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthKey, AuthValue);

            string jsonRequest = JsonConvert.SerializeObject(status);

            StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(UpdatePrinterURI, content);

                //if (response.IsSuccessStatusCode)
                //{
                //    //        LogHelper.LogError($"Update Print Status for Document ID :" + status.documentId + "and for printerId: " + status.printerId + " Sucessfull");
                //}
                //else
                //{
                //    //      LogHelper.LogError($"Failed Updating Print Status for Document ID :" + status.documentId);
                //}
            }
            catch (Exception ex)
            {
                //    LogHelper.LogError($"UpdatePrintStatus - An error occurred: {ex.Message}");
            }
        }


        private static string ReturnPrinrtURI(String printerName)
        {

            string firstTwoCharacters = printerName.Substring(0, 2);
            string ipAddress = null;

            if (firstTwoCharacters == "TM")
            {
                ipAddress = ConfigurationManager.AppSettings["Fisical_Printer_URI"];
                //      LogHelper.LogInfo("Fisical_Printer_URI :" + ipAddress);

                return $"http://{ipAddress}/cgi-bin/epos/service.cgi";
            }
            else
            {
                ipAddress = ConfigurationManager.AppSettings["NON_Fisical_Printer_URI"];
                //     LogHelper.LogInfo("NON_Fisical_Printer_URI :" + ipAddress);

                return $"http://{ipAddress}/cgi-bin/fpmate.cgi?timeout=20000";
            }
        }
    }
}
