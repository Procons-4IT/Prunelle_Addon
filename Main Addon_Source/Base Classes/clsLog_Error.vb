Public Class clsLog_Error
    Inherits Object

    Private Const log_PROCESS_ORDERS As String = "Log_ProcessOrders.txt"
    Private Const log_INVOICING As String = "Log_Invoicing.txt"

    'Private oFSO As Scripting.FileSystemObject
    '
    Public Enum Log As Integer
        lg_PROCESS_ORDER = 1
        lg_INVOICING
    End Enum

    Public Sub New()
        MyBase.New()
        ' oFSO = New Scripting.FileSystemObject
    End Sub

    

    Public Sub DeleteFile(ByVal Type As Log)
        Dim sLogFilePath As String

        Select Case Type
            Case Log.lg_PROCESS_ORDER
                sLogFilePath = oApplication.Utilities.getApplicationPath() & "\Log\" & log_PROCESS_ORDERS

            Case Log.lg_INVOICING
                sLogFilePath = oApplication.Utilities.getApplicationPath() & "\Log\" & log_INVOICING

        End Select
        
    End Sub

    Public Sub ShowLogFile(ByVal Type As Log)
        Dim sLogFilePath As String

        Select Case Type
            Case Log.lg_PROCESS_ORDER
                sLogFilePath = oApplication.Utilities.getApplicationPath() & "\Log\" & log_PROCESS_ORDERS

            Case Log.lg_INVOICING
                sLogFilePath = oApplication.Utilities.getApplicationPath() & "\Log\" & log_INVOICING

        End Select

        Shell("Notepad.exe " & sLogFilePath, AppWinStyle.NormalFocus)

    End Sub

    Protected Overrides Sub Finalize()

    End Sub

End Class
