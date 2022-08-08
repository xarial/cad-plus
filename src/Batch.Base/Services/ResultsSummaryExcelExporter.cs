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
using Xarial.CadPlus.Plus.Shared.ViewModels;
using Xarial.XToolkit.Reporting;
using Xarial.XToolkit.Wpf.Utils;

namespace Xarial.CadPlus.Batch.Base.Services
{
    public interface IResultsSummaryExcelExporter 
    {
        FileFilter Filter { get; }
        void Export(string name, JobResultBaseVM jobResult, string filePath);
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

        public void Export(string name, JobResultBaseVM jobResult, string filePath)
        {
            if (jobResult.JobItems.Any())
            {
                var rows = new List<ExcelRow>();

                rows.Add(CreateHeader(jobResult.OperationDefinitions));

                foreach (var item in jobResult.JobItems)
                {
                    rows.Add(CreateRowForItem(item, jobResult.OperationDefinitions));
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

        private ExcelRow CreateHeader(JobItemOperationDefinitionVM[] definition)
        {
            var cells = new ExcelCell[definition.Length + 3];

            cells[0] = new ExcelCell("Status");
            cells[1] = new ExcelCell("File");

            for (int i = 0; i < definition.Length; i++)
            {
                cells[i + 2] = new ExcelCell(definition[i].Name);
            }

            cells[cells.Length - 1] = new ExcelCell("Error");

            return new ExcelRow(cells);
        }

        private ExcelRow CreateRowForItem(JobItemVM item, JobItemOperationDefinitionVM[] definition)
        {
            var cells = new ExcelCell[definition.Length + 3];

            var errors = new List<string>();

            var cellColor = default(KnownColor?);

            switch (item.State)
            {
                case JobItemState_e.Failed:
                    cellColor = KnownColor.Red;
                    break;

                case JobItemState_e.InProgress:
                    cellColor = KnownColor.LightBlue;
                    break;

                case JobItemState_e.Succeeded:
                    cellColor = KnownColor.LightGreen;
                    break;

                case JobItemState_e.Warning:
                    cellColor = KnownColor.LightYellow;
                    break;

                case JobItemState_e.Initializing:
                    cellColor = KnownColor.LightGray;
                    break;
            }

            cells[0] = new ExcelCell(item.State, null, cellColor);
            cells[1] = new ExcelCell(item.Title, null, cellColor);

            for (int i = 0; i < item.Operations.Length; i++)
            {
                var macro = item.Operations[i];
                
                if (macro.Issues.Any())
                {
                    errors.Add($"[{macro.JobItemOperation.Definition.Name}] - {string.Join("; ", macro.Issues)}");
                }

                cells[i + 2] = new ExcelCell(macro.State, null, cellColor);
            }

            cells[cells.Length - 1] = new ExcelCell(string.Join(Environment.NewLine, errors), null, cellColor);

            return new ExcelRow(cells);
        }
    }
}
