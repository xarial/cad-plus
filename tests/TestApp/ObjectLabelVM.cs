using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Properties;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.UI;

namespace TestApp
{
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
            descMock.Setup(x => x.CutListSolidBodyIcon).Returns(Resources.cutlist_icon);
            descMock.Setup(x => x.ApplicationId).Returns("MockApp");
            Descriptor = descMock.Object;

            var confMock = new Mock<IXPartConfiguration>();
            confMock.Setup(x => x.Name).Returns("Default");
            confMock.Setup(x => x.Preview).Returns(() => new MockImage(Resources.preview));
            Configuration = confMock.Object;

            var cutListMock = new Mock<IXCutListItem>();
            cutListMock.Setup(x => x.Name).Returns("CutList-1");
            CutList = cutListMock.Object;

            var confsMock = new Mock<IXPartConfigurationRepository>();
            confsMock.Setup(x => x.Active).Returns(confMock.Object);

            var partMock = new Mock<IXPart>();
            partMock.Setup(x => x.Configurations).Returns(confsMock.Object);
            partMock.Setup(x => x.Path).Returns(@"D:\SubFolder\SubFolder\SubFolder\SubFolder\SubFolder\SubFolder\MockPart.sldprt");

            Document = partMock.Object;
        }
    }
}
