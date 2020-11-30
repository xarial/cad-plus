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
        IconFilter Filter { get; }
        bool Matches(string filePath);
        Image GetThumbnail(string filePath);
        IXImage GetIcon(string filePath);
    }

    public interface IToolbarModule : IExtensionModule
    {
        void RegisterIconsProvider(IIconsProvider provider);
    }
}
