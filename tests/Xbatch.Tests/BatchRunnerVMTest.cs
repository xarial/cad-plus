using Moq;
using NUnit.Framework;
using System.Linq;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.Base.ViewModels;
using Xarial.CadPlus.XBatch.Sw;
using Xarial.XCad.SolidWorks.Enums;

namespace Xbatch.Tests
{
    public class BatchRunnerVMTest
    {
        //TODO: fix unit tests

        //[Test]
        //public void BatchRunnerOptionsTest()
        //{
        //    var mock = new Mock<IBatchRunnerModel>();
        //    BatchJob opts = null;
        //    mock.Setup(m => m.BatchRun(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e);
        //    mock.Setup(m => m.InstalledVersions).Returns(new AppVersionInfo[] { new SwAppVersionInfo(SwVersion_e.Sw2019), new SwAppVersionInfo(SwVersion_e.Sw2020) });
        //    var vm = new _BatchRunnerVM(mock.Object, new Mock<IMessageService>().Object);

        //    vm.Input.Add("D:\\folder1");
        //    vm.Input.Add("D:\\folder2");
        //    vm.Filter = "*.sld*";
        //    vm.Macros.Add("C:\\macro1.swp");
        //    vm.Macros.Add("C:\\macro2.swp");
        //    vm.IsTimeoutEnabled = true;
        //    vm.Timeout = 30;
        //    vm.OpenFileOptions = OpenFileOptions_e.Silent | OpenFileOptions_e.ReadOnly;
        //    vm.StartupOptions = StartupOptions_e.Background;
        //    vm.Version = new SwAppVersionInfo(SwVersion_e.Sw2020);

        //    vm.RunBatchCommand.Execute(null);

        //    Assert.AreEqual("*.sld*", opts.Filter);
        //    Assert.IsTrue(new string[] { "C:\\macro1.swp", "C:\\macro2.swp" }.SequenceEqual(opts.Macros));
        //    Assert.IsTrue(new string[] { "D:\\folder1", "D:\\folder2" }.SequenceEqual(opts.Input));
        //    Assert.AreEqual(30, opts.Timeout);
        //    Assert.AreEqual(OpenFileOptions_e.Silent | OpenFileOptions_e.ReadOnly, opts.OpenFileOptions);
        //    Assert.AreEqual(StartupOptions_e.Background, opts.StartupOptions);
        //    Assert.AreEqual(new SwAppVersionInfo(SwVersion_e.Sw2020), opts.Version);
        //}

        //[Test]
        //public void BatchRunnerOptionsTimeoutTest()
        //{
        //    var mock = new Mock<IBatchRunnerModel>();
        //    BatchJob opts = null;
        //    mock.Setup(m => m.BatchRun(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e);
        //    mock.Setup(m => m.InstalledVersions).Returns(new AppVersionInfo[] { new SwAppVersionInfo(SwVersion_e.Sw2019), new SwAppVersionInfo(SwVersion_e.Sw2020) });
        //    var vm = new _BatchRunnerVM(mock.Object, new Mock<IMessageService>().Object);

        //    vm.Timeout = 300;
        //    vm.IsTimeoutEnabled = false;
        //    vm.IsTimeoutEnabled = true;

        //    vm.RunBatchCommand.Execute(null);

        //    Assert.AreEqual(300, opts.Timeout);
        //}

        //[Test]
        //public void BatchRunnerOptionsTimeoutDisableTest()
        //{
        //    var mock = new Mock<IBatchRunnerModel>();
        //    BatchJob opts = null;
        //    mock.Setup(m => m.BatchRun(It.IsAny<BatchJob>())).Callback<BatchJob>(e => opts = e);
        //    mock.Setup(m => m.InstalledVersions).Returns(new AppVersionInfo[] { new SwAppVersionInfo(SwVersion_e.Sw2019), new SwAppVersionInfo(SwVersion_e.Sw2020) });
        //    var vm = new _BatchRunnerVM(mock.Object, new Mock<IMessageService>().Object);

        //    vm.IsTimeoutEnabled = false;
            
        //    vm.RunBatchCommand.Execute(null);

        //    Assert.AreEqual(-1, opts.Timeout);
        //}
    }
}