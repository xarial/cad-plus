using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.UI;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public class ImageIconsProvider : IIconsProvider
    {
        public IconFilter Filter => new IconFilter()
        {
            Name = FileFilter.ImageFiles.Name,
            Extensions = FileFilter.ImageFiles.Extensions
        };

        public IXImage GetIcon(string fileName)
        {
            throw new NotImplementedException();
        }

        public Image GetThumbnail(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Matches(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
