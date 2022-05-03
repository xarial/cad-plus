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
using Xarial.XCad.Base.Attributes;

namespace Xarial.CadPlus.Drawing.Data
{
    public enum Source_e
    {
        [Title("Custom Property")]
        CustomProperty,

        [Title("File Path")]
        FilePath,

        [Title("Part Number")]
        PartNumber,

        [Title("PDM Vault Link")]
        PdmVaultLink,

        [Title("PDM Web2 Url")]
        PdmWeb2Url,

        Custom
    }
}
