using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Bom;
using Xarial.CadPlus.Plus.DI;

namespace Xarial.CadPlus.Plus.Samples
{
    public class UpperCaseBomValueExtractor : IBomValueExtractor
    {
        private readonly IBomValueExtractor m_Orig;

        public UpperCaseBomValueExtractor(IBomValueExtractor orig) 
        {
            m_Orig = orig;
        }

        public object GetValue(IBomItemBase item, ValueSource_e src, string arg)
        {
            var val = m_Orig.GetValue(item, src, arg);

            if (val is string) 
            {
                val = ((string)val).ToUpper();
            }

            return val;
        }
    }

    [Module]
    public class BomUpperCaseModule : IModule
    {
        public void Init(IHost host)
        {
            host.ConfigureServices += OnConfigureServices;
        }

        private void OnConfigureServices(IContainerBuilder builder)
        {
            builder.RegisterDecorator<IBomValueExtractor, UpperCaseBomValueExtractor>(LifetimeScope_e.Singleton);
        }

        public void Dispose()
        {
        }
    }
}
