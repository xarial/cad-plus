using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common
{
    public class ServiceProvider : IServiceProvider, IDisposable
    {
        public IContainer Container { get; }

        public ServiceProvider(IContainer container) 
        {
            Container = container;
        }

        public object GetService(Type serviceType)
            => Container.Resolve(serviceType);

        public void Dispose() => Container.Dispose();
    }
}
