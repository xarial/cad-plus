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
    public class BatchJobItemOperationVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public object UserResult => JobItemOperation.UserResult;

        public IBatchJobItemOperation JobItemOperation { get; }

        public BatchJobItemStateVM State { get; }

        public BatchJobItemOperationVM(IBatchJobItemOperation jobItemOperation)
        {
            JobItemOperation = jobItemOperation;

            JobItemOperation.UserResultChanged += OnUserResultChanged;

            State = new BatchJobItemStateVM(JobItemOperation.State);
        }

        private void OnUserResultChanged(IBatchJobItemOperation sender, object userResult)
            => this.NotifyChanged(nameof(UserResult));
    }
}
