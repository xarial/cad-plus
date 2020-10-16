using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.XBatch.MDI;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.XBatch.Base.ViewModels
{
    public class JobDocumentVM : INotifyPropertyChanged, IJobDocument
    {
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

        IJobSettings IJobDocument.Settings => Settings;
        IJobResults IJobDocument.Results => Results;
        
        public JobSettingsVM Settings { get; }
        public JobResultsVM Results { get; }
        
        public JobDocumentVM(string name) 
        {
            Name = name;
            Settings = new JobSettingsVM($"Settings of {name}");
            Results = new JobResultsVM();
            Results.Items.Add(new JobResultVM(name + "1"));
            Results.Items.Add(new JobResultVM(name + "2"));
        }
    }
}
