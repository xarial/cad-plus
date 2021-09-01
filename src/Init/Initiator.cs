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
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Base;
using Xarial.XCad.Exceptions;
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

    public class Initiator : IInitiator
    {
        private IHost m_Host;

        public Initiator() 
        {
            AppDomain.CurrentDomain.ResolveBindingRedirects(
                new LocalFolderReferencesResolver(Path.GetDirectoryName(typeof(Initiator).Assembly.Location)));
        }

        public void Init(IHost host)
        {
            m_Host = host;
            m_Host.ConfigureServices += OnConfigureServices;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.Register<AppLogger, IXLogger>();
            builder.Register<ILicenseInfo>(c => new LicenseInfo());
        }
    }
}
