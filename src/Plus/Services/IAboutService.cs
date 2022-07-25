using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IAboutService
    {
        void ShowAbout(Assembly assm, Image icon);
    }
}
