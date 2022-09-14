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
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Plus.Exceptions;

namespace Xarial.CadPlus.Batch.Base.Exceptions
{
    public class SaveForbiddenException : UserException
    {
        public SaveForbiddenException() : base("Saving is forbidden for the older version of the files")
        {
        }
    }
}
