//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.XBatch.Base;
using Xarial.CadPlus.XBatch.Base.Core;
using Xarial.CadPlus.XBatch.Base.Models;
using Xarial.CadPlus.XBatch.Base.ViewModels;
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
            var opts = WithDocumentMock(
                d =>
                {
                    d.Input.Add("D:\\folder1");
                    d.Input.Add("D:\\folder2");
                    d.Filters.Clear();
                    d.Filters.Add(new FilterVM("*.sld*"));
                    d.Macros.Add(new MacroData() { FilePath = "C:\\macro1.swp" });
                    d.Macros.Add(new MacroData() { FilePath = "C:\\macro2.swp" });
                    d.Settings.IsTimeoutEnabled = true;
                    d.Settings.Timeout = 30;
                    d.Settings.OpenFileOptionSilent = true;
                    d.Settings.OpenFileOptionReadOnly = true;
                    d.Settings.StartupOptionBackground = true;
                    d.Settings.StartupOptionSilent = false;
                    d.Settings.StartupOptionSafe = false;
                    d.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2020);
                });

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
            var opts = WithDocumentMock(
            d =>
            {
                d.Input.Add("abc");
                d.Macros.Add(new MacroData() { FilePath = "xyz" });
                d.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);
                
                d.Settings.Timeout = 300;
                d.Settings.IsTimeoutEnabled = false;
                d.Settings.IsTimeoutEnabled = true;
            });

            Assert.AreEqual(300, opts.Timeout);
        }

        [Test]
        public void BatchRunnerOptionsTimeoutDisableTest()
        {
            var opts = WithDocumentMock(d =>
            {
                d.Input.Add("abc");
                d.Macros.Add(new MacroData() { FilePath = "xyz" });
                d.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);
                d.Settings.IsTimeoutEnabled = false;
            });

            Assert.AreEqual(-1, opts.Timeout);
        }

        private BatchJob WithDocumentMock(Action<BatchDocumentVM> action)
        {
            var mock = new Mock<IBatchRunnerModel>();
            BatchJob opts = null;

            var macroFilterProviderMock = new Mock<IMacroFileFilterProvider>();
            macroFilterProviderMock.Setup(m => m.GetSupportedMacros()).Returns(new FileTypeFilter[0]);

            var appProviderMock = new Mock<IApplicationProvider>();
            appProviderMock.Setup(m => m.MacroFileFiltersProvider)
                .Returns(macroFilterProviderMock.Object);
            appProviderMock.Setup(m => m.GetVersionId(It.IsAny<IXVersion>())).Returns("Sw2020");
            appProviderMock.Setup(m => m.ParseVersion(It.IsAny<string>())).Returns(new Mock<IXVersion>().Object);

            var modelMock = mock.Object;
            var msgSvcMock = new Mock<IMessageService>().Object;
            
            var docVm = new BatchDocumentVM("", new BatchJob(), appProviderMock.Object, msgSvcMock,
                (j, p) =>
                {
                    opts = j;
                    return new Mock<IBatchRunJobExecutor>().Object;
                });

            action.Invoke(docVm);

            docVm.RunJobCommand.Execute(null);

            return opts;
        }
    }
}