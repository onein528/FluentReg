using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Principal;
using System.Runtime.Versioning;
using Vanara.PInvoke;
using System.Threading;

namespace FluentReg.FullTrust
{
    [SupportedOSPlatform("Windows10.0.10240")]
    public class Program
    {
        private static AppServiceConnection connection;

        static async Task Main()
        {
            Console.WriteLine("Initializing app service connection...");

            await InitializeAppServiceConnection();

            // Initialize message handler
            MessageHandler messageHandler = new(connection);

            Console.ReadLine();
        }

        private static async Task InitializeAppServiceConnection()
        {
            if (connection == null)
            {
                connection = new AppServiceConnection();
                connection.AppServiceName = "InProcessAppService";
                connection.PackageFamilyName = "FluentReg_jj4jk6607jjye";
                connection.ServiceClosed += AppServiceConnection_ServiceClosed;

                var r = await connection.OpenAsync();
                if (r == AppServiceConnectionStatus.Success)
                {
                    Console.WriteLine("Initialized app service connection");
                    return;
                }
                else
                {
                    Console.WriteLine("Could not initialize app service connection");
                    connection = null;
                    return;
                }
            }
        }

        private static void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Console.WriteLine("Closing app service connection...");
            Environment.Exit(0);
        }
    }
}
