Public Class clsDraftDocument
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
            oForm = oApplication.Utilities.LoadForm(xml_DocDetails, frm_DocDetails)
            oForm = oApplication.SBO_Application.Forms.ActiveForm()
            oForm.Freeze(True)
            oForm.PaneLevel = 1
            strQuery = "SELECT U_UdfName FROM ""@Z_DBSYN"""
            Dim Otemp As SAPbobsCOM.Recordset
            Otemp = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            Otemp.DoQuery(strQuery)
            If Otemp.RecordCount > 0 Then
                oApplication.Utilities.setEdittextvalue(oForm, "7", Otemp.Fields.Item("U_UdfName").Value.ToString())
            End If
            Databind(oForm)
            AddtoUDT1(oForm)
           
            oForm.Freeze(False)
        Catch ex As Exception
            oForm.Freeze(False)
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#Region "Databind"
    Private Sub Databind(ByVal aform As SAPbouiCOM.Form)
        Try
            aform.Freeze(True)
            oGrid = aform.Items.Item("8").Specific
            dtTemp = oGrid.DataTable
            dtTemp.ExecuteQuery("SELECT * FROM ""@Z_DBSYN""   where U_DocType='S' order by U_Order")
            oGrid.DataTable = dtTemp
            Formatgrid(oGrid)
            oApplication.Utilities.assignMatrixLineno(oGrid, aform)

            oGrid = aform.Items.Item("9").Specific
            dtTemp1 = oGrid.DataTable
            dtTemp1.ExecuteQuery("SELECT * FROM ""@Z_DBSYN""  where U_DocType='P' order by U_Order")
            oGrid.DataTable = dtTemp1
            Formatgrid(oGrid)
            oApplication.Utilities.assignMatrixLineno(oGrid, aform)
            aform.Freeze(False)
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            aform.Freeze(False)
        End Try
    End Sub
#End Region

#Region "FormatGrid"
    Private Sub Formatgrid(ByVal agrid As SAPbouiCOM.Grid)
        agrid.Columns.Item("Code").TitleObject.Caption = "Document Code"
        agrid.Columns.Item("Code").Editable = False
        agrid.Columns.Item("Name").TitleObject.Caption = "Document Name"
        agrid.Columns.Item("Name").Editable = False
        agrid.Columns.Item("U_Table").TitleObject.Caption = "Table Name"
        agrid.Columns.Item("U_Table").Visible = False
        agrid.Columns.Item("U_UdfName").TitleObject.Caption = "UDF Name"
        agrid.Columns.Item("U_UdfName").Visible = False
        agrid.Columns.Item("U_DocType").TitleObject.Caption = "Document Type"
        agrid.Columns.Item("U_DocType").Type = SAPbouiCOM.BoGridColumnType.gct_ComboBox
        oComboColumn = agrid.Columns.Item("U_DocType")
        oComboColumn.ValidValues.Add("S", "Sales")
        oComboColumn.ValidValues.Add("P", "Purchase")
        oComboColumn.ExpandType = SAPbouiCOM.BoExpandType.et_DescriptionOnly
        agrid.Columns.Item("U_DocType").Visible = False
        agrid.Columns.Item("U_Active").TitleObject.Caption = "Active"
        agrid.Columns.Item("U_Active").Type = SAPbouiCOM.BoGridColumnType.gct_CheckBox
        agrid.Columns.Item("U_Order").Visible = False
        agrid.Columns.Item("U_FrmDate").TitleObject.Caption = "From Date"
        agrid.Columns.Item("U_ToDate").TitleObject.Caption = "End Date"

        agrid.AutoResizeColumns()
        agrid.SelectionMode = SAPbouiCOM.BoMatrixSelect.ms_Single
    End Sub
#End Region

