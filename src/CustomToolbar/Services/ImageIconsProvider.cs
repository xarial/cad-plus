using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.CustomToolbar.Base;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.UI;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public class ImageIconsProvider : IIconsProvider
    {
        private readonly string[] m_SupportedExtensions;

        public ImageIconsProvider() 
        {
            m_SupportedExtensions = FileFilter.ImageFiles.Extensions
                .Select(e => Path.GetExtension(e)).ToArray();
        }

        public IconFilter Filter 
            => new IconFilter(FileFilter.ImageFiles.Name, FileFilter.ImageFiles.Extensions);

        public IXImage GetIcon(string filePath) => new ImageIcon(Image.FromFile(filePath));

        public Image GetThumbnail(string filePath) => Image.FromFile(filePath);

        public bool Matches(string filePath) 
            => !string.IsNullOrEmpty(filePath) && m_SupportedExtensions.Contains(Path.GetExtension(filePath),
                StringComparer.CurrentCultureIgnoreCase);
    }
}
