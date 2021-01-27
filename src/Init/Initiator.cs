//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xarial.CadPlus.Plus;
using Xarial.XToolkit.Reflection;

namespace Xarial.CadPlus.Init
{
    public class Initiator : IInitiator
    {
        public Initiator() 
        {
            AppDomain.CurrentDomain.ResolveBindingRedirects(
                new LocalFolderReferencesResolver(Path.GetDirectoryName(typeof(Initiator).Assembly.Location)));
        }

        public void Init(IHost host)
        {
        }
    }
}
