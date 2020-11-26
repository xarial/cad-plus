using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus.Modules
{
    public class IconFilter 
    {
        public string Name { get; }
        public string[] Extensions { get; }
    }

    public interface IIconsProvider 
    {
        IconFilter[] Filters { get; }
        Image GetThumbnail(string fileName);
        IXImage GetIcon(string fileName);
    }

    public interface IToolbarModule : IExtensionModule
    {
        void RegisterIconsProvider(IIconsProvider provider);
    }
}
