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
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public class JobResultSummaryVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private JobItemDocumentVM[] m_JobItemFiles;
        private int m_ProcessedFiles;
        private int m_FailedFiles;
        private DateTime? m_StartTime;
        private TimeSpan? m_Duration;

        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public bool IsInitializing 
        {
            get => m_IsInitializing;
            set 
            {
                if (value != m_IsInitializing)
                {
                    m_IsInitializing = value;
                    this.NotifyChanged();
                }
            }
        }

        public JobItemDocumentVM[] JobItemFiles 
        {
            get => m_JobItemFiles;
            set 
            {
                m_JobItemFiles = value;
                this.NotifyChanged();
            }
        }

        public int ProcessedFiles
        {
            get => m_ProcessedFiles;
            set 
            {
                m_ProcessedFiles = value;
                this.NotifyChanged();
            }
        }

        public int FailedFiles
        {
            get => m_FailedFiles;
            set
            {
                m_FailedFiles = value;
                this.NotifyChanged();
            }
        }

        public DateTime? StartTime 
        {
            get => m_StartTime;
            set 
            {
                m_StartTime = value;
                this.NotifyChanged();
            }
        }

        public TimeSpan? Duration
        {
            get => m_Duration;
            set
            {
                m_Duration = value;
                this.NotifyChanged();
            }
        }

        private readonly IBatchRunJobExecutor m_Executor;
        private double m_Progress;
        private bool m_IsInitializing;

        public JobResultSummaryVM(IBatchRunJobExecutor executor)
        {
            IsInitializing = true;

            m_Executor = executor;

            m_Executor.JobSet += OnJobSet;
            m_Executor.ProgressChanged += OnProgressChanged;
            m_Executor.JobCompleted += OnJobCompleted;
        }

        private void OnJobSet(IJobItem[] files, DateTime startTime)
        {
            JobItemFiles = files.Select(f => new JobItemDocumentVM((JobItemDocument)f)).ToArray();
            StartTime = startTime;
        }

        private void OnJobCompleted(TimeSpan duration)
        {
            Duration = duration;
        }

        private void OnProgressChanged(IJobItem file, bool result)
        {
            if (IsInitializing)
            {
                IsInitializing = false;
            }

            if (result)
            {
                ProcessedFiles++;
            }
            else 
            {
                FailedFiles++;
            }

            Progress = (ProcessedFiles + FailedFiles) / (double)JobItemFiles.Length;
        }
    }
}
