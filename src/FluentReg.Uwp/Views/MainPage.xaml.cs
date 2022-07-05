using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using muxc = Microsoft.UI.Xaml.Controls;

namespace FluentReg.Uwp.Views
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Loaded += OnMainPageLoaded;
            Current = this;

            ViewModel = new ViewModels.MainViewModel();
        }

        public ViewModels.MainViewModel ViewModel { get; private set; }

        private async void OnMainPageLoaded(object sender, RoutedEventArgs e)
            => await ViewModel.OnMainPageLoaded(sender, e);

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }


        private async void OnDirTreeViewExpanding(muxc.TreeView sender, muxc.TreeViewExpandingEventArgs args)
        {
            var item = args.Item as Models.RegistryKeyNode;
            var dividedPath = item?.Path.Split("\\").ToList();
            var rootKey = dividedPath[0];
            dividedPath.RemoveAt(0);
            var baseKey = string.Join('\\', dividedPath);

            var result = await ViewModel.LoadRegistry(rootKey, baseKey);

            item.Children.Clear();
            foreach (var res in result) item.Children.Add(res);

            args.Node.HasUnrealizedChildren = false;
        }

        private void OnDirTreeViewItemInvoked(muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs args)
        {
        }
    }
}
