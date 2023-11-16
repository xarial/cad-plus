'#Const TEST = True

Dim swApp As SldWorks.SldWorks

Sub main()

    Set swApp = Application.SldWorks
    
    Dim macroOper As IMacroOperation
    Set macroOper = GetMacroOperation()
    
    Dim vArgs As Variant
    vArgs = macroOper.Arguments
    
    Dim swModel As SldWorks.ModelDoc2
    Set swModel = macroOper.model
    
    Dim swPart As SldWorks.PartDoc
    
    Set swPart = swModel
    
    Dim vBodies As Variant
    vBodies = swPart.GetBodies2(swBodyType_e.swAllBodies, True)
    
    Dim i As Integer
    Dim swBody As SldWorks.Body2
    
    Dim customVarValProv As IMacroCustomVariableValueProvider
    Set customVarValProv = New CustomVariableValueProvider
    
    Dim resFilePaths() As String
    Dim inputBodies() As SldWorks.Body2
    
    For i = 0 To UBound(vBodies)
    
        Set swBody = vBodies(i)
        
        Dim j As Integer
        
        For j = 0 To UBound(vArgs)
        
            Dim macroArg As IMacroArgument
            Set macroArg = vArgs(j)
            
            Dim fileName As String
            fileName = macroArg.GetValue(customVarValProv, swBody)
            
            Dim filePath As String
            filePath = GetDirectory(swModel.GetPathName) & fileName
            
            If (Not resFilePaths) = -1 Then
                ReDim resFilePaths(0)
                ReDim inputBodies(0)
            Else
                ReDim Preserve resFilePaths(UBound(resFilePaths) + 1)
                ReDim Preserve inputBodies(UBound(inputBodies) + 1)
            End If
            
            resFilePaths(UBound(resFilePaths)) = filePath
            Set inputBodies(UBound(inputBodies)) = swBody
            
        Next
        
    Next
    
    Dim vResFiles As Variant
    vResFiles = macroOper.SetResultFiles(resFilePaths)
    
    For i = 0 To UBound(vResFiles)
        
        Dim resFile As IMacroOperationResultFile
        Set resFile = vResFiles(i)
        Set swBody = inputBodies(i)

        TryExportBody swModel, swBody, resFile, macroOper
            
    Next
    
End Sub

Sub TryExportBody(model As SldWorks.ModelDoc2, body As SldWorks.Body2, resFile As IMacroOperationResultFile, macroOper As MacroOperation)

try_:
    On Error GoTo catch_
    
    Dim swSelMgr As SldWorks.SelectionMgr
    Set swSelMgr = model.SelectionManager
    
    swSelMgr.SuspendSelectionList
    
    Dim swBodies(0) As SldWorks.Body2
    Set swBodies(0) = body
    
    If swSelMgr.AddSelectionListObjects(swBodies, Nothing) = 1 Then
        
        Dim filePath As String
        filePath = resFile.path
        
        Dim errs As Long
        Dim warns As Long
        Dim dir As String
        
        dir = GetDirectory(filePath)
        
        CreateDirectories dir
        
        If False <> model.Extension.SaveAs2(filePath, swSaveAsVersion_e.swSaveAsCurrentVersion, swSaveAsOptions_e.swSaveAsOptions_Silent, Nothing, "", False, errs, warns) Then
            resFile.Status = MacroOperationResultFileStatus_e_Succeeded
        Else
            Err.Raise vbError, "", "Failed to export '" & body.Name & "' to '" & filePath & "'. Error code: " & errs
        End If
    Else
        Err.Raise vbError, "", "Failed to select " & body.Name
    End If

    GoTo finally_
catch_:
    macroOper.ReportIssue Err.Description, MacroIssueType_e_Error
    resFile.Status = MacroOperationResultFileStatus_e_Failed
finally_:

    swSelMgr.ResumeSelectionList2 False
    
End Sub

Function GetMacroOperation() As IMacroOperation
    
    Dim macroOper As IMacroOperation
    
    #If TEST Then
        Dim swCadPlusFact As Object
        Set swCadPlusFact = CreateObject("CadPlusFactory.Sw")
        
        Set swCadPlus = swCadPlusFact.Create(swApp, False)
        
        Dim args(1) As String
        args(0) = "MFGs\STEP\{ path [FileNameWithoutExtension] }-{ bodyName }.step"
        args(1) = "MFGs\IGES\{ path [FileNameWithoutExtension] }-{ bodyName }.igs"
        Set macroOper = swCadPlus.CreateMacroOperation(swApp.ActiveDoc, "", args)
    #Else
        Dim macroOprMgr As IMacroOperationManager
        Set macroOprMgr = CreateObject("CadPlus.MacroOperationManager")
        
        Set macroOper = macroOprMgr.PopOperation(swApp)
    #End If
    
    Set GetMacroOperation = macroOper
    
End Function

Function GetDirectory(path As String)
    GetDirectory = Left(path, InStrRev(path, "\"))
End Function

Sub CreateDirectories(path As String)

    Dim fso As Object
    Set fso = CreateObject("Scripting.FileSystemObject")

    If fso.FolderExists(path) Then
        Exit Sub
    End If

    CreateDirectories fso.GetParentFolderName(path)
    
    fso.CreateFolder path
    
End Sub