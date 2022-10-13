using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Modules;
using Xarial.CadPlus.Plus.Modules.Toolbar;

namespace Xarial.CadPlus.Toolbar.Services
{
    public interface IIconsProviderRegister
    {
        IReadOnlyList<IIconsProvider> IconsProviders { get; }
        void Register(IIconsProvider iconsProvider);
    }

    public class IconsProviderRegister : IIconsProviderRegister
    {
        public IReadOnlyList<IIconsProvider> IconsProviders => m_IconsProvider;

        private readonly List<IIconsProvider> m_IconsProvider;

        public IconsProviderRegister() 
        {
            m_IconsProvider = new List<IIconsProvider>();
        }

        public void Register(IIconsProvider iconsProvider) 
        {
            m_IconsProvider.Add(iconsProvider);
        }
    }
}
