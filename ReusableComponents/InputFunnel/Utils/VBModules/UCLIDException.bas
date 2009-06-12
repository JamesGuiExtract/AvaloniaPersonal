Attribute VB_Name = "ExceptionHandling"
Sub DisplayUCLIDException(strELICode As String)
    ' get the current error string and then setup the new error handler
    Dim strErrDescription As String
    strErrDescription = Err.Description
    On Error GoTo errHandler
    
    ' Create handler for UCLID Exceptions
    Dim pEx As UCLID_EXCEPTIONMGMTLib.COMUCLIDException
    Set pEx = New COMUCLIDException
    
    ' Create the UCLID Exception from the ELI code and error description
    If (strErrDescription = "") Then
        Err.Raise vbObjectError + 513, "", "Error at location '" & strELICode & "'"
    Else
        pEx.CreateFromString strELICode, strErrDescription
    End If
    
    ' Display the UCLID Exception
    pEx.Display
    Exit Sub
    
errHandler:
    ' Display the error text in a MsgBox
    MsgBox Err.Description
End Sub
