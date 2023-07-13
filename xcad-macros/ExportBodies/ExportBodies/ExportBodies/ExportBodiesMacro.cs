using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Xarial.CadPlus.Examples.Properties;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Job;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Geometry;
using Xarial.XToolkit;
using Xarial.XToolkit.Services.Expressions;

namespace Xarial.CadPlus.Examples
{
    [XCadMacro]
    [XCadMacroCustomVariable(ExportBodiesMacroExpressionSolver.VAR_BODY_NAME, typeof(ExportBodiesMacroVariablesDescriptor))]
    [Title("Export Bodies")]
    [Description("Export All Bodies to the specified formats")]
    [Icon(typeof(Resources), nameof(Resources.export_bodies))]
    public class ExportBodiesMacro : IXCadMacro
    {
        public void Run(IJobItemRunMacroOperation operation)
        {
            var doc = operation.Document;

            if (doc is IXPart)
            {
                var part = (IXPart)doc;

                var exprParser = operation.Services.GetService<IExpressionParser>();

                var solver = new ExportBodiesMacroExpressionSolver();

                var fileNameTemplates = operation.Arguments?.Where(a => !string.IsNullOrEmpty(a))?.ToArray();

                if (fileNameTemplates?.Any() != true)
                {
                    throw new UserException($"File name templates are not specified");
                }

                var fileNameTokens = fileNameTemplates.Select(t => exprParser.Parse(t)).ToArray();

                var resFiles = new List<ExportedBodyFile>();

                foreach (var body in part.Bodies) 
                {
                    if (body.Visible)
                    {
                        foreach (var fileNameToken in fileNameTokens)
                        {
                            var outFilePath = solver.Solve(fileNameToken, body);
                            outFilePath = FileSystemUtils.ReplaceIllegalRelativePathCharacters(outFilePath, c => '_');

                            if (!Path.IsPathRooted(outFilePath))
                            {
                                outFilePath = FileSystemUtils.CombinePaths(Path.GetDirectoryName(doc.Path), outFilePath);
                            }

                            resFiles.Add(new ExportedBodyFile(outFilePath, body));
                        }
                    }
                    else 
                    {
                        operation.Log($"Body '{body.Name}' in '{doc.Path}' is hidden");
                    }
                }

                operation.SetResult(resFiles);

                foreach (var resFile in resFiles) 
                {
                    try
                    {
                        var saveOp = (IXDocument3DSaveOperation)part.PreCreateSaveAsOperation(resFile.Path);
                        saveOp.Bodies = new IXBody[]
                        {
                            resFile.Body
                        };
                        saveOp.Commit();

                        resFile.Status = JobItemOperationResultFileStatus_e.Succeeded;
                    }
                    catch (Exception ex)
                    {
                        operation.ReportIssue($"Failed to export '{resFile.Body.Name}': {ex.ParseUserError()}", JobItemIssueType_e.Error);
                        resFile.Status = JobItemOperationResultFileStatus_e.Failed;
                    }
                }
            }
            else 
            {
                throw new UserException("Only part files are supported");
            }
        }
    }
}
