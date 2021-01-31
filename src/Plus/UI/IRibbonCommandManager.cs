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

    public static class RibbonCommandManagerExtension 
    {
        public static bool TryGetTab(this IRibbonCommandManager cmdMgr, string name, out IRibbonTab tab) 
        {
            tab = cmdMgr.Tabs?.FirstOrDefault(t => string.Equals(t.Name, 
                name,
                StringComparison.CurrentCultureIgnoreCase));

            return tab != null;
        }
    }
}
