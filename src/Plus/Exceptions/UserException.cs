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
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Plus.Exceptions
{
    /// <summary>
    /// Default user friendly exception
    /// </summary>
    public class UserException : Exception, IUserMessageException, IUserException
    {
        public UserException(string userMessage) : base(userMessage)
        {
        }

        public UserException(string userMessage, Exception inner) : base(userMessage, inner)
        {
        }
    }
}
