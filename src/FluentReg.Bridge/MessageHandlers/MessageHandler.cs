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

namespace FluentReg.Bridge.MessageHandlers
{
    public class MessageHandler : IMessageHandler
    {
        private static Win32Error lastErrorCode { get; set; }

        public void Initialize(AppServiceConnection connection)
        {
        }

        public async Task ParseArgumentsAsync(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args, string arguments, Dictionary<string, object> message)
        {
            var rootKey = message["RootKey"] as string;
            var subKey = message["SubKey"] as string;
            AdvApi32.REGSAM samDesire = AdvApi32.REGSAM.KEY_READ;

            HKEY hKey = GetRootKey(rootKey);
            AdvApi32.RegOpenKeyEx(hKey, subKey, 0, samDesire, out var hKeyResult);

            switch (arguments)
            {
                case "EnumSubKeys":
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
                                    out var NumberOfKeys,
                                    out var MaxKeyNameSize,
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<uint>(),
                                    out Unsafe.NullRef<uint>(),
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

                        #region RegEnumKeyEx
                        System.Text.StringBuilder subKeyName = new System.Text.StringBuilder();
                        var subKeyNames = new List<string>();

                        for (uint index = 0; index < NumberOfKeys; index++)
                        {
                            if (
                                SetLastErrorWrapper
                                (
                                    AdvApi32.RegEnumKeyEx
                                    (
                                        hKeyResult,
                                        index,
                                        subKeyName,
                                        ref MaxKeyNameSize,
                                        IntPtr.Zero,
                                        null,
                                        ref Unsafe.NullRef<uint>(),
                                        out Unsafe.NullRef<FILETIME>()
                                    )
                                 )
                                != Win32Error.ERROR_SUCCESS)
                            {
                                return; // need to set ValueSet
                            }

                            subKeyNames.Add(subKeyName.ToString());
                            subKeyName.Clear();
                        }
                        #endregion

                        await args.Request.SendResponseAsync(new ValueSet()
                        {
                            { "NumberOfKeys", NumberOfKeys },
                            { "MaxKeyNameSize", MaxKeyNameSize },
                            { "SubKeyNames", subKeyNames },
                        });
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

                        await args.Request.SendResponseAsync(new ValueSet()
                        {
                            { "NumberOfValues", NumberOfValues },
                            { "MaxValueNameSize", MaxValueNameSize },
                            { "ValueNames", valueNames },
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
                return HKEY.HKEY_LOCAL_MACHINE;
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
