//*********************************************************************
//xTools
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtools.xarial.com
//License: https://xtools.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.XTools.Xport.Core
{
    public class ExportOptions
    {
        public string[] Input { get; set; }
        public string Filter { get; set; }
        public string OutputDirectory { get; set; }
        public string[] Format { get; set; }
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
    }
}
