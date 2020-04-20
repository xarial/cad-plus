//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using System;
using System.IO;
using System.Text;

namespace Xarial.XTools.Xport.ViewModels
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
