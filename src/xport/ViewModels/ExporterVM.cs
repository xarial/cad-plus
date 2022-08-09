//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using WK.Libraries.BetterFolderBrowserNS;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.CadPlus.Xport.Core;
using Xarial.CadPlus.Xport.Properties;
using Xarial.CadPlus.Xport.Services;
using Xarial.XCad.Base;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Services;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Dialogs;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Xport.ViewModels
{
    public class ExporterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int m_ActiveTabIndex;
        private string m_OutputDirectory;
        private bool m_IsSameDirectoryOutput;
        private bool m_IsTimeoutEnabled;

        private ICommand m_ExportCommand;
        private ICommand m_CancelExportCommand;
        private ICommand m_BrowseOutputDirectoryCommand;
        
        public Format_e Format { get; set; }

        public string OutputDirectory
        {
            get => m_OutputDirectory;
            set
            {
                m_OutputDirectory = value;
                this.NotifyChanged();
            }
        }

        public bool IsSameDirectoryOutput
        {
            get => m_IsSameDirectoryOutput;
            set
            {
                m_IsSameDirectoryOutput = value;
                this.NotifyChanged();
            }
        }

        private AsyncJobResultVM m_JobResult;

        public AsyncJobResultVM JobResult 
        {
            get => m_JobResult;
            private set 
            {
                m_JobResult = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<string> Input { get; }

        public string Filter { get; set; }

        public bool ContinueOnError { get; set; }

        public EDrawingAppVersion_e Version { get; set; }

        public int Timeout { get; set; }

        public bool IsTimeoutEnabled
        {
            get => m_IsTimeoutEnabled;
            set
            {
                m_IsTimeoutEnabled = value;
                this.NotifyChanged();
            }
        }

        public ICommand ExportCommand => m_ExportCommand ?? (m_ExportCommand = new RelayCommand(Export, () => JobResult?.IsBatchJobInProgress != true && Input.Any() && Format != 0));
        public ICommand CancelExportCommand => m_CancelExportCommand ?? (m_CancelExportCommand = new RelayCommand(CancelExport, () => JobResult?.IsBatchJobInProgress == true));
        public ICommand BrowseOutputDirectoryCommand => m_BrowseOutputDirectoryCommand ?? (m_BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory));

        public ICommand AboutCommand { get; }
        public ICommand HelpCommand { get; }

        public int ActiveTabIndex
        {
            get => m_ActiveTabIndex;
            set
            {
                m_ActiveTabIndex = value;
                this.NotifyChanged();
            }
        }

        internal MainWindow ParentWindow { get; set; }

        private readonly IJobProcessManager m_JobPrcMgr;

        private readonly IMessageService m_MsgSvc;
        private readonly IXLogger m_Logger;

        private readonly IAboutService m_AboutSvc;

        private readonly IBatchJobReportExporter[] m_ReportExporters;
        private readonly IBatchJobLogExporter[] m_LogExporters;

        public ExporterVM(IMessageService msgSvc, IJobProcessManager jobPrcMgr, IXLogger logger, IAboutService aboutSvc,
            IEnumerable<IBatchJobReportExporter> reportExporters, IEnumerable<IBatchJobLogExporter> logExporters)
        {
            m_MsgSvc = msgSvc;
            m_Logger = logger;

            m_JobPrcMgr = jobPrcMgr;

            m_AboutSvc = aboutSvc;

            m_ReportExporters = reportExporters.ToArray();
            m_LogExporters = logExporters.ToArray();
            
            Input = new ObservableCollection<string>();
            Format = Format_e.Html;
            Filter = "*.*";
            IsTimeoutEnabled = true;
            Timeout = 600;

            AboutCommand = new RelayCommand(ShowAbout);
            HelpCommand = new RelayCommand(OpenHelp);
        }

        private async void Export()
        {
            try
            {
                ActiveTabIndex = 1;

                var opts = new ExportOptions()
                {
                    Input = Input?.ToArray(),
                    Filter = Filter,
                    Format = ExtractFormats(),
                    OutputDirectory = IsSameDirectoryOutput ? "" : OutputDirectory,
                    ContinueOnError = ContinueOnError,
                    Timeout = IsTimeoutEnabled ? Timeout : -1,
                    Version = (int)Version
                };

                using (var exporter = new Exporter(m_JobPrcMgr, opts)) 
                {
                    JobResult = new AsyncJobResultVM(exporter, m_MsgSvc, m_Logger, new CancellationTokenSource(), m_ReportExporters, m_LogExporters);
                    await JobResult.TryRunBatchAsync().ConfigureAwait(false);
                }

                m_MsgSvc.ShowInformation("Operation completed");
            }
            catch(Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError("Processing error");
            }
        }

        private string[] ExtractFormats()
        {
            var res = new List<string>();

            foreach (Enum val in Enum.GetValues(typeof(Format_e)))
            {
                if (Format.HasFlag(val))
                {
                    if (!val.TryGetAttribute<FormatExtensionAttribute>(a => res.Add(a.Extension)))
                    {
                        throw new Exception("Invalid format");
                    }
                }
            }

            return res.ToArray();
        }

        private void CancelExport() => JobResult?.CancelJob();

        private void ShowAbout()
            => m_AboutSvc.ShowAbout(this.GetType().Assembly, Resources.export_plus_icon);

        private void OpenHelp()
        {
            try
            {
                Process.Start(Settings.Default.HelpLink);
            }
            catch
            {
            }
        }

        private void BrowseOutputDirectory()
        {
            var dlg = new BetterFolderBrowser()
            {
                Title = "Select output directory",
                Multiselect = false
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OutputDirectory = dlg.SelectedFolder;
            }
        }
    }
}