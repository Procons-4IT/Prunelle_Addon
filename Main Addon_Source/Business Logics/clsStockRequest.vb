Public Class clsStockRequest
    Inherits clsBase
    Private oCFLEvent As SAPbouiCOM.IChooseFromListEvent
    Private oDBSrc_Line As SAPbouiCOM.DBDataSource
    Private oMatrix As SAPbouiCOM.Matrix
    Private oEditText As SAPbouiCOM.EditText
    Private oCombobox As SAPbouiCOM.ComboBox
    Private oEditTextColumn As SAPbouiCOM.EditTextColumn
    Private oGrid As SAPbouiCOM.Grid
    Private dtTemp As SAPbouiCOM.DataTable
    Private dtResult As SAPbouiCOM.DataTable
    Private oMode As SAPbouiCOM.BoFormMode
    Private oItem As SAPbobsCOM.Items
    Private oInvoice As SAPbobsCOM.Documents
    Private InvBase As DocumentType
    Private InvBaseDocNo As String
    Private InvForConsumedItems As Integer
    Private blnFlag As Boolean = False
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0
    End Sub
#Region "Validate Item barcode Value"
    Private Function Validatebarcode(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Dim strMessage As String = ""
        oMatrix = aForm.Items.Item("11").Specific
        If oMatrix.RowCount < 1 Then
            strMessage = "Document Lines are missing"
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        If oApplication.Utilities.getMatrixValues(oMatrix, "V_0", 1) = "" Then
            strMessage = "Atleast one line item is required"
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        oEditText = aForm.Items.Item("10").Specific
        If oEditText.String = "" Then
            strMessage = "Warehouse code is missing.."
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        Dim intQty As Double
        For intRow As Integer = 1 To oMatrix.RowCount
            If oApplication.Utilities.getMatrixValues(oMatrix, "V_0", intRow) <> "" Then
                intQty = Convert.ToDouble(oApplication.Utilities.getMatrixValues(oMatrix, "V_1", intRow))
                If intQty < 1 Then
                    strMessage = "Quantity should be greater than zero : Line no : " & intRow
                    oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If
            End If
        Next
        Return True
    End Function
#End Region

#Region "DataBind"
    Private Sub DataBind(ByVal aForm As SAPbouiCOM.Form)
        aForm.Items.Item("16").DisplayDesc = True
        aForm.Items.Item("16").Enabled = False
    End Sub
#End Region

#Region "Menu Event"
    Public Overrides Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            Select Case pVal.MenuUID
                Case mnu_FIND
                    If pVal.BeforeAction = False Then
                        If pVal.BeforeAction = False Then
                            oForm = oApplication.SBO_Application.Forms.ActiveForm()
                            oForm.Items.Item("4").Enabled = True
                        End If
                    End If
                Case mnu_FIRST, mnu_LAST, mnu_NEXT, mnu_PREVIOUS
                    If pVal.BeforeAction = False Then
                        oForm = oApplication.SBO_Application.Forms.ActiveForm()
                        oForm.Freeze(True)
                        oForm.Items.Item("4").Enabled = False
                        oForm.Items.Item("6").Enabled = False
                        oForm.Items.Item("8").Enabled = False
                        oForm.Items.Item("10").Enabled = False
                        oForm.Items.Item("11").Enabled = False
                        oForm.Items.Item("12").Enabled = False
                        oForm.Items.Item("13").Enabled = False
                        oForm.Freeze(False)
                    End If
                Case mnu_ADD
                    If pVal.BeforeAction = False Then
                        oForm = oApplication.SBO_Application.Forms.ActiveForm()
                        oForm.Freeze(True)
                        oMatrix = oForm.Items.Item("11").Specific
                        'oForm.Items.Item("4").Enabled = True
                        oEditText = oForm.Items.Item("4").Specific
                        oEditText.String = getMaxNumber()
                        'oForm.Items.Item("4").Enabled = False
                        oForm.Items.Item("6").Enabled = True
                        oForm.Items.Item("8").Enabled = True
                        oForm.Items.Item("10").Enabled = True
                        oForm.Items.Item("11").Enabled = True
                        oForm.Items.Item("12").Enabled = True
                        oForm.Items.Item("13").Enabled = True
                        AddEmptyRowtoMatrix(oMatrix)
                        oForm.Freeze(False)
                    End If
                Case mnu_StRequest
                    If pVal.BeforeAction = False Then
                        oForm = oApplication.Utilities.LoadForm(xml_StRequest, "frm_StRequest")
                        oForm = oApplication.SBO_Application.Forms.ActiveForm()
                        oForm.Freeze(True)
                        oForm.DataBrowser.BrowseBy = "4"
                        oForm.Items.Item("6").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        '       oForm.Items.Item("4").Enabled = False
                        oForm.Items.Item("6").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        oEditText = oForm.Items.Item("6").Specific
                        oEditText.String = "t"
                        oApplication.SBO_Application.SendKeys("{Tab}")
                        DataBind(oForm)
                        oMatrix = oForm.Items.Item("11").Specific
                        oMatrix.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single
                        oMatrix.AutoResizeColumns()
                        If oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            oForm.Items.Item("4").Enabled = True
                            oEditText = oForm.Items.Item("4").Specific
                            oEditText.String = getMaxNumber()
                            oForm.Items.Item("4").Enabled = False
                        End If
                        AddEmptyRowtoMatrix(oMatrix)
                        oForm.Freeze(False)
                    End If
                Case mnu_ADD
            End Select
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            oForm.Freeze(False)
        End Try
    End Sub
#End Region

    Private Sub AddEmptyRowtoMatrix(ByVal aMatrix As SAPbouiCOM.Matrix)
        oMatrix = aMatrix
        If oMatrix.RowCount <= 0 Then
            oMatrix.AddRow()
            oMatrix.Columns.Item("V_0").Cells.Item(oMatrix.RowCount).Specific.value = ""
            oMatrix.Columns.Item("V_2").Cells.Item(oMatrix.RowCount).Specific.value = ""
            oMatrix.Columns.Item("V_1").Cells.Item(oMatrix.RowCount).Specific.value = 1
            
            Exit Sub
        End If
        If oMatrix.Columns.Item("V_0").Cells.Item(oMatrix.RowCount).Specific.value <> "" Then
            oMatrix.AddRow()
            oMatrix.Columns.Item("V_0").Cells.Item(oMatrix.RowCount).Specific.value = ""
            oMatrix.Columns.Item("V_2").Cells.Item(oMatrix.RowCount).Specific.value = ""
            oMatrix.Columns.Item("V_1").Cells.Item(oMatrix.RowCount).Specific.value = 1
        End If
    End Sub

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If BusinessObjectInfo.BeforeAction = False And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) Then
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

