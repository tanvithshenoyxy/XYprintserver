using System;
using Topshelf;
using XYPrinterWinSvc;

namespace WinServiceDemo.Console
{
    /// <summary>
    /// Program
    /// </summary>
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// It is used to set up the Windows Service.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            //Configure service host
            var rc = HostFactory.Run(configure =>
            {
                //Service actions
                configure.Service<ServiceManager>(s =>
                {
                    s.ConstructUsing(() => new ServiceManager());
                    s.WhenStarted(i => i.Start());
                    s.WhenStopped(i => i.Stop());
                    s.WhenShutdown(i => i.Stop());
                });

                //Service settings
                configure.RunAsLocalSystem();
                configure.StartAutomatically();
                configure.SetStopTimeout(TimeSpan.FromSeconds(10));
              //  configure.OnException(ex => { LogHelper.LogError(($"Windows Service level exception: {ex.Message}")); });

                //Service description
                configure.SetServiceName("XY.Print.Service");
                configure.SetDisplayName("XY.Print.Service");
                configure.SetDescription("XY Printer Windows Service");
            });

            //Exit code
            var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
            System.Console.WriteLine($"Exit code: {exitCode}");
            Environment.ExitCode = exitCode;
        }
    }
}
