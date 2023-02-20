Dim swApp As SldWorks.SldWorks

Sub main()

    Set swApp = Application.SldWorks
    
    Dim swCadPlus As ICadPlusSwAddIn
    
    Dim swCadPlusFact As CadPlusSwAddInFactory
    Set swCadPlusFact = New CadPlusSwAddInFactory
    
    Set swCadPlus = swCadPlusFact.Create(swApp, True)
        
    Dim swPart As SldWorks.PartDoc
    Set swPart = swApp.ActiveDoc
    
    Dim vConfNames As Variant
    vConfNames = swPart.GetConfigurationNames
    
    Dim i As Integer
    
    Dim activeConfName As String
    activeConfName = swPart.ConfigurationManager.ActiveConfiguration.Name
    
    For i = 0 To UBound(vConfNames)
        
        Dim swConf As SldWorks.Configuration
        Set swConf = swPart.GetConfigurationByName(vConfNames(i))
        
        If swConf.GetParent() Is Nothing Then
            
            swPart.ShowConfiguration2 swConf.Name
            
            Dim vBodies As Variant
            vBodies = swPart.GetBodies2(swBodyType_e.swSolidBody, True)
            
            Dim j As Integer
            
            Dim expData() As FlatPatternExportDataCom
                        
            Dim outDir As String
            outDir = swPart.GetPathName
            
            If outDir = "" Then
                Err.Raise vbError, "", "File is not saved on disc"
            End If
            
            outDir = Left(outDir, InStrRev(outDir, "\")) & "CAD++\" & swConf.Name & "\"
            
            For j = 0 To UBound(vBodies)
                
                Dim swBody As SldWorks.Body2
                Set swBody = vBodies(j)
                
                If swBody.IsSheetMetal() Then
                
                    AppendExportData swBody, expData, outDir, ".pdf"
                    AppendExportData swBody, expData, outDir, ".dxf"
                    
                End If
                
            Next
            
            Dim vRes As Variant
            vRes = swCadPlus.FlatPatternExport.BatchExportFlatPatterns(swPart, expData)
            
            Dim k As Integer
            
            For k = 0 To UBound(vRes)
                Dim res As FlatPatternExportResult
                Set res = vRes(k)
                Debug.Print expData(k).OutFilePath & " - " & res.Succeeded
            Next
            
            Erase expData
        
        End If
    
    Next
    
    swPart.ShowConfiguration2 activeConfName
    
End Sub

Sub AppendExportData(body As SldWorks.Body2, expData() As FlatPatternExportDataCom, outDir As String, extension As String)
    
    If (Not expData) = -1 Then
        ReDim expData(0)
    Else
        ReDim Preserve expData(UBound(expData) + 1)
    End If
    
    Set expData(UBound(expData)) = New FlatPatternExportDataCom
    
    Set expData(UBound(expData)).body = body
    expData(UBound(expData)).NoteText = "CAD++"
    expData(UBound(expData)).Options = FlatPatternOptionsCom_e.FlatPatternOptionsCom_e_BendLines Or FlatPatternOptionsCom_e.FlatPatternOptionsCom_e_BendNotes Or FlatPatternOptionsCom_e.FlatPatternOptionsCom_e_Sketches
    expData(UBound(expData)).OutFilePath = outDir & body.Name & extension
    
End Sub