#Region "Get Max Number"
    Private Function getMaxNumber() As Integer
        Dim oTempRec As SAPbobsCOM.Recordset
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("Select isnull(max(docEntry),0)+1 from [@DABT_STRHeader]")
        Return oTempRec.Fields.Item(0).Value
    End Function
#End Region
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormTypeEx = frm_StockRequest Then
                Select Case pVal.BeforeAction
                    Case True
                        If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "1" Then
                            oMode = pVal.FormMode
                            If oMode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then

                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                If Validatebarcode(oForm) = False Then
                                    BubbleEvent = False
                                    Exit Sub
                                End If
                            End If

                        End If
                    Case False
                        Select Case pVal.EventType

                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                            Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                Dim oCFLEvento As SAPbouiCOM.IChooseFromListEvent
                                Dim oCFL As SAPbouiCOM.ChooseFromList
                                Dim val1 As String
                                Dim oItm As SAPbobsCOM.Items
                                Dim sCHFL_ID, val As String
                                Dim intChoice As Integer
                                Try
                                    oCFLEvento = pVal
                                    sCHFL_ID = oCFLEvento.ChooseFromListUID
                                    oCFL = oForm.ChooseFromLists.Item(sCHFL_ID)
                                    If (oCFLEvento.BeforeAction = False) Then
                                        Dim oDataTable As SAPbouiCOM.DataTable
                                        oDataTable = oCFLEvento.SelectedObjects
                                        val = oDataTable.GetValue(0, 0)
                                        val1 = oDataTable.GetValue(1, 0)
                                        oForm.Freeze(True)
                                        If pVal.ItemUID = "11" And pVal.ColUID = "V_0" Then
                                            oMatrix = oForm.Items.Item("11").Specific
                                            oMatrix.Columns.Item("V_2").Editable = True
                                            oApplication.Utilities.SetMatrixValues(oMatrix, "V_2", pVal.Row, val1)
                                            oMatrix.Columns.Item("V_2").Editable = False
                                            oApplication.Utilities.SetMatrixValues(oMatrix, "V_1", pVal.Row, 1)
                                            Try
                                                oApplication.Utilities.SetMatrixValues(oMatrix, "V_0", pVal.Row, val)
                                            Catch ex As Exception

                                            End Try
                                            AddEmptyRowtoMatrix(oMatrix)
                                            oForm.Freeze(False)
                                        ElseIf pVal.ItemUID = "10" Then
                                            oEditText = oForm.Items.Item("10").Specific
                                            oEditText.Value = val
                                        End If
                                        oForm.Freeze(False)
                                    End If
                                Catch

                                End Try
                                oForm.Freeze(False)


                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                oMatrix = oForm.Items.Item("11").Specific
                                Select Case pVal.ItemUID
                                    Case "1"
                                        If oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Then
                                            oForm = oApplication.SBO_Application.Forms.ActiveForm()
                                            oForm.Items.Item("4").Enabled = False
                                            oForm.Items.Item("6").Enabled = False
                                            oForm.Items.Item("8").Enabled = False
                                            oForm.Items.Item("10").Enabled = False
                                            oForm.Items.Item("11").Enabled = False
                                            oForm.Items.Item("12").Enabled = False
                                            oForm.Items.Item("13").Enabled = False
                                        End If
                                    Case "12"
                                        AddEmptyRowtoMatrix(oMatrix)
                                    Case "13"
                                        For intRow As Integer = 1 To oMatrix.RowCount
                                            If oMatrix.IsRowSelected(intRow) Then
                                                oMatrix.DeleteRow(intRow)
                                                Exit Sub
                                            End If
                                        Next

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
