using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Plus.Shared.ViewModels
{
    public class BatchJobItemStateVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly IBatchJobItemState m_State;
        
        public ICommand ShowErrorCommand { get; }

        public BatchJobItemStateVM(IBatchJobItemState state) 
        {
            m_State = state;

            m_State.StatusChanged += OnStatusChanged;
            m_State.IssuesChanged += OnIssuesChanged;

            ShowErrorCommand = new RelayCommand<Popup>(ShowError);
        }

        public IReadOnlyList<IBatchJobItemIssue> Issues => m_State.Issues;
        public BatchJobItemStateStatus_e Status => m_State.Status;

        private void OnIssuesChanged(IBatchJobItemState sender, IReadOnlyList<IBatchJobItemIssue> issues)
            => this.NotifyChanged(nameof(Issues));

        private void OnStatusChanged(IBatchJobItemState sender, BatchJobItemStateStatus_e status)
            => this.NotifyChanged(nameof(Status));

        private void ShowError(Popup popup) => popup.IsOpen = true;
    }
}
