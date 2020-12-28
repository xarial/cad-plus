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

        public SettingsAttribute(string settsFileName) 
        {
            SettingsFileName = settsFileName;
        }
    }
}
