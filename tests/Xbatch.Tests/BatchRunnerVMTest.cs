using Moq;
using NUnit.Framework;
using System.Linq;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.CadPlus.XBatch.Sw;
using Xarial.XCad;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;

namespace Xbatch.Tests
{
    public class BatchRunnerVMTest
    {
        [Test]
        public void BatchRunnerOptionsTest()
        {
            var mock = new Mock<IBatchRunnerModel>();
            BatchJob opts = null;
            mock.Setup(m => m.CreateExecutor(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e).Returns(new Mock<IBatchRunJobExecutor>().Object);
            mock.Setup(m => m.InstalledVersions).Returns(new ISwVersion[] { SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019), SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020) });
            mock.Setup(m => m.GetVersionId(It.IsAny<IXVersion>())).Returns("Sw2020");
            mock.Setup(m => m.ParseVersion(It.IsAny<string>())).Returns(new Mock<IXVersion>().Object);

            var modelMock = mock.Object;
            var msgSvcMock = new Mock<IMessageService>().Object;
            var vm = new BatchManagerVM(modelMock, msgSvcMock);
            vm.Document = new BatchDocumentVM("", new BatchJob(), modelMock, msgSvcMock);

            vm.Document.Input.Add("D:\\folder1");
            vm.Document.Input.Add("D:\\folder2");
            vm.Document.Filters.Clear();
            vm.Document.Filters.Add(new FilterVM("*.sld*"));
            vm.Document.Macros.Add(new MacroData() { FilePath = "C:\\macro1.swp" });
            vm.Document.Macros.Add(new MacroData() { FilePath = "C:\\macro2.swp" });
            vm.Document.Settings.IsTimeoutEnabled = true;
            vm.Document.Settings.Timeout = 30;
            vm.Document.Settings.OpenFileOptionSilent = true;
            vm.Document.Settings.OpenFileOptionReadOnly = true;
            vm.Document.Settings.StartupOptionBackground = true;
            vm.Document.Settings.StartupOptionSilent = false;
            vm.Document.Settings.StartupOptionSafe = false;
            vm.Document.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);

            vm.Document.RunJobCommand.Execute(null);

            Assert.IsTrue(new string[] { "*.sld*" }.SequenceEqual(opts.Filters));
            Assert.IsTrue(new string[] { "C:\\macro1.swp", "C:\\macro2.swp" }.SequenceEqual(opts.Macros.Select(m => m.FilePath)));
            Assert.IsTrue(new string[] { "D:\\folder1", "D:\\folder2" }.SequenceEqual(opts.Input));
            Assert.AreEqual(30, opts.Timeout);
            Assert.AreEqual(OpenFileOptions_e.Silent | OpenFileOptions_e.ReadOnly | OpenFileOptions_e.ForbidUpgrade, opts.OpenFileOptions);
            Assert.AreEqual(StartupOptions_e.Background, opts.StartupOptions);
            Assert.AreEqual("Sw2020", opts.VersionId);
        }

        [Test]
        public void BatchRunnerOptionsTimeoutTest()
        {
            var mock = new Mock<IBatchRunnerModel>();
            BatchJob opts = null;
            mock.Setup(m => m.CreateExecutor(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e).Returns(new Mock<IBatchRunJobExecutor>().Object);
            mock.Setup(m => m.InstalledVersions).Returns(new ISwVersion[] { SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019), SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020) });
            mock.Setup(m => m.GetVersionId(It.IsAny<IXVersion>())).Returns("Sw2020");
            mock.Setup(m => m.ParseVersion(It.IsAny<string>())).Returns(new Mock<IXVersion>().Object);

            var modelMock = mock.Object;
            var msgSvcMock = new Mock<IMessageService>().Object;
            var vm = new BatchManagerVM(modelMock, msgSvcMock);
            vm.Document = new BatchDocumentVM("", new BatchJob(), modelMock, msgSvcMock);

            vm.Document.Input.Add("abc");
            vm.Document.Macros.Add(new MacroData() { FilePath = "xyz" });
            vm.Document.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);

            vm.Document.Settings.Timeout = 300;
            vm.Document.Settings.IsTimeoutEnabled = false;
            vm.Document.Settings.IsTimeoutEnabled = true;

            vm.Document.RunJobCommand.Execute(null);

            Assert.AreEqual(300, opts.Timeout);
        }

        [Test]
        public void BatchRunnerOptionsTimeoutDisableTest()
        {
            var mock = new Mock<IBatchRunnerModel>();
            BatchJob opts = null;
            mock.Setup(m => m.CreateExecutor(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e).Returns(new Mock<IBatchRunJobExecutor>().Object);
            mock.Setup(m => m.InstalledVersions).Returns(new ISwVersion[] { SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019), SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020) });
            mock.Setup(m => m.GetVersionId(It.IsAny<IXVersion>())).Returns("Sw2020");
            mock.Setup(m => m.ParseVersion(It.IsAny<string>())).Returns(new Mock<IXVersion>().Object);

            var modelMock = mock.Object;
            var msgSvcMock = new Mock<IMessageService>().Object;
            var vm = new BatchManagerVM(modelMock, msgSvcMock);
            vm.Document = new BatchDocumentVM("", new BatchJob(), modelMock, msgSvcMock);
            vm.Document.Input.Add("abc");
            vm.Document.Macros.Add(new MacroData() { FilePath = "xyz" });
            vm.Document.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);
            vm.Document.Settings.IsTimeoutEnabled = false;

            vm.Document.RunJobCommand.Execute(null);

            Assert.AreEqual(-1, opts.Timeout);
        }
    }
}