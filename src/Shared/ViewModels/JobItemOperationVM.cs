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

        public object UserResult => JobItemOperation.UserResult;

        public IJobItemOperation JobItemOperation { get; }

        public JobItemStateVM State { get; }

        public JobItemOperationVM(IJobItemOperation jobItemOperation)
        {
            JobItemOperation = jobItemOperation;

            JobItemOperation.UserResultChanged += OnUserResultChanged;

            State = new JobItemStateVM(JobItemOperation.State);
        }

        private void OnUserResultChanged(IJobItemOperation sender, object userResult)
            => this.NotifyChanged(nameof(UserResult));
    }
}
