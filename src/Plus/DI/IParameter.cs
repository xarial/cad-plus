using System;
using System.Reflection;

namespace Xarial.CadPlus.Plus.DI
{
    public interface IParameter
    {
        Type TargetType { get; }
        bool Matches(ParameterInfo paramInfo, int index);
        object ProvideValue(ParameterInfo paramInfo);
    }

    public class Parameter<TType> : IParameter
    {
        public static Parameter<TType> Create(Func<ParameterInfo, int, bool> selector, TType value)
            => new Parameter<TType>(selector, p => value);

        public static Parameter<TType> Named(string paramName, TType value)
            => Named(paramName, () => value);

        public static Parameter<TType> Named(string paramName, Func<TType> valueFunc)
            => new Parameter<TType>((p, i) => p.Name == paramName, p => valueFunc.Invoke());

        public static Parameter<TType> Indexed(int index, TType value)
            => Indexed(index, () => value);

        public static Parameter<TType> Indexed(int index, Func<TType> valueFunc)
            => new Parameter<TType>((p, i) => index == i, p => valueFunc.Invoke());

        public static Parameter<TType> Any(TType value)
            => Any(() => value);

        public static Parameter<TType> Any(Func<TType> valueFunc)
            => new Parameter<TType>((p, i) => true, p => valueFunc.Invoke());

        private readonly Func<ParameterInfo, int, bool> m_Selector;
        private readonly Func<ParameterInfo, TType> m_ValueProvider;

        public Type TargetType => typeof(TType);

        public Parameter(Func<ParameterInfo, int, bool> selector, Func<ParameterInfo, TType> valueProvider)
        {
            m_Selector = selector;
            m_ValueProvider = valueProvider;
        }

        public bool Matches(ParameterInfo paramInfo, int pos) => m_Selector.Invoke(paramInfo, pos);

        public TType ProvideValue(ParameterInfo paramInfo) => m_ValueProvider.Invoke(paramInfo);

        object IParameter.ProvideValue(ParameterInfo paramInfo) => ProvideValue(paramInfo);
    }
}
