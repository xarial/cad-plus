//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.CustomToolbar.Exceptions
{
    public class UserException : Exception
    {
        public UserException(string userMessage) : base(userMessage)
        {
        }

        public UserException(string userMessage, Exception inner) : base(userMessage, inner)
        {
        }
    }
}