//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2021 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Converters;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IResultsSummaryExcelExporter 
    {
        FileFilter Filter { get; }
        void Export(string name, JobResultSummaryVM summary, string filePath);
    }

    public class ResultsSummaryExcelExporter : IResultsSummaryExcelExporter
    {
        public FileFilter Filter { get; }

        public ResultsSummaryExcelExporter() 
        {
            Filter = new FileFilter("Excel file", "*.xlsx");
        }

        public void Export(string name, JobResultSummaryVM summary, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(name);

                WriteHeader(worksheet, summary.JobItemFiles.FirstOrDefault());

                var nextRow = 2;

                foreach (var item in summary.JobItemFiles) 
                {
                    WriteItem(worksheet, item, ref nextRow);
                    nextRow++;
                }
                
                var range = worksheet.Range(worksheet.FirstCellUsed().Address, worksheet.LastCellUsed().Address);

                var table = range.CreateTable("Results");
                table.ShowHeaderRow = true;

                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        private void WriteHeader(IXLWorksheet worksheet, JobItemDocumentVM template)
        {
            if (template != null)
            {
                worksheet.Cell(1, 1).Value = "Status";
                worksheet.Cell(1, 2).Value = "File";

                var colIndex = 2;

                for (int i = 0; i < template.Macros.Length; i++)
                {
                    worksheet.Cell(1, ++colIndex).Value = template.Macros[i].Name;
                }

                worksheet.Cell(1, ++colIndex).Value = "Error";
            }
        }

        private void WriteItem(IXLWorksheet worksheet, JobItemDocumentVM item, ref int nextRow)
        {
            var errors = new List<string>();

            if (item.Error != null)
            {
                errors.Add($"[File] - {ExceptionToErrorConverter.Convert(item.Error)}");
            }

            worksheet.Cell(nextRow, 1).Value = item.Status;
            worksheet.Cell(nextRow, 2).Value = item.DisplayObject.Path;

            var colIndex = 2;

            for (int i = 0; i < item.Macros.Length; i++)
            {
                var macro = item.Macros[i];
                
                if (macro.Error != null)
                {
                    errors.Add($"[{macro.Name}] - {ExceptionToErrorConverter.Convert(item.Macros[i].Error)}");
                }

                worksheet.Cell(nextRow, ++colIndex).Value = macro.Status;
            }

            worksheet.Cell(nextRow, ++colIndex).Value = string.Join(Environment.NewLine, errors);

            var cellColor = XLColor.Transparent;

            switch (item.Status)
            {
                case Common.Services.JobItemStatus_e.Failed:
                    cellColor = XLColor.Red;
                    break;

                case Common.Services.JobItemStatus_e.InProgress:
                    cellColor = XLColor.LightBlue;
                    break;

                case Common.Services.JobItemStatus_e.Succeeded:
                    cellColor = XLColor.LightGreen;
                    break;

                case Common.Services.JobItemStatus_e.Warning:
                    cellColor = XLColor.LightYellow;
                    break;

                case Common.Services.JobItemStatus_e.AwaitingProcessing:
                    cellColor = XLColor.LightGray;
                    break;
            }

            worksheet.Range(nextRow, 1, nextRow, colIndex).Style.Fill.BackgroundColor = cellColor;
        }
    }
}
