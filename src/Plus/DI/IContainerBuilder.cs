using System;
using System.Text;
using System.Threading.Tasks;
using Xarial.XToolkit.Reflection;
using Xarial.CadPlus.Plus.DI;

namespace Xarial.CadPlus.Plus.DI
{
    public interface IContainerBuilder
    {
        event Action<IContainerBuilder, IServiceProvider> ContainerCreated;

        IServiceProvider Build();

        void Register(IRegistration registration);

        void RegisterAdapter(Type fromSvcType, Type toSvcType, Func<object, object> adapter, LifetimeScope_e scope);

        void RegisterInstance(Type svcType, object inst);

        void RegisterDecorator(Type svcType, Type decorType, LifetimeScope_e scope);
    }

    public static class ContainerBuilderExtension
    {
        public static IRegistration<TService, TImplementation> Register<TService, TImplementation>(this IContainerBuilder contBuilder)
            where TService : class
            where TImplementation : class, TService
        {
            var reg = new Registration<TService, TImplementation>();
            contBuilder.Register(reg);
            return reg;
        }

        public static void RegisterAdapter<TFromService, TToService>(this IContainerBuilder contBuilder, Func<TFromService, TToService> adapter, LifetimeScope_e scope)
            where TFromService : class
            where TToService : class
            => contBuilder.RegisterAdapter(typeof(TFromService), typeof(TToService), x => adapter.Invoke((TFromService)x), scope);

        public static void RegisterInstance<TService>(this IContainerBuilder contBuilder, TService inst)
            where TService : class
            => contBuilder.RegisterInstance(typeof(TService), inst);

        public static void RegisterDecorator<TService, TDecorator>(this IContainerBuilder contBuilder, LifetimeScope_e scope)
            where TService : class
            where TDecorator : class, TService
            => contBuilder.RegisterDecorator(typeof(TService), typeof(TDecorator), scope);

        public static IRegistration<TService, TImplementation> RegisterSingleton<TService, TImplementation>(this IContainerBuilder contBuilder)
            where TService : class
            where TImplementation : class, TService
            => contBuilder.Register<TService, TImplementation>().UsingLifetime(LifetimeScope_e.Singleton);

        public static IRegistration<TService, TImplementation> RegisterTransient<TService, TImplementation>(this IContainerBuilder contBuilder)
            where TService : class
            where TImplementation : class, TService
            => contBuilder.Register<TService, TImplementation>().UsingLifetime(LifetimeScope_e.Transient);
    }
}
