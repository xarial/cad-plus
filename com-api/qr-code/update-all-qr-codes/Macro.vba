Dim swApp As SldWorks.SldWorks

Sub main()

    Set swApp = Application.SldWorks
    
    Dim swCadPlus As ICadPlusSwAddIn
    
    Dim swCadPlusFact As CadPlusSwAddInFactory
    Set swCadPlusFact = New CadPlusSwAddInFactory
    
    Set swCadPlus = swCadPlusFact.Create(swApp, True)
    
    Dim swDraw As SldWorks.DrawingDoc
    Set swDraw = swApp.ActiveDoc

    Dim vQrCodes As Variant
    
    vQrCodes = swCadPlus.QrCode.IterateQrCodes(swDraw)
    
    Dim i As Integer
    
    For i = 0 To UBound(vQrCodes)
        Dim swQrCode As QrCodeElementCom
        Set swQrCode = vQrCodes(i)
        swQrCode.Update True
    Next
    
End Sub