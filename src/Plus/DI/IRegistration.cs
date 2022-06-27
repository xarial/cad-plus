using System;
using Xarial.CadPlus.Plus.DI;

namespace Xarial.CadPlus.Plus.DI
{
    public interface IRegistration
    {
        Type ServiceType { get; }
        Type ImplementationType { get; }

        Delegate Factory { get; set; }
        LifetimeScope_e Lifetime { get; set; }
        IParameter[] Parameters { get; set; }
        Delegate Initializer { get; set; }
        bool IsCollectionItem { get; set; }
    }

    public interface IRegistration<TService, TImplementation> : IRegistration
        where TService : class
        where TImplementation : class, TService
    {
    }

    internal class Registration : IRegistration
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }

        public Delegate Factory { get; set; }
        public LifetimeScope_e Lifetime { get; set; }
        public IParameter[] Parameters { get; set; }
        public Delegate Initializer { get; set; }
        public bool IsCollectionItem { get; set; }

        internal Registration(Type svcType, Type impType)
        {
            ServiceType = svcType;
            ImplementationType = impType;
        }
    }

    internal class Registration<TService, TImplementation> : Registration, IRegistration<TService, TImplementation>
        where TService : class
        where TImplementation : class, TService
    {
        internal Registration() : base(typeof(TService), typeof(TImplementation))
        {
        }
    }

    public static class RegistrationExtension
    {
        public static IRegistration<TService, TImplementation> UsingFactory<TService, TImplementation>(this IRegistration<TService, TImplementation> reg, Func<TService> factory)
            where TService : class
            where TImplementation : class, TService
        {
            reg.Factory = factory;
            return reg;
        }

        public static IRegistration<TService, TImplementation> UsingParameters<TService, TImplementation>(this IRegistration<TService, TImplementation> reg, params IParameter[] parameters)
            where TService : class
            where TImplementation : class, TService
        {
            reg.Parameters = parameters;
            return reg;
        }

        public static IRegistration<TService, TImplementation> UsingLifetime<TService, TImplementation>(this IRegistration<TService, TImplementation> reg, LifetimeScope_e scope)
            where TService : class
            where TImplementation : class, TService
        {
            reg.Lifetime = scope;
            return reg;
        }

        public static IRegistration<TService, TImplementation> UsingInitializer<TService, TImplementation>(this IRegistration<TService, TImplementation> reg, Action<TService> init)
            where TService : class
            where TImplementation : class, TService
        {
            reg.Initializer = init;
            return reg;
        }

        public static IRegistration<TService, TImplementation> AsCollectionItem<TService, TImplementation>(this IRegistration<TService, TImplementation> reg)
            where TService : class
            where TImplementation : class, TService
        {
            reg.IsCollectionItem = true;
            return reg;
        }
    }
}
