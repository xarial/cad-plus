using System.Windows;
using System.Windows.Controls;
using Xarial.CadPlus.XToolbar.UI.ViewModels;

namespace Xarial.CadPlus.XToolbar.UI.Base
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