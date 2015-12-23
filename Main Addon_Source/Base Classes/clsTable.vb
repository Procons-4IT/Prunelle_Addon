Public NotInheritable Class clsTable

#Region "Private Functions"
    '*************************************************************************************************************
    'Type               : Private Function
    'Name               : AddTables
    'Parameter          : 
    'Return Value       : none
    'Author             : Manu
    'Created Dt         : 
    'Last Modified By   : 
    'Modified Dt        : 
    'Purpose            : Generic Function for adding all Tables in DB. This function shall be called by 
    '                     public functions to create a table
    '**************************************************************************************************************
    Private Sub AddTables(ByVal strTab As String, ByVal strDesc As String, ByVal nType As SAPbobsCOM.BoUTBTableType)
        Dim oUserTablesMD As SAPbobsCOM.UserTablesMD
        Try
            oUserTablesMD = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)
            'Adding Table
            If Not oUserTablesMD.GetByKey(strTab) Then
                oUserTablesMD.TableName = strTab
                oUserTablesMD.TableDescription = strDesc
                oUserTablesMD.TableType = nType
                If oUserTablesMD.Add <> 0 Then
                    Throw New Exception(oApplication.Company.GetLastErrorDescription)
                End If
            End If
        Catch ex As Exception
            Throw ex
        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTablesMD)
            oUserTablesMD = Nothing
            GC.WaitForPendingFinalizers()
            GC.Collect()
        End Try
    End Sub

    '*************************************************************************************************************
    'Type               : Private Function
    'Name               : AddFields
    'Parameter          : SstrTab As String,strCol As String,
    '                     strDesc As String,nType As Integer,i,nEditSize,nSubType As Integer
    'Return Value       : none
    'Author             : Manu
    'Created Dt         : 
    'Last Modified By   : 
    'Modified Dt        : 
    'Purpose            : Generic Function for adding all Fields in DB Tables. This function shall be called by 
    '                     public functions to create a Field
    '**************************************************************************************************************
    Private Sub AddFields(ByVal strTab As String, _
                            ByVal strCol As String, _
                                ByVal strDesc As String, _
                                    ByVal nType As SAPbobsCOM.BoFieldTypes, _
                                        Optional ByVal i As Integer = 0, _
                                            Optional ByVal nEditSize As Integer = 10, _
                                                Optional ByVal nSubType As SAPbobsCOM.BoFldSubTypes = 0, _
                                                    Optional ByVal Mandatory As SAPbobsCOM.BoYesNoEnum = SAPbobsCOM.BoYesNoEnum.tNO)
        Dim oUserFieldMD As SAPbobsCOM.UserFieldsMD
        Try

            If Not (strTab = "OADM" Or strTab = "OWHS" Or strTab = "OJDT" Or strTab = "OITM" Or strTab = "INV1" Or strTab = "OWTR" Or strTab = "OINV" Or strTab = "ORCT" Or strTab = "OVPM") Then
                strTab = "@" + strTab
            End If

            If Not IsColumnExists(strTab, strCol) Then
                oUserFieldMD = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)

                oUserFieldMD.Description = strDesc
                oUserFieldMD.Name = strCol
                oUserFieldMD.Type = nType
                oUserFieldMD.SubType = nSubType
                oUserFieldMD.TableName = strTab
                oUserFieldMD.EditSize = nEditSize
                oUserFieldMD.Mandatory = Mandatory
                If oUserFieldMD.Add <> 0 Then
                    Throw New Exception(oApplication.Company.GetLastErrorDescription)
                End If

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldMD)

            End If

        Catch ex As Exception
            Throw ex
        Finally
            oUserFieldMD = Nothing
            GC.WaitForPendingFinalizers()
            GC.Collect()
        End Try
    End Sub

    Public Sub addField(ByVal TableName As String, ByVal ColumnName As String, ByVal ColDescription As String, ByVal FieldType As SAPbobsCOM.BoFieldTypes, ByVal Size As Integer, ByVal SubType As SAPbobsCOM.BoFldSubTypes, ByVal ValidValues As String, ByVal ValidDescriptions As String, ByVal SetValidValue As String)
        Dim intLoop As Integer
        Dim strValue, strDesc As Array
        Dim objUserFieldMD As SAPbobsCOM.UserFieldsMD
        Try

            strValue = ValidValues.Split(Convert.ToChar(","))
            strDesc = ValidDescriptions.Split(Convert.ToChar(","))
            If (strValue.GetLength(0) <> strDesc.GetLength(0)) Then
                Throw New Exception("Invalid Valid Values")
            End If




            If (Not IsColumnExists(TableName, ColumnName)) Then
                objUserFieldMD = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)
                objUserFieldMD.TableName = TableName
                objUserFieldMD.Name = ColumnName
                objUserFieldMD.Description = ColDescription
                objUserFieldMD.Type = FieldType
                If (FieldType <> SAPbobsCOM.BoFieldTypes.db_Numeric) Then
                    objUserFieldMD.Size = Size
                Else
                    objUserFieldMD.EditSize = Size
                End If
                objUserFieldMD.SubType = SubType
                objUserFieldMD.DefaultValue = SetValidValue
                For intLoop = 0 To strValue.GetLength(0) - 1
                    objUserFieldMD.ValidValues.Value = strValue(intLoop)
                    objUserFieldMD.ValidValues.Description = strDesc(intLoop)
                    objUserFieldMD.ValidValues.Add()
                Next
                If (objUserFieldMD.Add() <> 0) Then
                End If
                System.Runtime.InteropServices.Marshal.ReleaseComObject(objUserFieldMD)
            Else
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            objUserFieldMD = Nothing
            GC.WaitForPendingFinalizers()
            GC.Collect()
        End Try


    End Sub

    '*************************************************************************************************************
    'Type               : Private Function
    'Name               : IsColumnExists
    'Parameter          : ByVal Table As String, ByVal Column As String
    'Return Value       : Boolean
    'Author             : Manu
    'Created Dt         : 
    'Last Modified By   : 
    'Modified Dt        : 
    'Purpose            : Function to check if the Column already exists in Table
    '**************************************************************************************************************
    Private Function IsColumnExists(ByVal Table As String, ByVal Column As String) As Boolean
        Dim oRecordSet As SAPbobsCOM.Recordset

        Try
            strSQL = "SELECT COUNT(*) FROM CUFD WHERE TableID = '" & Table & "' AND AliasID = '" & Column & "'"
            oRecordSet = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRecordSet.DoQuery(strSQL)

            If oRecordSet.Fields.Item(0).Value = 0 Then
                Return False
            Else
                Return True
            End If
        Catch ex As Exception
            Throw ex
        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRecordSet)
            oRecordSet = Nothing
            GC.Collect()
        End Try
    End Function

    Private Sub AddKey(ByVal strTab As String, ByVal strColumn As String, ByVal strKey As String, ByVal i As Integer)
        Dim oUserKeysMD As SAPbobsCOM.UserKeysMD

        Try
            '// The meta-data object must be initialized with a
            '// regular UserKeys object
            oUserKeysMD = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserKeys)

            If Not oUserKeysMD.GetByKey("@" & strTab, i) Then

                '// Set the table name and the key name
                oUserKeysMD.TableName = strTab
                oUserKeysMD.KeyName = strKey

                '// Set the column's alias
                oUserKeysMD.Elements.ColumnAlias = strColumn
                oUserKeysMD.Elements.Add()
                oUserKeysMD.Elements.ColumnAlias = "RentFac"

                '// Determine whether the key is unique or not
                oUserKeysMD.Unique = SAPbobsCOM.BoYesNoEnum.tYES

                '// Add the key
                If oUserKeysMD.Add <> 0 Then
                    Throw New Exception(oApplication.Company.GetLastErrorDescription)
                End If

            End If

        Catch ex As Exception
            Throw ex

        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserKeysMD)
            oUserKeysMD = Nothing
            GC.Collect()
            GC.WaitForPendingFinalizers()
        End Try

    End Sub

    '********************************************************************
    'Type		            :   Function    
    'Name               	:	AddUDO
    'Parameter          	:   
    'Return Value       	:	Boolean
    'Author             	:	
    'Created Date       	:	
    'Last Modified By	    :	
    'Modified Date        	:	
    'Purpose             	:	To Add a UDO for Transaction Tables
    '********************************************************************
    Private Sub AddUDO(ByVal strUDO As String, ByVal strDesc As String, ByVal strTable As String, _
                                Optional ByVal sFind1 As String = "", Optional ByVal sFind2 As String = "", _
                                        Optional ByVal strChildTbl As String = "", Optional ByVal nObjectType As SAPbobsCOM.BoUDOObjType = SAPbobsCOM.BoUDOObjType.boud_Document)

        Dim oUserObjectMD As SAPbobsCOM.UserObjectsMD
        Try
            oUserObjectMD = oApplication.Company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD)
            If oUserObjectMD.GetByKey(strUDO) = 0 Then
                oUserObjectMD.CanCancel = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjectMD.CanClose = SAPbobsCOM.BoYesNoEnum.tYES
                oUserObjectMD.CanCreateDefaultForm = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjectMD.CanDelete = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjectMD.CanFind = SAPbobsCOM.BoYesNoEnum.tYES

                If sFind1 <> "" And sFind2 <> "" Then
                    oUserObjectMD.FindColumns.ColumnAlias = sFind1
                    oUserObjectMD.FindColumns.Add()
                    oUserObjectMD.FindColumns.SetCurrentLine(1)
                    oUserObjectMD.FindColumns.ColumnAlias = sFind2
                    oUserObjectMD.FindColumns.Add()
                End If

                oUserObjectMD.CanLog = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjectMD.LogTableName = ""
                oUserObjectMD.CanYearTransfer = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjectMD.ExtensionName = ""

                If strChildTbl <> "" Then
                    oUserObjectMD.ChildTables.TableName = strChildTbl
                End If

                oUserObjectMD.ManageSeries = SAPbobsCOM.BoYesNoEnum.tNO
                oUserObjectMD.Code = strUDO
                oUserObjectMD.Name = strDesc
                oUserObjectMD.ObjectType = nObjectType
                oUserObjectMD.TableName = strTable

                If oUserObjectMD.Add() <> 0 Then
                    Throw New Exception(oApplication.Company.GetLastErrorDescription)
                End If
            End If

        Catch ex As Exception
            Throw ex

        Finally
            System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjectMD)
            oUserObjectMD = Nothing
            GC.WaitForPendingFinalizers()
            GC.Collect()
        End Try

    End Sub

