namespace FluentReg.Uwp.Models
{
    public class RegistryValueModel
    {
        public string FriendlyName { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public string FriendlyValue { get; set; }
        public object OriginalValue { get; set; }

        public bool ValueIsString { get; set; }
    }
}
