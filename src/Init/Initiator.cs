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
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.Init
{
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
        }
    }
}
