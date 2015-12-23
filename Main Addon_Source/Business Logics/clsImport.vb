
Imports System.Xml
Imports System.Net.Mail
Imports System.IO
Imports System
Imports System.Data.OleDb
Imports System.Collections.Generic
Imports System.Web
Imports System.Threading
Public Class clsImport

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
    Dim OCompany As SAPbobsCOM.Company
    Dim objForm As SAPbouiCOM.Form
    Dim ObjEdittext As SAPbouiCOM.EditText
    Dim oStaticText As SAPbouiCOM.StaticText
    Dim oCheckBox As SAPbouiCOM.CheckBox
    Dim objUtility As clsUtilities
    Dim XLPath As String
    Dim Locat As Integer
    Dim ISErr As Boolean = False
    Dim XLAttPath, strSelectedFilepath, sPath As String
    Public Sub New()
        MyBase.New()
        InvForConsumedItems = 0
    End Sub

#Region "Load Form"
    Public Sub LoadForm()
        objForm = oApplication.Utilities.LoadForm(xml_SerImport, frm_SerImport)
        objForm = oApplication.SBO_Application.Forms.ActiveForm()
        BindData(objForm)
    End Sub
#End Region
#Region "BindData"
    Private Sub BindData(ByVal oForm As SAPbouiCOM.Form)
        Try
            oForm.Freeze(True)
            oForm.DataSources.UserDataSources.Add("FileName", SAPbouiCOM.BoDataType.dt_LONG_TEXT)
            oForm.DataSources.UserDataSources.Add("FileType", SAPbouiCOM.BoDataType.dt_LONG_TEXT)
            oForm.DataSources.DataTables.Add("Import")
            'oCombobox = oForm.Items.Item("8").Specific
            'oCombobox.ValidValues.Add("", "")
            'oCombobox.ValidValues.Add("T", "Traffic Fines")
            'oCombobox.ValidValues.Add("S", "Salik")
            'oCombobox.DataBind.SetBound(True, "", "FileType")
            'oCombobox.Select(0, SAPbouiCOM.BoSearchKey.psk_Index)
            'oForm.Items.Item("8").DisplayDesc = True
            ObjEdittext = oForm.Items.Item("10").Specific
            '  Dim sPath As String = ReadiniFile()
            ObjEdittext.DataBind.SetBound(True, "", "FileName")
            ObjEdittext.String = sPath
            strSelectedFilepath = sPath
            XLPath = strSelectedFilepath
            '  dtTemp = objForm.DataSousrces.DataTables.Add("TEMP")
            oForm.Freeze(False)
        Catch ex As Exception
            oForm.Freeze(False)
        End Try
    End Sub
#End Region

#Region "ShowFileDialog"
    Private Sub fillopen()
        Dim mythr As New System.Threading.Thread(AddressOf ShowFileDialog)
        mythr.SetApartmentState(ApartmentState.STA)
        mythr.Start()
        mythr.Join()
    End Sub
    Private Sub ShowFileDialog()
        Dim oDialogBox As New OpenFileDialog
        Dim strMdbFilePath As String
        Dim oProcesses() As Process
        Try
            Dim oWinForm As New System.Windows.Forms.Form()
            oWinForm.TopMost = True

            oProcesses = Process.GetProcessesByName("SAP Business One")
            If oProcesses.Length <> 0 Then
                For i As Integer = 0 To oProcesses.Length - 1
                    Dim MyWindow As New WindowWrapper(oProcesses(i).MainWindowHandle)
                    If oDialogBox.ShowDialog(MyWindow) = DialogResult.OK Then
                        strMdbFilePath = oDialogBox.FileName
                    End If
                Next
            End If
            oForm.Items.Item("10").Specific.String = strMdbFilePath
            strSelectedFilepath = strMdbFilePath
            XLPath = strMdbFilePath
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)

        Finally

        End Try
    End Sub
#End Region

