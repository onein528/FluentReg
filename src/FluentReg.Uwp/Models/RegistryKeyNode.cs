using System.Collections.ObjectModel;

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
