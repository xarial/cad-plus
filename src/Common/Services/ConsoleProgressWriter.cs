//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Common.Services
{
    public class ConsoleProgressWriter : IProgress<double>
    {
        public void Report(double value)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Progress: {(value * 100).ToString("F")}%");
            Console.ResetColor();
        }
    }
}
