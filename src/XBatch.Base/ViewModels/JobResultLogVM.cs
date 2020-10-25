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
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobResultLogVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Output { get; }

        private readonly IBatchRunJobExecutor m_Executor;

        public JobResultLogVM(IBatchRunJobExecutor executor) 
        {
            Output = new ObservableCollection<string>();
            m_Executor = executor;

            m_Executor.Log += OnLog;
        }

        private void OnLog(string line)
        {
            Output.Add(line);
        }
    }
}
