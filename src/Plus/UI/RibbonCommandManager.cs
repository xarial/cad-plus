//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        IList<IRibbonButtonCommand> Backstage { get; }
        IList<IRibbonTab> Tabs { get; }
    }

    public class RibbonCommandManager : IRibbonCommandManager
    {
        public IList<IRibbonTab> Tabs { get; }
        public IList<IRibbonButtonCommand> Backstage { get; }

        public RibbonCommandManager(IRibbonButtonCommand[] backstage, params IRibbonTab[] tabs)
        {
            Tabs = new ObservableCollection<IRibbonTab>(tabs ?? new IRibbonTab[0]);
            Backstage = new List<IRibbonButtonCommand>(backstage ?? new IRibbonButtonCommand[0]);
        }
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
