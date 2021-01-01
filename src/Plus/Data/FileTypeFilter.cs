using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Data
{
    public class FileTypeFilter
    {
        public string Name { get; set; }
        public string[] Extensions { get; set; }

        public FileTypeFilter()
        {
        }

        public FileTypeFilter(string name, params string[] extensions)
        {
            Name = name;
            Extensions = extensions;
        }
    }
}
