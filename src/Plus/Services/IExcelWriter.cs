﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Plus.Data;

namespace Xarial.CadPlus.Plus.Services
{
    public class ExcelWriterOptions 
    {
        public string WorksheetName { get; set; }
        public bool CreateTable { get; set; }
        public string TableName { get; set; }
        public bool ShowStripes { get; set; }
        public string TableThemeName { get; set; }
        public ExcelCustomProperty[] CustomProperties { get; set; }
    }

    public interface IExcelWriter
    {
        void CreateWorkbook(string filePath, ExcelRow[] rows, ExcelWriterOptions options);
    }
}
