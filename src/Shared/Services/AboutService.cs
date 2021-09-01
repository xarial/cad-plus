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
        private readonly ILicenseInfo m_License;

        public AboutService(ILicenseInfo license) : this(IntPtr.Zero, license)
        {
        }

        public AboutService(Window wnd, ILicenseInfo license) 
        {
            m_Wnd = wnd;
            m_License = license;
        }

        public AboutService(IntPtr parent, ILicenseInfo license)
        {
            m_Parent = parent;
            m_License = license;
        }

        public void ShowAbout(Assembly assm, Image icon) 
        {
            var aboutDlg = new AboutDialog(
                new AboutDialogSpec(assm,
                icon, Licenses.ThirdParty)
                {
                    Edition = new PackageEditionSpec(
                        m_License.IsRegistered ? m_License.Edition.ToString() : "NOT REGISTERED",
                        m_License.IsRegistered ? m_License.TrialExpiryDate : null)
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
