//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit;
using Xarial.XToolkit.Helpers;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Wpf.Dialogs;

namespace Xarial.CadPlus.Init
{
    internal class LicenseInfo : ILicenseInfo
    {
        public bool IsRegistered => false;
        public EditionType_e Edition => EditionType_e.Community;
        public DateTime? TrialExpiryDate => null;
    }

    internal class LicenseInfoProvider : ILicenseInfoProvider
    {
        public ILicenseInfo ProvideLicense() => new LicenseInfo();
    }

    internal class JobProcessManager : IJobProcessManager
    {
        public void AddProcess(Process process)
        {
        }

        public void Dispose()
        {
        }
    }

    internal class PopupKiller : IPopupKiller
    {
        public event PopupNotClosedDelegate PopupNotClosed;
        public event ShouldClosePopupDelegate ShouldClosePopup;

        public bool IsStarted => m_IsStarted;

        private bool m_IsStarted;

        public void Start(Process prc, TimeSpan period, string popupClassName = "#32770")
        {
            m_IsStarted = true;
        }

        public void Stop()
        {
            m_IsStarted = false;
        }

        public void Dispose()
        {
            m_IsStarted = false;
        }
    }

    internal class PopupKillerFactory : IPopupKillerFactory
    {
        public IPopupKiller Create() => new PopupKiller();
    }

    internal class TaskRunner : ITaskRunner
    {
        public Task Run(Action action, CancellationToken cancellationToken)
            => Task.Run(action, cancellationToken);

        public Task<T> Run<T>(Func<T> func, CancellationToken cancellationToken)
            => Task.Run(func, cancellationToken);

        public void Dispose()
        {
        }
    }

    internal class TaskRunnerFactory : ITaskRunnerFactory
    {
        public ITaskRunner Create() => new TaskRunner();
    }

    internal class ExcelWriter : IExcelWriter
    {
        public void CreateWorkbook(string filePath, ExcelRow[] rows, ExcelWriterOptions options)
            => throw new NotSupportedException();
    }

    internal class ExcelReader : IExcelReader
    {
        public ExcelRow[] ReadWorkbook(string filePath, ExcelReaderOptions options, out ExcelCustomProperty[] customProperties)
            => throw new NotSupportedException();
    }

    public class AboutService : IAboutService
    {
        public void ShowAbout(Assembly assm, Image icon)
            => About.Show(assm, icon, IntPtr.Zero);
    }

    public class EmptyBatchJobReportExporter : IBatchJobReportExporter
    {
        public FileFilter Filter => FileFilter.Create("Text Files", "*.txt");

        public void Export(IBatchJobBase job, string filePath) => throw new NotSupportedException();
    }

    public class EmptyBatchJobLogExporter : IBatchJobLogExporter
    {
        public FileFilter Filter => FileFilter.Create("Text Files", "*.txt");

        public void Export(IBatchJobBase job, string filePath) => throw new NotSupportedException();
    }

    public class Initiator : IInitiator
    {
        private IHost m_Host;

        private readonly AssemblyResolver m_AssmResolver;

        public Initiator() 
        {
            m_AssmResolver = new AssemblyResolver(AppDomain.CurrentDomain, "CAD+ Toolset");
            m_AssmResolver.RegisterAssemblyReferenceResolver(
                new LocalFolderReferencesResolver(Path.GetDirectoryName(typeof(Initiator).Assembly.Location),
                AssemblyMatchFilter_e.Culture | AssemblyMatchFilter_e.PublicKeyToken | AssemblyMatchFilter_e.Version,
                "CAD+"));
        }

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.ConfigureServices += OnConfigureServices;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.RegisterSingleton<IXLogger, AppLogger>();
            builder.RegisterSingleton<IAboutService, AboutService>(RegistrationConflictResolveStrategy_e.KeepOriginal);
            builder.RegisterSingleton<ITaskRunnerFactory, TaskRunnerFactory>();
            builder.RegisterSingleton<ISettingsProvider, SettingsProvider>();
            builder.RegisterSingleton<ILicenseInfoProvider, LicenseInfoProvider>();
            builder.RegisterSingleton<ICadSpecificServiceFactory<IMacroExecutor>, CadSpecificServiceFactory<IMacroExecutor>>();
            builder.RegisterSingleton<ICadSpecificServiceFactory<ICadDescriptor>, CadSpecificServiceFactory<ICadDescriptor>>();
            builder.RegisterSingleton<IJobProcessManager, JobProcessManager>();
            builder.RegisterSingleton<IPopupKillerFactory, PopupKillerFactory>();
            builder.RegisterSingleton<IExcelReader, ExcelReader>();
            builder.RegisterSingleton<IExcelWriter, ExcelWriter>();
            builder.RegisterSingleton<IBatchJobReportExporter, EmptyBatchJobReportExporter>().AsCollectionItem();
            builder.RegisterSingleton<IBatchJobLogExporter, EmptyBatchJobLogExporter>().AsCollectionItem();
        }

        public void Dispose()
        {
        }
    }
}
