using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XTools.Xport.Core;
using Xarial.XTools.Xport.Reflection;
using Xarial.XTools.Xport.UI;

namespace Xarial.XTools.Xport.ViewModels
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FormatExtensionAttribute : Attribute 
    {
        public string Extension { get; }

        public FormatExtensionAttribute(string ext) 
        {
            Extension = ext;
        }
    }

    [Flags]
    public enum Format_e 
    {
        [FormatExtension("html")]
        Html = 1 << 0,

        [FormatExtension("pdf")]
        Pdf = 1 << 1,

        [FormatExtension("iges")]
        Iges = 1 << 2
    }

    public class LogWriter : TextWriter
    {
        private readonly ExporterSettingsVM m_Vm;

        internal LogWriter(ExporterSettingsVM vm)
        {
            m_Vm = vm;
        }

        public override void WriteLine(string value)
        {
            m_Vm.Log += Environment.NewLine + value;
        }

        public override Encoding Encoding => Encoding.Default;
    }

    public class ProgressHandler : IProgress<double>
    {
        private readonly ExporterSettingsVM m_Vm;

        internal ProgressHandler(ExporterSettingsVM vm)
        {
            m_Vm = vm;
        }

        public void Report(double value)
        {
            m_Vm.Progress = value;
        }
    }

    public class ExporterSettingsVM : NotifyPropertyChanged
    {
        public Format_e Format { get; set; }

        private string m_Log;
        private bool m_IsExportInProgress;
        private int m_ActiveTabIndex;
        private string m_OutputDirectory;
        private double m_Progress;
        private CancellationTokenSource m_CurrentCancellationToken;

        public string Log
        {
            get => m_Log;
            set
            {
                m_Log = value;
                NotifyChanged();
            }
        }

        public string OutputDirectory
        {
            get => m_OutputDirectory;
            set
            {
                m_OutputDirectory = value;
                NotifyChanged();
            }
        }

        public bool IsExportInProgress
        {
            get => m_IsExportInProgress;
            set
            {
                m_IsExportInProgress = value;
                NotifyChanged();
            }
        }

        public double Progress 
        {
            get => m_Progress;
            set 
            {
                m_Progress = value;
                NotifyChanged();
            }
        }

        public ObservableCollection<string> Input { get; }

        public string Filter { get; set; }

        public bool ContinueOnError { get; set; }

        private ICommand m_ExportCommand;
        private ICommand m_CancelExportCommand;
        private ICommand m_BrowseOutputDirectoryCommand;
        private ICommand m_AddFileCommand;
        private ICommand m_AddFolderCommand;
        private ICommand m_DeleteInputCommand;

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
                NotifyChanged();
            }
        }

        public ExporterSettingsVM() 
        {
            Input = new ObservableCollection<string>();
            Format = Format_e.Html | Format_e.Pdf;
            Filter = "*.*";
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
                    OutputDirectory = OutputDirectory,
                    ContinueOnError = ContinueOnError
                };

                m_CurrentCancellationToken = new CancellationTokenSource();

                using (var exporter = new Exporter(new LogWriter(this), new ProgressHandler(this)))
                {
                    await exporter.Export(opts, m_CurrentCancellationToken.Token).ConfigureAwait(false);
                }
            }
            catch
            {
            }
            finally 
            {
                Progress = 0;
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
            m_CurrentCancellationToken.Cancel();
        }

        private void BrowseOutputDirectory()
        {
            if (FsoBrowser.BrowseForFolder(out string path, "Select output directory"))
            {
                OutputDirectory = path;
            }
        }

        private void AddFolder()
        {
            if (FsoBrowser.BrowseForFolder(out string path, "Select folder to process"))
            {
                Input.Add(path);
            }
        }

        private void AddFile()
        {
            var filter = FsoBrowser.BuildFilterString(
                new FileFilter("SOLIDWORKS Files", "*.sldprt", "*.sldasm", "*.slddrw"),
                FileFilter.AllFiles);

            if (FsoBrowser.BrowseForFileOpen(out string path, "Select file to process", filter)) 
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
