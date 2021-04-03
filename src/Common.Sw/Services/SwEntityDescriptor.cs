﻿//*********************************************************************
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
using Xarial.CadPlus.Common.Sw.Properties;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Services;

namespace Xarial.CadPlus.Common.Sw.Services
{
    public class SwEntityDescriptor : ICadDescriptor
    {
        public string ApplicationId => CadApplicationIds.SolidWorks;
        public string ApplicationName => "SOLIDWORKS";
        public Image ApplicationIcon => Resources.sw_application;

        public Image PartIcon => Resources.document_icon;
        public Image AssemblyIcon => Resources.assembly_icon;
        public Image DrawingIcon => Resources.drawing_icon;
        public Image ConfigurationIcon => Resources.config_icon;
        public Image SheetIcon => Resources.sheet_icon;
        public Image CutListIcon => Resources.cutlist_icon;

        public FileTypeFilter PartFileFilter => new FileTypeFilter("SOLIDWORKS Parts", "*.sldprt");
        public FileTypeFilter AssemblyFileFilter => new FileTypeFilter("SOLIDWORKS Assemblies", "*.sldasm");
        public FileTypeFilter DrawingFileFilter => new FileTypeFilter("SOLIDWORKS Drawings", "*.slddrw");

        public FileTypeFilter[] MacroFileFilters => new FileTypeFilter[]
        {
            new FileTypeFilter("VBA Macros", "*.swp"),
            new FileTypeFilter("SWBasic Macros", "*.swb"),
            new FileTypeFilter("VSTA Macros", "*.dll"),
            new FileTypeFilter("All Macros", "*.swp", "*.swb", "*.dll")
        };
    }
}