//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultLogVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Output { get; }

        private readonly IBatchRunJobExecutor m_Executor;

        private object m_Lock;

        public JobResultLogVM(IBatchRunJobExecutor executor) 
        {
            m_Lock = new object();

            Output = new ObservableCollection<string>();
            BindingOperations.EnableCollectionSynchronization(Output, m_Lock);

            m_Executor = executor;

            m_Executor.Log += OnLog;
        }

        private void OnLog(string line)
        {
            Output.Add(line);
        }
    }
}
