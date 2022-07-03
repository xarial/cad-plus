using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IParentWindowProvider
    {
        Window Window { get; }
        IntPtr Handle { get; }
    }
}
