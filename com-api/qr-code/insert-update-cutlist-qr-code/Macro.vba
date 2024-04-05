Const QR_CODE_EXPRESSION As String = "{ partNmb }"
Const QR_CODE_CUT_LIST_EXPRESSION As String = "{ partNmb }-{ cutListName }"

Const QR_CODE_DOCK As Integer = QrCodeDockCom_e_BottomLeft
Const QR_CODE_SIZE As Double = 0.025
Const QR_CODE_OFFSET_X As Double = 0.02
Const QR_CODE_OFFSET_Y As Double = 0.005

Const CUT_LIST_NAME_PLACEHOLDER As String = "{ cutListName }"

Dim swApp As SldWorks.SldWorks

Sub main()

    Set swApp = Application.SldWorks
    
    Dim swCadPlus As ICadPlusSwAddIn
    
    Dim swCadPlusFact As CadPlusSwAddInFactory
    Set swCadPlusFact = New CadPlusSwAddInFactory
    
    Set swCadPlus = swCadPlusFact.Create(swApp, True)
    
    Dim swDraw As SldWorks.DrawingDoc
    Set swDraw = swApp.ActiveDoc
    
    Dim swSheet As SldWorks.sheet
    Set swSheet = swDraw.GetCurrentSheet
    
    Dim expr As String
    
    Dim cutListName As String
    
    If TryGetSingleCutListName(swSheet, cutListName) Then
        expr = Replace(QR_CODE_CUT_LIST_EXPRESSION, CUT_LIST_NAME_PLACEHOLDER, cutListName)
    Else
        expr = QR_CODE_EXPRESSION
    End If
        
    Dim swQrCodeElem As QrCodeElementCom
    
    Set swQrCodeElem = TryGetQrCode(swDraw, swSheet, swCadPlus)
    
    If Not swQrCodeElem Is Nothing Then
        swQrCodeElem.Edit swQrCodeElem.Dock, swQrCodeElem.Size, swQrCodeElem.OffsetX, swQrCodeElem.OffsetY, expr, True
    Else
        Set swQrCodeElem = swCadPlus.QrCode.Insert(swDraw, swSheet, QR_CODE_DOCK, QR_CODE_SIZE, QR_CODE_OFFSET_X, QR_CODE_OFFSET_Y, expr, True)
    End If
    
End Sub

Function TryGetQrCode(draw As SldWorks.DrawingDoc, sheet As SldWorks.sheet, cadPlus As ICadPlusSwAddIn) As QrCodeElementCom
    
    Dim swSheetView As SldWorks.View
    Set swSheetView = GetSheetView(draw, sheet)
    
    Dim swSheetSketch As SldWorks.Sketch
    Set swSheetSketch = swSheetView.GetSketch()
    
    Dim vSkPicts As Variant
    vSkPicts = swSheetSketch.GetSketchPictures
    
    If Not IsEmpty(vSkPicts) Then
    
        Dim i As Integer
        
        For i = 0 To UBound(vSkPicts)
            
            Dim swSkPict As SldWorks.SketchPicture
            Set swSkPict = vSkPicts(i)
            
            On Error Resume Next
            
            Dim swQrCodeElem As QrCodeElementCom
            Set swQrCodeElem = cadPlus.QrCode.GetQrCode(draw, swSkPict)
            
            If Not swQrCodeElem Is Nothing Then
                Set TryGetQrCode = swQrCodeElem
                Exit Function
            End If
            
        Next
    
    End If
    
End Function

Function TryGetSingleCutListName(sheet As SldWorks.sheet, ByRef cutListName As String) As Boolean
    
    Dim swView As SldWorks.View
    
    Set swView = sheet.GetViews()(0)
    
    If swView.GetBodiesCount() = 1 Then
    
        Dim swRefDoc As SldWorks.ModelDoc2
        
        Set swRefDoc = swView.ReferencedDocument
        
        Dim swBody As SldWorks.Body2
        Set swBody = swView.Bodies(0)
        
        Dim swCutList As SldWorks.Feature
        
        Set swCutList = GetCutListFromBody(swRefDoc, swBody)
        
        If Not swCutList Is Nothing Then
            TryGetSingleCutListName = True
            cutListName = swCutList.Name
        Else
            TryGetSingleCutListName = False
            cutListName = ""
        End If
    
    Else
        TryGetSingleCutListName = False
        cutListName = ""
    End If
    
End Function

Function GetCutListFromBody(model As SldWorks.ModelDoc2, body As SldWorks.Body2) As SldWorks.Feature
    
    Dim swFeat As SldWorks.Feature
    Dim swBodyFolder As SldWorks.BodyFolder
    
    Set swFeat = model.FirstFeature
    
    Do While Not swFeat Is Nothing
        
        If swFeat.GetTypeName2 = "CutListFolder" Then
            
            Set swBodyFolder = swFeat.GetSpecificFeature2
            
            Dim vBodies As Variant
            
            vBodies = swBodyFolder.GetBodies
            
            Dim i As Integer
            
            If Not IsEmpty(vBodies) Then
                For i = 0 To UBound(vBodies)
                    
                    Dim swCutListBody As SldWorks.Body2
                    Set swCutListBody = vBodies(i)
                    
                    If LCase(swCutListBody.Name) = LCase(body.Name) Then
                        Set GetCutListFromBody = swFeat
                        Exit Function
                    End If
                    
                Next
            End If
            
        End If
        
        Set swFeat = swFeat.GetNextFeature
        
    Loop

End Function

Function GetSheetView(draw As SldWorks.DrawingDoc, sheet As SldWorks.sheet) As SldWorks.View
    
    Dim vSheets As Variant
    vSheets = draw.GetViews
    
    Dim i As Integer
    
    For i = 0 To UBound(vSheets)
        
        Dim swSheetView As SldWorks.View
        Set swSheetView = vSheets(i)(0)
        
        If LCase(swSheetView.Name) = LCase(sheet.GetName()) Then
            Set GetSheetView = swSheetView
            Exit Function
        End If
        
    Next
    
End Function