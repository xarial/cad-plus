//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Reflection;

namespace Xarial.CadPlus.Plus.DI
{
    public interface IParameter
    {
        Type TargetType { get; }
        bool Matches(ParameterInfo paramInfo, int index);
        object ProvideValue(ParameterInfo paramInfo, IServiceProvider svcProvider);
    }

    public class Parameter<TType> : IParameter
    {
        public static Parameter<TType> Named(string paramName, TType value)
            => Named(paramName, () => value);

        public static Parameter<TType> Named(string paramName, Func<TType> valueFunc)
            => Named(paramName, s => valueFunc.Invoke());

        public static Parameter<TType> Named(string paramName, Func<IServiceProvider, TType> valueFunc)
            => new Parameter<TType>((p, i) => p.Name == paramName, (p, s) => valueFunc.Invoke(s));

        public static Parameter<TType> Indexed(int index, TType value)
            => Indexed(index, () => value);

        public static Parameter<TType> Indexed(int index, Func<TType> valueFunc)
            => Indexed(index, s => valueFunc.Invoke());

        public static Parameter<TType> Indexed(int index, Func<IServiceProvider, TType> valueFunc)
            => new Parameter<TType>((p, i) => index == i, (p, s) => valueFunc.Invoke(s));

        public static Parameter<TType> Any(TType value)
            => Any(() => value);

        public static Parameter<TType> Any(Func<TType> valueFunc)
            => Any(s => valueFunc.Invoke());

        public static Parameter<TType> Any(Func<IServiceProvider, TType> valueFunc)
            => new Parameter<TType>((p, i) => true, (p, s) => valueFunc.Invoke(s));

        private readonly Func<ParameterInfo, int, bool> m_Selector;
        private readonly Func<ParameterInfo, IServiceProvider, TType> m_ValueProvider;

        public Type TargetType => typeof(TType);

        public Parameter(Func<ParameterInfo, int, bool> selector, Func<ParameterInfo, IServiceProvider, TType> valueProvider)
        {
            m_Selector = selector;
            m_ValueProvider = valueProvider;
        }

        public bool Matches(ParameterInfo paramInfo, int pos)
        {
            if (paramInfo.ParameterType == TargetType)
            {
                return m_Selector.Invoke(paramInfo, pos);
            }
            else 
            {
                return false;
            }
        }

        public TType ProvideValue(ParameterInfo paramInfo, IServiceProvider svcProvider) => m_ValueProvider.Invoke(paramInfo, svcProvider);

        object IParameter.ProvideValue(ParameterInfo paramInfo, IServiceProvider svcProvider) => ProvideValue(paramInfo, svcProvider);
    }
}
