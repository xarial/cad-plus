//*********************************************************************
//CAD+ Toolset
//Copyright(C) 2022 Xarial Pty Limited
//Product URL: https://cadplus.xarial.com
//License: https://cadplus.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xarial.CadPlus.Batch.Base.Converters;
using Xarial.CadPlus.Batch.Base.ViewModels;
using Xarial.CadPlus.Plus.Data;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Services;
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

        private readonly IExcelWriter m_ExcelWriter;

        public ResultsSummaryExcelExporter(IExcelWriter excelWriter) 
        {
            m_ExcelWriter = excelWriter;
            Filter = new FileFilter("Excel file", "*.xlsx");
        }

        public void Export(string name, JobResultSummaryVM summary, string filePath)
        {
            if (summary.JobItemFiles.Any())
            {
                var rows = new List<ExcelRow>();

                rows.Add(CreateHeader(summary.JobItemFiles.FirstOrDefault()));

                foreach (var item in summary.JobItemFiles)
                {
                    rows.Add(CreateRowForItem(item));
                }

                m_ExcelWriter.CreateWorkbook(filePath, rows.ToArray(), new ExcelWriterOptions()
                {
                    CreateTable = true,
                    TableName = "Results",
                    WorksheetName = name
                });
            }
            else 
            {
                throw new UserException("No results to export"); ;
            }
        }

        private ExcelRow CreateHeader(JobItemDocumentVM template)
        {
            var cells = new ExcelCell[template.Macros.Length + 3];

            cells[0] = new ExcelCell("Status");
            cells[1] = new ExcelCell("File");

            for (int i = 0; i < template.Macros.Length; i++)
            {
                cells[i + 2] = new ExcelCell(template.Macros[i].Name);
            }

            cells[cells.Length - 1] = new ExcelCell("Error");

            return new ExcelRow(cells);
        }

        private ExcelRow CreateRowForItem(JobItemDocumentVM item)
        {
            var cells = new ExcelCell[item.Macros.Length + 3];

            var errors = new List<string>();

            if (item.Issues.Any())
            {
                errors.Add($"[File] - {string.Join(", ", item.Issues)}");
            }

            var cellColor = default(KnownColor?);

            switch (item.Status)
            {
                case Common.Services.JobItemStatus_e.Failed:
                    cellColor = KnownColor.Red;
                    break;

                case Common.Services.JobItemStatus_e.InProgress:
                    cellColor = KnownColor.LightBlue;
                    break;

                case Common.Services.JobItemStatus_e.Succeeded:
                    cellColor = KnownColor.LightGreen;
                    break;

                case Common.Services.JobItemStatus_e.Warning:
                    cellColor = KnownColor.LightYellow;
                    break;

                case Common.Services.JobItemStatus_e.AwaitingProcessing:
                    cellColor = KnownColor.LightGray;
                    break;
            }

            cells[0] = new ExcelCell(item.Status, null, cellColor);
            cells[1] = new ExcelCell(item.DisplayObject.Path, null, cellColor);

            for (int i = 0; i < item.Macros.Length; i++)
            {
                var macro = item.Macros[i];
                
                if (macro.Issues.Any())
                {
                    errors.Add($"[{macro.Name}] - {string.Join("; ", macro.Issues)}");
                }

                cells[i + 2] = new ExcelCell(macro.Status, null, cellColor);
            }

            cells[cells.Length - 1] = new ExcelCell(string.Join(Environment.NewLine, errors), null, cellColor);

            return new ExcelRow(cells);
        }
    }
}
