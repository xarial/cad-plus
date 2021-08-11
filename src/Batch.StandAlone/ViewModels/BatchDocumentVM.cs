//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
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
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.CadPlus.Batch.Base;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.Exceptions;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XToolkit;
using Xarial.XToolkit.Services.UserSettings;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.CadPlus.Plus.Shared.Helpers;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
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

        public ObservableCollection<MacroDataVM> Macros { get; }

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

        private readonly Func<BatchJob, IBatchRunJobExecutor> m_ExecFact;
        private readonly ICadApplicationInstanceProvider m_AppProvider;

        public event Action<BatchDocumentVM, BatchJob, string> Save;

        private readonly IBatchApplicationProxy m_BatchAppProxy;

        private readonly MainWindow m_ParentWnd;

        private readonly IXLogger m_Logger;

        private readonly IJournalExporter[] m_JournalExporters;
        private readonly IResultsSummaryExcelExporter[] m_ResultsExporters;

        public BatchDocumentVM(FileInfo file, BatchJob job, ICadApplicationInstanceProvider[] appProviders,
            IJournalExporter[] journalExporters, IResultsSummaryExcelExporter[] resultsExporters,
            IMessageService msgSvc, IXLogger logger,
            Func<BatchJob, IBatchRunJobExecutor> execFact,
            IBatchApplicationProxy batchAppProxy, MainWindow parentWnd, IRibbonButtonCommand[] backstageCmds)
            : this(Path.GetFileNameWithoutExtension(file.FullName), job, appProviders, 
                  msgSvc, logger, execFact, batchAppProxy, parentWnd, backstageCmds)
        {
            m_JournalExporters = journalExporters;
            m_ResultsExporters = resultsExporters;

            m_FilePath = file.FullName;
            IsDirty = false;
        }

        public BatchDocumentVM(string name, BatchJob job,
            ICadApplicationInstanceProvider[] appProviders,
            IMessageService msgSvc, IXLogger logger, Func<BatchJob, IBatchRunJobExecutor> execFact,
            IBatchApplicationProxy batchAppProxy, MainWindow parentWnd, IRibbonButtonCommand[] backstageCmds)
        {
            m_ExecFact = execFact;
            m_AppProvider = job.FindApplicationProvider(appProviders);
            m_Job = job;
            m_MsgSvc = msgSvc;
            m_Logger = logger;
            m_BatchAppProxy = batchAppProxy;
            m_ParentWnd = parentWnd;

            CommandManager = LoadRibbonCommands(backstageCmds);

            InputFilesFilter = GetFileFilters(m_AppProvider.Descriptor);
            MacroFilesFilter = m_AppProvider.Descriptor.MacroFileFilters
                .Select(f => new FileFilter(f.Name, f.Extensions))
                .Concat(new FileFilter[] { XCadMacroProvider.Filter, FileFilter.AllFiles })
                .ToArray();

            IsDirty = true;

            RunJobCommand = new RelayCommand(RunJob, () => CanRunJob);
            SaveDocumentCommand = new RelayCommand(SaveDocument, () => IsDirty);
            FilterEditEndingCommand = new RelayCommand<DataGridCellEditEndingEventArgs>(FilterEditEnding);

            Name = name;
            Settings = new BatchDocumentSettingsVM(m_Job, m_AppProvider, m_Logger);
            Settings.Modified += OnSettingsModified;
            Results = new JobResultsVM(m_Job, m_ExecFact, m_AppProvider.Descriptor);

            Filters = new ObservableCollection<FilterVM>((m_Job.Filters ?? Enumerable.Empty<string>()).Select(f => new FilterVM(f)));
            Filters.CollectionChanged += OnFiltersCollectionChanged;
            BindFilters();

            Input = new ObservableCollection<string>(m_Job.Input ?? Enumerable.Empty<string>());
            Input.CollectionChanged += OnInputCollectionChanged;

            Macros = new ObservableCollection<MacroDataVM>((m_Job.Macros ?? Enumerable.Empty<MacroData>()).Select(m => new MacroDataVM(m)));
            Macros.CollectionChanged += OnMacrosCollectionChanged;

            foreach (var macro in Macros)
            {
                macro.Modified += OnMacroDataModified;
            }
        }

        protected virtual FileFilter[] GetFileFilters(ICadDescriptor cadEntDesc)
        {
            return new FileFilter[]
            {
                new FileFilter(cadEntDesc.PartFileFilter.Name, cadEntDesc.PartFileFilter.Extensions),
                new FileFilter(cadEntDesc.AssemblyFileFilter.Name, cadEntDesc.AssemblyFileFilter.Extensions),
                new FileFilter(cadEntDesc.DrawingFileFilter.Name, cadEntDesc.DrawingFileFilter.Extensions),
                new FileFilter($"{cadEntDesc.ApplicationName} Files", cadEntDesc.PartFileFilter.Extensions
                                                                      .Union(cadEntDesc.AssemblyFileFilter.Extensions)
                                                                      .Union(cadEntDesc.DrawingFileFilter.Extensions).ToArray()),
                FileFilter.AllFiles
            };
        }

        protected virtual RibbonCommandManager LoadRibbonCommands(IRibbonButtonCommand[] backstageCmds)
        {
            var cmdMgr = new RibbonCommandManager(backstageCmds,
                new RibbonTab(BatchApplicationCommandManager.InputTab.Name, "Input",
                    new RibbonGroup(BatchApplicationCommandManager.InputTab.FilesGroupName, "Files",
                        new RibbonButtonCommand("Add Files...", Resources.add_file, "Browse files to be added to the current scope",
                            () => m_ParentWnd.lstInputs.AddFilesCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.AddFilesCommand.CanExecute(null)),
                        new RibbonButtonCommand("Add Folders...", Resources.add_folder, "Browse folders to be added to the current scope",
                            () => m_ParentWnd.lstInputs.AddFoldersCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.AddFoldersCommand.CanExecute(null)),
                        new RibbonButtonCommand("Add From File...", Resources.add_from_file, "Add files and folders from the text file",
                            AddFromFile, null),
                        new RibbonButtonCommand("Remove Files And Folders", Resources.remove_file_folder, "Remove selected files and folders from the scope",
                            () => m_ParentWnd.lstInputs.DeleteSelectedCommand.Execute(null),
                            () => m_ParentWnd.lstInputs.DeleteSelectedCommand.CanExecute(null))),
                new RibbonGroup(BatchApplicationCommandManager.InputTab.FolderFiltersGroupName, "Folder Filters",
                        new RibbonCustomCommand("Folder Filters", Resources.filter, "List of filters for the files in the folders in the scope",
                        this, (System.Windows.DataTemplate)m_ParentWnd.FindResource("folderFilterGridTemplate"))),
                new RibbonGroup(BatchApplicationCommandManager.InputTab.MacrosGroupName, "Macros",
                        new RibbonButtonCommand("Add Macros...", Resources.add_macro, "Browse macros to add to the scope",
                            () => m_ParentWnd.lstMacros.AddFilesCommand.Execute(null),
                            () => m_ParentWnd.lstMacros.AddFilesCommand.CanExecute(null)),
                        new RibbonButtonCommand("Remove Macros", Resources.remove_macro, "Remove selected macros from the scope",
                            () => m_ParentWnd.lstMacros.DeleteSelectedCommand.Execute(null),
                            () => m_ParentWnd.lstMacros.DeleteSelectedCommand.CanExecute(null)))),
                new RibbonTab(BatchApplicationCommandManager.SettingsTab.Name, "Settings",
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.StartupOptionsGroupName, "Startup Options",
                        new RibbonDropDownButton("Version", m_AppProvider.Descriptor.ApplicationIcon, $"Version of {m_AppProvider.Descriptor.ApplicationName} to use in batch job",
                            () => new VersionVM(Settings.Version),
                            v => Settings.Version = ((VersionVM)v).Version,
                            () => m_AppProvider.GetInstalledVersions().Select(v => new VersionVM(v))),
                        new RibbonToggleCommand("Safe Mode", Resources.safe_mode, $"Runs {m_AppProvider.Descriptor.ApplicationName} in lightweight mode with default settings and without add-ins",
                            () => Settings.StartupOptionSafe,
                            v => Settings.StartupOptionSafe = v),
                        new RibbonToggleCommand("Background", Resources.background_mode, $"Runs {m_AppProvider.Descriptor.ApplicationName} in the background mode which improves the performance",
                            () => Settings.StartupOptionBackground,
                            v => Settings.StartupOptionBackground = v),
                        new RibbonToggleCommand("Silent", Resources.no_popup_mode, "Suppresses all popups while running the batch operation",
                            () => Settings.StartupOptionSilent,
                            v => Settings.StartupOptionSilent = v),
                        new RibbonToggleCommand("Hidden", Resources.hidden_application, $"Runs {m_AppProvider.Descriptor.ApplicationName} in invisible mode",
                            () => Settings.StartupOptionHidden,
                            v => Settings.StartupOptionHidden = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.FileOpenOptionsGroupName, "File Open Options",
                        new RibbonToggleCommand("Silent", Resources.silent_mode, "Suppresses popups while opening the file",
                            () => Settings.OpenFileOptionSilent,
                            v => Settings.OpenFileOptionSilent = v),
                        new RibbonToggleCommand("Rapid", Resources.rapid_mode, "Opens files in rapid mode. This greately improves the performance, but some of the functionality may be unsupported",
                            () => Settings.OpenFileOptionRapid,
                            v => Settings.OpenFileOptionRapid = v),
                        new RibbonToggleCommand("Invisible", Resources.invisible_mode, "Opens files in invisible mode",
                            () => Settings.OpenFileOptionInvisible,
                            v => Settings.OpenFileOptionInvisible = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ProtectionGroupName, "Protection",
                        new RibbonToggleCommand("Read Only", Resources.read_only_mode, "Opens files without write access",
                            () => Settings.OpenFileOptionReadOnly,
                            v => Settings.OpenFileOptionReadOnly = v),
                        new RibbonToggleCommand("Forbid Files Upgrade", Resources.forbid_upgrade, $"Disables the file saving if the version of the file is older that the version of {m_AppProvider.Descriptor.ApplicationName}",
                            () => Settings.ForbidUpgrade,
                            v => Settings.ForbidUpgrade = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ActionsGroupName, "Actions",
                        new RibbonToggleCommand("Automatically Save Documents", Resources.auto_save_docs, "Automatically saves file after processing",
                            () => Settings.AutoSaveDocuments,
                            v => Settings.AutoSaveDocuments = v)),
                    new RibbonGroup(BatchApplicationCommandManager.SettingsTab.ResilienceGroupName, "Resilience",
                        new RibbonNumericSwitchCommand("Timeout (seconds)", null, $"Restarts the {m_AppProvider.Descriptor.ApplicationName} if the file failed to process within the specified period of time", "Timeout On", "Timeout Off",
                            () => Settings.IsTimeoutEnabled,
                            v => Settings.IsTimeoutEnabled = v,
                            () => Convert.ToDouble(Settings.Timeout),
                            t => Settings.Timeout = Convert.ToInt32(t), new RibbonNumericSwitchCommandOptions(0, 3600, false, "0")),
                        new RibbonSwitchCommand("Continue On Error", null, "Continues the batch job if the error is encountered", "Continue on error", "Stop on error",
                            () => Settings.ContinueOnError,
                            v => Settings.ContinueOnError = v),
                        new RibbonNumericSwitchCommand("Batch Size", null, $"Limits the number of files which are processed in a single operation. {m_AppProvider.Descriptor.ApplicationName} will be restarted to release the resources", "Limit Batch Size", "Unlimited Batch Size",
                            () => Settings.IsBatchSizeLimited,
                            v => Settings.IsBatchSizeLimited = v,
                            () => Convert.ToDouble(Settings.BatchSize),
                            t => Settings.BatchSize = Convert.ToInt32(t == 0 ? 1 : t), new RibbonNumericSwitchCommandOptions(0, 1000, true, "0")))),//TODO: some issues when limit is set from 1 (binding does not work) - implemented workaround
                new RibbonTab(BatchApplicationCommandManager.JobTab.Name, "Job",
                    new RibbonGroup(BatchApplicationCommandManager.JobTab.ExecutionGroupName, "Execution",
                        new RibbonButtonCommand("Run Job", Resources.run_job, "Start the current job", RunJob, () => CanRunJob),
                        new RibbonButtonCommand("Cancel Job", Resources.cancel_job, "Cancel currently running job",
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

                        })),
                    new RibbonGroup(BatchApplicationCommandManager.JobTab.ResultsGroupName, "Results",
                        new RibbonButtonCommand("Export Summary Results", Resources.export_summary, "Export results statuses",
                        () =>
                        {
                            if (Results?.Selected != null)
                            {
                                TryExportResults();
                            }
                        },
                        () => Results?.Selected != null),
                        new RibbonButtonCommand("Export Journal", Resources.export_log, "Export journal to a text file",
                        () =>
                        {
                            if (Results?.Selected != null)
                            {
                                TryExportJournal();
                            }
                        },
                        () => Results?.Selected != null))
                ));
            
            m_BatchAppProxy.CreateCommandManager(cmdMgr);

            return cmdMgr;
        }

        public Func<string, object> PathToMacroDataConverter { get; }
            = new Func<string, object>(p => new MacroDataVM(new MacroData() { FilePath = p }));

        public Func<object, string> MacroDataToPathConverter { get; }
        = new Func<object, string>((m) => ((MacroDataVM)m).FilePath);

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

        private void TryExportResults()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export journal for job '{Results.Selected.Name}'",
                    FileSystemBrowser.BuildFilterString(
                        m_ResultsExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
                {
                    var exp = m_ResultsExporters.FirstOrDefault(j => FileHelper.MatchesFilter(filePath, j.Filter.Extensions));

                    if (exp != null)
                    {
                        exp.Export(Results.Selected.Name, Results.Selected.Summary, filePath);
                    }
                    else
                    {
                        throw new UserException("Unrecognized file format");
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
        }

        private void TryExportJournal()
        {
            try
            {
                if (FileSystemBrowser.BrowseFileSave(out string filePath,
                    $"Select file to export journal for job '{Results.Selected.Name}'",
                    FileSystemBrowser.BuildFilterString(
                        m_JournalExporters.Select(e => e.Filter).Concat(new FileFilter[] { FileFilter.AllFiles }).ToArray())))
                {
                    var exp = m_JournalExporters.FirstOrDefault(j => FileHelper.MatchesFilter(filePath, j.Filter.Extensions));

                    if (exp != null)
                    {
                        exp.Export(Results.Selected.Journal, filePath);
                    }
                    else
                    {
                        throw new UserException("Unrecognized file format");
                    }
                }
            }
            catch (Exception ex)
            {
                m_Logger.Log(ex);
                m_MsgSvc.ShowError(ex);
            }
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
            catch(Exception ex)
            {
                m_Logger.Log(ex);
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
            if (e.OldItems != null) 
            {
                foreach (MacroDataVM macro in e.OldItems)
                {
                    macro.Modified -= OnMacroDataModified;
                }
            }

            if (e.NewItems != null)
            {
                foreach (MacroDataVM macro in e.NewItems)
                {
                    macro.Modified += OnMacroDataModified;
                }
            }

            m_Job.Macros = Macros.Select(m => m.Data).ToArray();
            IsDirty = true;
        }

        private void OnMacroDataModified(MacroDataVM obj)
        {
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
