using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.CadPlus.Batch.Base.ViewModels
{
    public class MacroDataVM : INotifyPropertyChanged
    {
        public event Action<MacroDataVM> Modified;

        public event PropertyChangedEventHandler PropertyChanged;

        public MacroData Data { get; }

        public string FilePath => Data.FilePath;

        public string Arguments
        {
            get => Data.Arguments;
            set
            {
                Data.Arguments = value;
                this.NotifyChanged();
                Modified?.Invoke(this);
            }
        }

        public MacroDataVM(MacroData data)
        {
            Data = data;
        }
    }
}
