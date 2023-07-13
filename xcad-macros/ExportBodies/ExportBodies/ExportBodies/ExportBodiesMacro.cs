﻿using System;
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
    [XCadMacroCustomVariables(typeof(ExportBodiesMacroVariablesDescriptor),
        typeof(ExportBodiesMacroVariableLinks),
        typeof(ExportBodiesMacroVariableValueProvider),
        ExportBodiesMacroVariableValueProvider.VAR_BODY_NAME)]
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

                foreach (var body in part.Bodies) 
                {
                    if (body.Visible)
                    {
                        foreach (var fileNameArg in operation.Arguments)
                        {
                            var outFilePath = fileNameArg.GetValue(body);

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
    }
}
