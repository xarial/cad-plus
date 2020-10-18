//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Services;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobItemMacroVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => m_JobItemMacro.DisplayName;
        public JobItemStatus_e Status => m_JobItemMacro.Status;

        private readonly IJobItemOperation m_JobItemMacro;

        public JobItemMacroVM(IJobItemOperation jobItemMacro)
        {
            m_JobItemMacro = jobItemMacro;
            m_JobItemMacro.StatusChanged += OnJobItemMacroStatusChanged;
        }

        private void OnJobItemMacroStatusChanged(IJobItem item, JobItemStatus_e newStatus)
        {
            this.NotifyChanged(nameof(Status));
        }
    }
}
