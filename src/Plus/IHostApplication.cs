using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus
{
    public interface IHostApplication
    {
        event Action Loaded;
        IntPtr ParentWindow { get; }
    }
}
