Dim swApp As SldWorks.SldWorks

Sub main()

    Set swApp = Application.SldWorks
    
    Dim swCadPlus As ICadPlusSwAddIn
    
    Dim swCadPlusFact As CadPlusSwAddInFactory
    Set swCadPlusFact = New CadPlusSwAddInFactory
    
    Set swCadPlus = swCadPlusFact.Create(swApp, True)
    
    Dim swDraw As SldWorks.DrawingDoc
    Set swDraw = swApp.ActiveDoc
    
    Dim swSheet As SldWorks.Sheet
    Set swSheet = swDraw.GetCurrentSheet
    
    Dim expr As String
    expr = "{ partNmb }" & vbLf & "{ prp [Title] [True] }" & vbLf & "{ prp [Description] [True] }"
    
    Dim swQrCodeElem As QrCodeElementCom
    
    Set swQrCodeElem = swCadPlus.QrCode.Insert(swDraw, swSheet, QrCodeDockCom_e_TopLeft, 0.02, 0.002, 0.002, expr, True)
    
End Sub