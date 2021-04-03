using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public interface IAboutService 
    {
        void ShowAbout(Assembly assm, Image icon);
    }

    public class AboutService
    {
        private readonly Window m_Wnd;

        public AboutService(Window wnd) 
        {
            m_Wnd = wnd;
        }

        public void ShowAbout(Assembly assm, Image icon) 
        {
            var aboutDlg = new AboutDialog(
                new AboutDialogSpec(assm,
                icon, Licenses.ThirdParty));

            aboutDlg.Owner = m_Wnd;
            aboutDlg.ShowDialog();
        }
    }
}
