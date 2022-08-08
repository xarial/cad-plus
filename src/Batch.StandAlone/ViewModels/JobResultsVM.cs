//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XCad.Base;
using Xarial.CadPlus.Batch.StandAlone.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class JobResultsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private JobResultBaseVM m_Selected;

        public JobResultBaseVM Selected 
        {
            get => m_Selected;
            set 
            {
                m_Selected = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<JobResultBaseVM> Items { get; }

        private readonly BatchJob m_Job;

        private readonly IBatchMacroRunJobStandAloneFactory m_JobFact;

        private readonly IXLogger m_Logger;

        public JobResultsVM(BatchJob job,
            IBatchMacroRunJobStandAloneFactory jobFact, IXLogger logger) 
        {
            m_Job = job;

            m_Logger = logger;

            m_JobFact = jobFact;
            Items = new ObservableCollection<JobResultBaseVM>();
        }

        public void StartNewJob()
        {
            var newRes = new AsyncJobResultVM($"Job #{Items.Count + 1}", m_JobFact.Create(m_Job), m_Logger, new System.Threading.CancellationTokenSource());
            Items.Add(newRes);
            Selected = newRes;
            newRes.TryRunBatchAsync();
        }
    }
}
