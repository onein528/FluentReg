using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentReg.Uwp.Models
{
    public class RegistryKeyNode
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public ObservableCollection<RegistryKeyNode> Children { get; set; }
            = new ObservableCollection<RegistryKeyNode>();
    }
}
