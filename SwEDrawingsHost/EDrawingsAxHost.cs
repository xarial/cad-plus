using eDrawings.Interop.EModelViewControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xarial.XTools.Xport.EDrawingsHost
{
    public class EDrawingsAxHost : AxHost
    {
        private bool m_IsLoaded;

        public EModelViewControl Control { get; private set; }

        public EDrawingsAxHost() : base("22945A69-1191-4DCF-9E6F-409BDE94D101")
        {
            m_IsLoaded = false;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!m_IsLoaded) //this function is called twice
            {
                m_IsLoaded = true;
                Control = GetOcx() as EModelViewControl;

                if (Control == null)
                {
                    throw new Exception("Failed to access eDrawings control");
                }

                const int SIMPLE_UI = 0;
                Control.FullUI = SIMPLE_UI;
            }
        }
    }
}
