//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;
using Xarial.XCad.UI;

namespace Xarial.CadPlus.Plus.Modules
{
    public interface IIconsProvider 
    {
        FileTypeFilter Filter { get; }
        bool Matches(string filePath);
        Image GetThumbnail(string filePath);
        IXImage GetIcon(string filePath);
    }

    public interface IToolbarModule : IModule
    {
        void RegisterIconsProvider(IIconsProvider provider);
    }
}
