Public Class clsDocuments
    Inherits clsBase
    Private oCFLEvent As SAPbouiCOM.IChooseFromListEvent
    Private oDBSrc_Line As SAPbouiCOM.DBDataSource
    Private oMatrix As SAPbouiCOM.Matrix
    Private oEditTextColumn As SAPbouiCOM.EditTextColumn
    Private oGrid As SAPbouiCOM.Grid
    Private dtTemp As SAPbouiCOM.DataTable
    Private dtResult As SAPbouiCOM.DataTable
    Private oMode As SAPbouiCOM.BoFormMode
    Private oItem As SAPbobsCOM.Items
    Private oInvoice As SAPbobsCOM.Documents
    Private InvBase As DocumentType
    Private InvBaseDocNo, strBarcode, strQty As String
    Private InvForConsumedItems As Integer
    Private blnFlag As Boolean = False
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0
    End Sub

#Region "Validate Item barcode Value"
    Private Function Validatebarcode(ByVal aItemCode As String, ByVal aBarCode As String) As Boolean
        Dim oTempRec As SAPbobsCOM.Recordset
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("Select * from OITM where codebars='" & aBarCode & "' and ItemCode <>'" & aItemCode & "'")
        If oTempRec.RecordCount > 0 Then
            Return True
        Else
            Return False
        End If
        Return False
    End Function
#End Region

#Region "Check Rounding off Values"
    Private Sub RoundingOff(ByVal aForm As SAPbouiCOM.Form)
        Dim dblDocTotal, dblRound, difference As Double
        Dim intDecimal As Integer
        Dim strDocTotal As String
        Dim strCurrency As String
        Dim oCombobox As SAPbouiCOM.ComboBox
        strDocTotal = oApplication.Utilities.getEdittextvalue(aForm, "33")
        'strCurrency = oApplication.Utilities.getEdittextvalue(aForm, "63")
        oCombobox = aForm.Items.Item("63").Specific
        strCurrency = oCombobox.Selected.Value
        If strCurrency <> "LBP" Then
            Exit Sub
        End If
        Dim intLength As Integer
        intLength = strDocTotal.Length
        strDocTotal = strDocTotal.Replace(strCurrency, "")
        ' strDocTotal = strDocTotal.Substring(3, intLength - 3)
        If strDocTotal.Length > 0 Then
            dblDocTotal = Convert.ToDouble(strDocTotal)
            intDecimal = dblDocTotal Mod 250
            If intDecimal > 150 Then
                dblRound = dblDocTotal - intDecimal + 250
            Else
                dblRound = dblDocTotal - intDecimal
            End If
            difference = dblRound - dblDocTotal
        End If
        Dim oCheckbox As SAPbouiCOM.CheckBox
        oCheckbox = aForm.Items.Item("105").Specific
        If oCheckbox.Checked = True Then

        Else
            oCheckbox.Checked = True
        End If
        oApplication.Utilities.setEdittextvalue(aForm, "103", difference.ToString)

    End Sub
#End Region

#Region "Get Qty"
    Private Sub GetQty(ByVal aMatrix As SAPbouiCOM.Matrix, ByVal intRow As Integer)
        Dim strinteger, strDecimal As String
        Dim dblQty As Double
        strBarcode = aMatrix.Columns.Item("4").Cells.Item(intRow).Specific.value
        If strBarcode.Length > 11 Then
            strQty = strBarcode.Substring(7, 6)
            strinteger = strQty.Substring(0, 2)
            strDecimal = strQty.Substring(2, 3)
            If CompanyDecimalSeprator <> "." Then
                strQty = strinteger & CompanyDecimalSeprator & strDecimal
                dblQty = Convert.ToDouble(strQty)
            Else
                strQty = strinteger & CompanyDecimalSeprator & strDecimal
                dblQty = Convert.ToDouble(strQty)
            End If
            aMatrix.Columns.Item("11").Cells.Item(intRow).Specific.string = strQty
        End If
    End Sub
#End Region

#Region "Menu Event"
    Public Overrides Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Select Case pVal.MenuUID
            Case mnu_FIRST, mnu_LAST, mnu_NEXT, mnu_PREVIOUS
            Case mnu_BatchOrders
                If pVal.BeforeAction = False Then
                End If
            Case mnu_ADD
        End Select
    End Sub
#End Region

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)

    End Sub
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormType = frm_SalesInvoice Or pVal.FormType = frm_CreditNotes Or pVal.FormType = frm_InvoicePayment Then
                Select Case pVal.BeforeAction
                    Case True
                        If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "1" Then
                            blnFlag = False
                            oMode = pVal.FormMode
                            oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            If oMode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                                RoundingOff(oForm)
                            End If
                        End If
                    Case False
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)

                            Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                If pVal.ItemUID = "38" And pVal.ColUID = "4" And pVal.CharPressed = 9 Then
                                    oMatrix = oForm.Items.Item("38").Specific
                                    strBarcode = oMatrix.Columns.Item("4").Cells.Item(pVal.Row).Specific.value
                                    Try
                                        oForm.Freeze(True)
                                        GetQty(oMatrix, pVal.Row)
                                        oForm.Freeze(False)
                                    Catch ex As Exception
                                        oForm.Freeze(False)
                                    End Try
                                End If

                            Case SAPbouiCOM.BoEventTypes.et_LOST_FOCUS
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)

                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                Select Case pVal.ItemUID
                                    Case "1"

                                End Select

                            Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                                oCFLEvent = pVal
                        End Select
                End Select
            End If


        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region


End Class
