using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestApp.Properties;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.UI;
using Xarial.XToolkit.Wpf.Extensions;

namespace TestApp
{
    public class TestVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool m_IsWorkInProgress;
        private double? m_Progress;
        private string m_ProgressMessage;

        public ObjectLabelVM ObjectLabel { get; }

        public bool IsWorkInProgress 
        {
            get => m_IsWorkInProgress;
            set
            {
                m_IsWorkInProgress = value;
                this.NotifyChanged();
            }
        }

        public string ProgressMessage
        {
            get => m_ProgressMessage;
            set
            {
                m_ProgressMessage = value;
                this.NotifyChanged();
            }
        }

        public double? Progress
        {
            get => m_Progress;
            set
            {
                m_Progress = value;
                this.NotifyChanged();
            }
        }

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
        private bool m_Toggle1;
        private bool m_Toggle2;
        private bool m_Toggle3;

        public TestVM() 
        {
            ObjectLabel = new ObjectLabelVM();

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
                        new RibbonSwitchCommand("Switch1", Resources.icon3, "Some toggle switch", "Toggle On", "Toggle Off", () => m_Switch1, x => m_Switch1 = x))));
        }
    }

    public class MockImage : IXImage
    {
        public byte[] Buffer { get; }

        public MockImage(Image img) 
        {
            Buffer = img.GetBytes();
        }
    }

    public class ObjectLabelVM 
    {
        public IXConfiguration Configuration { get; }
        public IXPart Document { get; }
        public IXCutListItem CutList { get; }

        public ICadDescriptor Descriptor { get; }

        public ObjectLabelVM() 
        {
            var descMock = new Mock<ICadDescriptor>();
            descMock.Setup(x => x.PartIcon).Returns(Resources.document_icon);
            descMock.Setup(x => x.ConfigurationIcon).Returns(Resources.config_icon);
            descMock.Setup(x => x.CutListIcon).Returns(Resources.cutlist_icon);
            descMock.Setup(x => x.ApplicationId).Returns("MockApp");
            Descriptor = descMock.Object;

            var confMock = new Mock<IXConfiguration>();
            confMock.Setup(x => x.Name).Returns("Default");
            confMock.Setup(x => x.Preview).Returns(() => new MockImage(Resources.preview));
            Configuration = confMock.Object;

            var cutListMock = new Mock<IXCutListItem>();
            cutListMock.Setup(x => x.Name).Returns("CutList-1");
            CutList = cutListMock.Object;

            var confsMock = new Mock<IXConfigurationRepository>();
            confsMock.Setup(x => x.Active).Returns(confMock.Object);

            var partMock = new Mock<IXPart>();
            partMock.Setup(x => x.Configurations).Returns(confsMock.Object);
            partMock.Setup(x => x.Path).Returns(@"D:\SubFolder\SubFolder\SubFolder\SubFolder\SubFolder\SubFolder\MockPart.sldprt");

            Document = partMock.Object;
        }
    }
}
