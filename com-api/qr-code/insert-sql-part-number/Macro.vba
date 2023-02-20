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
    
    Dim swView As SldWorks.View
    Set swView = swSheet.GetViews()(0)
    
    Dim fileName As String
    fileName = swView.ReferencedDocument.GetPathName
    
    fileName = Mid(fileName, InStrRev(fileName, "\") + 1, InStrRev(fileName, ".") - InStrRev(fileName, "\") - 1)
        
    Dim partNo As String
    partNo = GetPartNumberFromSql(fileName)
    
    Dim swQrCodeElem As QrCodeElementCom
    
    Set swQrCodeElem = swCadPlus.QrCode.Insert(swDraw, swSheet, QrCodeDockCom_e_BottomLeft, 0.05, 0.02, 0.01, partNo, True)
    
End Sub

Function GetPartNumberFromSql(fileName As String) As String

    Dim adoDbConnection As Object
    Set adoDbConnection = CreateObject("ADODB.Connection")
    
    Dim connStr As String
    
    connStr = "Provider=SQLOLEDB.1;Initial Catalog=data;Data Source=MSI;User Id=admin;Password=123;"
        
    adoDbConnection.Open connStr
    
    Dim recordSet As Object
    
    Set recordSet = adoDbConnection.execute("SELECT [PartIndex], [PartCategory] FROM dbo.PartNumbers WHERE [FileName] LIKE '" & fileName & "'")
    
    Dim index As Integer
    index = recordSet.Fields("PartIndex").Value
    Dim category As String
    category = recordSet.Fields("PartCategory").Value
    
    recordSet.Close
    adoDbConnection.Close
    
    Set recordSet = Nothing
    Set adoDbConnection = Nothing
    
    GetPartNumberFromSql = category & "-" & index
    
End Function