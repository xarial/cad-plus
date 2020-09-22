//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using Xarial.CadPlus.Common.Attributes;

namespace Xarial.CadPlus.Xport.ViewModels
{
    [Flags]
    public enum Format_e
    {
        [EnumDescription("eDrawings Files (*.eprt, *.easm, *.edrw)")]
        [FormatExtension("e")]
        Edrw = 1 << 0,

        [EnumDescription("eDrawings Zip Files (*.zip)")]
        [FormatExtension("zip")]
        Zip = 1 << 1,

        [EnumDescription("eDrawings Executable Files (*.exe)")]
        [FormatExtension("exe")]
        Exe = 1 << 2,

        [EnumDescription("eDrawings Web Html Files (*.html)")]
        [FormatExtension("html")]
        Html = 1 << 3,

        [EnumDescription("eDrawings ActiveX Html Files (*.htm)")]
        [FormatExtension("htm")]
        Htm = 1 << 4,

        [EnumDescription("Stereolithography Files (*.stl)")]
        [FormatExtension("stl")]
        Stl = 1 << 5,

        [EnumDescription("Bitmap Files (*.bmp)")]
        [FormatExtension("bmp")]
        Bmp = 1 << 6,

        [EnumDescription("TIFF Image Files (*.tif)")]
        [FormatExtension("tif")]
        Tiff = 1 << 7,

        [EnumDescription("JPEG Image Files (*.jpg)")]
        [FormatExtension("jpg")]
        Jpeg = 1 << 8,

        [EnumDescription("PNG Image Files (*.png)")]
        [FormatExtension("png")]
        Png = 1 << 9,

        [EnumDescription("GIF Image Files (*.gif)")]
        [FormatExtension("gif")]
        Gif = 1 << 10,

        [EnumDescription("PDF Files (*.pdf) - EXPERIMENTAL")]
        [EnumDisplayName("pdf - EXPERIMENTAL")]
        [FormatExtension("pdf")]
        Pdf = 1 << 11
    }
}