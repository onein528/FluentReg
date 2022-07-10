using FluentReg.Uwp.Models;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentReg.Uwp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            ColumnName = new GridLength(256, GridUnitType.Pixel);
            ColumnType = new GridLength(128, GridUnitType.Pixel);

            _subKeyItems = new ObservableCollection<RegistryKeyNode>();
            SubKeyItems = new ReadOnlyObservableCollection<RegistryKeyNode>(_subKeyItems);

            _valueItems = new ObservableCollection<RegistryValueModel>();
            ValueItems = new ReadOnlyObservableCollection<RegistryValueModel>(_valueItems);

            InitializeRegistryNodes();
        }

        private readonly ObservableCollection<RegistryKeyNode> _subKeyItems;
        public ReadOnlyObservableCollection<RegistryKeyNode> SubKeyItems { get; }

        private readonly ObservableCollection<RegistryValueModel> _valueItems;
        public ReadOnlyObservableCollection<RegistryValueModel> ValueItems { get; }

        private string _exceptionMessage;
        public string ExceptionMessage { get => _exceptionMessage; set => SetProperty(ref _exceptionMessage, value); }

        private GridLength _columnName;
        public GridLength ColumnName { get => _columnName; set => SetProperty(ref _columnName, value); }

        private GridLength _columnType;
        public GridLength ColumnType { get => _columnType; set => SetProperty(ref _columnType, value); }

        public async Task OnMainPageLoaded(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        public void ConnectionHandler()
        {
            if (App.ConnectionService != null)
            {
            }
        }

        private void InitializeRegistryNodes()
        {
            _subKeyItems.Clear();
            _subKeyItems.Add(new RegistryKeyNode() { Name = "HKEY_CLASSES_ROOT", Path = "HKCR" });
            _subKeyItems.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_USER", Path = "HKCU" });
            _subKeyItems.Add(new RegistryKeyNode() { Name = "HKEY_LOCAL_MACHINE", Path = "HKLM" });
            _subKeyItems.Add(new RegistryKeyNode() { Name = "HKEY_USERS", Path = "HKU" });
            _subKeyItems.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_CONFIG", Path = "HKCC" });
        }

        public async Task<List<RegistryKeyNode>> LoadSubKeys(string path)
        {
            try
            {
                var (root, key) = DivideRootAndKey(path);

                AppServiceResponse response =
                    await App.ConnectionService.Connection.SendMessageAsync(new ValueSet()
                    {
                        { "Arguments", "EnumSubKeys" },
                        { "RootKey", root },
                        { "SubKey", key },
                    });

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    ValueSet message = response.Message;

                    if (message["Status"] as string == "Success")
                    {
                        List<RegistryKeyNode> items = new List<RegistryKeyNode>();

                        foreach (string item in response.Message["SubKeyNames"] as string[])
                        {
                            if (string.IsNullOrEmpty(key))
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{item}" });
                            else
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{key}\\{item}" });
                        }

                        return items;
                    }
                    else if(message["Status"] as string == "Failure")
                    {
                        ExceptionMessage = message["Error"] as string;
                    }
                }
                else
                {
                    ExceptionMessage = "Could not get a response from app service connection";
                }

                return null;
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
                return null;
            }
        }

        public async Task LoadValues(string path)
        {
            try
            {
                var (root, key) = DivideRootAndKey(path);

                AppServiceResponse response =
                    await App.ConnectionService.Connection.SendMessageAsync(new ValueSet()
                    {
                        { "Arguments", "EnumValues" },
                        { "RootKey", root },
                        { "SubKey", key },
                    });

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    ValueSet message = response.Message;

                    if (message["Status"] as string == "Success")
                    {
                        var valueNames = (response.Message["ValueNames"] as string[]).ToList();
                        var valueTypes = (response.Message["ValueTypes"] as string[]).ToList();
                        var valueDataAll = (response.Message["ValueDataAll"] as object[]).ToList();

                        _valueItems.Clear();
                        for (int idx = 0; idx < valueNames.Count(); idx++)
                        {
                            RegistryValueModel model = new RegistryValueModel()
                            {
                                Name = valueNames[idx],
                                FriendlyName = valueNames[idx],
                                Type = valueTypes[idx],
                                OriginalValue = valueDataAll[idx],
                            };

                            if (model.Type == "REG_SZ" ||
                                model.Type == "REG_EXPAND_SZ" ||
                                model.Type == "REG_MULTI_SZ")
                            {
                                model.ValueIsString = true;
                            }
                            else model.ValueIsString = false;

                            _valueItems.Add(model);
                        }

                        NormalizeValues();
                    }
                    else if (message["Status"] as string == "Failure")
                    {
                        ExceptionMessage = message["Error"] as string;
                    }
                }
                else
                {
                    ExceptionMessage = "Could not get a response from app service connection";
                }
            }
            catch (Exception ex)
            {
                ExceptionMessage = ex.Message;
            }
        }

        public void NormalizeValues()
        {
            bool hasSetDefaultKey = false;
            int index = 0;

            var orderedByItemType = new ObservableCollection<RegistryValueModel>(ValueItems.OrderBy(x => x.Name));
            _valueItems.Clear();
            foreach (var orderedItem in orderedByItemType) _valueItems.Add(orderedItem);

            foreach (var model in _valueItems)
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    hasSetDefaultKey = true;
                    model.FriendlyName = "(Default)";
                    model.FriendlyValue = model.OriginalValue.ToString();
                }
                else
                {
                    switch (model.Type)
                    {
                        case "REG_BINARY":
                            {
                                var value = model.OriginalValue as byte[];
                                if (value.Count() == 0)
                                {
                                    model.FriendlyValue += $"(zero-length binary value)";
                                    break;
                                }

                                foreach (var item in value)
                                {
                                    model.FriendlyValue += $"{item} ";
                                }
                                break;
                            }
                        case "REG_MULTI_SZ":
                            {
                                var value = model.OriginalValue as string[];
                                foreach (var item in value)
                                {
                                    model.FriendlyValue += $"{item} ";
                                }
                                break;
                            }
                        case "REZ_EXPAND_SZ":
                        case "REG_SZ":
                            {
                                model.FriendlyValue = model.OriginalValue.ToString();
                                break;
                            }
                        case "REG_QWORD":
                        case "REG_DWORD":
                            {
                                model.FriendlyValue = string.Format("0x{0,8:x8} ({1})", Convert.ToUInt64(model.OriginalValue.ToString()), model.OriginalValue.ToString());
                                break;
                            }
                    }

                    if (!hasSetDefaultKey)
                    {
                        // Indedx of empty name
                        index++;
                    }
                }
            }

            if (hasSetDefaultKey)
            {
                _valueItems.Move(index, 0);
            }
            else
            {
                _valueItems.Insert(0, new RegistryValueModel()
                {
                    FriendlyName = "(Default)",
                    FriendlyValue = "(Value not set)",
                    Type = "REG_SZ",
                    ValueIsString = true,
                });
            }
        }

        public (string, string) DivideRootAndKey(string path)
        {
            if (path.Contains("\\"))
            {
                var dividedPath = path.Split("\\", 2).ToList();
                var rootKey = dividedPath[0];
                var baseKey = dividedPath[1];

                return (rootKey, baseKey);
            }
            else
            {
                return (path, "");
            }
        }
    }
}
