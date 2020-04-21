//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using CommandLine;
using System.Collections.Generic;

namespace Xarial.CadPlus.Xport
{
    public class Arguments
    {
        [Option('i', "input", Required = true, HelpText = "List of input directories or file paths to process. These are files which can be opened by eDrawings (e.g. SOLIDWORKS files, CATIA, STEP, DXF/DWG, etc.)")]
        public IEnumerable<string> Input { get; set; }

        [Option("filter", Required = false, HelpText = "Filter to extract input files, if input parameter contains directories")]
        public string Filter { get; set; }

        [Option('o', "out", Required = false, HelpText = "Path to the directory to export results to. Tool will automatically create directory if it doesn’t exist. If this parameter is not specified, files will be exported to the same folder as the input file")]
        public string OutputDirectory { get; set; }

        [Option('f', "format", Required = true, HelpText = "List of formats to export the files to. Supported formats: .jpg, .tif, .bmp, .png, .stl, .exe, .htm, .html, .pdf, .zip, .edrw, .eprt, and .easm. Specify .e to export to the corresponding format of eDrawings (e.g. .sldprt is exported to .eprt, .sldasm to .easm, .slddrw to .edrw). If this parameter is not specified than file will be exported to eDrawings. PDF format is only supported on Windows 10")]
        public IEnumerable<string> Format { get; set; }

        [Option('e', "error", Required = false, HelpText = "If this option is used export will continue if any of the files or formats failed to process, otherwise the export will terminate")]
        public bool ContinueOnError { get; set; }

        [Option('t', "timeout", Required = false, HelpText = "Timeout in seconds for processing a single item (e.g. exporting single file to a single format)")]
        public int Timeout { get; set; }
    }
}