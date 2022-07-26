using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IPersistentMessageBoxFactory 
    {
        IPersistentMessageBox Create(IParentWindowProvider windowProvider);
    }

    public interface IPersistentMessageBox : IDisposable
    {
        Image Icon { get; }
        string Caption { get; }
        string Message { get; }
        void ShowAppendBody(string msg);
    }
}
