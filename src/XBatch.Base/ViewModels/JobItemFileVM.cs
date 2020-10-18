using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobItemFileVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => m_JobItemFile.DisplayName;
        public JobItemStatus_e Status => m_JobItemFile.Status;

        private readonly IJobItemFile m_JobItemFile;

        public JobItemMacroVM[] Macros { get; }

        public JobItemFileVM(IJobItemFile jobItemFile) 
        {
            m_JobItemFile = jobItemFile;
            m_JobItemFile.StatusChanged += OnJobItemFileStatusChanged;
            Macros = jobItemFile.Operations.Select(o => new JobItemMacroVM(o)).ToArray();
        }

        private void OnJobItemFileStatusChanged(IJobItem item, JobItemStatus_e newStatus)
        {
            this.NotifyChanged(nameof(Status));
        }
    }
}
