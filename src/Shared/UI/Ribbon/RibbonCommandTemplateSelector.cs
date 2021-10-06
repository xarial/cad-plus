//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xarial.CadPlus.Plus.UI;

namespace Xarial.CadPlus.Plus.Shared.UI
{
    public class RibbonCommandTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Button { get; set; }
        public DataTemplate Toggle { get; set; }
        public DataTemplate Switch { get; set; }
        public DataTemplate NumericSwitch { get; set; }
        public DataTemplate DropDownButton { get; set; }
        public DataTemplate Custom { get; set; }
        public DataTemplate Separator { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IRibbonNumericSwitchCommand) 
            {
                return NumericSwitch;
            }
            if (item is IRibbonSwitchCommand)
            {
                return Switch;
            }
            else if (item is IRibbonButtonCommand)
            {
                return Button;
            }
            else if (item is IRibbonToggleCommand)
            {
                return Toggle;
            }
            else if (item is IRibbonDropDownButton)
            {
                return DropDownButton;
            }
            else if (item is IRibbonCustomCommand)
            {
                return Custom;
            }
            else if (item is null) 
            {
                return Separator;
            }
            else
            {
                return base.SelectTemplate(item, container);
            }            
        }
    }
}
