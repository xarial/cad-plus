using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingsAttribute : Attribute
    {
        public string SettingsFileName { get; }

        public string[] AltSettsFileNames { get; }

        public SettingsAttribute(string settsFileName, params string[] altSettsFileNames) 
        {
            SettingsFileName = settsFileName;
            AltSettsFileNames = altSettsFileNames;
        }
    }
}
