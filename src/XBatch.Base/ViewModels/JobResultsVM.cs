using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
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

        private readonly IBatchRunnerModel m_Model;
        private readonly BatchJob m_Job;

        public JobResultsVM(IBatchRunnerModel model, BatchJob job) 
        {
            m_Model = model;
            m_Job = job;

            Items = new ObservableCollection<JobResultVM>();
        }

        public void StartNewJob()
        {
            var newRes = new JobResultVM(DateTime.Now.ToString(), m_Model.CreateExecutor(m_Job));
            Items.Add(newRes);
            Selected = newRes;
            newRes.RunBatchAsync();
        }
    }
}
