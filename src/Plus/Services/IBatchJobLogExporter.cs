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
using Xarial.XToolkit;

namespace Xarial.CadPlus.Plus.Services
{
    public interface IBatchJobLogExporter
    {
        FileFilter Filter { get; }
        void Export(IBatchJobBase job, string filePath);
    }
}
