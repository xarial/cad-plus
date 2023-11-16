using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Resources;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Job;
using Xarial.CadPlus.Plus;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XToolkit;
using System.Linq;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Services;
using Xarial.XCad.Documents.Extensions;
using Xarial.CadPlus.Examples.Properties;
using Xarial.XCad.Base;

namespace Xarial.CadPlus.Examples
{
    [XCadMacro]
    [Title("Zip Export")]
    [Description("Export files in the specified format and zip into a single file")]
    [Icon(typeof(Resources), nameof(Resources.zip_file))]
    public class ZipExportMacro : IXCadMacro
    {
        public void Run(IJobItemRunMacroOperation operation)
        {
            if (operation.Arguments.Length <= 1) 
            {
                throw new UserException($"Missing arguments. First argument is a name of the ZIP file, all other arguments are the names of the files to include to zip");
            }

            var doc = operation.Document;

            var firstItem = GetFirstItem(operation.Job.JobItems);
            var isFirst = firstItem == operation.Item;

            ZipFile zipFile;

            if (isFirst)
            {
                var zipWriter = operation.Services.GetService<IZipWriter>();
                
                var zipFilePath = operation.Arguments[0].GetValue();

                if (!Path.IsPathRooted(zipFilePath)) 
                {
                    zipFilePath = Path.Combine(Path.GetDirectoryName(doc.Path), zipFilePath);
                }

                var zipDir = Path.GetDirectoryName(zipFilePath);

                if (!Directory.Exists(zipDir)) 
                {
                    Directory.CreateDirectory(zipDir);
                }

                zipFile = new ZipFile(zipFilePath, new Lazy<IZipStream>(() => zipWriter.Write(zipFilePath)), operation.Logger);

                operation.SetResult(zipFile);
            }
            else
            {
                zipFile = (ZipFile)firstItem.Operations.First().UserResult;
                operation.SetResult(zipFile);
            }

            var results = new List<bool>();

            foreach (var outFileArg in operation.Arguments.Skip(1)) 
            {
                var outFileName = "";

                try 
                {
                    outFileName = outFileArg.GetValue();

                    AddFileToZip(doc, outFileName, zipFile.ZipStream, zipFile.TempFolder);
                    results.Add(true);
                }
                catch (Exception ex)
                {
                    operation.Logger.Log(ex);

                    operation.ReportIssue($"{outFileName} : {ex.ParseUserError()}", JobItemIssueType_e.Error);
                    results.Add(false);
                }
            }

            if (results.All(r => r == true)) 
            {
                operation.SetStatus(JobItemStateStatus_e.Succeeded);
            }
            else if (results.All(r => r == false))
            {
                operation.SetStatus(JobItemStateStatus_e.Failed);
            }
            else
            {
                operation.SetStatus(JobItemStateStatus_e.Warning);
            }

            zipFile.Succeeded &= results.All(r => r == true);

            var isLast = GetLastItem(operation.Job.JobItems) == operation.Item;

            if (isLast) 
            {
                zipFile.Status = zipFile.Succeeded ? JobItemOperationResultFileStatus_e.Succeeded : JobItemOperationResultFileStatus_e.Failed;
                zipFile.ZipStream.Dispose();
            }
        }

        private void AddFileToZip(IXDocument doc, string outFileName, IZipStream zipStream, string outFolder)
        {
            var outFilePath = Path.Combine(outFolder, outFileName);

            var dir = Path.GetDirectoryName(outFilePath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            doc.SaveAs(outFilePath);

            zipStream.AddFile(outFilePath, outFileName);

            File.Delete(outFilePath);
        }

        private IJobItem GetFirstItem(IReadOnlyList<IJobItem> items) 
        {
            var first = items.First();

            if (first.Nested?.Any() == true)
            {
                return first.Nested.First();
            }
            else 
            {
                return first;
            }
        }

        private IJobItem GetLastItem(IReadOnlyList<IJobItem> items) 
        {
            var last = items.Last();

            if (last.Nested?.Any() == true)
            {
                return GetLastItem(last.Nested);
            }
            else 
            {
                return last;
            }
        }
    }
}
