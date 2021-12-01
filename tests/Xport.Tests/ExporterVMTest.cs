//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using Moq;
using NUnit.Framework;
using Xarial.CadPlus.Xport.Core;
using Xarial.CadPlus.Xport.Models;
using Xarial.CadPlus.Xport.ViewModels;
using System.Linq;
using System;
using Xarial.CadPlus.Common.Services;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;

namespace Xport.Tests
{
    public class ExporterVMTest
    {
        [Test]
        public void ExportOptionsTest() 
        {
            var mock = new Mock<IExporterModel>();
            ExportOptions opts = null;
            mock.Setup(m => m.Export(It.IsAny<ExportOptions>())).Callback<ExportOptions>(e => opts = e);

            var vm = new ExporterVM(mock.Object, new Mock<IMessageService>().Object, new Mock<IXLogger>().Object, new Mock<IAboutService>().Object);

            vm.Filter = "*.sld*";
            vm.Format = Format_e.Pdf | Format_e.Html;
            vm.IsTimeoutEnabled = true;
            vm.Timeout = 30;
            vm.OutputDirectory = "D:\\outdir";
            
            vm.ExportCommand.Execute(null);

            Assert.AreEqual("*.sld*", opts.Filter);
            Assert.IsTrue(new string[] { "html", "pdf" }.SequenceEqual(opts.Format));
            Assert.AreEqual(30, opts.Timeout);
            Assert.AreEqual("D:\\outdir", opts.OutputDirectory);
        }

        [Test]
        public void ExportAutoOptionsTest()
        {
            var mock = new Mock<IExporterModel>();
            ExportOptions opts = null;
            mock.Setup(m => m.Export(It.IsAny<ExportOptions>())).Callback<ExportOptions>(e => opts = e);

            var vm = new ExporterVM(mock.Object, new Mock<IMessageService>().Object, new Mock<IXLogger>().Object, new Mock<IAboutService>().Object);

            vm.IsTimeoutEnabled = false;
            vm.Timeout = 30;
            vm.OutputDirectory = "D:\\outdir";
            vm.IsSameDirectoryOutput = true;

            vm.ExportCommand.Execute(null);

            Assert.AreEqual(-1, opts.Timeout);
            Assert.AreEqual("", opts.OutputDirectory);
        }

        [Test]
        public void CompletionMessagesTest() 
        {
            var mock1 = new Mock<IExporterModel>();

            var mock2 = new Mock<IExporterModel>();
            mock2.Setup(m => m.Export(It.IsAny<ExportOptions>())).Callback<ExportOptions>(e => throw new Exception());

            int errShown = 0;
            int succShown = 0;
            
            var msgMock = new Mock<IMessageService>();
            msgMock.Setup(m => m.ShowMessage(It.IsAny<string>(), It.IsAny<MessageServiceIcon_e>(), It.IsAny<MessageServiceButtons_e>()))
                .Callback<string, MessageServiceIcon_e, MessageServiceButtons_e>((m, i, b) => 
                {
                    if (i == MessageServiceIcon_e.Error)
                    {
                        errShown++;
                    }
                    else if (i == MessageServiceIcon_e.Information)
                    {
                        succShown++;
                    }
                });

            new ExporterVM(mock1.Object, msgMock.Object, new Mock<IXLogger>().Object, new Mock<IAboutService>().Object).ExportCommand.Execute(null);
            var res1 = errShown == 0 && succShown == 1;
            errShown = 0;
            succShown = 0;

            new ExporterVM(mock2.Object, msgMock.Object, new Mock<IXLogger>().Object, new Mock<IAboutService>().Object).ExportCommand.Execute(null);
            var res2 = errShown == 1 && succShown == 0;

            Assert.IsTrue(res1, "Success message and no error messages");
            Assert.IsTrue(res2, "Error message and no success messages");
        }
    }
}