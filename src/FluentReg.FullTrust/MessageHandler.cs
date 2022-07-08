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
        private static Win32Error lastErrorCode { get; set; }

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
            AdvApi32.REGSAM samDesire = AdvApi32.REGSAM.KEY_ALL_ACCESS;

            HKEY hKey = GetRootKey(rootKey);
            AdvApi32.RegOpenKeyEx(hKey, subKey, 0, samDesire, out var hKeyResult);

            Console.WriteLine("RootKey: {0}, SubKey: {1}", rootKey, subKey);

            switch (arguments)
            {
                case "EnumSubKeys":
                    {
                        #region RegEnumKeyEx
                        using (hKeyResult)
                        {
                            var idx = 0U;
                            var sb = new StringBuilder(256);
                            var names = new List<string>();
                            uint csb = 0, ccls = 0;
                            Win32Error err = 0;

                            while (err.Succeeded)
                            {
                                csb = (uint)sb.Capacity;

                                if ((err = AdvApi32.RegEnumKeyEx(hKeyResult, idx++, sb, ref csb, default, null, ref ccls, out _)).Succeeded)
                                {
                                    Console.WriteLine(sb);
                                    names.Add(sb.ToString());
                                }
                            }

                            Console.WriteLine("RegEnumKeyEx Done");

                            await asrrea.Request.SendResponseAsync(new ValueSet()
                            {
                                { "SubKeyNames", names.ToArray() },
                            });
                        }
                        #endregion
                        break;
                    }
                case "EnumValues":
                    {
                        #region RegQueryInfoKey
                        if (
                            SetLastErrorWrapper
                            (
                                AdvApi32.RegQueryInfoKey
                                (
                                    hKeyResult,
                                    null,
                                    ref Unsafe.NullRef<uint>(),
                                    IntPtr.Zero,
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<uint>(),
                                    out var NumberOfValues,
                                    out var MaxValueNameSize,
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<FILETIME>()
                                )
                            )
                            != Win32Error.ERROR_SUCCESS)
                        {
                            return; // need to set ValueSet
                        }
                        #endregion

                        #region RegEnumValue
                        System.Text.StringBuilder valueName = new System.Text.StringBuilder();
                        var valueNames = new List<string>();

                        for (uint index = 0; index < NumberOfValues; index++)
                        {
                            if (
                                SetLastErrorWrapper
                                (
                                    AdvApi32.RegEnumValue
                                    (
                                        hKeyResult,
                                        index,
                                        valueName,
                                        ref MaxValueNameSize,
                                        IntPtr.Zero,
                                        IntPtr.Zero, // When enumurating values, only it's name is needed, not it's data
                                        IntPtr.Zero
                                    )
                                 )
                                != Win32Error.ERROR_SUCCESS)
                            {
                                return; // need to set ValueSet
                            }

                            valueNames.Add(valueName.ToString());
                            valueName.Clear();
                        }
                        #endregion

                        await asrrea.Request.SendResponseAsync(new ValueSet()
                        {
                            { "NumberOfValues", NumberOfValues },
                            { "MaxValueNameSize", MaxValueNameSize },
                            { "ValueNames", valueNames.ToArray() },
                            { "Status", "Success" },
                        });
                        break;
                    }
                case "GetValueData":
                    {
                        break;
                    }
            }
        }



        private HKEY GetRootKey(string hKey)
        {
            if (hKey == "HKEY_LOCAL_MACHINE" || hKey == "HKLM")
            {
                return HKEY.HKEY_LOCAL_MACHINE;
            }
            else if (hKey == "HKEY_CURRENT_USER" || hKey == "HKCU")
            {
                return HKEY.HKEY_CURRENT_USER;
            }
            else if (hKey == "HKEY_CLASSES_ROOT" || hKey == "HKCR")
            {
                return HKEY.HKEY_CLASSES_ROOT;
            }
            else if (hKey == "HKEY_USERS" || hKey == "HKU")
            {
                return HKEY.HKEY_USERS;
            }
            else if (hKey == "HKEY_CURRENT_CONFIG" || hKey == "HKCC")
            {
                return HKEY.HKEY_CURRENT_CONFIG;
            }
            else
            {
                return HKEY.NULL;
            }
        }

        public static Win32Error GetLastErrorWrapper()
            => lastErrorCode;

        public static Win32Error SetLastErrorWrapper(Win32Error errorCode)
            => lastErrorCode = errorCode;
    }
}
