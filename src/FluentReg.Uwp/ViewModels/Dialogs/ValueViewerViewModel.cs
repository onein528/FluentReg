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

namespace FluentReg.Uwp.ViewModels.Dialogs
{
    public class ValueViewerViewModel : ObservableObject
    {
        public ValueViewerViewModel()
        {
        }

        private RegistryValueModel _valueModel;
        public RegistryValueModel ValueModel { get => _valueModel; set => SetProperty(ref _valueModel, value); }
    }
}
