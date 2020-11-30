using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Modules;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.CustomToolbar.Services
{
    public class ImageIconsProvider : IIconsProvider
    {
        public IconFilter[] Filters => throw new NotImplementedException();

        public IXImage GetIcon(string fileName)
        {
            throw new NotImplementedException();
        }

        public Image GetThumbnail(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
