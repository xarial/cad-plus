//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonSwitchCommand : IRibbonToggleCommand 
    {
        string OnText { get; }
        string OffText { get; }
    }

    public class RibbonSwitchCommand : RibbonToggleCommand, IRibbonSwitchCommand
    {
        public string OnText { get; }
        public string OffText { get; }

        public RibbonSwitchCommand(string title, Image icon, string description,
            string onText, string offText,
            Func<bool> getter, Action<bool> setter)
            : base(title, icon, description, getter, setter)
        {
            OnText = onText;
            OffText = offText;
        }

        public override void Update()
        {
            base.Update();

            this.NotifyChanged(nameof(OnText));
            this.NotifyChanged(nameof(OffText));
        }
    }
}
