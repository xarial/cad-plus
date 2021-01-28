using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.UI
{
    public interface IRibbonCommandManager
    {
        ObservableCollection<IRibbonTab> Tabs { get; }
    }
}
