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
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.Plus.Shared.Services
{
    public interface IAboutService 
    {
        void ShowAbout(Assembly assm, Image icon);
    }

    public class AboutService : IAboutService
    {
        private readonly IParentWindowProvider m_WndProvider;
        private readonly ILicenseInfoProvider m_LicProvider;

        public AboutService(IParentWindowProvider parentWndProvider, ILicenseInfoProvider licProvider) 
        {
            m_WndProvider = parentWndProvider;
            m_LicProvider = licProvider;
        }

        public void ShowAbout(Assembly assm, Image icon) 
        {
            var licInfo = m_LicProvider.ProvideLicense();

            var vers = assm.GetName().Version;

            var buildTypeId = vers.Revision % 10;

            string buildType;

            switch (buildTypeId) 
            {
                case 0:
                    buildType = "Dev Build";
                    break;

                case 4:
                    buildType = "Preview Build";
                    break;

                case 5:
                    buildType = "Alpha";
                    break;

                case 7:
                    buildType = "Beta";
                    break;

                case 8:
                    buildType = "Pre-Release";
                    break;

                case 9:
                    buildType = "";
                    break;

                default:
                    buildType = "Custom Build";
                    break;
            }

            var edition = licInfo.IsRegistered ? $"{licInfo.Edition.ToString()} Edition" : "NOT REGISTERED";

            if (!string.IsNullOrEmpty(buildType)) 
            {
                edition += $" [{buildType}]";
            }

            var spec = new AboutDialogSpec(assm, icon, Licenses.ThirdParty)
            {
                Edition = new PackageEditionSpec(
                    edition,
                    licInfo.IsRegistered ? licInfo.TrialExpiryDate : null)
            };

            var aboutDlg = new AboutDialog(spec);

            if (m_WndProvider.Window != null)
            {
                aboutDlg.Owner = m_WndProvider.Window;
            }
            else if (!IntPtr.Zero.Equals(m_WndProvider.Handle)) 
            {
                var interopHelper = new WindowInteropHelper(aboutDlg);
                interopHelper.Owner = m_WndProvider.Handle;
            }

            aboutDlg.ShowDialog();
        }
    }
}
