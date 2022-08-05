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
    public class JobItemOperationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IReadOnlyList<IJobItemIssue> Issues => JobItemOperation.Issues;
        public JobItemState_e State => JobItemOperation.State;
        public object UserResult => JobItemOperation.UserResult;

        public IJobItemOperation JobItemOperation { get; }

        public ICommand ShowErrorCommand { get; }

        public JobItemOperationVM(IJobItemOperation jobItemOperation)
        {
            JobItemOperation = jobItemOperation;

            JobItemOperation.StateChanged += OnStateChanged;
            JobItemOperation.IssuesChanged += OnIssuesChanged;
            JobItemOperation.UserResultChanged += OnUserResultChanged;

            ShowErrorCommand = new RelayCommand<Popup>(ShowError);
        }

        private void ShowError(Popup popup) => popup.IsOpen = true;

        private void OnUserResultChanged(IJobItemOperation sender, object userResult)
            => this.NotifyChanged(nameof(UserResult));

        private void OnIssuesChanged(IJobItemOperation sender, IReadOnlyList<IJobItemIssue> issues)
            => this.NotifyChanged(nameof(Issues));

        private void OnStateChanged(IJobItemOperation sender, JobItemState_e state)
            => this.NotifyChanged(nameof(State));
    }
}
