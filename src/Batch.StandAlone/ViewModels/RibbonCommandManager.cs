using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.StandAlone.Properties;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class RibbonCommandManager : IRibbonCommandManager
    {
        public ObservableCollection<IRibbonTab> Tabs { get; }

        public RibbonCommandManager()
        {
            Tabs = new ObservableCollection<IRibbonTab>();
        }
    }
}
