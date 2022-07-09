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
            DataContext = ViewModel = new ViewModels.MainViewModel();

            InitializeComponent();
            Loaded += OnMainPageLoaded;
            Current = this;
        }

        public ViewModels.MainViewModel ViewModel { get; private set; }

        private async void OnMainPageLoaded(object sender, RoutedEventArgs e)
            => await ViewModel.OnMainPageLoaded(sender, e);

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }


        private async void OnDirTreeViewExpanding(muxc.TreeView sender, muxc.TreeViewExpandingEventArgs args)
        {
            if (args.Node.HasUnrealizedChildren)
            {
                LoadingProgressBar.Visibility = Visibility.Visible;
                LoadingProgressBar.IsIndeterminate = true;

                var item = args.Item as Models.RegistryKeyNode;
                item.Children.Clear();

                var result = await ViewModel.LoadSubKeys(item?.Path);
                if (result != null)
                {
                    foreach (var res in result) item.Children.Add(res);
                    args.Node.HasUnrealizedChildren = false;
                }

                LoadingProgressBar.Visibility = Visibility.Collapsed;
                LoadingProgressBar.IsIndeterminate = false;
            }
        }

        private async void OnDirTreeViewItemInvoked(muxc.TreeView sender, muxc.TreeViewItemInvokedEventArgs args)
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            LoadingProgressBar.IsIndeterminate = true;

            var item = args.InvokedItem as Models.RegistryKeyNode;
            await ViewModel.LoadValues(item?.Path);

            LoadingProgressBar.Visibility = Visibility.Collapsed;
            LoadingProgressBar.IsIndeterminate = false;
        }

        private async void OnValueListViewDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dialog = new Dialogs.ValueViewerDialog();
            await dialog.ShowAsync();
        }
    }
}
