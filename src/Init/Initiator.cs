//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.DI;
using Xarial.CadPlus.Plus.Services;
using Xarial.CadPlus.Plus.Shared.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;
using Xarial.XToolkit.Helpers;
using Xarial.XToolkit.Reflection;
using Xarial.XToolkit.Reporting;

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

    internal class ExcelWriter : IExcelWriter
    {
        public void CreateWorkbook(string filePath, ExcelRow[] rows, ExcelWriterOptions options)
            => throw new NotImplementedException();
    }

    internal class ExcelReader : IExcelReader
    {
        public ExcelRow[] ReadWorkbook(string filePath, ExcelReaderOptions options, out ExcelCustomProperty[] customProperties)
            => throw new NotImplementedException();
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
            builder.RegisterSingleton<IExcelWriter, ExcelWriter>();
            builder.RegisterSingleton<IExcelReader, ExcelReader>();
            builder.RegisterSingleton<ILicenseInfoProvider, LicenseInfoProvider>();
            builder.RegisterSingleton<ICadSpecificServiceFactory<IMacroExecutor>, CadSpecificServiceFactory<IMacroExecutor>>();
            builder.RegisterSingleton<ICadSpecificServiceFactory<ICadDescriptor>, CadSpecificServiceFactory<ICadDescriptor>>();
            builder.RegisterSingleton<IJobManager, JobManager>().UsingInitializer(x => x.Init());
        }

        public void Dispose()
        {
        }
    }
}
