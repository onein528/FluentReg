using FluentReg.Uwp.Models;
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

namespace FluentReg.Uwp.ViewModels
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            _items = new ObservableCollection<RegistryKeyNode>();
            Items = new ReadOnlyObservableCollection<RegistryKeyNode>(_items);

            InitializeRegistryNodes();
        }

        private readonly ObservableCollection<RegistryKeyNode> _items;
        public ReadOnlyObservableCollection<RegistryKeyNode> Items { get; }

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
            _items.Clear();
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CLASSES_ROOT", Path = "HKCR" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_USER", Path = "HKCU" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_LOCAL_MACHINE", Path = "HKLM" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_USERS", Path = "HKU" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_CONFIG", Path = "HKCC" });
        }

        public async Task<ObservableCollection<RegistryKeyNode>> LoadRegistry(string root, string key)
        {
            await ElevateFullTrustApp();

            ValueSet valueSet = new ValueSet();

            valueSet.Clear();
            valueSet.Add("Arguments", "EnumSubKeys");
            valueSet.Add("RootKey", root);
            valueSet.Add("SubKey", key);

            try
            {
                AppServiceResponse response = await App.ConnectionService.Connection.SendMessageAsync(valueSet);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    ValueSet test = response.Message;

                        ObservableCollection<RegistryKeyNode> items = new ObservableCollection<RegistryKeyNode>();
                        foreach (string item in response.Message["SubKeyNames"] as string[])
                        {
                            if (string.IsNullOrEmpty(key))
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{item}" });
                            else
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{key}\\{item}" });
                        }

                        return items;

                    //else
                    //{
                    //}
                }
                //else
                //{
                //}

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        private async Task ElevateFullTrustApp()
        {
        }
    }
}
