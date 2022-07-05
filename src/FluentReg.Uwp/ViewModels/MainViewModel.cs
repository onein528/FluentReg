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
        }

        private readonly ObservableCollection<RegistryKeyNode> _items;
        public ReadOnlyObservableCollection<RegistryKeyNode> Items { get; }

        public async Task OnMainPageLoaded(object sender, RoutedEventArgs e)
        {
            await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        public async Task ConnectionHandler()
        {
            if (App.ConnectionService != null)
            {
                App.ConnectionService.Connection.RequestReceived += Connection_RequestReceived;

                InitializeRegistryNodes();
                await LoadRegistry("HKLM", "");
            }
        }

        private void Connection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
        }

        private void InitializeRegistryNodes()
        {
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CLASSES_ROOT", Path = "HKCR" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_USER", Path = "HKCU" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_LOCAL_MACHINE", Path = "HKLM" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_USERS", Path = "HKU" });
            _items.Add(new RegistryKeyNode() { Name = "HKEY_CURRENT_CONFIG", Path = "HKCC" });
        }

        public async Task<ObservableCollection<RegistryKeyNode>> LoadRegistry(string root, string key)
        {
            ValueSet valueSet = new ValueSet();

            valueSet.Clear();
            valueSet.Add("action", "get");
            valueSet.Add("args", new string[] { "/sk" });
            valueSet.Add("key", key);
            valueSet.Add("root", root);

            try
            {
                AppServiceResponse response = await App.ConnectionService.Connection.SendMessageAsync(valueSet);

                if (response.Status == AppServiceResponseStatus.Success)
                {
                    ValueSet test = response.Message;

                    if (response.Message["status"] as string == "succeeded")
                    {
                        ObservableCollection<RegistryKeyNode> items = new ObservableCollection<RegistryKeyNode>();
                        foreach (string item in response.Message["response"] as string[])
                        {
                            if (string.IsNullOrEmpty(key))
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{item}" });
                            else
                                items.Add(new RegistryKeyNode() { Name = item, Path = $"{root}\\{key}\\{item}" });
                        }

                        return items;
                    }
                    //else if (response.Message["status"] as string == "failed")
                    //{
                    //}
                    //else
                    //{
                    //}
                    return null;
                }
                //else
                //{
                //}

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{ex.Message}");
                return null;
            }
        }
    }
}
