//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;
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
            m_AssmResolver = new AssemblyResolver(AppDomain.CurrentDomain);
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
            builder.Register<AppLogger, IXLogger>();
            builder.Register<ExcelWriter, IExcelWriter>();
            builder.Register<ExcelReader, IExcelReader>();
            builder.Register<ILicenseInfo>(c => new LicenseInfo());
        }

        public void Dispose()
        {
        }
    }
}
