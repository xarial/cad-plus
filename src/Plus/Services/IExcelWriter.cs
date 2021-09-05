using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xarial.CadPlus.Plus.Services
{
    [Serializable]
    public class ExcelRow 
    {
        public ExcelCell[] Cells { get; set; }

        public ExcelRow() 
        {
        }

        public ExcelRow(ExcelCell[] cells) 
        {
            Cells = cells;
        }
    }

    [Serializable]
    public class ExcelCell 
    {
        public object Value { get; set; }
        public KnownColor? ForegroundColor { get; set; }
        public KnownColor? BackgroundColor { get; set; }

        public ExcelCell() 
        {
        }

        public ExcelCell(object value) 
        {
            Value = value;
        }
    }

    [Serializable]
    public class ExcelWriterOptions 
    {
        public string SpreadsheetName { get; set; }
        public bool CreateTable { get; set; }
        public string TableName { get; set; }
    }

    public interface IExcelWriter
    {
        void Create(string filePath, ExcelRow[] rows, ExcelWriterOptions options);
    }
}
