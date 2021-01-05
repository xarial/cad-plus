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
        void ShowError(string error);
        void ShowInformation(string msg);
        bool? ShowQuestion(string question);
    }

    public static class IMessageServiceExtension
    {
        public static void ShowError(this IMessageService msgSvc, Exception ex, string baseMsg)
        {
            var err = ex.ParseUserError(out _, baseMsg);
            msgSvc.ShowError(err);
        }
    }
}
