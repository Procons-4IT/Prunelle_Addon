Public Class clsGoodIssues
    Inherits clsBase
    Private oCFLEvent As SAPbouiCOM.IChooseFromListEvent
    Private oDBSrc_Line As SAPbouiCOM.DBDataSource
    Private oMatrix As SAPbouiCOM.Matrix
    Private oEditText As SAPbouiCOM.EditText
    Private oEditTextColumn As SAPbouiCOM.EditTextColumn
    Private oGrid As SAPbouiCOM.Grid
    Private dtTemp As SAPbouiCOM.DataTable
    Private dtResult As SAPbouiCOM.DataTable
    Private oMode As SAPbouiCOM.BoFormMode
    Private oItem As SAPbobsCOM.Items
    Private oInvoice As SAPbobsCOM.Documents
    Private InvBase As DocumentType
    Private InvBaseDocNo, strGoodsIssueDocnum As String
    Private InvForConsumedItems, intGoodsIssueDocEntry As Integer
    Private blnFlag As Boolean = False
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0
    End Sub
#Region "Validate Item barcode Value"
    Private Function Validatebarcode(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Dim strMessage As String = ""
        Dim strWhs As String
        oMatrix = aForm.Items.Item("11").Specific
        If oMatrix.RowCount < 1 Then
            strMessage = "Document Lines are missing"
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        If oApplication.Utilities.getMatrixValues(oMatrix, "IssueQty", 1) = "" Then
            strMessage = "Atleast one line item is required"
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End If
        oEditText = aForm.Items.Item("10").Specific
        If oEditText.String = "" Then
            strMessage = "Warehouse code is missing.."
            oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        Else
            strWhs = oEditText.String
        End If
        Dim intQty, intIssueqty As Double
        For intRow As Integer = 1 To oMatrix.RowCount
            intIssueqty = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oMatrix, "IssueQty", intRow))
            '  If intIssueqty <> 0 Then
            'intIssueqty = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oMatrix, "IssueQty", intRow))
            intQty = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oMatrix, "ReqQty", intRow))
            If intIssueqty < 1 Then
                strMessage = "Quantity should be greater than zero : Line no : " & intRow
                oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            ElseIf CheckOnHandQty(oApplication.Utilities.getMatrixValues(oMatrix, "ItemCode", intRow), strWhs, intIssueqty) = False Then
                strMessage = "Issued Quantity falls under Negative inventory. Line No :" & intRow
                oApplication.Utilities.Message(strMessage, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            ElseIf intIssueqty <> intQty Then
                strMessage = "Issue quantity does not match with required quanity in line no : " & intRow & ". Do you want to continue ?"
                If oApplication.SBO_Application.MessageBox(strMessage, , "Continue", "Cancel") = 2 Then
                    Return False
                Else
                    Return True
                End If
           
            End If
            'End If
        Next
        Return True
    End Function
#End Region

#Region "Get DocEntry"

#End Region


#Region "Get Max Number"
    Private Function getMaxNumber() As Integer
        Dim oTempRec As SAPbobsCOM.Recordset
        oTempRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTempRec.DoQuery("Select isnull(max(docEntry),0)+1 from [@DABT_GIHeader]")
        Return oTempRec.Fields.Item(0).Value
    End Function
#End Region

    Private Sub AddEmptyRowtoMatrix(ByVal aMatrix As SAPbouiCOM.Matrix)
        oMatrix = aMatrix
        Dim oRecSet As SAPbobsCOM.Recordset
        oRecSet = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRecSet.DoQuery("Select U_ItemCode,U_ItemName,U_ReqQty,U_TransWhs,Convert(nvarchar(10),U_Transdate,101),code from [@DABT_STImport] where name not like '%N'")
        For intRow As Integer = 1 To oRecSet.RecordCount
            oMatrix.AddRow()
            oApplication.Utilities.SetMatrixValues(oMatrix, "ItemCode", intRow, oRecSet.Fields.Item("U_ItemCode").Value)
            oApplication.Utilities.SetMatrixValues(oMatrix, "ItemName", intRow, oRecSet.Fields.Item("U_ItemName").Value)
            oApplication.Utilities.SetMatrixValues(oMatrix, "ReqWhs", intRow, oRecSet.Fields.Item("U_TransWhs").Value)
            oApplication.Utilities.SetMatrixValues(oMatrix, "ReqQty", intRow, oRecSet.Fields.Item("U_ReqQty").Value)
            oApplication.Utilities.SetMatrixValues(oMatrix, "IssueQty", intRow, "1")
            oApplication.Utilities.SetMatrixValues(oMatrix, "RefNo", intRow, oRecSet.Fields.Item("Code").Value)
            oRecSet.MoveNext()
        Next
    End Sub

#Region "Check OnHand Qty"
    Private Function CheckOnHandQty(ByVal aItemCode As String, ByVal aWhsCode As String, ByVal dblQuantity As Double) As Boolean
        Dim oRecSet As SAPbobsCOM.Recordset
        Dim dblQAty As Double
        oRecSet = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRecSet.DoQuery("Select isnull(OnHand,0) from OITW where Itemcode='" & aItemCode & "' and WhsCode='" & aWhsCode & "'")
        dblQAty = oRecSet.Fields.Item(0).Value
        If dblQAty < dblQuantity Then
            Return False
        Else
            Return True
        End If


    End Function
#End Region

#Region "Create Goods Issue Document"
    Private Function CreateGoodsIssue(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Dim oGoodsIssue As SAPbobsCOM.Documents
        Dim strItemcode As String
        Dim dblQty As Double
        Dim intCount As Integer
        oGoodsIssue = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit)
        oMatrix = aForm.Items.Item("11").Specific
        intCount = 0
        For intRow As Integer = 1 To oMatrix.RowCount
            strItemcode = oApplication.Utilities.getMatrixValues(oMatrix, "ItemCode", intRow)
            dblQty = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oMatrix, "IssueQty", intRow))
            If dblQty > 0 Then
                If intCount > 0 Then
                    oGoodsIssue.Lines.Add()
                    oGoodsIssue.Lines.SetCurrentLine(intCount)
                End If
                oGoodsIssue.Lines.ItemCode = strItemcode
                oGoodsIssue.Lines.Quantity = dblQty
                intCount = intCount + 1
            End If
        Next
        If intCount > 0 Then
            oGoodsIssue.DocDate = Now.Date
            If oGoodsIssue.Add <> 0 Then
                oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            Else
                oApplication.Company.GetNewObjectCode(strGoodsIssueDocnum)
                oEditText = aForm.Items.Item("22").Specific
                oEditText.String = Convert.ToInt32(strGoodsIssueDocnum)
                oGoodsIssue = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit)
                If oGoodsIssue.GetByKey(Convert.ToInt32(strGoodsIssueDocnum)) Then
                    oEditText = aForm.Items.Item("19").Specific
                    oEditText.String = oGoodsIssue.DocNum
                End If
                oApplication.Utilities.Message("Goods Issue Document created successfully", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
                Return True
            End If
        End If
    End Function
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
                        'oForm.Items.Item("8").Enabled = False
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
                        ' oForm.Items.Item("8").Enabled = True
                        oForm.Items.Item("10").Enabled = True
                        oForm.Items.Item("11").Enabled = True
                        oForm.Items.Item("12").Enabled = True
                        oForm.Items.Item("13").Enabled = True
                        AddEmptyRowtoMatrix(oMatrix)
                        oForm.Freeze(False)
                    End If
                Case mnu_GoodsIssue
                    If pVal.BeforeAction = False Then
                        oForm = oApplication.Utilities.LoadForm(xml_GoodsIssue, "frm_GoodsIssue")
                        oForm = oApplication.SBO_Application.Forms.ActiveForm()
                        oForm.Freeze(True)
                        oForm.DataBrowser.BrowseBy = "4"
                        oForm.Items.Item("6").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        'oForm.Items.Item("4").Enabled = False
                        oForm.Items.Item("6").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                        oEditText = oForm.Items.Item("6").Specific
                        oEditText.String = "t"
                        oApplication.SBO_Application.SendKeys("{Tab}")
                        oMatrix = oForm.Items.Item("11").Specific
                        oForm.Items.Item("16").DisplayDesc = True
                        oMatrix.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single
                        If oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                            '    oForm.Items.Item("4").Enabled = True
                            oEditText = oForm.Items.Item("4").Specific
                            oEditText.String = getMaxNumber()
                            '   oForm.Items.Item("4").Enabled = False
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

    Public Sub FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean)
        Try
            If BusinessObjectInfo.BeforeAction = False And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                Dim strDocNum As String
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
                oEditText = oForm.Items.Item("4").Specific
                strDocNum = oEditText.String
                updateSTImport(strDocNum)
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub


