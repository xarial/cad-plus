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

        public ImageSource Icon => JobItem.Icon;
        public ImageSource Preview => JobItem.Preview;
        public string Title => JobItem.Title;
        public string Description => JobItem.Description;
        public ICommand LinkCommand { get; }

        public IJobItem JobItem { get; }

        public JobItemOperationVM[] Operations { get; }

        public JobItemStateVM State { get; }

        public JobItemVM(IJobItem jobItem) 
        {
            JobItem = jobItem;

            if (jobItem.Link != null)
            {
                LinkCommand = new RelayCommand(jobItem.Link);
            }

            Operations = (JobItem.Operations ?? new IJobItemOperation[0]).Select(o => new JobItemOperationVM(o)).ToArray();

            State = new JobItemStateVM(JobItem.State);
        }
    }
}
