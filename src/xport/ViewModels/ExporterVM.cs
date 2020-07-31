//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Xarial.CadPlus.Xport.Core;
using Xarial.CadPlus.Xport.Models;
using Xarial.CadPlus.Xport.Services;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Wpf;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Xport.ViewModels
{
    public class ExporterVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string m_Log;
        private bool m_IsExportInProgress;
        private int m_ActiveTabIndex;
        private string m_OutputDirectory;
        private double m_Progress;
        private bool m_IsSameDirectoryOutput;
        private bool m_IsTimeoutEnabled;

        private ICommand m_ExportCommand;
        private ICommand m_CancelExportCommand;
        private ICommand m_BrowseOutputDirectoryCommand;
        private ICommand m_AddFileCommand;
        private ICommand m_AddFolderCommand;
        private ICommand m_DeleteInputCommand;

        public Format_e Format { get; set; }

        public string Log
        {
            get => m_Log;
            set
            {
                m_Log = value;
                this.NotifyChanged();
            }
        }

        public string OutputDirectory
        {
            get => m_OutputDirectory;
            set
            {
                m_OutputDirectory = value;
                this.NotifyChanged();
            }
        }

        public bool IsExportInProgress
        {
            get => m_IsExportInProgress;
            set
            {
                m_IsExportInProgress = value;
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

        public double Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

        public ObservableCollection<string> Input { get; }

        public string Filter { get; set; }

        public bool ContinueOnError { get; set; }

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

        public ICommand ExportCommand => m_ExportCommand ?? (m_ExportCommand = new RelayCommand(Export, () => !IsExportInProgress && Input.Any() && Format != 0));
        public ICommand CancelExportCommand => m_CancelExportCommand ?? (m_CancelExportCommand = new RelayCommand(CancelExport, () => IsExportInProgress));
        public ICommand BrowseOutputDirectoryCommand => m_BrowseOutputDirectoryCommand ?? (m_BrowseOutputDirectoryCommand = new RelayCommand(BrowseOutputDirectory));
        public ICommand AddFileCommand => m_AddFileCommand ?? (m_AddFileCommand = new RelayCommand(AddFile));
        public ICommand AddFolderCommand => m_AddFolderCommand ?? (m_AddFolderCommand = new RelayCommand(AddFolder));
        public ICommand DeleteInputCommand => m_DeleteInputCommand ?? (m_DeleteInputCommand = new RelayCommand<IEnumerable>(DeleteInput));

        public int ActiveTabIndex
        {
            get => m_ActiveTabIndex;
            set
            {
                m_ActiveTabIndex = value;
                this.NotifyChanged();
            }
        }

        private readonly IExporterModel m_Model;
        private readonly IMessageService m_MsgSvc;

        public ExporterVM(IExporterModel model, IMessageService msgSvc)
        {
            m_Model = model;
            m_MsgSvc = msgSvc;

            m_Model.ProgressChanged += OnProgressChanged;
            m_Model.Log += OnLog;
            Input = new ObservableCollection<string>();
            Format = Format_e.Html;
            Filter = "*.*";
            IsTimeoutEnabled = true;
            Timeout = 600;
        }

        private void OnProgressChanged(double prg)
        {
            Progress = prg;
        }

        private void OnLog(string line)
        {
            Log += !string.IsNullOrEmpty(Log) ? Environment.NewLine + line : line;
        }

        private async void Export()
        {
            try
            {
                ActiveTabIndex = 1;
                IsExportInProgress = true;
                Progress = 0;
                Log = "";

                var opts = new ExportOptions()
                {
                    Input = Input?.ToArray(),
                    Filter = Filter,
                    Format = ExtractFormats(),
                    OutputDirectory = IsSameDirectoryOutput ? "" : OutputDirectory,
                    ContinueOnError = ContinueOnError,
                    Timeout = IsTimeoutEnabled ? Timeout : -1
                };

                await m_Model.Export(opts).ConfigureAwait(false);

                m_MsgSvc.ShowInformation("Operation completed");
            }
            catch
            {
                m_MsgSvc.ShowError("Processing error");
            }
            finally
            {
                IsExportInProgress = false;
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

        private void CancelExport()
        {
            m_Model.Cancel();
        }

        private void BrowseOutputDirectory()
        {
            if (FileSystemBrowser.BrowseFolder(out string path, "Select output directory"))
            {
                OutputDirectory = path;
            }
        }

        private void AddFolder()
        {
            if (FileSystemBrowser.BrowseFolder(out string path, "Select folder to process"))
            {
                Input.Add(path);
            }
        }

        private void AddFile()
        {
            var filter = FileSystemBrowser.BuildFilterString(
                new FileFilter("SOLIDWORKS Files", "*.sldprt", "*.sldasm", "*.slddrw"),
                FileFilter.AllFiles);

            if (FileSystemBrowser.BrowseFileOpen(out string path, "Select file to process", filter))
            {
                Input.Add(path);
            }
        }

        private void DeleteInput(IEnumerable inputs)
        {
            foreach (var input in inputs.Cast<string>().ToArray())
            {
                Input.Remove(input);
            }
        }
    }
}