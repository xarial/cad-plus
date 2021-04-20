//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Xarial.CadPlus.AddIn.Sw.UI
{
    [ComVisible(true)]
    [Guid("170AEC14-BB88-4B09-94FA-9831F91393CE")]
    [ProgId(GeneralControlHost.ProgId)]
    public partial class GeneralControlHost : UserControl
    {
        internal const string ProgId = "Xarial.CadPlus.GeneralControlHost";

        public GeneralControlHost()
        {
            InitializeComponent();
        }
    }
}
