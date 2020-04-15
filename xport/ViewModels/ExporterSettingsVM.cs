using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xarial.XTools.Xport.UI;

namespace Xarial.XTools.Xport.ViewModels
{
    [Flags]
    public enum Format_e 
    {
        Html = 1 << 0,
        Pdf = 1 << 1,
        Iges = 1 << 2
    }

    public class ExporterSettingsVM : NotifyPropertyChanged
    {
        public Format_e Format { get; set; }

        private string m_Log;

        public string Log 
        {
            get => m_Log;
            set 
            {
                m_Log = value;
                NotifyChanged();
            }
        }

        public ObservableCollection<string> Input { get; }

        public string Filter { get; set; }

        public bool ContinueOnError { get; set; }

        private ICommand m_ExportCommand;
        private ICommand m_CancelExportCommand;

        public ICommand ExportCommand => m_ExportCommand ?? (m_ExportCommand = new RelayCommand(Export));
        public ICommand CancelExportCommand => m_CancelExportCommand ?? (m_CancelExportCommand = new RelayCommand(CancelExport));

        public ExporterSettingsVM() 
        {
            Input = new ObservableCollection<string>();
            Format = Format_e.Html;
        }

        private void Export() 
        {
        }

        private void CancelExport() 
        {
        }
    }
}
