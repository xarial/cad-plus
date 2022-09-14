//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Common.Attributes;

namespace Xarial.CadPlus.Xport.ViewModels
{
    public enum EDrawingAppVersion_e
    {
        [EnumDescription("Default")]
        Default = 0,

        [EnumDescription("2019")]
        v2019 = 2019,

        [EnumDescription("2020")]
        v2020 = 2020,

        [EnumDescription("2021")]
        v2021 = 2021,

        [EnumDescription("2022")]
        v2022 = 2022
    }
}
