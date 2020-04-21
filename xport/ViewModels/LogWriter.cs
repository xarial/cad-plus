//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Text;

namespace Xarial.CadPlus.Xport.ViewModels
{
    public class LogWriter : TextWriter
    {
        private readonly ExporterSettingsVM m_Vm;

        internal LogWriter(ExporterSettingsVM vm)
        {
            m_Vm = vm;
        }

        public override void WriteLine(string value)
        {
            m_Vm.Log += !string.IsNullOrEmpty(m_Vm.Log) ? Environment.NewLine + value : value;
        }

        public override Encoding Encoding => Encoding.Default;
    }
}