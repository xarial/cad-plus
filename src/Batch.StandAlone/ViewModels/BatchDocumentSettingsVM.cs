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
using System.Windows.Input;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XCad;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class BatchDocumentSettingsVM : INotifyPropertyChanged
    {
        public event Action Modified;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ContinueOnError
        {
            get => m_Job.ContinueOnError;
            set
            {
                m_Job.ContinueOnError = value;
                this.NotifyChanged();
                Modified?.Invoke();
            }
        }

        public int Timeout
        {
            get => m_Job.Timeout;
            set
            {
                m_CachedTimeout = value;
                m_Job.Timeout = value;
                this.NotifyChanged();
                Modified?.Invoke();
            }
        }

        public int BatchSize
        {
            get => m_Job.BatchSize;
            set
            {
                m_CachedBatchSize = value;
                m_Job.BatchSize = value;
                this.NotifyChanged();
                Modified?.Invoke();
            }
        }

        public bool IsTimeoutEnabled
        {
            get => m_Job.Timeout > 0;
            set
            {
                if (!value)
                {
                    m_Job.Timeout = -1;
                }
                else
                {
                    m_Job.Timeout = m_CachedTimeout;
                }

                this.NotifyChanged();
                Modified?.Invoke();
            }
        }

        public bool IsBatchSizeLimited
        {
            get => m_Job.BatchSize > 0;
            set
            {
                if (!value)
                {
                    m_Job.BatchSize = -1;
                }
                else
                {
                    m_Job.BatchSize = m_CachedBatchSize;
                }

                this.NotifyChanged();
                Modified?.Invoke();
            }
        }
        
        public bool StartupOptionSafe 
        {
            get => m_Job.StartupOptions.HasFlag(StartupOptions_e.Safe);
            set 
            {
                if (value)
                {
                    m_Job.StartupOptions |= StartupOptions_e.Safe;
                }
                else 
                {
                    m_Job.StartupOptions -= StartupOptions_e.Safe;
                }

                Modified?.Invoke();
            }
        }

        public bool StartupOptionSilent
        {
            get => m_Job.StartupOptions.HasFlag(StartupOptions_e.Silent);
            set
            {
                if (value)
                {
                    m_Job.StartupOptions |= StartupOptions_e.Silent;
                }
                else
                {
                    m_Job.StartupOptions -= StartupOptions_e.Silent;
                }

                Modified?.Invoke();
            }
        }

        public bool StartupOptionBackground
        {
            get => m_Job.StartupOptions.HasFlag(StartupOptions_e.Background);
            set
            {
                if (value)
                {
                    m_Job.StartupOptions |= StartupOptions_e.Background;
                }
                else
                {
                    m_Job.StartupOptions -= StartupOptions_e.Background;
                }

                Modified?.Invoke();
            }
        }

        public bool StartupOptionHidden
        {
            get => m_Job.StartupOptions.HasFlag(StartupOptions_e.Hidden);
            set
            {
                if (value)
                {
                    m_Job.StartupOptions |= StartupOptions_e.Hidden;
                }
                else
                {
                    m_Job.StartupOptions -= StartupOptions_e.Hidden;
                }

                Modified?.Invoke();
            }
        }

        public bool OpenFileOptionSilent
        {
            get => m_Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Silent);
            set
            {
                if (value)
                {
                    m_Job.OpenFileOptions |= OpenFileOptions_e.Silent;
                }
                else
                {
                    m_Job.OpenFileOptions -= OpenFileOptions_e.Silent;
                }

                Modified?.Invoke();
            }
        }

        public bool OpenFileOptionReadOnly
        {
            get => m_Job.OpenFileOptions.HasFlag(OpenFileOptions_e.ReadOnly);
            set
            {
                if (value)
                {
                    m_Job.OpenFileOptions |= OpenFileOptions_e.ReadOnly;
                }
                else
                {
                    m_Job.OpenFileOptions -= OpenFileOptions_e.ReadOnly;
                }

                Modified?.Invoke();
            }
        }

        public bool OpenFileOptionRapid
        {
            get => m_Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Rapid);
            set
            {
                if (value)
                {
                    m_Job.OpenFileOptions |= OpenFileOptions_e.Rapid;
                }
                else
                {
                    m_Job.OpenFileOptions -= OpenFileOptions_e.Rapid;
                }

                Modified?.Invoke();
            }
        }

        public bool OpenFileOptionInvisible
        {
            get => m_Job.OpenFileOptions.HasFlag(OpenFileOptions_e.Invisible);
            set
            {
                if (value)
                {
                    m_Job.OpenFileOptions |= OpenFileOptions_e.Invisible;
                }
                else
                {
                    m_Job.OpenFileOptions -= OpenFileOptions_e.Invisible;
                }

                Modified?.Invoke();
            }
        }

        public bool ForbidUpgrade 
        {
            get => m_Job.OpenFileOptions.HasFlag(OpenFileOptions_e.ForbidUpgrade);
            set 
            {
                if (value)
                {
                    m_Job.OpenFileOptions |= OpenFileOptions_e.ForbidUpgrade;
                }
                else
                {
                    m_Job.OpenFileOptions -= OpenFileOptions_e.ForbidUpgrade;
                }

                Modified?.Invoke();
            }
        }

        public bool AutoSaveDocuments
        {
            get => m_Job.Actions.HasFlag(Actions_e.AutoSaveDocuments);
            set
            {
                if (value)
                {
                    m_Job.Actions |= Actions_e.AutoSaveDocuments;
                }
                else
                {
                    m_Job.Actions -= Actions_e.AutoSaveDocuments;
                }

                Modified?.Invoke();
            }
        }

        public IXVersion Version
        {
            get => AppProvider.ParseVersion(m_Job.VersionId);
            set
            {
                m_Job.VersionId = AppProvider.GetVersionId(value);
                this.NotifyChanged();
                Modified?.Invoke();
            }
        }

        public IXVersion[] InstalledVersions { get; set; }

        //public ICommand SelectVersionCommand { get; }

        private int m_CachedTimeout;
        private int m_CachedBatchSize;
        private readonly BatchJob m_Job;
        public ICadApplicationInstanceProvider AppProvider { get; }

        public BatchDocumentSettingsVM(BatchJob job, ICadApplicationInstanceProvider appProvider) 
        {
            m_Job = job;
            m_CachedTimeout = m_Job.Timeout;
            m_CachedBatchSize = m_Job.BatchSize;

            AppProvider = appProvider;
            
            InstalledVersions = AppProvider.GetInstalledVersions().ToArray();

            if (!string.IsNullOrEmpty(m_Job.VersionId))
            {
                try
                {
                    Version = AppProvider.ParseVersion(m_Job.VersionId);
                }
                catch
                {
                }
            }
            else
            {
                Version = InstalledVersions.FirstOrDefault();
            }

            //SelectVersionCommand = new RelayCommand<IXVersion>(SelectVersion);
        }

        //private void SelectVersion(IXVersion versInfo) 
        //{
        //    Version = versInfo;
        //}
    }
}