#Region "Assign Serial Number"
    Private Function AssignSerialNo(ByVal aForm As SAPbouiCOM.Form) As Boolean
        Dim oTest As SAPbobsCOM.Recordset
        Dim strItemCode, strSerialno As String
        Dim dblSerialRequired, dblCount As Double
        Dim oRowsMatrix, oSerialMatrix As SAPbouiCOM.Matrix
        Try
            aForm.Freeze(True)
            oTest = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRowsMatrix = aForm.Items.Item("43").Specific
            For intRow As Integer = 1 To oRowsMatrix.RowCount
                strItemCode = oApplication.Utilities.getMatrixValues(oRowsMatrix, "5", intRow)
                dblSerialRequired = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oRowsMatrix, "40", intRow))
                If dblSerialRequired > 0 Then
                    oTest.DoQuery("Select Count(*) from [@Z_OSRI] where U_Z_Available='New' and  U_Z_ItemCode='" & strItemCode & "' and U_Z_Status='N'")
                    dblCount = oTest.Fields.Item(0).Value
                    If (dblSerialRequired > dblCount) Then
                        oApplication.Utilities.Message("Insufficient Serial number in the importing file for item code : " & strItemCode, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        aForm.Freeze(False)
                        Return False
                    End If
                End If
            Next

            For intRow As Integer = 1 To oRowsMatrix.RowCount
                strItemCode = oApplication.Utilities.getMatrixValues(oRowsMatrix, "5", intRow)
                dblSerialRequired = oApplication.Utilities.getDocumentQuantity(oApplication.Utilities.getMatrixValues(oRowsMatrix, "40", intRow))
                If dblSerialRequired > 0 Then
                    oTest.DoQuery("Select Count(*) from [@Z_OSRI] where  U_Z_Available='New' and U_Z_ItemCode='" & strItemCode & "' and U_Z_Status='N'")
                    dblCount = oTest.Fields.Item(0).Value
                    oRowsMatrix.Columns.Item("0").Cells.Item(intRow).Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                    'If (dblSerialRequired > dblCount) Then
                    '    oApplication.Utilities.Message("Insufficient Serial number in the importing file for item code : " & strItemCode, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                    '    Return False
                    'End If
                    oTest.DoQuery("Select * from [@Z_OSRI] where  U_Z_Available='New' and U_Z_ItemCode='" & strItemCode & "' and U_Z_Status='N'")
                    oSerialMatrix = aForm.Items.Item("3").Specific
                    For intLoop As Integer = 0 To dblSerialRequired - 1 ' oTest.RecordCount - 1
                        oApplication.Utilities.SetMatrixValues(oSerialMatrix, "54", oSerialMatrix.VisualRowCount, oTest.Fields.Item("U_Z_SerialNo").Value)
                        oApplication.SBO_Application.SendKeys("{TAB}")
                        oTest.MoveNext()
                    Next
                    If aForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE Then
                        aForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                    End If
                End If
            Next
            If aForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE Then
                 aForm.Freeze(False)
                aForm.Items.Item("1").Click(SAPbouiCOM.BoCellClickType.ct_Regular)
                Return True
            End If
            aForm.Freeze(False)
            Return True
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            aForm.Freeze(False)
            Return False
        End Try
    End Function
#End Region
#Region "Item Event"
    Public Overrides Sub ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean)
        Try
            If pVal.FormTypeEx = frm_SerImport Then
                If pVal.Before_Action = True Then
                    Select Case pVal.EventType
                        Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                            oForm = oApplication.SBO_Application.Forms.Item(FormUID)
                            If pVal.ItemUID = "3" Then
                                Dim oDt As SAPbouiCOM.DataTable
                                oDt = Nothing
                                ObjEdittext = oForm.Items.Item("10").Specific
                                strSelectedFilepath = ObjEdittext.String
                                If strSelectedFilepath = "" Then
                                    oApplication.Utilities.Message("Import  file is missing...", SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                                    Exit Sub
                                End If
                                Dim strFolderlogfile As String
                                If Directory.Exists(System.Windows.Forms.Application.StartupPath & "\Logs") Then
                                Else
                                    Directory.CreateDirectory(System.Windows.Forms.Application.StartupPath & "\Logs")
                                End If
                                strFolderlogfile = System.Windows.Forms.Application.StartupPath & "\Logs\Folders_Log.txt"
                                ReadSerial(strSelectedFilepath, oForm)
                            ElseIf pVal.ItemUID = "8" Then
                                If AssignSerialNo(frm_SourceSerialForm) = True Then
                                    oForm.Close()
                                End If
                                'If oCombobox.Selected.Value = "T" Then
                                '    ReadTraffic(strSelectedFilepath, oForm)
                                'ElseIf oCombobox.Selected.Value = "S" Then
                                '    ReadSalik(strSelectedFilepath, oForm)
                                'End If
                            End If
                            If pVal.ItemUID = "11" Then
                                fillopen()
                                ObjEdittext = oForm.Items.Item("10").Specific
                                ObjEdittext.String = strSelectedFilepath
                            End If
                    End Select
                End If
            End If


        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
        End Try
    End Sub
#End Region


#Region "Read From the XL Files"
    'Public Function ReadXlDataFile(ByVal aform As SAPbouiCOM.Form, ByVal afilename As String, ByVal optionCaption As String) As SAPbouiCOM.DataTable
    '    Dim Connstring As String = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + afilename + ";Extended Properties=""Excel 12.0;HDR=No;"""
    '    Dim strCompany, strSql, StrTmp As String
    '    Dim objDS As DataSet = New DataSet
    '    Dim dblPrice, dblRate As Double
    '    Dim dtpostingdate As Date
    '    Dim dt As System.Data.DataTable = New DataTable
    '    Dim objConexcel As OleDbConnection = New OleDbConnection(Connstring)
    '    Dim strCardcode, strCardName, strLocation, strTax, strAccount, strProject, strProfitcenter, strlocalcurrency, strDocCurrency, strNumAtCard As String
    '    Try
    '        ISErr = False
    '        strSql = "SELECT * FROM [Import$]"
    '        Dim objOleDbDataAdapter As OleDbDataAdapter = New OleDbDataAdapter(strSql, Connstring)
    '        objOleDbDataAdapter.Fill(objDS)
    '        StrTmp = "Select A.CardCode,A.DocDate,A.DocCur,A.NumAtCard,A.DocDueDate,B.Dscription,B.Currency,B.Price,B.Rate,B.LineTotal,B.VatPrcnt,B.PriceAfVAT,B.AcctCode,B.Project,B.TaxCode ,B.LocCode 'Location' ,CardName from OPCH as A,PCH1 as B where 1=2"
    '        dtTemp = aform.DataSources.DataTables.Item(0)
    '        dtTemp.ExecuteQuery(StrTmp)
    '        ' strlocalcurrency = GetLocalCurrency()
    '        For i As Integer = 7 To objDS.Tables(0).Rows.Count - 1
    '            strCardName = "" 'Add By Rakesh Maharjan on 23 July 2010
    '            strCardcode = objDS.Tables(0).Rows(i)(1).ToString
    '            strDocCurrency = objDS.Tables(0).Rows(i)(3).ToString
    '            strNumAtCard = objDS.Tables(0).Rows(i)(4)
    '            dtpostingdate = objDS.Tables(0).Rows(i)(2) 'DocDate from EXCEL
    '            strAccount = objDS.Tables(0).Rows(i)(13).ToString
    '            strProject = objDS.Tables(0).Rows(i)(14).ToString
    '            strLocation = objDS.Tables(0).Rows(i)(16).ToString
    '            strTax = objDS.Tables(0).Rows(i)(15).ToString

    '            Dim oTempRecset As SAPbobsCOM.Recordset
    '            oTempRecset = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)

    '        Next
    '        Return dtTemp
    '    Catch ex As Exception
    '        oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
    '        ISErr = True
    '        Return Nothing
    '    End Try
    'End Function

#Region "AddtoUDT"
    Private Function AddtoTraffic(ByVal aField1 As String, ByVal aField2 As String, ByVal afield3 As String, ByVal afield4 As String, ByVal afield5 As String, ByVal afield6 As String, ByVal aField7 As String, ByVal aField8 As String, ByVal afield9 As String) As Boolean
        Dim oUserTable As SAPbobsCOM.UserTable
        Dim orec, orec1 As SAPbobsCOM.Recordset
        Dim strCode, stFromdate, stToDate, strHoursworked As String
        Dim dblDifference As Double
        orec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        orec1 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Dim dtDate, dtTodate, dtTemp As Date
        Dim strWorkingHours, strActualworkinghours, strItemCode As String
        Dim dblworkinghours, dblOverTime As Double
        For intRow As Integer = 1 To 1
            If aField1 <> "" Then
                'strCode = oGrid.DataTable.GetValue(0, intRow)
                oUserTable = oApplication.Company.UserTables.Item("Z_TRAF")
                orec.DoQuery("Select * from [@Z_TRAF] where U_Z_FineNo='" & aField1 & "'")
                If orec.RecordCount > 0 Then
                    strCode = orec.Fields.Item("Code").Value
                Else
                    strCode = ""
                End If
                strItemCode = afield5 & "-" & afield6
                If strCode = "" Then
                    strCode = oApplication.Utilities.getMaxCode("@Z_TRAF", "Code")
                    oUserTable.Code = strCode
                    oUserTable.Name = strCode
                    oUserTable.UserFields.Fields.Item("U_Z_FineNo").Value = aField1
                    oUserTable.UserFields.Fields.Item("U_Z_Date").Value = aField2
                    oUserTable.UserFields.Fields.Item("U_Z_Time").Value = afield3
                    oUserTable.UserFields.Fields.Item("U_Z_Source").Value = afield4
                    oUserTable.UserFields.Fields.Item("U_Z_PlateNo").Value = afield5
                    oUserTable.UserFields.Fields.Item("U_Z_PlateCode").Value = afield6
                    oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = strItemCode
                    oUserTable.UserFields.Fields.Item("U_Z_Fee").Value = aField7
                    oUserTable.UserFields.Fields.Item("U_Z_TkDesc").Value = aField8
                    oUserTable.UserFields.Fields.Item("U_Z_TkLoc").Value = afield9
                    oUserTable.UserFields.Fields.Item("U_Z_Amount").Value = oApplication.Utilities.getDocumentQuantity(aField7.Replace("AED", ""))
                    oUserTable.UserFields.Fields.Item("U_Z_Total").Value = oApplication.Utilities.getDocumentQuantity(aField7.Replace("AED", ""))
                    oUserTable.UserFields.Fields.Item("U_Z_Status").Value = "O"
                    oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = ""
                    oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = "" ' oRec2.Fields.Item(1).Value.ToString
                    oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = "" ' oRec2.Fields.Item(2).Value.ToString
                    oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = "" ' oRec2.Fields.Item(3).Value.ToString
                    oUserTable.UserFields.Fields.Item("U_Z_Active").Value = "Y"
                    'oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                    If oUserTable.Add() <> 0 Then
                        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    Else

                        Dim strsql, strDate As String
                        Try
                            strsql = "select x.DocEntry,x.OutDate,x.ItemCode,x.Pickupdate,x.ChkOutDriver,x.ChkOutHirer,x.ChkInDriver,x.ChkInHirer,x.Status from "
                            strsql = strsql & " (select  docentry,U_Z_OutDate 'OutDate',U_Z_ItemCode 'ItemCode',U_Z_InDate,U_Z_InTime,U_Z_Status 'Status',"
                            strsql = strsql & " (U_Z_InDate + cast(CAST(U_Z_InTime / 100 as varchar) + ':' + CAST(U_Z_InTime % 100 as varchar) as datetime)) as 'PickUpDate',"
                            strsql = strsql & " (U_Z_ChkOutDt + cast(CAST(U_Z_ChkOutTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutTm % 100 as varchar) as datetime)) as 'ChkOutDriver',"
                            strsql = strsql & " (U_Z_ChkOutCuDt + cast(CAST(U_Z_ChkOutCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutCUTm % 100 as varchar) as datetime)) as 'ChkOutHirer',"
                            strsql = strsql & " (U_Z_ChkInDt + cast(CAST(U_Z_ChkInTm / 100 as varchar) + ':' + CAST(U_Z_ChkInTm % 100 as varchar) as datetime)) as 'ChkInDriver',"
                            strsql = strsql & " (U_Z_ChkInCuDt + cast(CAST(U_Z_ChkInCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkInCUTm % 100 as varchar) as datetime)) as 'ChkInHirer'"
                            strsql = strsql & " from [@Z_ORDR] ) as x "
                            Dim oTestRs, oRec2 As SAPbobsCOM.Recordset
                            oTestRs = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            oRec2 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            strDate = " Update [@Z_TRAF] set U_Z_FineDate=convert(Datetime,U_Z_Date,105) + U_Z_Time  where Code='" & strCode & "'"
                            oTestRs.DoQuery(strDate)
                            strDate = " select U_Z_FineDate from [@Z_TRAF] where Code='" & strCode & "'"
                            strsql = strsql & " where (x.ItemCode='" & strItemCode & "') and  (" & strDate & ") between x.chkOutDriver and isnull(x.ChkInDriver,x.OutDate)"
                            oTestRs.DoQuery(strsql)
                            If oTestRs.RecordCount > 0 Then
                                oUserTable.GetByKey(strCode)
                                oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                oRec2.DoQuery("select DocEntry,case U_Z_Type when 'C' then 'Customer' else 'NRM' end,U_Z_CardCode,U_Z_CardName,U_Z_ItemCode,U_Z_ItemName,U_Z_FromLoc,U_Z_InDate,U_Z_OutDate,U_Z_toLoc from [@Z_ORDR] where U_Z_ItemCode='" & strItemCode & "' and  DocEntry=" & oTestRs.Fields.Item(0).Value)
                                oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = oRec2.Fields.Item(1).Value.ToString
                                oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = oRec2.Fields.Item(2).Value.ToString
                                oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = oRec2.Fields.Item(3).Value.ToString
                                'oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString

                                oUserTable.Update()
                            End If
                        Catch ex As Exception

                        End Try
                    End If
                Else
                    If oUserTable.GetByKey(strCode) Then
                        oUserTable.Name = strCode
                        oUserTable.UserFields.Fields.Item("U_Z_FineNo").Value = aField1
                        oUserTable.UserFields.Fields.Item("U_Z_Date").Value = aField2
                        oUserTable.UserFields.Fields.Item("U_Z_Time").Value = afield3
                        oUserTable.UserFields.Fields.Item("U_Z_Source").Value = afield4
                        oUserTable.UserFields.Fields.Item("U_Z_PlateNo").Value = afield5
                        oUserTable.UserFields.Fields.Item("U_Z_PlateCode").Value = afield6
                        oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = strItemCode
                        oUserTable.UserFields.Fields.Item("U_Z_Fee").Value = aField7
                        oUserTable.UserFields.Fields.Item("U_Z_TkDesc").Value = aField8
                        oUserTable.UserFields.Fields.Item("U_Z_TkLoc").Value = afield9
                        oUserTable.UserFields.Fields.Item("U_Z_Amount").Value = oApplication.Utilities.getDocumentQuantity(aField7.Replace("AED", ""))
                        oUserTable.UserFields.Fields.Item("U_Z_Total").Value = oApplication.Utilities.getDocumentQuantity(aField7.Replace("AED", ""))
                        oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = ""
                        oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = "" ' oRec2.Fields.Item(1).Value.ToString
                        oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = "" ' oRec2.Fields.Item(2).Value.ToString
                        oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = "" ' oRec2.Fields.Item(3).Value.ToString
                        oUserTable.UserFields.Fields.Item("U_Z_Active").Value = "Y"
                        If oUserTable.Update() <> 0 Then
                            oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            Return False
                        Else
                            Dim strsql, strDate As String
                            Try
                                strsql = "select x.DocEntry,x.Outdate,x.ItemCode,x.Pickupdate,x.ChkOutDriver,x.ChkOutHirer,x.ChkInDriver,x.ChkInHirer,x.Status from "
                                strsql = strsql & " (select  docentry,U_Z_OutDate 'OutDate',U_Z_ItemCode 'ItemCode',U_Z_InDate,U_Z_InTime,U_Z_Status 'Status',"
                                strsql = strsql & " (U_Z_InDate + cast(CAST(U_Z_InTime / 100 as varchar) + ':' + CAST(U_Z_InTime % 100 as varchar) as datetime)) as 'PickUpDate',"
                                strsql = strsql & " (U_Z_ChkOutDt + cast(CAST(U_Z_ChkOutTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutTm % 100 as varchar) as datetime)) as 'ChkOutDriver',"
                                strsql = strsql & " (U_Z_ChkOutCuDt + cast(CAST(U_Z_ChkOutCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutCUTm % 100 as varchar) as datetime)) as 'ChkOutHirer',"
                                strsql = strsql & " (U_Z_ChkInDt + cast(CAST(U_Z_ChkInTm / 100 as varchar) + ':' + CAST(U_Z_ChkInTm % 100 as varchar) as datetime)) as 'ChkInDriver',"
                                strsql = strsql & " (U_Z_ChkInCuDt + cast(CAST(U_Z_ChkInCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkInCUTm % 100 as varchar) as datetime)) as 'ChkInHirer'"
                                strsql = strsql & " from [@Z_ORDR] ) as x "
                                Dim oTestRs, oRec2 As SAPbobsCOM.Recordset
                                oTestRs = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                oRec2 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                strDate = " Update [@Z_TRAF] set U_Z_FineDate=convert(Datetime,U_Z_Date,105) + U_Z_Time  where Code='" & strCode & "'"
                                oTestRs.DoQuery(strDate)
                                strDate = " select U_Z_FineDate from [@Z_TRAF] where Code='" & strCode & "'"
                                strsql = strsql & " where (x.ItemCode='" & strItemCode & "') and  (" & strDate & ") between x.chkOutDriver and isnull(x.ChkInDriver,x.OutDate)"
                                oTestRs.DoQuery(strsql)
                                If oTestRs.RecordCount > 0 Then
                                    oUserTable.GetByKey(strCode)
                                    oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                    oRec2.DoQuery("select DocEntry,case U_Z_Type when 'C' then 'Customer' else 'NRM' end,U_Z_CardCode,U_Z_CardName,U_Z_ItemCode,U_Z_ItemName,U_Z_FromLoc,U_Z_InDate,U_Z_OutDate,U_Z_toLoc from [@Z_ORDR] where  U_Z_ItemCode='" & strItemCode & "' and  DocEntry=" & oTestRs.Fields.Item(0).Value)
                                    oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = oRec2.Fields.Item(1).Value.ToString
                                    oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = oRec2.Fields.Item(2).Value.ToString
                                    oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = oRec2.Fields.Item(3).Value.ToString
                                    oUserTable.Update()
                                End If
                            Catch ex As Exception

                            End Try
                        End If
                    End If
                End If
            End If
        Next
    End Function
    Private Function addtoSalik(ByVal aField1 As String, ByVal aField2 As String, ByVal afield3 As String, ByVal afield4 As String, ByVal afield5 As String, ByVal afield6 As String) As Boolean
        Dim oUserTable As SAPbobsCOM.UserTable
        Dim orec, orec1 As SAPbobsCOM.Recordset
        Dim strCode, stFromdate, stToDate, strHoursworked As String
        Dim dblDifference As Double
        orec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        orec1 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Dim dtDate, dtTodate, dtTemp As Date
        Dim strWorkingHours, strActualworkinghours, strItemCode As String
        Dim dblworkinghours, dblOverTime As Double
        For intRow As Integer = 1 To 1
            If aField1 <> "" Then
                'strCode = oGrid.DataTable.GetValue(0, intRow)
                oUserTable = oApplication.Company.UserTables.Item("Z_SALIK")
                orec.DoQuery("Select * from [@Z_SALIK] where U_Z_TransNo='" & aField1 & "'")
                If orec.RecordCount > 0 Then
                    strCode = orec.Fields.Item("Code").Value
                Else
                    strCode = ""
                End If
                strItemCode = afield3 & "-" & afield4
                If strCode = "" Then
                    strCode = oApplication.Utilities.getMaxCode("@Z_SALIK", "Code")
                    oUserTable.Code = strCode
                    oUserTable.Name = strCode
                    oUserTable.UserFields.Fields.Item("U_Z_TransNo").Value = aField1
                    oUserTable.UserFields.Fields.Item("U_Z_Date").Value = aField2
                    oUserTable.UserFields.Fields.Item("U_Z_PlateNo").Value = afield3
                    oUserTable.UserFields.Fields.Item("U_Z_PlateCode").Value = afield4
                    oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = strItemCode
                    oUserTable.UserFields.Fields.Item("U_Z_Source").Value = afield5
                    oUserTable.UserFields.Fields.Item("U_Z_Amount").Value = afield6
                    oUserTable.UserFields.Fields.Item("U_Z_Status").Value = "O"
                    oUserTable.UserFields.Fields.Item("U_Z_SalikAmount").Value = oApplication.Utilities.getDocumentQuantity(afield6.Replace("AED", ""))
                    oUserTable.UserFields.Fields.Item("U_Z_Total").Value = oApplication.Utilities.getDocumentQuantity(afield6.Replace("AED", ""))

                    If oUserTable.Add() <> 0 Then
                        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    Else
                        Dim strsql, strDate As String
                        Try
                            strsql = "select x.DocEntry,x.OutDate,x.ItemCode,x.Pickupdate,x.ChkOutDriver,x.ChkOutHirer,x.ChkInDriver,x.ChkInHirer,x.Status from "
                            strsql = strsql & " (select  docentry,U_Z_OutDate 'OutDate',U_Z_ItemCode 'ItemCode',U_Z_InDate,U_Z_InTime,U_Z_Status 'Status',"
                            strsql = strsql & " (U_Z_InDate + cast(CAST(U_Z_InTime / 100 as varchar) + ':' + CAST(U_Z_InTime % 100 as varchar) as datetime)) as 'PickUpDate',"
                            strsql = strsql & " (U_Z_ChkOutDt + cast(CAST(U_Z_ChkOutTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutTm % 100 as varchar) as datetime)) as 'ChkOutDriver',"
                            strsql = strsql & " (U_Z_ChkOutCuDt + cast(CAST(U_Z_ChkOutCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutCUTm % 100 as varchar) as datetime)) as 'ChkOutHirer',"
                            strsql = strsql & " (U_Z_ChkInDt + cast(CAST(U_Z_ChkInTm / 100 as varchar) + ':' + CAST(U_Z_ChkInTm % 100 as varchar) as datetime)) as 'ChkInDriver',"
                            strsql = strsql & " (U_Z_ChkInCuDt + cast(CAST(U_Z_ChkInCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkInCUTm % 100 as varchar) as datetime)) as 'ChkInHirer'"
                            strsql = strsql & " from [@Z_ORDR] ) as x "
                            Dim oTestRs, oRec2 As SAPbobsCOM.Recordset
                            oTestRs = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            oRec2 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                            strDate = " select convert(dateTime,U_Z_Date,105) from [@Z_Salik] where Code='" & strCode & "'"
                            strsql = strsql & " where (x.ItemCode='" & strItemCode & "') and  (" & strDate & ") between x.chkOutDriver and isnull(x.ChkInDriver,x.OutDate)"
                            oTestRs.DoQuery(strsql)
                            If oTestRs.RecordCount > 0 Then
                                oUserTable.GetByKey(strCode)
                                oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                oRec2.DoQuery("select DocEntry,case U_Z_Type when 'C' then 'Customer' else 'NRM' end,U_Z_CardCode,U_Z_CardName,U_Z_ItemCode,U_Z_ItemName,U_Z_FromLoc,U_Z_InDate,U_Z_OutDate,U_Z_toLoc from [@Z_ORDR] where  U_Z_ItemCode='" & strItemCode & "' and  DocEntry=" & oTestRs.Fields.Item(0).Value)
                                oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = oRec2.Fields.Item(1).Value.ToString
                                oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = oRec2.Fields.Item(2).Value.ToString
                                oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = oRec2.Fields.Item(3).Value.ToString
                                'oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                oUserTable.Update()
                            End If
                        Catch ex As Exception
                        End Try
                    End If
                Else
                    If oUserTable.GetByKey(strCode) Then
                        oUserTable.Name = strCode
                        oUserTable.UserFields.Fields.Item("U_Z_TransNo").Value = aField1
                        oUserTable.UserFields.Fields.Item("U_Z_Date").Value = aField2
                        oUserTable.UserFields.Fields.Item("U_Z_PlateNo").Value = afield3
                        oUserTable.UserFields.Fields.Item("U_Z_PlateCode").Value = afield4
                        oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = strItemCode
                        oUserTable.UserFields.Fields.Item("U_Z_Source").Value = afield5
                        oUserTable.UserFields.Fields.Item("U_Z_Amount").Value = afield6
                        oUserTable.UserFields.Fields.Item("U_Z_SalikAmount").Value = oApplication.Utilities.getDocumentQuantity(afield6.Replace("AED", ""))
                        oUserTable.UserFields.Fields.Item("U_Z_Total").Value = oApplication.Utilities.getDocumentQuantity(afield6.Replace("AED", ""))
                        'ousertable.UserFields.Fields.Item("U_Z_Status").Value="O"
                        If oUserTable.Update() <> 0 Then
                            oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            Return False
                        Else
                            Dim strsql, strDate As String
                            Try
                                strsql = "select x.DocEntry,x.OutDate,x.ItemCode,x.Pickupdate,x.ChkOutDriver,x.ChkOutHirer,x.ChkInDriver,x.ChkInHirer,x.Status from "
                                strsql = strsql & " (select  docentry,U_Z_OutDate 'OutDate',U_Z_ItemCode 'ItemCode',U_Z_InDate,U_Z_InTime,U_Z_Status 'Status',"
                                strsql = strsql & " (U_Z_InDate + cast(CAST(U_Z_InTime / 100 as varchar) + ':' + CAST(U_Z_InTime % 100 as varchar) as datetime)) as 'PickUpDate',"
                                strsql = strsql & " (U_Z_ChkOutDt + cast(CAST(U_Z_ChkOutTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutTm % 100 as varchar) as datetime)) as 'ChkOutDriver',"
                                strsql = strsql & " (U_Z_ChkOutCuDt + cast(CAST(U_Z_ChkOutCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkOutCUTm % 100 as varchar) as datetime)) as 'ChkOutHirer',"
                                strsql = strsql & " (U_Z_ChkInDt + cast(CAST(U_Z_ChkInTm / 100 as varchar) + ':' + CAST(U_Z_ChkInTm % 100 as varchar) as datetime)) as 'ChkInDriver',"
                                strsql = strsql & " (U_Z_ChkInCuDt + cast(CAST(U_Z_ChkInCUTm / 100 as varchar) + ':' + CAST(U_Z_ChkInCUTm % 100 as varchar) as datetime)) as 'ChkInHirer'"
                                strsql = strsql & " from [@Z_ORDR] ) as x "
                                Dim oTestRs, oRec2 As SAPbobsCOM.Recordset
                                oTestRs = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                oRec2 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                                'strDate = " Update [@Z_TRAF] set U_Z_FineDate=convert(Datetime,U_Z_Date,105) + U_Z_Time  where Code='" & strCode & "'"
                                'oTestRs.DoQuery(strDate)
                                strDate = " select convert(dateTime,U_Z_Date,105) from [@Z_Salik] where Code='" & strCode & "'"
                                strsql = strsql & " where  (x.ItemCode='" & strItemCode & "') and  (" & strDate & ") between x.chkOutDriver and isnull(x.ChkInDriver,x.OutDate)"
                                oTestRs.DoQuery(strsql)
                                If oTestRs.RecordCount > 0 Then
                                    oUserTable.GetByKey(strCode)
                                    oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                    oRec2.DoQuery("select DocEntry,case U_Z_Type when 'C' then 'Customer' else 'NRM' end,U_Z_CardCode,U_Z_CardName,U_Z_ItemCode,U_Z_ItemName,U_Z_FromLoc,U_Z_InDate,U_Z_OutDate,U_Z_toLoc from [@Z_ORDR] where  U_Z_ItemCode='" & strItemCode & "' and  DocEntry=" & oTestRs.Fields.Item(0).Value)
                                    oUserTable.UserFields.Fields.Item("U_Z_BookType").Value = oRec2.Fields.Item(1).Value.ToString
                                    oUserTable.UserFields.Fields.Item("U_Z_CardCode").Value = oRec2.Fields.Item(2).Value.ToString
                                    oUserTable.UserFields.Fields.Item("U_Z_CardName").Value = oRec2.Fields.Item(3).Value.ToString
                                    'oUserTable.UserFields.Fields.Item("U_Z_BookRef").Value = oTestRs.Fields.Item(0).Value.ToString
                                    oUserTable.Update()
                                End If
                            Catch ex As Exception
                            End Try
                        End If
                    End If
                End If
            End If
        Next

    End Function

    Private Function addtoSerial(ByVal aField1 As String, ByVal aField2 As String, ByVal afield3 As String) As Boolean
        Dim oUserTable As SAPbobsCOM.UserTable
        Dim orec, orec1 As SAPbobsCOM.Recordset
        Dim strCode, stFromdate, stToDate, strHoursworked As String
        Dim dblDifference As Double
        orec = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        orec1 = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        Dim dtDate, dtTodate, dtTemp As Date
        Dim strWorkingHours, strActualworkinghours, strItemCode As String
        Dim dblworkinghours, dblOverTime As Double
        For intRow As Integer = 1 To 1
            If aField1 <> "" Then
                'strCode = oGrid.DataTable.GetValue(0, intRow)
                oUserTable = oApplication.Company.UserTables.Item("Z_OSRI")
                orec.DoQuery("Select * from [@Z_OSRI] where U_Z_ItemCode='" & aField1 & "' and U_Z_SerialNo='" & afield3 & "'")
                If orec.RecordCount > 0 Then
                    strCode = orec.Fields.Item("Code").Value
                Else
                    strCode = ""
                End If
                '  strItemCode = afield3 & "-" & afield4
                If strCode = "" Then
                    strCode = oApplication.Utilities.getMaxCode("@Z_OSRI", "Code")
                    oUserTable.Code = strCode
                    oUserTable.Name = strCode
                    oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = aField1
                    oUserTable.UserFields.Fields.Item("U_Z_ItemName").Value = aField2
                    oUserTable.UserFields.Fields.Item("U_Z_SerialNo").Value = afield3
                    oUserTable.UserFields.Fields.Item("U_Z_Status").Value = "N"
                    orec.DoQuery("Select * from OSRI where ItemCode='" & aField1 & "' and IntrSerial='" & afield3 & "'") ' and status=0")
                    If orec.RecordCount > 0 Then
                        oUserTable.UserFields.Fields.Item("U_Z_Available").Value = "Already Exists "
                    Else
                        oUserTable.UserFields.Fields.Item("U_Z_Available").Value = "New"
                    End If


                    If oUserTable.Add() <> 0 Then
                        oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                        Return False
                    Else
                    End If
                Else
                    If oUserTable.GetByKey(strCode) Then
                        oUserTable.Code = strCode
                        oUserTable.Name = strCode
                        oUserTable.UserFields.Fields.Item("U_Z_ItemCode").Value = aField1
                        oUserTable.UserFields.Fields.Item("U_Z_ItemName").Value = aField2
                        oUserTable.UserFields.Fields.Item("U_Z_SerialNo").Value = afield3
                        oUserTable.UserFields.Fields.Item("U_Z_Status").Value = "N"
                        orec.DoQuery("Select * from OSRI where ItemCode='" & aField1 & "' and IntrSerial='" & afield3 & "' and status=0")
                        If orec.RecordCount > 0 Then
                            oUserTable.UserFields.Fields.Item("U_Z_Available").Value = "Already Available"
                        Else
                            oUserTable.UserFields.Fields.Item("U_Z_Available").Value = "New"
                        End If

                        If oUserTable.Update() <> 0 Then
                            oApplication.Utilities.Message(oApplication.Company.GetLastErrorDescription, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
                            Return False
                        Else
                        End If
                    End If
                End If
            End If
        Next
        Return True
    End Function
#End Region

    Public Function ReadSerial(ByVal afilename As String, ByVal aForm As SAPbouiCOM.Form) As SAPbouiCOM.DataTable
        Dim StrTmp, strcode As String
        Dim dt As System.Data.DataTable = New DataTable
        Dim strCardcode, strNumAtCard As String
        Dim oUsertable As SAPbobsCOM.UserTable
        Dim oTempPick As SAPbobsCOM.Recordset
        Try
            ISErr = False
            Dim intBaseEntry, intBaseLine As Integer
            Dim dblRecQty, dblUnitprice, dblQty As Double
            Dim strPOdate, strDocDate, strComments, strBatch, strMsg1, strMsg2, strMsg3, strItemName, strItemcode As String
            Dim wholeFile As String
            Dim strField1, strField2, strField3, strField4, strField5, strField6, strField7 As String
            Dim lineData() As String
            Dim fieldData() As String
            Dim filepath As String = afilename
            wholeFile = My.Computer.FileSystem.ReadAllText(filepath)
            lineData = Split(wholeFile, vbNewLine)
            oTempPick = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oTempPick.DoQuery("Delete from [@Z_OSRI]")

            Dim i As Integer = -1
            For Each lineOfText As String In lineData
                i = i + 1
                fieldData = lineOfText.Split(vbTab)
                If fieldData.Length >= 2 Then
                    oStaticText = aForm.Items.Item("12").Specific
                    oStaticText.Caption = "Processing...."
                    'oApplication.Utilities.Message("Processin...", SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                    strField1 = fieldData(0)
                    strField2 = fieldData(1)
                    strField3 = fieldData(2)
                    If strField1 <> "" Then
                        addtoSerial(strField1, strField2, strField3)
                    End If
                End If
            Next lineOfText
            oStaticText = aForm.Items.Item("12").Specific
            oStaticText.Caption = " "
            oApplication.Utilities.Message("Import completed successfully", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
            oGrid = aForm.Items.Item("7").Specific
            oGrid.DataTable.ExecuteQuery("Select U_Z_ItemCode 'ItemCode',U_Z_ItemName 'Item Name',U_Z_SerialNo 'Serial Number',U_Z_Available 'Status' from [@Z_OSRI]")
            Return dtTemp
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            ISErr = True
            Return Nothing
        End Try
    End Function

    Public Function ReadTraffic(ByVal afilename As String, ByVal aForm As SAPbouiCOM.Form) As SAPbouiCOM.DataTable
        Dim StrTmp, strcode As String
        Dim dt As System.Data.DataTable = New DataTable
        Dim strCardcode, strNumAtCard As String
        Dim oUsertable As SAPbobsCOM.UserTable
        Dim oTempPick As SAPbobsCOM.Recordset
        Try
            ISErr = False
            Dim intBaseEntry, intBaseLine As Integer
            Dim dblRecQty, dblUnitprice, dblQty As Double
            Dim strPOdate, strDocDate, strComments, strBatch, strMsg1, strMsg2, strMsg3, strItemName, strItemcode As String
            Dim wholeFile As String
            Dim strField1, strField2, strField3, strField4, strField5, strField6, strField7, strField8, strField9 As String
            Dim lineData() As String
            Dim fieldData() As String
            Dim filepath As String = afilename
            wholeFile = My.Computer.FileSystem.ReadAllText(filepath)
            lineData = Split(wholeFile, vbNewLine)
            Dim i As Integer = -1
            For Each lineOfText As String In lineData
                i = i + 1
                fieldData = lineOfText.Split(vbTab)
                If fieldData.Length = 9 Then
                    oStaticText = aForm.Items.Item("12").Specific
                    oStaticText.Caption = "Processing...."
                    'oApplication.Utilities.Message("Processin...", SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
                    strField1 = fieldData(0)
                    strField2 = fieldData(1)
                    strField3 = fieldData(2)
                    strField4 = fieldData(3)
                    strField5 = fieldData(4)
                    strField6 = fieldData(5)
                    strField7 = fieldData(6)
                    strField8 = fieldData(7)
                    strField9 = fieldData(8)
                    If strField1 <> "" Then
                        AddtoTraffic(strField1, strField2, strField3, strField4, strField5, strField6, strField7, strField8, strField9)
                    End If
                End If
            Next lineOfText
            oStaticText = aForm.Items.Item("12").Specific
            oStaticText.Caption = " "
            oApplication.Utilities.Message("Import completed successfully", SAPbouiCOM.BoStatusBarMessageType.smt_Success)
            Return dtTemp
        Catch ex As Exception
            oApplication.Utilities.Message(ex.Message, SAPbouiCOM.BoStatusBarMessageType.smt_Error)
            ISErr = True
            Return Nothing
        End Try
    End Function
#End Region

#Region "Menu Event"
    Public Overrides Sub MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean)
        Try
            Select Case pVal.MenuUID
                'Case mnu_Import

                '    oForm = objForm
                Case mnu_FIRST, mnu_LAST, mnu_NEXT, mnu_PREVIOUS
            End Select
            If (pVal.MenuUID = "BOC_FImport" And pVal.BeforeAction = True) Then
                Try
                Catch ex As Exception
                End Try


            End If
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
Public Class WindowWrapper
    Implements System.Windows.Forms.IWin32Window
    Private _hwnd As IntPtr

    Public Sub New(ByVal handle As IntPtr)
        _hwnd = handle
    End Sub
    Public ReadOnly Property Handle() As System.IntPtr Implements System.Windows.Forms.IWin32Window.Handle
        Get
            Return _hwnd
        End Get
    End Property
End Class

