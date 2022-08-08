//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public abstract class JobItemVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public string Name => m_JobItem.DisplayName;
        public JobItemStatus_e Status => m_JobItem.Status;
        public IReadOnlyList<string> Issues => m_JobItem.Issues?.Select(i => i.Content).ToList();

        private readonly IJobItem m_JobItem;

        public JobItemVM(IJobItem jobItem)
        {
            m_JobItem = jobItem;
            m_JobItem.StatusChanged += OnJobItemFileStatusChanged;
            m_JobItem.IssuesChanged += OnIssuesChanged;
        }

        private void OnIssuesChanged(IJobItem sender, IReadOnlyList<IJobItemIssue> issues)
            => this.NotifyChanged(nameof(Issues));

        private void OnJobItemFileStatusChanged(IJobItem sender, JobItemStatus_e newStatus)
            => this.NotifyChanged(nameof(Status));
    }
}
