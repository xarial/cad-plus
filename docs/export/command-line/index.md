---
caption: Command Line
title: Command line for running and configuring eXport+
description: Technical documentation for configuring and running the publishing job of eXport+ using command line
order: 2
image: command-line-output.png
redirect-from:
  - /xport/command-line/
---
![Command line exporting process](command-line-output.png){ width=600 }

eXport+ can be executed from the command line. This can be especially useful if the exporting functionality needs to be integrated in the automation workflow.

Navigate to **exportplus.exe** file in the instalation folder (usually *C:\Program Files\Xarial\CAD+ Toolset\exportplus.exe*).

Refer the list of available arguments below. Use -- symbol to use arguments. Use --help argument to display help i the console.

For example the below command will export all drawing files (*.slddrw) from the *D:\Input Files* folder to html and pdf formats to the same folder as original files. Export operation will ignore errors and continue with next file with timeout of 2 minutes (120 seconds).

> \> exportplus.exe --i "D:\Input Files" --f html pdf --filter *.slddrw --e --t 120

| Short Flag  | Flag  |Required   | Summary  |
|---|---|---|---|
| -i  | --input  |Yes   | List of input directories or file paths to process. These are files which can be opened by eDrawings (e.g. SOLIDWORKS files, CATIA, STEP, DXF/DWG, etc.)  |
|   | --filter  | No  |  Filter to extract input files, if input parameter contains directories |
|  -o |  --out |  No | Path to the directory to export results to. Tool will automatically create directory if it doesn’t exist. If this parameter is not specified, files will be exported to the same folder as the input file  |
| -f  | --format  |  Yes | List of formats to export the files to. Supported formats: .jpg, .tif, .bmp, .png, .stl, .exe, .htm, .html, .pdf, .zip, .edrw, .eprt, and .easm. Specify .e to export to the corresponding format of eDrawings (e.g. .sldprt is exported to .eprt, .sldasm to .easm, .slddrw to .edrw). If this parameter is not specified than file will be exported to eDrawings. PDF format is only supported on Windows 10  |
| -e  |  --error | No  | If this option is used export will continue if any of the files or formats failed to process, otherwise the export will terminate  |
| -t  |  --timeout | No  | Timeout in seconds for processing a single item (e.g. exporting single file to a single format)")]