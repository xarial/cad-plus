//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Module.Init;
using Xarial.CadPlus.Plus;

namespace Xarial.CadPlus.Common
{
    public class ConsoleHostApplication : BaseHostApplication, IHostConsoleApplication
    {
        public override IntPtr ParentWindow => IntPtr.Zero;

        public override event Action Connect;
        public override event Action Disconnect;
        public override event Action Initialized;
        public override event Action<IContainerBuilder> ConfigureServices;
        public override event Action Started;

        public override IModule[] Modules => throw new NotImplementedException();

        public override IServiceProvider Services { get; }

        public override Guid Id { get; }

        internal ConsoleHostApplication(IServiceProvider svcProvider, Guid hostId) 
        {
            Id = hostId;
            Services = svcProvider;
            Started?.Invoke();
        }
    }
}
