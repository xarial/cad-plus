//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

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
        public string Name { get; set; }
        public string[] Extensions { get; set; }

        public IconFilter() 
        {
        }

        public IconFilter(string name, params string[] extensions)
        {
            Name = name;
            Extensions = extensions;
        }
    }

    public interface IIconsProvider 
    {
        IconFilter Filter { get; }
        bool Matches(string filePath);
        Image GetThumbnail(string filePath);
        IXImage GetIcon(string filePath);
    }

    public interface IToolbarModule : IModule
    {
        void RegisterIconsProvider(IIconsProvider provider);
    }
}
