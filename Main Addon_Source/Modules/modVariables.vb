Public Module modVariables

    Public oApplication As clsListener
    Public strSQL As String
    Public cfl_Text As String
    Public cfl_Btn As String

    Public CompanyDecimalSeprator As String
    Public CompanyThousandSeprator As String
    Public frm_SourceSerialForm As SAPbouiCOM.Form

    Public Enum ValidationResult As Integer
        CANCEL = 0
        OK = 1
    End Enum

    Public Enum DocumentType As Integer
        RENTAL_QUOTATION = 1
        RENTAL_ORDER
        RENTAL_RETURN
    End Enum
    Public Const frm_SerialNoGeneration As Integer = 21
    Public Const frm_COA As String = "804"
    Public Const frm_SerImport As String = "frm_SerialImport"
    Public Const xml_SerImport As String = "xml_SerialImport.xml"

    Public Const frm_APServiceinvoice As String = "141"

    Public Const frm_SerialAssigment As String = "25"
    Public Const frm_SerialAssigmnetImport As String = "frm_SerialAssign"
    Public Const xml_SerialAssigment As String = "xml_SerialAssign.xml"

    Public Const frm_WAREHOUSES As Integer = 62
    Public Const frm_ItemMaster As Integer = 150
    Public Const frm_BPMaster As Integer = 134
    Public Const frm_SalesInvoice As Integer = 133
    Public Const frm_CreditNotes As Integer = 179
    Public Const frm_InvoicePayment As Integer = 60090
    Public Const frm_StockTransfer As String = "940"

    Public Const frm_StockRequest As String = "frm_StRequest"
    Public Const frm_GoodsIssue As String = "frm_GoodsIssue"

    Public Const frm_BatchOrders As String = "frm_BatchOrders"
    Public Const mnu_FIND As String = "1281"
    Public Const mnu_ADD As String = "1282"
    Public Const mnu_CLOSE As String = "1286"
    Public Const mnu_NEXT As String = "1288"
    Public Const mnu_PREVIOUS As String = "1289"
    Public Const mnu_FIRST As String = "1290"
    Public Const mnu_LAST As String = "1291"
    Public Const mnu_ADD_ROW As String = "1292"
    Public Const mnu_DELETE_ROW As String = "1293"
    Public Const mnu_TAX_GROUP_SETUP As String = "8458"
    Public Const mnu_DEFINE_ALTERNATIVE_ITEMS As String = "11531"
    Public Const mnu_BatchOrders As String = "DABT_411"
    Public Const mnu_StRequest As String = "DABT_511"
    Public Const mnu_GoodsIssue As String = "DABT_512"

    Public Const xml_MENU As String = "Menu.xml"
    Public Const xml_MENU_REMOVE As String = "RemoveMenus.xml"
    Public Const xml_BatchOrders As String = "BatchOrders.xml"
    Public Const xml_StRequest As String = "StRequest.xml"
    Public Const xml_GoodsIssue As String = "GoodsIssue.xml"


    Public Const frm_DocDetails As String = "frm_DocDetails1"
    Public Const xml_DocDetails As String = "frm_DocDetails.xml"
    Public Const mnu_DocDetails As String = "mnu_Draft"

    Public Const frm_UpdateJournal As String = "frm_Update"
    Public Const xml_UpdateJournal As String = "frm_Update.xml"
    Public Const mnu_UpdateJournal As String = "mnu_Update"

End Module
