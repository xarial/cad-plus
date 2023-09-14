using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Xarial.CadPlus.Examples.Properties;
using Xarial.CadPlus.Plus;
using Xarial.CadPlus.Plus.Attributes;
using Xarial.CadPlus.Plus.Exceptions;
using Xarial.CadPlus.Plus.Extensions;
using Xarial.CadPlus.Plus.Job;
using Xarial.XCad.Base;
using Xarial.XCad.Base.Attributes;
using Xarial.XCad.Documents;
using Xarial.XCad.Features;
using Xarial.XCad.Geometry;
using Xarial.XToolkit;

namespace Xarial.CadPlus.Examples
{
    [XCadMacro]
    [XCadMacroCustomVariables(typeof(ExportBodiesMacroVariablesDescriptor),
        typeof(ExportBodiesMacroVariableLinks),
        typeof(ExportBodiesMacroVariableValueProvider),
        ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME, ExportBodiesMacroVariableValueProvider.VAR_CUT_LIST_PRP, ExportBodiesMacroVariableValueProvider.VAR_QTY)]
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

                if (operation.Arguments?.Any() != true)
                {
                    throw new UserException($"File name templates are not specified");
                }

                var resFiles = new List<ExportedBodyFile>();

                foreach (var bodyInfo in EnumerateBodies(part)) 
                {
                    foreach (var fileNameArg in operation.Arguments)
                    {
                        var outFilePath = fileNameArg.GetValue(bodyInfo);

                        if (!Path.IsPathRooted(outFilePath))
                        {
                            outFilePath = FileSystemUtils.CombinePaths(Path.GetDirectoryName(doc.Path),
                                FileSystemUtils.ReplaceIllegalRelativePathCharacters(outFilePath, c => '_'));
                        }

                        resFiles.Add(new ExportedBodyFile(outFilePath, bodyInfo.Body));
                    }
                }

                operation.SetResult(resFiles);

                foreach (var resFile in resFiles) 
                {
                    try
                    {
                        operation.Log($"Exporting '{resFile.Body.Name}' to '{resFile.Path}'");

                        var saveOp = part.PreCreateSaveAsOperation(resFile.Path);

                        saveOp.Bodies = new IXBody[]
                        {
                            resFile.Body
                        };

                        var dir = Path.GetDirectoryName(resFile.Path);
                        
                        if (!Directory.Exists(dir)) 
                        {
                            Directory.CreateDirectory(dir);
                        }

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

        private IEnumerable<BodyInfo> EnumerateBodies(IXPart part)
        {
            var cutLists = part.Configurations.Active.CutLists.ToArray();

            if (cutLists.Any())
            {
                foreach (var cutList in cutLists) 
                {
                    yield return new BodyInfo(cutList.Bodies.First(), cutList.Name, cutList.Quantity(), cutList.Properties);
                }
            }
            else
            {
                foreach (var body in part.Bodies)
                {
                    yield return new BodyInfo(body, body.Name, 1, null);
                }
            }
        }
    }
}
