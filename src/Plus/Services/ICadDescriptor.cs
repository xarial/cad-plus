//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
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
using Xarial.XCad;

namespace Xarial.CadPlus.Plus.Services
{
    public interface ICadDescriptor
    {
        string ApplicationId { get; }
        string ApplicationName { get; }
        Image ApplicationIcon { get; }

        Image PartIcon { get; }
        Image AssemblyIcon { get; }
        Image DrawingIcon { get; }
        
        Image ConfigurationIcon { get; }
        Image SheetIcon { get; }
        
        Image SolidBodyIcon { get; }
        Image SheetBodyIcon { get; }

        Image CutListSolidBodyIcon { get; }
        Image CutListSheetMetalIcon { get; }
        Image CutListWeldmentIcon { get; }

        FileTypeFilter PartFileFilter { get; }
        FileTypeFilter AssemblyFileFilter { get; }
        FileTypeFilter DrawingFileFilter { get; }

        FileTypeFilter[] MacroFileFilters { get; }

        bool IsSystemFile(string filePath);
    }
}
