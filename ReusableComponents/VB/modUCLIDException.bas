Attribute VB_Name = "modUCLIDException"
Public Sub DisplayUCLIDException(strELI As String)
    
    ' copy the error description before setting the error handler
    Dim strData As String
    strData = Err.Description
    On Error GoTo handleError

    ' create an exception object and display the exception
    Dim pEx As New UCLIDEXCEPTIONMGMTLib.COMUCLIDException
    pEx.createFromString strELI, strData
    pEx.display
    Exit Sub
    
    ' if there was a problem with displaying the exception,
    ' display a simple warning message.
handleError:
    MsgBox "An error was raised, but the application is unable to bring up the UCLIDException viewer!", vbExclamation
End Sub
