﻿//*********************************************************************
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

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class CsvRow
    {
        public string[] Cells { get; }

        public CsvRow(string[] cells)
        {
            Cells = cells;
        }
    }
}