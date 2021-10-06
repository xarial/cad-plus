//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class ExcelRow 
    {
        public ExcelCell[] Cells { get; }

        public ExcelRow(ExcelCell[] cells) 
        {
            Cells = cells;
        }
    }
}
