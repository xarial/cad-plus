Dim swApp As SldWorks.SldWorks
Dim swModel As SldWorks.ModelDoc2

Sub main()
    
    Set swApp = Application.SldWorks

    Dim operMgr As IMacroOperationManager
    Set operMgr = CreateObject("CadPlus.MacroOperationManager")

    Dim oper As IMacroOperation
    Set oper = operMgr.PopOperation(swApp)

    Set swModel = swApp.ActiveDoc

    Dim i As Integer
    
    For i = 0 To UBound(oper.arguments)
    
        Dim arg As IMacroArgument
        Set arg = oper.arguments(i)
        
        Dim outFilePath As String
        outFilePath = arg.GetValue()
        
        Dim errs As Long
        Dim warns As Long
        
        If False = swModel.Extension.SaveAs(outFilePath, swSaveAsVersion_e.swSaveAsCurrentVersion, swSaveAsOptions_e.swSaveAsOptions_Silent, Nothing, errs, warns) Then
            Err.Raise vbError, "", "Failed to save file. Error code: " & errs
        End If
        
    Next

End Sub