using FluentReg.Uwp.ViewModels.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace FluentReg.Uwp.Dialogs
{
    public sealed partial class ValueViewerDialog : ContentDialog
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ValueViewerViewModel),
                typeof(ValueViewerDialog),
                new PropertyMetadata(null)
                );

        public ValueViewerViewModel ViewModel
        {
            get => (ValueViewerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public ValueViewerDialog()
            => InitializeComponent();

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
            => Hide();
    }
}
