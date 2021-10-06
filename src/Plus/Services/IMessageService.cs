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
using Xarial.XToolkit.Reporting;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IMessageService
    {
        /// <summary>
        /// Type of exceptions which should be considered as user friendly errors
        /// </summary>
        Type[] UserErrors { get; }
        void ShowError(string error);
        void ShowWarning(string warn);
        void ShowInformation(string msg);
        bool? ShowQuestion(string question);
    }

    public static class IMessageServiceExtension
    {
        public static void ShowError(this IMessageService msgSvc, Exception ex, string baseMsg = "Generic error. Please see log for more details")
        {
            var err = ex.ParseUserError(out _, baseMsg, msgSvc.UserErrors ?? new Type[0]);
            msgSvc.ShowError(err);
        }
    }
}
