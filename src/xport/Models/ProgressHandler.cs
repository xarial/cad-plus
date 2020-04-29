//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Xport.Models
{
    public class ProgressHandler : IProgress<double>
    {
        public event Action<double> ProgressChanged;
        
        public void Report(double value)
        {
            ProgressChanged?.Invoke(value);
        }
    }
}