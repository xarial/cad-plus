using System.Drawing;
using System.IO;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.XToolbar.Base
{
    public class MacroButtonIcon : IXImage
    {
        public byte[] Buffer { get; }

        internal MacroButtonIcon(Image icon)
        {
            using (var ms = new MemoryStream())
            {
                icon.Save(ms, icon.RawFormat);
                Buffer = ms.ToArray();
            }
        }
    }
}
