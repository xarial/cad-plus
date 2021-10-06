//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
    [AttributeUsage(AttributeTargets.Enum)]
    public class PartialCommandGroupAttribute : Attribute
    {
        public int BaseCommandGroupId { get; }

        public PartialCommandGroupAttribute(int baseCommandGroupId) 
        {
            BaseCommandGroupId = baseCommandGroupId;
        }
    }
}
