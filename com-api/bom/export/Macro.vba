Const BOM_TEMPLATE_NAME As String = "Full"
Const OUT_FILE_EXT As String = ".xlsx"
Const SUFFIX_PRP_NAME = "Revision"
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
    
    Dim outFilePath As String
    outFilePath = GetOutputFileName(swModel, confName)
        
    Dim bomTemplate As IBomTemplateCom
    Set bomTemplate = GetTemplateByName(bomModule, BOM_TEMPLATE_NAME)
    
    Dim bomItem As IBomItemCom
    Set bomItem = bomModule.BuildBom(swModel, confName)
        
    bomModule.Export bomItem, bomTemplate, outFilePath, INCLUDE_ROOT
    
End Sub

Function GetOutputFileName(model As SldWorks.ModelDoc2, confName As String) As String
    
    Dim outPath As String
    
    outPath = model.GetPathName
    
    If outPath <> "" Then
        Dim suffixPrpVal As String
        
        If SUFFIX_PRP_NAME <> "" Then
            suffixPrpVal = GetModelPropertyValue(model, confName, SUFFIX_PRP_NAME)
            
            If suffixPrpVal <> "" Then
                suffixPrpVal = " " & suffixPrpVal
            End If
            
        End If
        
        GetOutputFileName = GetDirectoryName(outPath) & GetFileNameWithoutExtension(outPath) & suffixPrpVal & OUT_FILE_EXT
    Else
        Err.Raise vbError, "", "Model is not saved"
    End If
    
End Function

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


Function GetModelPropertyValue(model As SldWorks.ModelDoc2, confName As String, prpName As String) As String
    
    Dim prpVal As String
    Dim swCustPrpMgr As SldWorks.CustomPropertyManager
    
    Set swCustPrpMgr = model.Extension.CustomPropertyManager(confName)
    prpVal = GetPropertyValue(swCustPrpMgr, prpName)
    
    If prpVal = "" Then
        Set swCustPrpMgr = model.Extension.CustomPropertyManager("")
        prpVal = GetPropertyValue(swCustPrpMgr, prpName)
    End If
    
    GetModelPropertyValue = prpVal
    
End Function

Function GetPropertyValue(custPrpMgr As SldWorks.CustomPropertyManager, prpName As String) As String
    
    Dim resVal As String
    custPrpMgr.Get2 prpName, "", resVal
    GetPropertyValue = resVal
    
End Function

Function GetDirectoryName(filePath As String) As String
    GetDirectoryName = Left(filePath, InStrRev(filePath, "\"))
End Function

Function GetFileNameWithoutExtension(filePath As String) As String
    GetFileNameWithoutExtension = Mid(filePath, InStrRev(filePath, "\") + 1, InStrRev(filePath, ".") - InStrRev(filePath, "\") - 1)
End Function