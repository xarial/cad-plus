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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Batch.StandAlone.Properties;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.CadPlus.Common.Exceptions;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Exceptions;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.XCad;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class FilterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string[] m_Src;
        private int m_Index;
        private string m_Value;

        public string Value
        {
            get => m_Value;
            set 
            {
                m_Value = value;
                m_Src[m_Index] = value;
                this.NotifyChanged();
            }
        }

        public FilterVM() : this("*.*")
        {
        }

        public FilterVM(string value) 
        {
            m_Value = value;
        }

        internal void SetBinding(string[] src, int index) 
        {
            m_Src = src;
            m_Index = index;
        }
    }

    public class BatchDocumentVM : INotifyPropertyChanged
    {
        internal static FileFilter[] FileFilters { get; }
            = new FileFilter[] 
            {
                new FileFilter("Batch+ Job File", "*.bpj"),
                FileFilter.AllFiles };

        public event PropertyChangedEventHandler PropertyChanged;

        private string m_Name;

        public string Name
        {
            get => m_Name;
            private set
            {
                m_Name = value;
                this.NotifyChanged();
            }
        }

        public BatchDocumentSettingsVM Settings { get; }
        public JobResultsVM Results { get; }

        public ObservableCollection<string> Input { get; }

        public ObservableCollection<MacroData> Macros { get; }

        public ObservableCollection<FilterVM> Filters { get; }

        public FileFilter[] InputFilesFilter { get; }

        public FileFilter[] MacroFilesFilter { get; }

        public ICommand RunJobCommand { get; }

        public ICommand SaveDocumentCommand { get; }

        public ICommand FilterEditEndingCommand { get; }

        public bool IsDirty
        {
            get => m_IsDirty;
            private set
            {
                m_IsDirty = value;
                this.NotifyChanged();
            }
        }

        public RibbonCommandManager CommandManager { get; }

        private readonly BatchJob m_Job;
        private readonly IMessageService m_MsgSvc;

        private bool m_IsDirty;
        private string m_FilePath;

        public string FilePath => m_FilePath;

        private readonly Func<BatchJob, IApplicationProvider, IBatchRunJobExecutor> m_ExecFact;
        private readonly IApplicationProvider m_AppProvider;

        public event Action<BatchDocumentVM, BatchJob, string> Save;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        private readonly MainWindow m_ParentWnd;

        public BatchDocumentVM(FileInfo file, BatchJob job, IApplicationProvider appProvider, 
            IMessageService msgSvc,
            Func<BatchJob, IApplicationProvider, IBatchRunJobExecutor> execFact,
            IBatchApplicationProxy batchAppProxy, MainWindow parentWnd, IRibbonButtonCommand[] backstageCmds)
            : this(Path.GetFileNameWithoutExtension(file.FullName), job, appProvider, 
                  msgSvc, execFact, batchAppProxy, parentWnd, backstageCmds)
        {
            m_FilePath = file.FullName;
            IsDirty = false;
        }

        public BatchDocumentVM(string name, BatchJob job,
            IApplicationProvider appProvider,
            IMessageService msgSvc, Func<BatchJob, IApplicationProvider, IBatchRunJobExecutor> execFact,
            IBatchApplicationProxy batchAppProxy, MainWindow parentWnd, IRibbonButtonCommand[] backstageCmds)
        {
            m_ExecFact = execFact;
            m_AppProvider = appProvider;
            m_Job = job;
            m_MsgSvc = msgSvc;
            m_BatchAppProxy = batchAppProxy;
            m_ParentWnd = parentWnd;

            CommandManager = LoadRibbonCommands(backstageCmds);

            InputFilesFilter = appProvider.InputFilesFilter?.Select(f => new FileFilter(f.Name, f.Extensions)).ToArray();
            MacroFilesFilter = appProvider.MacroFileFiltersProvider.GetSupportedMacros()
                .Select(f => new FileFilter(f.Name, f.Extensions)).Union(new FileFilter[] { FileFilter.AllFiles }).ToArray();

            IsDirty = true;

            RunJobCommand = new RelayCommand(RunJob, () => CanRunJob);
            SaveDocumentCommand = new RelayCommand(SaveDocument, () => IsDirty);
            FilterEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(FilterEditEnding);

            Name = name;
            Settings = new BatchDocumentSettingsVM(m_Job, m_AppProvider);
            Settings.Modified += OnSettingsModified;
            Results = new JobResultsVM(m_Job, m_AppProvider, m_ExecFact);

            Filters = new ObservableCollection<FilterVM>((m_Job.Filters ?? Enumerable.Empty<string>()).Select(f => new FilterVM(f)));
            Filters.CollectionChanged += OnFiltersCollectionChanged;
            BindFilters();

            Input = new ObservableCollection<string>(m_Job.Input ?? Enumerable.Empty<string>());
            Input.CollectionChanged += OnInputCollectionChanged;

            Macros = new ObservableCollection<MacroData>(m_Job.Macros ?? Enumerable.Empty<MacroData>());
            Macros.CollectionChanged += OnMacrosCollectionChanged;
        }

        private RibbonCommandManager LoadRibbonCommands(IRibbonButtonCommand[] backstageCmds)
        {
            var cmdMgr = new RibbonCommandManager(backstageCmds,
                new RibbonTab(BatchApplicationCommandManager.InputTab.Name, "Input",
                    new RibbonGroup(BatchApplicationCommandManager.InputTab.FilesGroupName, "Files",
                        new RibbonButtonCommand("Add Files...", Resources.add_file, "",
                            () => m_ParentWnd.lstInputs.AddFilesCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.AddFilesCommand.CanExecute(null)),
                        new RibbonButtonCommand("Add Folders...", Resources.add_folder, "",
                            () => m_ParentWnd.lstInputs.AddFoldersCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.AddFoldersCommand.CanExecute(null)),
                        new RibbonButtonCommand("Add From File...", Resources.add_from_file, "",
                            AddFromFile, null),
                        new RibbonButtonCommand("Remove Files And Folders", Resources.remove_file_folder, "",
                            () => m_ParentWnd.lstInputs.DeleteSelectedCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.DeleteSelectedCommand.CanExecute(null))),
                new RibbonGroup(BatchApplicationCommandManager.InputTab.FolderFiltersGroupName, "Folder Filters",
                        new RibbonCustomCommand("Folder Filters", Resources.filter, "",
                        this, (System.Windows.DataTemplate)m_ParentWnd.FindResource("folderFilterGridTemplate"))),
                new RibbonGroup(BatchApplicationCommandManager.InputTab.MacrosGroupName, "Macros",
                        new RibbonButtonCommand("Add Macros...", Resources.add_macro, "",
                            () => m_ParentWnd.lstMacros.AddFilesCommand.Execute(null),
                            () => m_ParentWnd.lstMacros.AddFilesCommand.CanExecute(null)),
                        new RibbonButtonCommand("Remove Macros", Resources.remove_macro, "",
                            () => m_ParentWnd.lstMacros.DeleteSelectedCommand.Execute(null),
                            () => m_ParentWnd.lstMacros.DeleteSelectedCommand.CanExecute(null)))),
                new RibbonTab(BatchApplicationCommandManager.SettingsTab.Name, "Settings",
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.StartupOptionsGroupName, "Startup Options",
                        new RibbonDropDownButton("Version", m_AppProvider.ApplicationIcon, "",
                            () => new VersionVM(Settings.Version),
                            v => Settings.Version = ((VersionVM)v).Version,
                            m_AppProvider.GetInstalledVersions().Select(v => new VersionVM(v))),
                        new RibbonToggleCommand("Safe Mode", Resources.safe_mode, "",
                            () => Settings.StartupOptionSafe,
                            v => Settings.StartupOptionSafe = v),
                        new RibbonToggleCommand("Background", Resources.background_mode, "",
                            () => Settings.StartupOptionBackground,
                            v => Settings.StartupOptionBackground = v),
                        new RibbonToggleCommand("Silent", Resources.no_popup_mode, "",
                            () => Settings.StartupOptionSilent,
                            v => Settings.StartupOptionSilent = v),
                        new RibbonToggleCommand("Hidden", Resources.hidden_application, "",
                            () => Settings.StartupOptionHidden,
                            v => Settings.StartupOptionHidden = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.FileOpenOptionsGroupName, "File Open Options",
                        new RibbonToggleCommand("Silent", Resources.silent_mode, "",
                            () => Settings.OpenFileOptionSilent,
                            v => Settings.OpenFileOptionSilent = v),
                        new RibbonToggleCommand("Rapid", Resources.rapid_mode, "",
                            () => Settings.OpenFileOptionRapid,
                            v => Settings.OpenFileOptionRapid = v),
                        new RibbonToggleCommand("Invisible", Resources.invisible_mode, "",
                            () => Settings.OpenFileOptionInvisible,
                            v => Settings.OpenFileOptionInvisible = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ProtectionGroupName, "Protection",
                        new RibbonToggleCommand("Read Only", Resources.read_only_mode, "",
                            () => Settings.OpenFileOptionReadOnly,
                            v => Settings.OpenFileOptionReadOnly = v),
                        new RibbonToggleCommand("Forbid Files Upgrade", Resources.forbid_upgrade, "",
                            () => Settings.ForbidUpgrade,
                            v => Settings.ForbidUpgrade = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ActionsGroupName, "Actions",
                        new RibbonToggleCommand("Automatically Save Documents", Resources.auto_save_docs, "",
                            () => Settings.AutoSaveDocuments,
                            v => Settings.AutoSaveDocuments = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ResilienceGroupName, "Resilience",
                        new RibbonNumericSwitchCommand("Timeout", null, "", "Timeout On", "Timeout Off",
                            () => Settings.IsTimeoutEnabled,
                            v => Settings.IsTimeoutEnabled = v,
                            () => Convert.ToDouble(Settings.Timeout),
                            t => Settings.Timeout = Convert.ToInt32(t), new RibbonNumericSwitchCommandOptions(0, 3600, false, "0")),
                        new RibbonSwitchCommand("Continue On Error", null, "", "Continue on error", "Stop on error",
                            () => Settings.ContinueOnError,
                            v => Settings.ContinueOnError = v),
                        new RibbonNumericSwitchCommand("Batch Size", null, "", "Limit Batch Size", "Unlimited Batch Size",
                            () => Settings.IsBatchSizeLimited,
                            v => Settings.IsBatchSizeLimited = v,
                            () => Convert.ToDouble(Settings.BatchSize),
                            t => Settings.BatchSize = Convert.ToInt32(t), new RibbonNumericSwitchCommandOptions(0, 1000, true, "0")))),
                new RibbonTab(BatchApplicationCommandManager.JobTab.Name, "Job",
                    new RibbonGroup(BatchApplicationCommandManager.JobTab.ExecutionGroupName, "Execution",
                        new RibbonButtonCommand("Run Job", Resources.run_job, "", RunJob, () => CanRunJob),
                        new RibbonButtonCommand("Cancel Job", Resources.cancel_job, "",
                        () =>
                        {
                            if (Results?.Selected != null)
                            {
                                Results.Selected.CancelJob();
                            }
                        },
                        () =>
                        {
                            if (Results?.Selected != null)
                            {
                                return Results.Selected.IsBatchInProgress;
                            }
                            else
                            {
                                return false;
                            }

                        }))
                ));
            
            m_BatchAppProxy.CreateCommandManager(cmdMgr);

            return cmdMgr;
        }

        public Func<string, object> PathToMacroDataConverter { get; }
            = new Func<string, object>(p => new MacroData() { FilePath = p });

        public Func<object, string> MacroDataToPathConverter { get; }
        = new Func<object, string>((m) => ((MacroData)m).FilePath);

        private bool CanRunJob => Input.Any() && Macros.Any() && Settings.Version != null;

        private void FilterEditEnding(DataGridCellEditEndingEventArgs args)
        {
            var curFilter = args.EditingElement.DataContext as FilterVM;

            for (int i = Filters.Count - 1; i >= 0; i--)
            {
                if (Filters[i] != curFilter &&
                    string.Equals(Filters[i].Value, curFilter.Value, StringComparison.CurrentCultureIgnoreCase))
                {
                    Filters.RemoveAt(i);
                }
            }
        }

        private void OnSettingsModified()
        {
            IsDirty = true;
        }

        internal void SaveAsDocument()
        {
            if (FileSystemBrowser.BrowseFileSave(out m_FilePath, 
                "Select file path",
                FileSystemBrowser.BuildFilterString(FileFilters)))
            {
                SaveDocument();
                Name = Path.GetFileNameWithoutExtension(m_FilePath);
            }
        }

        public void SaveDocument()
        {
            if (string.IsNullOrEmpty(m_FilePath))
            {
                SaveAsDocument();
                return;
            }

            try
            {
                Save?.Invoke(this, m_Job, m_FilePath);
                IsDirty = false;
            }
            catch
            {
                m_MsgSvc.ShowError("Failed to save document");
            }
        }

        private void OnInputCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Input = Input.ToArray();
            IsDirty = true;
        }

        private void OnMacrosCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            m_Job.Macros = Macros.ToArray();
            IsDirty = true;
        }

        private void OnFiltersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            BindFilters();
            IsDirty = true;
        }

        private void BindFilters() 
        {
            m_Job.Filters = Filters.Select(f => f.Value).ToArray();

            for (int i = 0; i < Filters.Count; i++) 
            {
                Filters[i].SetBinding(m_Job.Filters, i);
            }
        }

        private void AddFromFile()
        {
            if (FileSystemBrowser.BrowseFileOpen(out string path, "Select text file",
                FileSystemBrowser.BuildFilterString(new FileFilter("Text Files", "*.txt", "*.csv"), FileFilter.AllFiles))) 
            {
                if (File.Exists(path))
                {
                    foreach (var input in File.ReadAllLines(path)) 
                    {
                        Input.Add(input);
                    }
                }
                else 
                {
                    m_MsgSvc.ShowError("File does not exist");
                }
            }
        }

        internal void RunJob() 
        {
            if (!CanRunJob) 
            {
                throw new UserException("Cannot run this job as preconditions are not met");
            }

            Results.StartNewJob();
        }
    }
}