#End Region

#Region "Public Functions"
    '*************************************************************************************************************
    'Type               : Public Function
    'Name               : CreateTables
    'Parameter          : 
    'Return Value       : none
    'Author             : Manu
    'Created Dt         : 
    'Last Modified By   : 
    'Modified Dt        : 
    'Purpose            : Creating Tables by calling the AddTables & AddFields Functions
    '**************************************************************************************************************
    Public Sub CreateTables()
        Dim oProgressBar As SAPbouiCOM.ProgressBar
        Try

            'oProgressBar = oApplication.SBO_Application.StatusBar.CreateProgressBar("Initializing Database...", 8, False)
            'oProgressBar.Value = 0
            'oProgressBar.Text = "Initializing Database... "

            oApplication.Utilities.Message("Initializing Database...", SAPbouiCOM.BoStatusBarMessageType.smt_Warning)
            ' AddFields("OADM", "SupCode", "Default Supplier Code", SAPbobsCOM.BoFieldTypes.db_Alpha, 20)
            addField("OINV", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OINV", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("ORCT", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("ODPS", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")

            AddFields("OINV", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OINV", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OINV", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)


            AddFields("ORCT", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("ORCT", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("ORCT", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)

            AddFields("OVPM", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OVPM", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OVPM", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)

            addField("OITB", "ItemType", "Item Group Type", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "F,R,P,O,C,G,S", "FinishedProduct,Raw Material,Packaging,Office Supliers,Consumables,General,Semi-Finished", "G")


            addField("OWTR", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OWTR", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")

            addField("OCRD", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")
            addField("OITM", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")
            AddFields("OWHS", "Warcode", "Branch Warehouse", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            AddFields("OWHS", "Branch", "Branch DB Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)



            addField("OADM", "MasExport", "Master Data Exporting", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            ' addField("OADM", "JEExport", "Journal Entry Exporting", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")

            addField("OJDT", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OJDT", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")

            addField("OCRD", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")
            addField("OITM", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")
            addField("OACT", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")
            addField("OITT", "Action", "Record Action", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "A,U,N", "New Addition,Updation,Not Applicable", "N")


            AddFields("OJDT", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OJDT", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OJDT", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)


            AddFields("OWOR", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OWOR", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OWOR", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)
            addField("OWOR", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OWOR", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")

            AddFields("OVPM", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OVPM", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OVPM", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)
            addField("OVPM", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OVPM", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")



            AddFields("ORDR", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("ORDR", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("ORDR", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)


            AddFields("OWTR", "BaseEntry", "Base Document Entry", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OWTR", "BaseNum", "Base Document Number", SAPbobsCOM.BoFieldTypes.db_Numeric, , 10)
            AddFields("OWTR", "Branch", "Base Company  Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)

            'addField("ORDR", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("ORDR", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")


            addField("OITT", "Export", "Exported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("OITT", "Import", "Imported", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")


            'AddTables("Z_OSRI", "Serial Number temporary table", SAPbobsCOM.BoUTBTableType.bott_NoObject)
            'AddFields("Z_OSRI", "Z_ItemCode", "Item Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("Z_OSRI", "Z_ItemName", "Item Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 200)
            'AddFields("Z_OSRI", "Z_SerialNo", "Manufacture Serial Number", SAPbobsCOM.BoFieldTypes.db_Alpha, , 100)
            'AddFields("Z_OSRI", "Z_Status", "Status", SAPbobsCOM.BoFieldTypes.db_Alpha, , 1)
            'AddFields("Z_OSRI", "Z_Available", "Available in SAP", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)



            'AddTables("DABT_STRHeader", "Stock Transfer request Header", SAPbobsCOM.BoUTBTableType.bott_Document)
            'AddFields("DABT_STRHeader", "DocDate", "Document Date", SAPbobsCOM.BoFieldTypes.db_Date)
            'AddFields("DABT_STRHeader", "DueDate", "Doc Due Date", SAPbobsCOM.BoFieldTypes.db_Date)
            'AddFields("DABT_STRHeader", "WhsCode", "Warehouse Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'addField("DABT_STRHeader", "Status", "Status", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "O,I", "Open,Imported", "O")

            'AddTables("DABT_STRLines", "Stock Transfer request Lines", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
            'AddFields("DABT_STRLines", "ItemCode", "Item Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("DABT_STRLines", "ItemName", "Item Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 200)
            'AddFields("DABT_STRLines", "Qty", "Quantity", SAPbobsCOM.BoFieldTypes.db_Float, , , SAPbobsCOM.BoFldSubTypes.st_Quantity)


            'AddTables("DABT_StImport", "Stock transfer Import", SAPbobsCOM.BoUTBTableType.bott_NoObject)
            'AddFields("DABT_StImport", "TransDate", "Transfer request date", SAPbobsCOM.BoFieldTypes.db_Date)
            'AddFields("DABT_StImport", "TransWhs", "Transfer Request warehouse", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("DABT_StImport", "ItemCode", "Item Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("DABT_StImport", "ItemName", "Item Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 200)
            'AddFields("DABT_StImport", "ReqQty", "Requested Quantity", SAPbobsCOM.BoFieldTypes.db_Float, , , SAPbobsCOM.BoFldSubTypes.st_Quantity)

            'AddTables("DABT_GIHeader", "Stock Transfer Release Header", SAPbobsCOM.BoUTBTableType.bott_Document)
            'AddFields("DABT_GIHeader", "DocDate", "Document Date", SAPbobsCOM.BoFieldTypes.db_Date)
            'AddFields("DABT_GIHeader", "WhsCode", "Warehouse Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'addField("DABT_GIHeader", "Status", "Status", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "O,C", "Open,Closed", "O")
            'AddFields("DABT_GIHeader", "GIDocEntry", "Goods Issue DocEntry", SAPbobsCOM.BoFieldTypes.db_Numeric)
            'AddFields("DABT_GIHeader", "GIDocNum", "Goods Issue Docment Number", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)


            'AddTables("DABT_GILines", "Stock Transfer Release Lines", SAPbobsCOM.BoUTBTableType.bott_DocumentLines)
            'AddFields("DABT_GILines", "ItemCode", "Item Code", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("DABT_GILines", "ItemName", "Item Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 200)
            'AddFields("DABT_GILines", "ReqDate", "Requested Date", SAPbobsCOM.BoFieldTypes.db_Date)
            'AddFields("DABT_GILines", "ReqQty", "ReqQuantity", SAPbobsCOM.BoFieldTypes.db_Float, , , SAPbobsCOM.BoFldSubTypes.st_Quantity)
            'AddFields("DABT_GILines", "ReqWhs", "Requested Warehouse", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            'AddFields("DABT_GILines", "IssueQty", "Issued Qty", SAPbobsCOM.BoFieldTypes.db_Float, , , SAPbobsCOM.BoFldSubTypes.st_Quantity)
            'AddFields("DABT_GILines", "RefNo", "Reference No", SAPbobsCOM.BoFieldTypes.db_Alpha, , 8)


            AddTables("Z_DBSYN", "Document Synchronization Setup", SAPbobsCOM.BoUTBTableType.bott_NoObject)
            AddFields("Z_DBSYN", "Table", "Table Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 10)
            AddFields("Z_DBSYN", "UdfName", "UDF Name", SAPbobsCOM.BoFieldTypes.db_Alpha, , 20)
            addField("@Z_DBSYN", "Active", "Active", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "Y,N", "Yes,No", "N")
            addField("@Z_DBSYN", "DocType", "Document Type", SAPbobsCOM.BoFieldTypes.db_Alpha, 1, SAPbobsCOM.BoFldSubTypes.st_Address, "S,P", "Sales,Purchase", "S")
            AddFields("Z_DBSYN", "Order", "Order", SAPbobsCOM.BoFieldTypes.db_Numeric)
            AddFields("Z_DBSYN", "FrmDate", "From Date", SAPbobsCOM.BoFieldTypes.db_Date)
            AddFields("Z_DBSYN", "ToDate", "End Date", SAPbobsCOM.BoFieldTypes.db_Date)


            AddFields("OJDT", "DocNum", "Document Number", SAPbobsCOM.BoFieldTypes.db_Alpha, 30)


            oApplication.Company.StartTransaction()

            '---- User Defined Object's
            '   CreateUDO()

            If oApplication.Company.InTransaction() Then
                oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
            End If

        Catch ex As Exception
            If oApplication.Company.InTransaction() Then
                oApplication.Company.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
            End If
            Throw ex
        Finally
            'oProgressBar.Stop()
            ' System.Runtime.InteropServices.Marshal.ReleaseComObject(oProgressBar)
            GC.Collect()
            GC.WaitForPendingFinalizers()
        End Try
    End Sub

    Public Sub CreateUDO()
        Try
            '  AddUDO("DABT_STR", "Stock Transfer Request", "DABT_STRHeader", "DocEntry", "U_DocDate", "DABT_STRLines", SAPbobsCOM.BoUDOObjType.boud_Document)
            ' AddUDO("DABT_STRelease", "Stock Transfer Release", "DABT_GIHeader", "DocEntry", "U_DocDate", "DABT_GILines", SAPbobsCOM.BoUDOObjType.boud_Document)

        Catch ex As Exception
            Throw ex
        End Try
    End Sub
#End Region

End Class
