using System;

namespace Xarial.CadPlus.XToolbar.Exceptions
{
    public class UserException : Exception
    {
        public UserException(string userMessage) : base(userMessage)
        {
        }
    }
}