Public Class clsUpdateJournal
    Inherits clsBase
    Private oCFLEvent As SAPbouiCOM.IChooseFromListEvent
    Private oDBSrc_Line As SAPbouiCOM.DBDataSource
    Private oMatrix As SAPbouiCOM.Matrix
    Private oEditText As SAPbouiCOM.EditText
    Private oCombobox As SAPbouiCOM.ComboBox
    Private oCheckBox As SAPbouiCOM.CheckBox
    Private oEditTextColumn As SAPbouiCOM.EditTextColumn
    Private oGrid As SAPbouiCOM.Grid
    Private dtTemp, dtTemp1 As SAPbouiCOM.DataTable
    Private dtResult As SAPbouiCOM.DataTable
    Private oMode As SAPbouiCOM.BoFormMode
    Private oItem As SAPbobsCOM.Items
    Private oInvoice As SAPbobsCOM.Documents
    Private oComboColumn As SAPbouiCOM.ComboBoxColumn
    Private InvBase As DocumentType
    Private InvBaseDocNo, strQuery As String
    Private InvForConsumedItems As Integer
    Private blnFlag As Boolean = False
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0

    End Sub
    Private Sub LoadForm()
        Try
            oForm = oApplication.Utilities.LoadForm(xml_UpdateJournal, frm_UpdateJournal)
            oForm = oApplication.SBO_Application.Forms.ActiveForm()
            oForm.Freeze(True)
            oForm.Freeze(False)
        Catch ex As Exception
            oForm.Freeze(False)
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
    Private Function validation(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Try
            Dim strStartDate, strEndDate, strNumber As String
            Dim dtStartDate, dtEndDate As Date
            Dim ORec, oRec1 As SAPbobsCOM.Recordset
            If oApplication.SBO_Application.MessageBox("Do you want to Reset Journal Document Numbers?", , "Continue", "Cancel") = 2 Then
                Return False
            End If
            If oApplication.Utilities.getEdittextvalue(aForm, "4") = "" Then
                oApplication.Utilities.Message("Journal From Date missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            Else
                dtStartDate = oApplication.Utilities.GetDateTimeValue(oApplication.Utilities.getEdittextvalue(aForm, "4"))
            End If
            If oApplication.Utilities.getEdittextvalue(aForm, "6") = "" Then
                oApplication.Utilities.Message("Journal Till Date missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            Else
                dtEndDate = oApplication.Utilities.GetDateTimeValue(oApplication.Utilities.getEdittextvalue(aForm, "6"))
            End If
            If oApplication.Utilities.getEdittextvalue(aForm, "8") = "" Then
                oApplication.Utilities.Message("Journal Starting Number missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            Else
                strNumber = oApplication.Utilities.getEdittextvalue(aForm, "8")
            End If
            ORec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRec1 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            ORec.DoQuery("Select * from OJDT where RefDate >='" & dtStartDate.ToString("yyyy-MM-dd") & "' and refdate<='" & dtEndDate.ToString("yyyy-MM-dd") & "' order by RefDate,TransID")
            Dim dblNumber As Double = oApplication.Utilities.getDocumentQuantity(strNumber)
            Dim ostatic As SAPbouiCOM.StaticText
            ostatic = aForm.Items.Item("10").Specific
            Dim inCount As Integer = ORec.RecordCount
            If inCount > 0 Then
                ORec.MoveFirst()
                For intRow As Integer = 0 To ORec.RecordCount - 1
                    ostatic.Caption = "Processing " & intRow & " of " & inCount
                    oRec1.DoQuery("Update OJDT set U_DocNum='" & dblNumber.ToString & "' where TransID=" & ORec.Fields.Item("TransID").Value)
                    dblNumber = dblNumber + 1
                    ORec.MoveNext()
                Next
            Else
                oApplication.Utilities.Message("No Record found", SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                Return False
            End If
            ostatic.Caption = ""
            Return True
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End Try
    End Function
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormTypeEx = frm_UpdateJournal Then
                Select Case pVal.BeforeAction
                    Case True
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                If pVal.ItemUID = "3" Then
                                    If validation(oForm) = False Then
                                        BubbleEvent = False
                                        Exit Sub
                                    End If
                                End If

                        End Select

                    Case False
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN

                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                Select Case pVal.ItemUID
                                    Case "3"
                                        oForm.Close()
                                End Select

                        End Select
                End Select
            End If


        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region

#Region "Menu Event"
    Public Overrides Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            Select Case pVal.MenuUID
                Case mnu_UpdateJournal
                    LoadForm()
                Case mnu_FIRST, mnu_LAST, mnu_NEXT, mnu_PREVIOUS
            End Select
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            oForm.Freeze(False)
        End Try
    End Sub
#End Region

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If BusinessObjectInfo.BeforeAction = False And BusinessObjectInfo.ActionSuccess = True And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
End Class
