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
            Console.WriteLine($"+----------------------------------------------------------------------");
            Console.WriteLine($"|  Called FluentHub.Bridge.Program.Main()");
            Console.WriteLine($"|  Initializing app service connection...");

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
                    Console.WriteLine($"|  Status: Failure");
                    Console.WriteLine($"|  Error: Failed to open.");

                    _appServiceConnection = null;
                    return;
                }

                Console.WriteLine($"|  Status: Success");
                Console.WriteLine($"+----------------------------------------------------------------------");
                Console.ReadLine();
            }
        }

        private static async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();

            ValueSet message = args.Request.Message;
            ValueSet returnData = new ValueSet();

            var action = message["action"] as string;
            var parameters = (message["args"] as string[]).ToList();
            var key = message["key"] as string;
            var rootKey = message["root"] as string;

            switch (action)
            {
                case "get":
                    {
                        try
                        {
                            RegistryHive hive = RegistryHive.LocalMachine;

                            Console.WriteLine($"+----------------------------------------------------------------------");
                            Console.WriteLine($"| Querying:");
                            Console.WriteLine($"|  Params: \"{string.Join(" ", (message["args"] as string[]))}\"");
                            Console.WriteLine($"|  Root:   \"{rootKey}\"");
                            Console.WriteLine($"|  Key:    \"{key}\"");

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
                                case "HKCR":
                                    hive = RegistryHive.ClassesRoot;
                                    break;
                                case "HKCU":
                                    hive = RegistryHive.CurrentUser;
                                    break;
                                case "HKLM":
                                    hive = RegistryHive.LocalMachine;
                                    break;
                                case "HKU":
                                    hive = RegistryHive.Users;
                                    break;
                                case "HKCC":
                                    hive = RegistryHive.CurrentConfig;
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

                            Console.WriteLine($"|  Status: Success");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"+----------------------------------------------------------------------");
                            Console.WriteLine($"|  Status: Failure");
                            Console.WriteLine($"|  Exception: {ex.Message}");

                            returnData.Add("status", "failed");
                            returnData.Add("exception", ex.Message);
                        }

                        break;
                    }
                case "set":
                    {
                        var value = message["value"] as string;
                        var data = message["data"] as string;

                        Console.WriteLine($"+----------------------------------------------------------------------");
                        Console.WriteLine($"| Mutation:");
                        Console.WriteLine($"|  Params: \"{string.Join(" ", (message["args"] as string[]))}\"");
                        Console.WriteLine($"|  Root:   \"{rootKey}\"");
                        Console.WriteLine($"|  Key:    \"{key}\"");
                        Console.WriteLine($"|  Value: \"{value}\"");
                        Console.WriteLine($"|  Data:  \"{data}\"");

                        int exitCode = LaunchRegistryHandler();

                        returnData.Add("exitcode", exitCode);

                        Console.WriteLine($"|  Info:  LaunchRegistryHandler() exited with \"{exitCode}\"");
                        Console.WriteLine($"|  Status: Success");
                        break;
                    }
            }

            try
            {
                await args.Request.SendResponseAsync(returnData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"+----------------------------------------------------------------------");
                Console.WriteLine($"| Error: Could not sent response");
                Console.WriteLine($"|  Exception: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"+----------------------------------------------------------------------");
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
