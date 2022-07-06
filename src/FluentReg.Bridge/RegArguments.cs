using Microsoft.Win32;
using Vanara.PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentReg.Bridge
{
    public class RegArguments
    {
        public HKEY HKey { get; set; }
        public ulong Switches { get; set; }
        public string Arguments { get; set; }

        public string FullKey { get; set; }
        public string RootKey { get; set; }
        public string SubKey { get; set; }

        public string ValueName { get; set; }
        public object Data { get; set; }

        public string MachineName { get; set; }

        public bool SpecifiedValue { get; set; }
    }
}
