//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Common.Services;

namespace Xarial.CadPlus.XBatch.Base.Core
{
    public class JobItem : IJobItem
    {
        public event Action<IJobItem, JobItemStatus_e> StatusChanged;

        public string DisplayName { get; protected set; }
        
        public string FilePath { get; }

        public JobItemStatus_e Status 
        {
            get => m_Status;
            set 
            {
                m_Status = value;
                StatusChanged?.Invoke(this, value);
            }
        }

        private JobItemStatus_e m_Status;

        internal JobItem(string filePath) 
        {
            FilePath = filePath;
            m_Status = JobItemStatus_e.AwaitingProcessing;
        }
    }
}
