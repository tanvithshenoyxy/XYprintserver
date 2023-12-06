using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XYPrinterWinSvc
{
    public static class LogHelper
    {
        private static readonly string InfoLogFilePath = @"C:\XY\Printer\info_log_" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";
        private static readonly string ErrorLogFilePath = @"C:\XY\Printer\error_log_" + DateTime.Now.Date.ToString("yyyyMMdd") + ".txt";

        static bool logInfo = bool.Parse(ConfigurationManager.AppSettings["LogInfo"]);
        static bool logError = bool.Parse(ConfigurationManager.AppSettings["LogError"]);

        public static void LogInfo(string message)
        {
            if (logInfo)
                LogMessage(InfoLogFilePath, "INFO: " + message);
        }

        public static void LogError(string message)
        {
            if (logError)
                LogMessage(ErrorLogFilePath, "ERROR: " + message);
        }

        static void LogMessage(string filePath, string message)
        {
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (!File.Exists(filePath))
                {
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  -  {message}");
                    }
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  -  {message}");
                    }
                }


            }
            catch (Exception ex)
            {
                LogError($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
