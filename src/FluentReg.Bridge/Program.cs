using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace FluentReg.Bridge
{
    public class Program
    {
        private static AppServiceConnection _appServiceConnection;
        const int E_FAIL = unchecked((int)0x80004005);

        static async Task Main(string[] args)
        {
            Console.WriteLine("DBG: Called FluentHub.Bridge.Program.Main()");

            if (_appServiceConnection == null)
            {
                _appServiceConnection = new AppServiceConnection();
                _appServiceConnection.AppServiceName = "CommunicationService";
                _appServiceConnection.PackageFamilyName = "FluentReg_jj4jk6607jjye";
                _appServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
                _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;

                var r = await _appServiceConnection.OpenAsync();
                if (r != AppServiceConnectionStatus.Success)
                {
                    Console.WriteLine("ERR: App service connection was failed to open.");

                    _appServiceConnection = null;
                    return;
                }

                Console.WriteLine("INF: App service connection was opened successufully.");
                Console.ReadLine();
            }
        }

        private static async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();

            Console.WriteLine("DBG: App service connection recieved a request.");

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            var action = message["action"] as string;
            var parameters = (message["args"] as string[]).ToList();

            switch (action)
            {
                case "get":
                    {
                        try
                        {
                            string key = message["key"] as string;
                            string rootKey = message["root"] as string;
                            RegistryHive hive = RegistryHive.LocalMachine;

                            Console.WriteLine("INF: Action is GET");
                            Console.WriteLine($"INF: Key:   {key}");

                            returnData.Add("status", "succeeded");

                            int queryType = 0;
                            RegistryView regView = RegistryView.Default;
                            if (parameters.Contains(@"/va") == true) queryType = 1;
                            if (parameters.Contains(@"/vs") == true) queryType = 2;
                            if (parameters.Contains(@"/ve") == true) queryType = 3;
                            if (parameters.Contains(@"/sk") == true) queryType = 4;
                            if (parameters.Contains(@"/key:32") == true) regView = RegistryView.Registry32;
                            if (parameters.Contains(@"/key:64") == true) regView = RegistryView.Registry64;

                            switch (rootKey)
                            {
                                case "HKLM":
                                    hive = RegistryHive.LocalMachine;
                                    break;
                            }

                            RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, regView);
                            RegistryKey subKey = baseKey.OpenSubKey(key, false);

                            switch (queryType)
                            {
                                case 1:
                                    var allValues = subKey.GetValueNames();
                                    break;
                                case 2:
                                    var valueName = message["value"] as string;
                                    var value = baseKey.GetValue(valueName);
                                    var valueKind = subKey.GetValueKind(valueName);
                                    break;
                                case 3:
                                    var emptryValue = subKey.GetValue("");
                                    break;
                                case 4:
                                    var subKeys = subKey.GetSubKeyNames();
                                    returnData.Add("response", subKeys);
                                    break;
                            }

                            Console.WriteLine("INF: Get action was completed successfully.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ERR: App service connection was failed to read registry.");
                            Console.WriteLine("     Exception: {0}", ex.Message);

                            returnData.Add("status", "failed");
                            returnData.Add("exception", ex.Message);
                        }

                        break;
                    }
                case "set":
                    {
                        string key = message["key"] as string;
                        string value = message["value"] as string;
                        object data = message["data"] as string;

                        Console.WriteLine("INF: Action is SET");
                        Console.WriteLine("INF: Key:   {0}", key);
                        Console.WriteLine("INF: Value: {0}", value);
                        Console.WriteLine("INF: Data:  {0}", data);

                        int exitCode = LaunchRegistryHandler();

                        returnData.Add("exitcode", exitCode);

                        Console.WriteLine("DBG: LaunchRegistryHandler() exited with {0}", exitCode);
                        Console.WriteLine("INF: Set action was completed successfully.");
                        break;
                    }
            }

            try
            {
                Console.WriteLine("DBG: App service connection sent the result.");
                await args.Request.SendResponseAsync(returnData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERR: App service connection was failed to send response.");
                Console.WriteLine("     Exception: {0}", ex.Message);
            }
            finally
            {
                deferral.Complete();
            }
        }

        private static int LaunchRegistryHandler()
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.Verb = "RunAs";
            info.UseShellExecute = true;

            // this path is a proxy for the Package
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            info.FileName = localAppDataPath + @"\microsoft\windowsapps\FluentReg.Registry.exe";

            Process elevatedProcess;
            int exitCode;

            try
            {
                elevatedProcess = Process.Start(info);
                elevatedProcess?.WaitForExit(10000);

                exitCode = elevatedProcess.ExitCode;
            }
            catch (Exception ex)
            {
                exitCode = 3;

                if (ex.HResult == E_FAIL)
                    exitCode = 1;
            }

            return exitCode;
        }

        private static void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Environment.Exit(0);
        }
    }
}
