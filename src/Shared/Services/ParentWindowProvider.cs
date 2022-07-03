using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public class ParentWindowProvider : IParentWindowProvider
    {
        public Window Window { get; }

        public IntPtr Handle { get; }

        public ParentWindowProvider(Window window) 
        {
            Window = window;
        }

        public ParentWindowProvider(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
