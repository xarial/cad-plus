//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System.Drawing;
using Xarial.CadPlus.Plus.Data;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus.Modules.Toolbar
{
    public interface IIconsProvider
    {
        FileTypeFilter Filter { get; }
        bool Matches(string filePath);
        Image GetThumbnail(string filePath);
        IXImage GetIcon(string filePath);
    }
}
