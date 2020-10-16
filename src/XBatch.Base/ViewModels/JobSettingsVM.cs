using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobSettingsVM : IJobSettings
    {
        public string Message { get; set; }

        public JobSettingsVM(string msg) 
        {
            Message = msg;
        }
    }
}
