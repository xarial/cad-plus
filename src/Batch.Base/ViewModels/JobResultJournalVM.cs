//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using System.Windows.Data;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public class JobResultJournalVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Output { get; }

        private readonly IBatchRunJobExecutorBase m_Executor;

        private object m_Lock;

        public JobResultJournalVM(IBatchRunJobExecutorBase executor) 
        {
            m_Lock = new object();

            Output = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(Output, m_Lock);

            m_Executor = executor;

            m_Executor.Log += OnLog;
        }

        private void OnLog(IBatchJobExecutorBase sender, string line)
        {
            Output.Add(line);
        }
    }
}
