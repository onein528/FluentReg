using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentReg.Bridge
{
    public class RegistryHandler
    {
        public string[] Parameters { get; set; }

        public RegistryHandler(string[] parameters)
        {
            parameters.CopyTo(Parameters, 0);
        }

        public void QueryRegistry()
        {

        }

        public void ModifyRegistry()
        {

        }

        private void ParseQueryParameters()
        {

        }

        private void ParseModifyParameters()
        {

        }
    }
}
