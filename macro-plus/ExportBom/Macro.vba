Const INCLUDE_ROOT As Boolean = True

Dim swApp As SldWorks.SldWorks

Sub main()
    
    Set swApp = Application.SldWorks

    Dim swCadPlus As ICadPlusSwAddIn
    
    Dim swCadPlusFact As CadPlusSwAddInFactory
    Set swCadPlusFact = New CadPlusSwAddInFactory
    
    Set swCadPlus = swCadPlusFact.Create(swApp, True)
    
    Dim bomModule As IBomModuleCom
    Set bomModule = swCadPlus.Bom
    
    Dim swModel As SldWorks.ModelDoc2
    Set swModel = swApp.ActiveDoc
    
    Dim confName As String
    
    confName = swModel.GetActiveConfiguration().name
    
    Dim operMgr As IMacroOperationManager
    Set operMgr = CreateObject("CadPlus.MacroOperationManager")

    Dim oper As IMacroOperation
    Set oper = operMgr.PopOperation(swApp)
    
    Dim bomTemplateName As String
    bomTemplateName = oper.Arguments(0).GetValue()
    
    Dim outFilePath(0) As String
    outFilePath(0) = oper.Arguments(1).GetValue()
    
    Dim vResFiles As Variant
    vResFiles = oper.SetResultFiles(outFilePath)
    
    Dim resFile As IMacroOperationResultFile
    Set resFile = vResFiles(0)
    
    Dim bomTemplate As IBomTemplateCom
    Set bomTemplate = GetTemplateByName(bomModule, bomTemplateName)
    
    Dim bomItem As IBomItemCom
    Set bomItem = bomModule.BuildBom(swModel, confName)
        
    bomModule.Export bomItem, bomTemplate, outFilePath(0), INCLUDE_ROOT
    
    resFile.Status = MacroOperationResultFileStatus_e_Succeeded
    
End Sub

Function GetTemplateByName(bomModule As IBomModuleCom, name As String) As IBomTemplateCom
    
    Dim vTemplates As Variant
    vTemplates = bomModule.GetTemplates
    
    Dim i As Integer
    
    For i = 0 To UBound(vTemplates)
        
        Dim bomTemplate As IBomTemplateCom
        Set bomTemplate = vTemplates(i)
        
        If LCase(bomTemplate.name) = LCase(name) Then
            Set GetTemplateByName = bomTemplate
            Exit Function
        End If
        
    Next
    
    Err.Raise vbError, "", "BOM template is not found"
    
End Function