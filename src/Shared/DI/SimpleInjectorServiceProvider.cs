//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Plus.Shared.DI
{
    public class SimpleInjectorServiceProvider : IServiceProvider, IDisposable
    {
        public SimpleInjector.Container m_Containter { get; }

        public SimpleInjectorServiceProvider(SimpleInjector.Container container)
        {
            m_Containter = container;
            //NOTE: do not do any validations here as this service will be passed before constructors registered
        }

        public object GetService(Type serviceType)
            => m_Containter.GetInstance(serviceType);

        public object GetService(Type serviceType, string name)
            => throw new NotImplementedException();

        public void Dispose()
        {
            m_Containter.Dispose();
        }
    }
}
