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
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.XBatch.Base.Exceptions
{
    public class UserMessageException : Exception, IUserMessageException
    {
        public UserMessageException(string err) : base(err)
        {
        }

        public UserMessageException(string err, Exception inner) : base(err, inner) 
        {
        }
    }
}
