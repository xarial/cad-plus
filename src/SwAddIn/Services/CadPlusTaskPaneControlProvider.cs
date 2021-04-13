using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xarial.CadPlus.AddIn.Sw.UI;
using Xarial.XCad.SolidWorks.Services;

namespace Xarial.CadPlus.AddIn.Sw.Services
{
    internal class CadPlusTaskPaneControlProvider : ITaskPaneControlProvider
    {
        public object ProvideComControl(ITaskpaneView taskPaneView, string progId)
            => taskPaneView.AddControl(progId, "");

        public bool ProvideNetControl(ITaskpaneView taskPaneView, Control ctrl)
        {
            var genCtrl = (GeneralControlHost)ProvideComControl(taskPaneView, GeneralControlHost.ProgId);

            if (genCtrl != null)
            {
                ctrl.Dock = DockStyle.Fill;
                genCtrl.Controls.Add(ctrl);
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}
