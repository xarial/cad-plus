//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;
using Xarial.CadPlus.XToolbar.UI.ViewModels;

namespace Xarial.CadPlus.XToolbar.UI.Base
{
    public class CommandDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NewCommandTemplate { get; set; }
        public DataTemplate CommandTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is NewCommandPlaceholderVM)
            {
                return NewCommandTemplate;
            }
            else
            {
                return CommandTemplate;
            }
        }
    }
}