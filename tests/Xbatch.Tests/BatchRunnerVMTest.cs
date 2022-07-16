//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Batch.Base.Models;
using Xarial.CadPlus.Batch.StandAlone;
using Xarial.CadPlus.Batch.StandAlone.ViewModels;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Applications;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.UI;
using Xarial.CadPlus.Batch.Base;
using Xarial.CadPlus.Batch.Base.Core;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XCad;
using Xarial.XCad.Base;
using Xarial.XCad.SolidWorks;
using Xarial.XCad.SolidWorks.Enums;
using Xarial.XToolkit.Wpf.Utils;
using Xarial.CadPlus.Batch.Base.Services;
using Xarial.XToolkit.Services;
using Xarial.CadPlus.Batch.StandAlone.Services;

namespace Xbatch.Tests
{

    public class BatchDocumentMockVM : BatchDocumentVM
    {
        public BatchDocumentMockVM(string name, BatchJob job, ICadApplicationInstanceProvider appProvider,
            IJournalExporter[] journalExporters, IResultsSummaryExcelExporter[] resultsExporters,
            IMessageService msgSvc, IXLogger logger, IBatchRunJobExecutorFactory execFact,
            IBatchApplicationProxy batchAppProxy, MainWindow parentWnd, IRibbonButtonCommand[] backstageCmds) 
            : base(name, job, appProvider, journalExporters, resultsExporters,
                  msgSvc, logger, execFact, batchAppProxy, parentWnd, backstageCmds)
        {
        }

        protected override RibbonCommandManager LoadRibbonCommands(IRibbonButtonCommand[] backstageCmds)
            => null;

        protected override FileFilter[] GetFileFilters(ICadDescriptor cadEntDesc)
            => new FileFilter[0];
    }

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
                    d.Macros.Add(new MacroDataVM(new MacroData() { FilePath = "C:\\macro1.swp" }));
                    d.Macros.Add(new MacroDataVM( new MacroData() { FilePath = "C:\\macro2.swp" }));
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
                d.Macros.Add(new MacroDataVM(new MacroData() { FilePath = "xyz" }));
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
                d.Macros.Add(new MacroDataVM(new MacroData() { FilePath = "xyz" }));
                d.Settings.Version = SwApplicationFactory.CreateVersion(SwVersion_e.Sw2019);
                d.Settings.IsTimeoutEnabled = false;
            });

            Assert.AreEqual(-1, opts.Timeout);
        }

        private BatchJob WithDocumentMock(Action<BatchDocumentVM> action)
        {
            var mock = new Mock<IBatchRunnerModel>();
            var job = new BatchJob();

            var cadEntDescMock = new Mock<ICadDescriptor>();
            cadEntDescMock.Setup(m => m.MacroFileFilters).Returns(new FileTypeFilter[0]);

            var appProviderMock = new Mock<ICadApplicationInstanceProvider>();
            appProviderMock.Setup(m => m.Descriptor)
                .Returns(cadEntDescMock.Object);
            appProviderMock.Setup(m => m.GetVersionId(It.IsAny<IXVersion>())).Returns("Sw2020");
            appProviderMock.Setup(m => m.ParseVersion(It.IsAny<string>())).Returns(new Mock<IXVersion>().Object);

            var modelMock = mock.Object;
            var msgSvcMock = new Mock<IMessageService>().Object;

            var jobExecFactMock = new Mock<IBatchRunJobExecutorFactory>();
            jobExecFactMock.Setup(m => m.Create(It.IsAny<BatchJob>())).Returns(new Mock<IBatchRunJobExecutor>().Object);

            var docVm = new BatchDocumentMockVM("", job,
                appProviderMock.Object,
                new IJournalExporter[] { new Mock<IJournalExporter>().Object },
                new IResultsSummaryExcelExporter[] { new Mock<IResultsSummaryExcelExporter>().Object },
                msgSvcMock, new Mock<IXLogger>().Object,
                jobExecFactMock.Object,
                new Mock<IBatchApplicationProxy>().Object, null, null);

            action.Invoke(docVm);

            docVm.RunJobCommand.Execute(null);

            return job;
        }
    }
}