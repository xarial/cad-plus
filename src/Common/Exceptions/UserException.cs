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

namespace Xarial.CadPlus.Common.Exceptions
{
    public class UserException : Exception, IUserMessageException
    {
        public UserException(string userMessage) : base(userMessage)
        {
        }

        public UserException(string userMessage, Exception inner) : base(userMessage, inner)
        {
        }
    }
}