#Region "AddtoUDT"
    Private Function AddtoUDT1(ByVal aform As SAPbouiCOM.Form) As Boolean
        Dim oUserTable, oUserTable1 As SAPbobsCOM.UserTable
        Dim strCode, strType, strQuery As String
        oGrid = aform.Items.Item("8").Specific
        oUserTable = oApplication.Company.UserTables.Item("Z_DBSYN")
        strType = oApplication.Utilities.getEdittextvalue(aform, "7")
        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strCode = oGrid.DataTable.GetValue("Code", intRow)
            Try
                If strType = "" Then
                    strType = oGrid.DataTable.GetValue("U_UdfName", intRow)
                End If
            Catch ex As Exception
                strType = ""
            End Try
            strType = "1"
            If strCode <> "" Then
                If oUserTable.GetByKey(strCode) Then
                    oUserTable.Code = strCode
                    oUserTable.Name = oGrid.DataTable.GetValue("Name", intRow)
                    oUserTable.UserFields.Fields.Item("U_UdfName").Value = oApplication.Utilities.getEdittextvalue(aform, "7")
                    oUserTable.UserFields.Fields.Item("U_Active").Value = oGrid.DataTable.GetValue("U_Active", intRow)
                    oUserTable.UserFields.Fields.Item("U_Order").Value = oGrid.DataTable.GetValue("U_Order", intRow)
                    Dim strDate As String = oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    If strDate <> "" Then
                        oUserTable.UserFields.Fields.Item("U_FrmDate").Value = oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    End If
                    strDate = oGrid.DataTable.GetValue("U_ToDate", intRow)
                    If strDate <> "" Then
                        oUserTable.UserFields.Fields.Item("U_ToDate").Value = oGrid.DataTable.GetValue("U_ToDate", intRow)
                    End If


                    If oUserTable.Update <> 0 Then
                        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    End If
                End If
            ElseIf strCode = "" Then
                Dim intOrder As Integer = 0

                oUserTable.Code = "171"
                oUserTable.Name = "A/R Reserve Invoice"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 1
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                oUserTable.Code = "15"
                oUserTable.Name = "Delivery"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 2
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If
                'oUserTable.Code = "16"
                'oUserTable.Name = "Sales Return"
                'oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                'oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                'oUserTable.UserFields.Fields.Item("U_Order").Value = 3
                'intOrder = intOrder + 1
                'If oUserTable.Add <> 0 Then
                '    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                '    Return False
                'End If


                oUserTable.Code = "13"
                oUserTable.Name = "A/R Invoice"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 4
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If



                oUserTable.Code = "14"
                oUserTable.Name = "A/R Credit Note"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 5
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                oUserTable.Code = "24"
                oUserTable.Name = "InComming Payment"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 5
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If

                oUserTable.Code = "30"
                oUserTable.Name = "Journal Entry"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 5
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If
                oUserTable.Code = "25"
                oUserTable.Name = "Bank Deposit"
                oUserTable.UserFields.Fields.Item("U_DocType").Value = "S"
                oUserTable.UserFields.Fields.Item("U_Table").Value = "ODPS"
                oUserTable.UserFields.Fields.Item("U_Order").Value = 5
                intOrder = intOrder + 1
                If oUserTable.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If
            End If




        Next

        oGrid = aform.Items.Item("9").Specific
        oUserTable1 = oApplication.Company.UserTables.Item("Z_DBSYN")
        strType = oApplication.Utilities.getEdittextvalue(aform, "7")
        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strCode = oGrid.DataTable.GetValue("Code", intRow)
            Try
                If strType = "" Then
                    strType = oGrid.DataTable.GetValue("U_UdfName", intRow)
                End If
            Catch ex As Exception
                strType = ""
            End Try
            If strType <> "" Or strCode <> "" Then
                If oUserTable1.GetByKey(strCode) Then
                    oUserTable1.Code = strCode
                    oUserTable1.Name = oGrid.DataTable.GetValue("Name", intRow)
                    oUserTable1.UserFields.Fields.Item("U_DocType").Value = oGrid.DataTable.GetValue("U_DocType", intRow)
                    oUserTable1.UserFields.Fields.Item("U_UdfName").Value = oApplication.Utilities.getEdittextvalue(aform, "7")
                    oUserTable1.UserFields.Fields.Item("U_Active").Value = oGrid.DataTable.GetValue("U_Active", intRow)
                    oUserTable1.UserFields.Fields.Item("U_Order").Value = oGrid.DataTable.GetValue("U_Order", intRow)
                    Dim strDate As String = oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    If strDate <> "" Then
                        oUserTable.UserFields.Fields.Item("U_FrmDate").Value = oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    Else
                        ' oUserTable.UserFields.Fields.Item("U_FrmDate").Value = Nothing ' oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    End If
                    strDate = oGrid.DataTable.GetValue("U_ToDate", intRow)
                    If strDate <> "" Then
                        oUserTable.UserFields.Fields.Item("U_ToDate").Value = oGrid.DataTable.GetValue("U_ToDate", intRow)
                    Else
                        ' oUserTable.UserFields.Fields.Item("U_FrmDate").Value = oGrid.DataTable.GetValue("U_FrmDate", intRow)
                    End If

                    If oUserTable1.Update <> 0 Then
                        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    End If
                End If
            ElseIf strCode = "" Then
                Dim intOrder As Integer = 0

                'oUserTable1.Code = "1470000113"
                'oUserTable1.Name = "Purchase Request"
                'oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                'oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                'oUserTable1.UserFields.Fields.Item("U_Order").Value = 0
                'If oUserTable1.Add <> 0 Then
                '    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                '    Return False
                'End If

                'oUserTable1.Code = "540000006"
                'oUserTable1.Name = "Purchase Quotation"
                'oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                'oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                'oUserTable1.UserFields.Fields.Item("U_Order").Value = 1
                'If oUserTable1.Add <> 0 Then
                '    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                '    Return False
                'End If


                'oUserTable1.Code = "22"
                'oUserTable1.Name = "Purchase Order"
                'oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                'oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                'oUserTable1.UserFields.Fields.Item("U_Order").Value = 2
                'If oUserTable1.Add <> 0 Then
                '    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                '    Return False
                'End If
                oUserTable1.Code = "22"
                oUserTable1.Name = "Purchase Order"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 3
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                oUserTable1.Code = "20"
                oUserTable1.Name = "Goods Receipt PO"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 3
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                oUserTable1.Code = "21"
                oUserTable1.Name = "Goods Returns"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 4
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                oUserTable1.Code = "18"
                oUserTable1.Name = "A/P Invoice"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 5
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If

                oUserTable1.Code = "19"
                oUserTable1.Name = "A/P Credit Memo"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 6
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If

                oUserTable1.Code = "46"
                oUserTable1.Name = "Outgoing Payment"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 6
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If

                oUserTable1.Code = "461"
                oUserTable1.Name = "Landed Cost"
                oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                oUserTable1.UserFields.Fields.Item("U_Order").Value = 6
                If oUserTable1.Add <> 0 Then
                    oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    Return False
                End If


                'For Each element1 As DictionaryEntry In PurchaseHash()
                '    oUserTable1.Code = element1.Key
                '    oUserTable1.Name = element1.Value
                '    oUserTable1.UserFields.Fields.Item("U_DocType").Value = "P"
                '    oUserTable1.UserFields.Fields.Item("U_Table").Value = "ODRF"
                '    oUserTable1.UserFields.Fields.Item("U_Order").Value = intOrder
                '    intOrder = intOrder + 1
                '    If oUserTable1.Add <> 0 Then
                '        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                '        Return False
                '    End If
                'Next
            End If
        Next
        oApplication.Utilities.Message("Operation completed successfully", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
        Databind(aform)
        Return True
    End Function

    Public Sub cmdDraftToOrder()
        Dim strCode, strqry, Active As String
        Dim aDocEntry As String = ""
        Dim oRec As SAPbobsCOM.Recordset
        oRec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Dim pDraft As SAPbobsCOM.Documents
        pDraft = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDrafts)
        oGrid = oForm.Items.Item("8").Specific
        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strCode = oGrid.DataTable.GetValue("Code", intRow)
            Active = oGrid.DataTable.GetValue("U_Active", intRow)
            If Active = "Y" Then
                strqry = "Select * from ODRF where objType='" & strCode & "' and DocStatus='O'"
                oRec.DoQuery(strqry)
                If oRec.RecordCount > 0 Then
                    For intRow1 As Integer = 0 To oRec.RecordCount - 1
                        aDocEntry = oRec.Fields.Item("DocEntry").Value
                        pDraft.GetByKey(aDocEntry)
                        If pDraft.SaveDraftToDocument() <> 0 Then
                            oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        End If
                        oRec.MoveNext()
                    Next
                End If
            End If
        Next

        oGrid = oForm.Items.Item("9").Specific
        For intRow As Integer = 0 To oGrid.DataTable.Rows.Count - 1
            strCode = oGrid.DataTable.GetValue("Code", intRow)
            Active = oGrid.DataTable.GetValue("U_Active", intRow)
            If Active = "Y" Then
                strqry = "Select * from ODRF where objType='" & strCode & "' and DocStatus='O'"
                oRec.DoQuery(strqry)
                If oRec.RecordCount > 0 Then
                    For intRow1 As Integer = 0 To oRec.RecordCount - 1
                        aDocEntry = oRec.Fields.Item("DocEntry").Value
                        pDraft.GetByKey(aDocEntry)
                        If pDraft.SaveDraftToDocument() <> 0 Then
                            oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        End If
                        oRec.MoveNext()
                    Next
                End If
            End If
        Next
      
    End Sub
#End Region
    Private Function validation(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Try
            If oApplication.Utilities.getEdittextvalue(aForm, "7") = "" Then
                oApplication.Utilities.Message("UDF Name can not be empty", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                Return False
            End If
            Return True
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            Return False
        End Try
    End Function
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormTypeEx = frm_DocDetails Then
                Select Case pVal.BeforeAction
                    Case True
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                'If pVal.ItemUID = "3" Then
                                '    If validation(oForm) = False Then
                                '        BubbleEvent = False
                                '        Exit Sub
                                '    End If
                                'End If

                        End Select

                    Case False
                        Select Case pVal.EventType
                            Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            Case SAPbouiCOM.BoEventTypes.et_KEY_DOWN

                            Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                                Select Case pVal.ItemUID
                                    Case "1"
                                        oForm.PaneLevel = 1
                                    Case "4"
                                        oForm.PaneLevel = 2
                                    Case "3"
                                        AddtoUDT1(oForm)
                                        '   cmdDraftToOrder()
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
                Case mnu_DocDetails
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
