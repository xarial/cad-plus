# Insert/Update Cut-List QR Codes

This VBA macro allows to insert and update QR code and supports drawing views which reference single cut-list body 

If the first drawing view in the sheet is a single body of the cut-list, **{ cutListName }** placeholder can be used to in the QR code expression to differentiate QR code for different cut-list views.

~~~ vb
Const QR_CODE_EXPRESSION As String = "{ partNmb }"
Const QR_CODE_CUT_LIST_EXPRESSION As String = "{ partNmb }-{ cutListName }"
~~~

Configure the expression in the **QR_CODE_CUT_LIST_EXPRESSION** constant variable to specify the expression for the QR code of the single cut-list body, otherwise the **QR_CODE_EXPRESSION** value is used.

QR code parameters can be specified by modifying the following constants

~~~ vb
Const QR_CODE_DOCK As Integer = QrCodeDockCom_e_BottomLeft
Const QR_CODE_SIZE As Double = 0.025
Const QR_CODE_OFFSET_X As Double = 0.02
Const QR_CODE_OFFSET_Y As Double = 0.005
~~~

This macro will insert new QR code if there is no QR codes in the current sheet, otherwise QR code will be updated

> If **{ cutListName }** placehold is used and cut-list name is changed, it is required to rerun the macro to update the code, updating the QR code via conext menu will not update cut-list