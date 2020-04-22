//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Xport.ViewModels
{
    public class ProgressHandler : IProgress<double>
    {
        private readonly ExporterSettingsVM m_Vm;

        internal ProgressHandler(ExporterSettingsVM vm)
        {
            m_Vm = vm;
        }

        public void Report(double value)
        {
            m_Vm.Progress = value;
        }
    }
}