using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base;

namespace Xbatch.Tests
{
    [RequiresThread(System.Threading.ApartmentState.STA)]
    public class UITest
    {
#if _RUN_UI_TESTS_
        [Test]
#endif
        public void DisplayMainWindow()
        {
            var wnd = new MainWindow();
            wnd.ShowDialog();
        }
    }
}
