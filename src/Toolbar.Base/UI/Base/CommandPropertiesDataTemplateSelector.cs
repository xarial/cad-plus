//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;
using Xarial.CadPlus.CustomToolbar.UI.ViewModels;

namespace Xarial.CadPlus.CustomToolbar.UI.Base
{
    public class CommandPropertiesDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CommandMacroTemplate { get; set; }
        public DataTemplate CommandGroupTemplate { get; set; }
        public DataTemplate DefaultTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item?.GetType() == typeof(CommandMacroVM))
            {
                return CommandMacroTemplate;
            }
            if (item?.GetType() == typeof(CommandGroupVM))
            {
                return CommandGroupTemplate;
            }
            else
            {
                return DefaultTemplate;
            }
        }
    }
}