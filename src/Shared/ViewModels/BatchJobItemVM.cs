//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
    public class BatchJobItemVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource Icon => JobItem.Icon;
        public ImageSource Preview => JobItem.Preview;
        public string Title => JobItem.Title;
        public string Description => JobItem.Description;
        public ICommand LinkCommand { get; }

        public IBatchJobItem JobItem { get; }

        public BatchJobItemOperationVM[] Operations { get; }

        public BatchJobItemStateVM State { get; }

        public BatchJobItemVM(IBatchJobItem jobItem) 
        {
            JobItem = jobItem;

            if (jobItem.Link != null)
            {
                LinkCommand = new RelayCommand(jobItem.Link);
            }

            Operations = (JobItem.Operations ?? new IBatchJobItemOperation[0]).Select(o => new BatchJobItemOperationVM(o)).ToArray();

            State = new BatchJobItemStateVM(JobItem.State);
        }
    }
}
