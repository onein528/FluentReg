using Microsoft.Win32;
using Vanara.PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.ApplicationModel.AppService;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace FluentReg.FullTrust
{
    [SupportedOSPlatform("Windows10.0.10240")]
    public class MessageHandler
    {
        private static AppServiceConnection connection;
        AppServiceRequestReceivedEventArgs asrrea;

        public MessageHandler(AppServiceConnection sender)
        {
            connection = sender;
            connection.RequestReceived += OnAppServiceConnectionRequestReceived;
        }

        private async void OnAppServiceConnectionRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var deferral = args.GetDeferral();

            var message = args.Request.Message;

            if (message.ContainsKey("Arguments"))
            {
                var arguments = (string)message["Arguments"];
                Console.WriteLine("Recieved request: Arguments {0}", arguments);
                asrrea = args;
                Dictionary<string, object> messageDictionary = message.ToDictionary(x => x.Key, x => x.Value);

                await ParseArgumentsAsync(arguments, messageDictionary);
            }

            deferral.Complete();
        }

        public async Task ParseArgumentsAsync(string arguments, Dictionary<string, object> message)
        {
            var rootKey = message["RootKey"] as string;
            var subKey = message["SubKey"] as string;

            var hKey = GetRootPredefinedHKey(rootKey);
            RegistryKey baseKey = RegistryKey.OpenBaseKey(hKey, RegistryView.Registry64);
            RegistryKey key = baseKey.OpenSubKey(subKey, false);

            Console.WriteLine("RegOpenKeyEx Done.");
            Console.WriteLine("RootKey: \"{0}\", SubKey: \"{1}\"", rootKey, subKey);

            switch (arguments)
            {
                case "EnumSubKeys":
                    {
                        using (key)
                        {
                            var names = key.GetSubKeyNames();

                            await asrrea.Request.SendResponseAsync(new ValueSet()
                            {
                                { "SubKeyNames", names.ToArray() },
                                { "Status", "Success" },
                            });
                        }
                        break;
                    }
                case "EnumValues":
                    {
                        using (key)
                        {
                            var names = key.GetValueNames();

                            await asrrea.Request.SendResponseAsync(new ValueSet()
                            {
                                { "ValueNames", names.ToArray() },
                                { "Status", "Success" },
                            });
                        }
                        break;
                    }
                case "GetValueData":
                    {
                        break;
                    }
            }
        }

        private RegistryHive GetRootPredefinedHKey(string hKey)
        {
            if (hKey == "HKEY_LOCAL_MACHINE" || hKey == "HKLM")
            {
                return RegistryHive.LocalMachine;
            }
            else if (hKey == "HKEY_CURRENT_USER" || hKey == "HKCU")
            {
                return RegistryHive.CurrentUser;
            }
            else if (hKey == "HKEY_CLASSES_ROOT" || hKey == "HKCR")
            {
                return RegistryHive.ClassesRoot;
            }
            else if (hKey == "HKEY_USERS" || hKey == "HKU")
            {
                return RegistryHive.Users;
            }
            else if (hKey == "HKEY_CURRENT_CONFIG" || hKey == "HKCC")
            {
                return RegistryHive.CurrentConfig;
            }
            else
            {
                return 0;
            }
        }
    }
}
