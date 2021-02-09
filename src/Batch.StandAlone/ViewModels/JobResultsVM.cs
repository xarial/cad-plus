//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class JobResultsVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private JobResultVM m_Selected;

        public JobResultVM Selected 
        {
            get => m_Selected;
            set 
            {
                m_Selected = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<JobResultVM> Items { get; }

        private readonly BatchJob m_Job;

        private readonly Func<BatchJob, IBatchRunJobExecutor> m_ExecFact;

        public JobResultsVM(BatchJob job, 
            Func<BatchJob, IBatchRunJobExecutor> execFact) 
        {
            m_Job = job;

            m_ExecFact = execFact;
            Items = new ObservableCollection<JobResultVM>();
        }

        public void StartNewJob()
        {
            var newRes = new JobResultVM($"Job #{Items.Count + 1}", m_ExecFact.Invoke(m_Job));
            Items.Add(newRes);
            Selected = newRes;
            newRes.RunBatchAsync();
        }
    }
}
