Public Class clsInvoice
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
                    Case frm_COA
                        Dim oCOA As SAPbobsCOM.ChartOfAccounts
                        oCOA = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oChartOfAccounts)
                        If oCOA.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oCOA.UserFields.Fields.Item("U_Action").Value = "U"
                            oCOA.Update()
                        End If
                End Select
            ElseIf BusinessObjectInfo.BeforeAction = False And BusinessObjectInfo.ActionSuccess = True And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                Dim strDocNum, strDocType As String
                oForm = oApplication.SBO_Application.Forms.ActiveForm()
                Dim bisObj As SAPbouiCOM.BusinessObject = Form.BusinessObject
                Dim uid As String = bisObj.Key
                Dim oItem As SAPbobsCOM.Items
                Dim oBP As SAPbobsCOM.BusinessPartners
                Dim oSt As SAPbobsCOM.StockTransfer
                Dim BP1 As SAPbobsCOM.Documents '= oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes)
                Select Case BusinessObjectInfo.FormTypeEx
                    Case frm_COA
                        Dim oCOA As SAPbobsCOM.ChartOfAccounts
                        oCOA = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oChartOfAccounts)
                        If oCOA.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                            oCOA.UserFields.Fields.Item("U_Action").Value = "A"
                            oCOA.Update()
                        End If
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
                        'Case frm_StockTransfer
                        '    oSt = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer)
                        '    If oSt.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                        '        oSt.UserFields.Fields.Item("U_Export").Value = "N"
                        '        oSt.Update()
                        '    End If
                        'Case frm_APServiceinvoice
                        '    Dim oDoc As SAPbobsCOM.Documents
                        '    Dim oJE As SAPbobsCOM.JournalEntries
                        '    oDoc = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices)
                        '    oJE = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oJournalEntries)
                        '    If oDoc.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                        '        Dim intDocNum As Integer
                        '        intDocNum = oDoc.TransNum
                        '        For intRow As Integer = 0 To oDoc.Lines.Count - 1
                        '            oDoc.Lines.SetCurrentLine(intRow)
                        '            Try
                        '                If oJE.GetByKey(intDocNum) Then
                        '                    For intLoop As Integer = 0 To oJE.Lines.Count - 1
                        '                        oJE.Lines.SetCurrentLine(intLoop)
                        '                        If oJE.Lines.AccountCode = oDoc.Lines.AccountCode Then
                        '                            oJE.Lines.LineMemo = oDoc.Lines.ItemDescription
                        '                            If oJE.Update() <> 0 Then
                        '                                oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        '                            End If
                        '                        End If

                        '                    Next
                        '                End If
                        '            Catch ex As Exception

                        '            End Try

                        '        Next
                        '    End If
                End Select
            ElseIf BusinessObjectInfo.BeforeAction = True And (BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE Or BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD) Then
                Dim oItem As SAPbobsCOM.Items
                If BusinessObjectInfo.FormTypeEx = frm_ItemMaster Then
                    oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)
                    If blnFlag = True Then
                        Exit Sub
                    End If
                    'If oItem.Browser.GetByKeys(BusinessObjectInfo.ObjectKey) Then
                    '    If Validatebarcode(oItem.ItemCode, oItem.BarCode) = True Then
                    '        oApplication.Utilities.Message("Barcode already exists", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    '        '   BubbleEvent = False
                    '        '  Exit Sub
                    '    End If
                    'End If
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormType = frm_ItemMaster Then
                Select Case pVal.BeforeAction
                    Case True
                        If pVal.EventType = SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED And pVal.ItemUID = "1" Then
                            blnFlag = False
                            oMode = pVal.FormMode
                            If oMode = SAPbouiCOM.BoFormMode.fm_ADD_MODE Or oMode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                If Validatebarcode(oApplication.Utilities.getEdittextvalue(oForm, "5"), oApplication.Utilities.getEdittextvalue(oForm, "107")) Then
                                    oApplication.Utilities.Message("Barcode already exists", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                    BubbleEvent = False
                                    blnFlag = True
                                    Exit Sub
                                End If
                            End If

                        End If
                    Case False
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                oItem = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oItems)

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
