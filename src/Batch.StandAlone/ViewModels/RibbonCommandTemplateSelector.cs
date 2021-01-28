using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Xarial.CadPlus.Batch.StandAlone.ViewModels
{
    public class RibbonCommandTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Button { get; set; }
        public DataTemplate Toggle { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is RibbonButtonCommand)
            {
                return Button;
            }
            else if (item is ToggleButtonCommand)
            {
                return Toggle;
            }
            else 
            {
                return base.SelectTemplate(item, container);
            }            
        }
    }
}
