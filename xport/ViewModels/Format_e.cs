//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;

namespace Xarial.CadPlus.Xport.ViewModels
{
    [Flags]
    public enum Format_e
    {
        [EnumDisplayName("eDrawings Files (*.eprt, *.easm, *.edrw)")]
        [FormatExtension("e")]
        Edrw = 1 << 0,

        [EnumDisplayName("eDrawings Zip Files (*.zip)")]
        [FormatExtension("zip")]
        Zip = 1 << 1,

        [EnumDisplayName("eDrawings Executable Files (*.exe)")]
        [FormatExtension("exe")]
        Exe = 1 << 2,

        [EnumDisplayName("eDrawings Web Html Files (*.html)")]
        [FormatExtension("html")]
        Html = 1 << 3,

        [EnumDisplayName("eDrawings ActiveX Html Files (*.htm)")]
        [FormatExtension("htm")]
        Htm = 1 << 4,

        [EnumDisplayName("Stereolithography Files (*.stl)")]
        [FormatExtension("stl")]
        Stl = 1 << 5,

        [EnumDisplayName("Bitmap Files (*.bmp)")]
        [FormatExtension("bmp")]
        Bmp = 1 << 6,

        [EnumDisplayName("TIFF Image Files (*.tif)")]
        [FormatExtension("tif")]
        Tiff = 1 << 7,

        [EnumDisplayName("JPEG Image Files (*.jpg)")]
        [FormatExtension("jpg")]
        Jpeg = 1 << 8,

        [EnumDisplayName("PNG Image Files (*.png)")]
        [FormatExtension("png")]
        Png = 1 << 9,

        [EnumDisplayName("GIF Image Files (*.gif)")]
        [FormatExtension("gif")]
        Gif = 1 << 10,

        [EnumDisplayName("PDF Files (*.pdf)")]
        [FormatExtension("pdf")]
        Pdf = 1 << 11,
    }
}