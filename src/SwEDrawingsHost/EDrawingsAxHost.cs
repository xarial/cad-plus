//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Windows.Forms;
using Xarial.CadPlus.Xport.SwEDrawingsHost;

namespace Xarial.CadPlus.Xport.EDrawingsHost
{
    public class EDrawingsAxHost : AxHost
    {
        private bool m_IsLoaded;

        public IEDrawingsControl Control { get; private set; }

        public EDrawingsAxHost(EDrawingsVersion_e version = EDrawingsVersion_e.Default) 
            : base(EDrawingsControl.GetOcxGuid(version))
        {
        }
        
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            if (!m_IsLoaded) //this function is called twice
            {
                m_IsLoaded = true;
                var ocx = GetOcx();

                if (ocx != null)
                {
                    Control = new EDrawingsControl(ocx);
                }
                else
                {
                    throw new Exception("Failed to create eDrawings control");
                }
            }
        }
    }
}