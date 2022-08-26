//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Attributes
{
    [Flags]
    public enum BatchJobHandlerBehavior_e 
    {
        CanOpenLinkWhileRunning = 1,
        CanShowPreviewWhileRunning = 2,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class BatchJobHandlerBehaviorAttribute : Attribute
    {
        public BatchJobHandlerBehavior_e Behavior { get; }

        public BatchJobHandlerBehaviorAttribute(BatchJobHandlerBehavior_e behavior)
        {
            Behavior = behavior;
        }
    }
}
