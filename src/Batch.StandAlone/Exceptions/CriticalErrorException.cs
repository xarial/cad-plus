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
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.XCad.Exceptions;

namespace Xarial.CadPlus.Batch.StandAlone.Exceptions
{
    public class CriticalErrorException : UserException, ICriticalException
    {
        public CriticalErrorException(Exception inner) 
            : base("Critical error has been raised while performing the operation. This has resulted into the instable state or crash of the host application", inner)
        {
        }
    }
}
