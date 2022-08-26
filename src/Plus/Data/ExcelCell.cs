//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Drawing;

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class ExcelCell 
    {
        public object Value { get; }
        public KnownColor? TextColor { get; }
        public KnownColor? BackgroundColor { get; }
        public FontStyle? FontStyle { get; }
        public string Comment { get; }

        public ExcelCell(object value) 
        {
            Value = value;
        }

        public ExcelCell(object value, KnownColor? textColor, KnownColor? backgroundColor) : this(value)
        {
            TextColor = textColor;
            BackgroundColor = backgroundColor;
        }

        public ExcelCell(object value, KnownColor? textColor, KnownColor? backgroundColor, FontStyle? fontStyle) 
            : this(value, textColor, backgroundColor)
        {
            FontStyle = fontStyle;
        }

        public ExcelCell(object value, KnownColor? textColor, KnownColor? backgroundColor, FontStyle? fontStyle, string comment)
            : this(value, textColor, backgroundColor, fontStyle)
        {
            Comment = comment;
        }
    }
}
