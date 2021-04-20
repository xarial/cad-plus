//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
    public class JobCancelledException : UserException
    {
        public JobCancelledException() : base("Cancelled by the user") 
        {
        }
    }
}
