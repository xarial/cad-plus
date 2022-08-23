using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
    public class BatchJobItemStateBaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IBatchJobItemStateBase m_State;

        public BatchJobItemStateBaseVM(IBatchJobItemStateBase state) 
        {
            m_State = state;
            Issues = m_State.Issues?.ToArray();
        }

        public IReadOnlyList<IBatchJobItemIssue> Issues { get; private set; }
        public BatchJobItemStateStatus_e Status => m_State.Status;

        protected void RaiseIssuesChanged(IReadOnlyList<IBatchJobItemIssue> issues)
        {
            //need to create new instance as the DependencyProperty will not react on changes if pinter is the same, ObservableCollection requires cross-thread sync
            Issues = issues?.ToArray();
            this.NotifyChanged(nameof(Issues));
        }

        protected void RaiseStatusChanged(BatchJobItemStateStatus_e status)
            => this.NotifyChanged(nameof(Status));
    }

    public class BatchJobItemStateVM : BatchJobItemStateBaseVM
    {
        public BatchJobItemStateVM(IBatchJobItemState state) : base(state)
        {
            state.StatusChanged += OnStatusChanged;
            state.IssuesChanged += OnIssuesChanged;
        }

        private void OnIssuesChanged(IBatchJobItemState sender, IBatchJobItem item, IReadOnlyList<IBatchJobItemIssue> issues)
            => RaiseIssuesChanged(issues);

        private void OnStatusChanged(IBatchJobItemState sender, IBatchJobItem item, BatchJobItemStateStatus_e status)
            => RaiseStatusChanged(status);
    }

    public class BatchJobItemOperationStateVM : BatchJobItemStateBaseVM
    {
        public BatchJobItemOperationStateVM(IBatchJobItemOperationState state) : base(state)
        {
            state.StatusChanged += OnStatusChanged;
            state.IssuesChanged += OnIssuesChanged;
        }

        private void OnIssuesChanged(IBatchJobItemOperationState sender, IBatchJobItem item, IBatchJobItemOperation operation, IReadOnlyList<IBatchJobItemIssue> issues)
            => RaiseIssuesChanged(issues);

        private void OnStatusChanged(IBatchJobItemOperationState sender, IBatchJobItem item, IBatchJobItemOperation operation, BatchJobItemStateStatus_e status)
            => RaiseStatusChanged(status);
    }
}
