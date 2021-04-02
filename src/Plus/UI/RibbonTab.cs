//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
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
    public interface IRibbonTab
    {
        string Name { get; }
        string Title { get; }
        IList<IRibbonGroup> Groups { get; }
    }

    public class RibbonTab : IRibbonTab
    {
        public string Name { get; }
        public string Title { get; }
        public IList<IRibbonGroup> Groups { get; }

        public RibbonTab(string name, string title, params IRibbonGroup[] groups) 
        {
            Name = name;
            Title = title;
            Groups = new List<IRibbonGroup>(groups ?? new IRibbonGroup[0]);
        }
    }

    public static class RibbonTabExtension
    {
        public static bool TryGetGroup(this IRibbonTab tab, string name, out IRibbonGroup group)
        {
            group = tab.Groups?.FirstOrDefault(g => string.Equals(g.Name,
                name,
                StringComparison.CurrentCultureIgnoreCase));

            return group != null;
        }
    }
}
