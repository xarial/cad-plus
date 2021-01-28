using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.StandAlone.Properties;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class RibbonCommandManager
    {
        public ObservableCollection<RibbonCommand> ExecutionGroup { get; }
        public ObservableCollection<RibbonCommand> FileOpenOptionsGroup { get; }

        public RibbonCommandManager() 
        {
            ExecutionGroup = new ObservableCollection<RibbonCommand>();
            FileOpenOptionsGroup = new ObservableCollection<RibbonCommand>();
        }
    }
}
