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

        private string _path;
        public string Path { get => _path; set => SetProperty(ref _path, value); }

        private string _valueName;
        public string ValueName { get => _valueName; set => SetProperty(ref _valueName, value); }

        private string _valueType;
        public string ValueType { get => _valueType; set => SetProperty(ref _valueType, value); }
    }
}
