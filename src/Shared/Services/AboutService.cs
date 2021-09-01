//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public interface IAboutService 
    {
        void ShowAbout(Assembly assm, Image icon);
    }

    public class AboutService : IAboutService
    {
        private readonly Window m_Wnd;
        private readonly IntPtr m_Parent;
        private readonly Func<ILicenseInfo> m_GetLicense;

        public AboutService(Func<ILicenseInfo> getLicense) : this(IntPtr.Zero, getLicense)
        {
        }

        public AboutService(Window wnd, Func<ILicenseInfo> getLicense) 
        {
            m_Wnd = wnd;
            m_GetLicense = getLicense;
        }

        public AboutService(IntPtr parent, Func<ILicenseInfo> getLicense)
        {
            m_Parent = parent;
            m_GetLicense = getLicense;
        }

        public void ShowAbout(Assembly assm, Image icon) 
        {
            var licInfo = m_GetLicense.Invoke();

            var aboutDlg = new AboutDialog(
                new AboutDialogSpec(assm,
                icon, Licenses.ThirdParty)
                {
                    Edition = new PackageEditionSpec(
                        licInfo.IsRegistered ? licInfo.Edition.ToString() : "NOT REGISTERED",
                        licInfo.IsRegistered ? licInfo.TrialExpiryDate : null)
                });

            if (m_Wnd != null)
            {
                aboutDlg.Owner = m_Wnd;
            }
            else if (!IntPtr.Zero.Equals(m_Parent)) 
            {
                var interopHelper = new WindowInteropHelper(aboutDlg);
                interopHelper.Owner = m_Parent;
            }

            aboutDlg.ShowDialog();
        }
    }
}
