using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
    public class JobItemVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public JobItemState_e State 
        {
            get => m_State;
            private set 
            {
                m_State = value;
                this.NotifyChanged();
            }
        }

        public ImageSource Icon => JobItem.Icon;
        public ImageSource Preview => JobItem.Preview;
        public string Title => JobItem.Title;
        public string Description => JobItem.Description;
        public ICommand LinkCommand { get; }

        public IJobItem JobItem { get; }

        public JobItemOperationVM[] Operations { get; }

        private JobItemState_e m_State;

        public JobItemVM(IJobItem jobItem) 
        {
            JobItem = jobItem;

            if (jobItem.Link != null)
            {
                LinkCommand = new RelayCommand(jobItem.Link);
            }

            Operations = (JobItem.Operations ?? new IJobItemOperation[0]).Select(o => new JobItemOperationVM(o)).ToArray();

            ResolveState();

            //TODO: also consider all nested job items

            foreach (var oper in Operations) 
            {
                oper.JobItemOperation.StateChanged += OnOperationStateChanged;
            }
        }

        private void OnOperationStateChanged(IJobItemOperation sender, JobItemState_e state)
        {
            ResolveState();
        }

        private void ResolveState() 
        {
            var states = Operations.Select(o => State).ToArray();

            if (states.Contains(JobItemState_e.Initializing))
            {
                State = JobItemState_e.Initializing;
            }
            else if (states.Contains(JobItemState_e.InProgress))
            {
                State = JobItemState_e.InProgress;
            }
            else if (states.All(s => s == JobItemState_e.Succeeded))
            {
                State = JobItemState_e.Succeeded;
            }
            else if (states.All(s => s == JobItemState_e.Failed))
            {
                State = JobItemState_e.Failed;
            }
            else 
            {
                State = JobItemState_e.Warning;
            }
        }
    }
}
