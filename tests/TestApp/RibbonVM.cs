using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestApp.Properties;
using Xarial.CadPlus.Plus.UI;

namespace TestApp
{
    public class RibbonVM
    {
        public IRibbonCommandManager CommandManager { get; }

        public class DropDownItem
        {
            public string Title { get; set; }

            public DropDownItem(string title)
            {
                Title = title;
            }

            public override string ToString()
                => Title;
        }

        private DropDownItem[] m_DropDown1Items = new DropDownItem[]
        {
            new DropDownItem("Item 1"),
            new DropDownItem("Long Item 2"),
            new DropDownItem("Very Long Item 3")
        };

        private DropDownItem m_SelectedDropDown1Item;

        private bool m_Switch1;
        private bool m_NumericSwitch1;
        private bool m_Toggle1;
        private bool m_Toggle2;
        private bool m_Toggle3;
        private double m_NumericValue1;

        public RibbonVM()
        {
            m_Toggle2 = true;

            m_SelectedDropDown1Item = m_DropDown1Items.First();

            CommandManager = new RibbonCommandManager(
                new IRibbonButtonCommand[]
                {
                    new RibbonButtonCommand("Command1", Resources.icon1, "Backstage Command 1 Backstage Command 1 Backstage Command 1 Backstage Command 1 Backstage Command 1 Backstage Command 1 Backstage Command 1", () => MessageBox.Show("Command1 is clicked"), () => true),
                    null,
                    new RibbonButtonCommand("Command2", null, "Backstage Command 2", () => Debug.Print("Command 2 is clicked"), null),
                    new RibbonButtonCommand("Command3", Resources.icon2, "Backstage Command 3", () => { }, null),
                },
                new RibbonTab("Tab1", "Tab1",
                    new RibbonGroup("Group1", "Group1",
                        new RibbonDropDownButton("DropDown1", Resources.icon3,
                        "DropDown1 Tooltip",
                        () => m_SelectedDropDown1Item, x => m_SelectedDropDown1Item = (DropDownItem)x, () => m_DropDown1Items)),
                    new RibbonGroup("Group2", "Group2",
                        new RibbonToggleCommand("Toggle1", Resources.icon1, "Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1 Some Toggle 1", () => m_Toggle1, x => m_Toggle1 = x),
                        null,
                        new RibbonToggleCommand("Wide Toggle2", Resources.icon1, "Wide Toggle 2", () => m_Toggle2, x => m_Toggle2 = x),
                        null,
                        new RibbonToggleCommand("Toggle3", Resources.icon4, "Some Toggle 3", () => m_Toggle3, x => m_Toggle3 = x)),
                    new RibbonGroup("Group3", "Group3",
                        new RibbonButtonCommand("Button1", Resources.icon5, "Button1 Tooltip", () => { }, null),
                        null,
                        new RibbonButtonCommand("Very Wide Button2", Resources.icon1, "Button 2 Tooltip", () => { }, null),
                        new RibbonNumericSwitchCommand("NumericSwitch1", Resources.icon3, "Some numeric toggle switch", "Numeric Toggle On", "Numeric Toggle Off", () => m_NumericSwitch1, x => m_NumericSwitch1 = x, () => m_NumericValue1, x => m_NumericValue1 = x, new RibbonNumericSwitchCommandOptions(10, 100, true, "0")),
                        new RibbonSwitchCommand("Switch1", Resources.icon3, "Some toggle switch", "Toggle On", "Toggle Off", () => m_Switch1, x => m_Switch1 = x))));
        }
    }
}