#Region "Update STImport Table"
    Private Sub updateSTImport(ByVal strDocNum As String)
        Dim oTemprec, oRS As SAPbobsCOM.Recordset
        oTemprec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRS = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oTemprec.DoQuery("Select * from [@DABT_GILines] where docentry=" & Convert.ToInt32(strDocNum))
        For intRow As Integer = 0 To oTemprec.RecordCount - 1

            oRS.DoQuery("Update [@DABT_StImport] set Name=name +'N' where code='" & oTemprec.Fields.Item("U_RefNo").Value & "'")
            oTemprec.MoveNext()

        Next

    End Sub
#End Region

#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormTypeEx = frm_GoodsIssue Then
                Select Case pVal.BeforeAction
                    Case True
                        If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "1" Then
                            oMode = pVal.FormMode
                            If oMode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                If Validatebarcode(oForm) = False Then
                                    BubbleEvent = False
                                    Exit Sub
                                Else
                                    If CreateGoodsIssue(oForm) = False Then
                                        BubbleEvent = False
                                        Exit Sub
                                    End If

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
                                            oForm.Items.Item("10").Enabled = False
                                            oForm.Items.Item("11").Enabled = False
                                            oForm.Items.Item("12").Enabled = False
                                            oForm.Items.Item("13").Enabled = False
                                        ElseIf oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Then
                                            '    CreateGoodsIssue(oForm)
                                        End If
                                    Case "12"
                                        ' AddEmptyRowtoMatrix(oMatrix)
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
