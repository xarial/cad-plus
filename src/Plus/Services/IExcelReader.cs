using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;

namespace Xarial.CadPlus.Plus.Services
{
    public class ExcelReaderOptions
    {
        public int WorksheetIndex { get; set; }
        public bool ReadCustomProperites { get; set; }
        public bool ReadCellBackgroundColor { get; set; }
        public bool ReadCellTextColor { get; set; }
        public bool ReadCellFontStyle { get; set; }
    }

    public interface IExcelReader
    {
        ExcelRow[] ReadWorkbook(string filePath, ExcelReaderOptions options, out ExcelCustomProperty[] customProperties);
    }
}
