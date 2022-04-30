//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

namespace Xarial.CadPlus.Xport.Core
{
    public class ExportOptions
    {
        public string[] Input { get; set; }
        public string Filter { get; set; }
        public string OutputDirectory { get; set; }
        public string[] Format { get; set; }
        public bool ContinueOnError { get; set; }
        public int Timeout { get; set; }
        public int Version { get; set; }
    }
}