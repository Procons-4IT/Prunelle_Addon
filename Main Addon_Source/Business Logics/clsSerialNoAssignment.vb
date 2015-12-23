Public Class clsSerialNoAssignment
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
    Private InvBaseDocNo As String
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
        Try
            If BusinessObjectInfo.BeforeAction = False And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE) Then
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
                Dim bisObj As SAPbouiCOM.BusinessObject = Form.BusinessObject
                Dim uid As String = bisObj.Key
                Dim oItem As SAPbobsCOM.Items
                Dim oBP As SAPbobsCOM.BusinessPartners
                Select Case BusinessObjectInfo.FormTypeEx
                    Case frm_ItemMaster
                        If blnFlag = True Then
                            Exit Sub
                        End If
                        oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        If oItem.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oItem.UserFields.Fields.Item("U_Action").Value = "U"
                            oItem.Update()
                        End If
                    Case frm_BPMaster
                        oBP = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners)
                        If oBP.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oBP.UserFields.Fields.Item("U_Action").Value = "U"
                            oBP.Update()
                        End If
                End Select
            ElseIf BusinessObjectInfo.BeforeAction = False And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                Dim strDocNum, strDocType As String
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
                Dim bisObj As SAPbouiCOM.BusinessObject = Form.BusinessObject
                Dim uid As String = bisObj.Key
                Dim oItem As SAPbobsCOM.Items
                Dim oBP As SAPbobsCOM.BusinessPartners
                Dim oSt As SAPbobsCOM.StockTransfer
                Dim BP1 As SAPbobsCOM.Documents '= oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes)
                Select Case BusinessObjectInfo.FormTypeEx
                    Case frm_ItemMaster
                        If blnFlag = True Then
                            Exit Sub
                        End If
                        oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                        If oItem.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oItem.UserFields.Fields.Item("U_Action").Value = "A"
                            oItem.Update()
                        End If
                    Case frm_BPMaster
                        oBP = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oBusinessPartners)
                        If oBP.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oBP.UserFields.Fields.Item("U_Action").Value = "A"
                            oBP.Update()
                        End If
                    Case frm_StockTransfer
                        oSt = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                        If oSt.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oSt.UserFields.Fields.Item("U_Export").Value = "N"
                            oSt.Update()
                        End If
                End Select
            ElseIf BusinessObjectInfo.BeforeAction = True And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                Dim oItem As SAPbobsCOM.Items
                If BusinessObjectInfo.FormTypeEx = frm_ItemMaster Then
                    oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                    If blnFlag = True Then
                        Exit Sub
                    End If
                    If oItem.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                        If Validatebarcode(oItem.ItemCode, oItem.BarCode) = True Then
                            oApplication.Utilities.Message("Barcode already exists", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            '   BubbleEvent = False
                            '  Exit Sub
                        End If
                    End If
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormType = frm_SerialAssigment Then
                Select Case pVal.BeforeAction
                    Case True
                        If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "1" Then
                            blnFlag = False
                            oMode = pVal.FormMode
                            If oMode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Or oMode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                'If Validatebarcode(oApplication.Utilities.getEdittextvalue(oForm, "5"), oApplication.Utilities.getEdittextvalue(oForm, "107")) Then
                                '    oApplication.Utilities.Message("Barcode already exists", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                '    BubbleEvent = False
                                '    blnFlag = True
                                '    Exit Sub
                                'End If
                            End If

                        End If
                    Case False
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                'oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                                oApplication.Utilities.AddControls(oForm, "btnSelect", "2", SAPbouiCOM.BoFormItemTypes.it_BUTTON, "RIGHT", , , "2", "Import")

                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                Select Case pVal.ItemUID
                                    Case "btnSelect"
                                        frm_SourceSerialForm = oForm
                                        If ValidaterowsFromDocument(oForm) = True Then
                                            Dim oObj As New clsSerialImport
                                            oObj.LoadForm()
                                        Else
                                            If oApplication.SBO_Application.MessageBox("Serial Numbers already imported. Do you want to re-Import the file again ?", , "Yes", "No") = 2 Then
                                                Exit Sub
                                            Else
                                                ClearSerialNumbers(oForm)
                                                Dim oObj As New clsImport
                                                oObj.LoadForm()
                                            End If

                                        End If

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

#Region "Validate Rows from Document"
    Private Function ValidaterowsFromDocument(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Try
            aForm.Freeze(True)
            oMatrix = aForm.Items.Item("3").Specific
            For intRow As Integer = 1 To oMatrix.RowCount
                If oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oMatrix, "8", intRow)) > 0 Then
                    aForm.Freeze(False)
                    Return True
                End If
            Next
            aForm.Freeze(False)
            Return False
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            aForm.Freeze(False)
            Return False
        End Try
    End Function

    Private Sub ClearSerialNumbers(ByVal aForm As SAPbouiCOM.Form)
        Try
            aForm.Freeze(True)
            Dim aMatrix As SAPbouiCOM.Matrix
            aMatrix = aForm.Items.Item("3").Specific
            oMatrix = aForm.Items.Item("55").Specific
            For intLoop As Integer = 1 To aMatrix.RowCount
                aMatrix.Columns.Item("0").Cells.Item(intLoop).Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                For intRow As Integer = oMatrix.VisualRowCount To 1 Step -1
                    oMatrix.Columns.Item("1").Cells.Item(intRow).Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                    'oMatrix.DeleteRow(intRow)
                    aForm.Items.Item("6").Click(SAPbouiCOM.BoCellClickType.ct_Regular)

                    'oApplication.SBO_Application.ActivateMenuItem(mnu_DELETE_ROW)
                    If aForm.Mode <> SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                        aForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE
                    End If
                Next
                If aForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                    aForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                End If
            Next
            If aForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                aForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
            End If
            aForm.Freeze(False)
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            aForm.Freeze(False)
        End Try
        aForm.Freeze(False)
    End Sub
#End Region
End Class
