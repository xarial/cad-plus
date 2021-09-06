using System;

namespace Xarial.CadPlus.Plus.Data
{
    [Serializable]
    public class ExcelRow 
    {
        public ExcelCell[] Cells { get; }

        public ExcelRow(ExcelCell[] cells) 
        {
            Cells = cells;
        }
    }
}
