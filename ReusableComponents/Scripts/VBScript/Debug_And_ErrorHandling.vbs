'--------------------------------------------------------------------------------------------------
' Extract Systems Utility Subroutines
'--------------------------------------------------------------------------------------------------
' Parses the specified command-line string starting with the specified zero-relative 
' argument index.  Will set one or more of the following global boolean parameters.
'    GBoolShowDebugInfo ===> -debug
'    GBoolLogErrorsOnly ===> -silent
' Requires:
'    GObjArgs - array of command-line arguments from WScript object
'    GBoolLogErrorsOnly - To be True if Exceptions will be logged and NOT displayed
'    GBoolShowDebugInfo - To be True if intermediate debug information should be displayed 
'       in message boxes
' Notes:
'    The default values for each of these parameters is False.
'    The parameters are declared in ScriptHeader.vbs
'--------------------------------------------------------------------------------------------------
Sub ParseCommandLineOptions(nFirstIndex)
    ' Define error handler
    On Error Resume Next

    ' Step through each option - starting with specified index
    Dim I, strOption, nCount
    nCount = GObjArgs.Count - 1
    For I = nFirstIndex To nCount
        ' Retrieve this command-line argument
        strOption = GObjArgs(I)
        
        ' Test for "-silent"
        If LCase(strOption) = "-silent" Then
            GBoolLogErrorsOnly = True
        End If

        ' Test for "-debug"
        If LCase(strOption) = "-debug" Then
            GBoolShowDebugInfo = True
        End If
    Next
End Sub

'--------------------------------------------------------------------------------------------------
' Handles errors by wrapping them in a COMUCLIDException object and 
' displaying or logging them depending on a command-line setting.  The 
' default behavior is Display.
' Requires:
'    ExceptionObject - UCLIDExceptionMgmt.COMUCLIDException object
' Notes:
'    Several variations of HandleScriptError are defined, providing different levels of support 
'       for debug information.
'    HandleScriptError0 - supports only strELI, strText
'    HandleScriptError1 - adds strName1, strValue1 for debug info
'    HandleScriptError2 - adds strName2, strValue2 for debug info
'    HandleScriptError3 - adds strName3, strValue3 for debug info
'    HandleScriptError_Internal - uses strELI, strText and three sets of name/value pairs 
'       for debug info
'--------------------------------------------------------------------------------------------------
Sub HandleScriptError0(strELI,strText)
    ' Do not define error handler

    ' Call the internal HandleScriptError subroutine w/o debug info
    Call HandleScriptError_Internal(strELI,strText,"","","","","","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub HandleScriptError1(strELI,strText,strName1,strValue1)
    ' Do not define error handler

    ' Call the internal HandleScriptError subroutine with 1 name/value pair for debug info
    Call HandleScriptError_Internal(strELI,strText,strName1,strValue1,"","","","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub HandleScriptError2(strELI,strText,strName1,strValue1,strName2,strValue2)
    ' Do not define error handler

    ' Call the internal HandleScriptError subroutine with 2 name/value pairs for debug info
    Call HandleScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,"","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub HandleScriptError3(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
    ' Do not define error handler

    ' Call the internal HandleScriptError subroutine with 3 name/value pairs for debug info
    Call HandleScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
End Sub
'--------------------------------------------------------------------------------------------------
Sub HandleScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
    ' Do not define error handler

    ' Check Error object status and return if no error is active
    If Err.number = 0 Then 
        Exit Sub
    End If

    ' Create script exception from string
    ExceptionObject.CreateFromString "ELI15737", "Script Exception"

    ' Add a history record with provided information
    ExceptionObject.AddHistoryRecord strELI, strText

    ' Add debug records with error number and description
    ExceptionObject.AddDebugInfo "Err.Number", CStr(Err.number)
    ExceptionObject.AddDebugInfo "Err.Description", Err.Description

    ' Add debug record for first name/value pair
    If strName1 <> "" Then
        ExceptionObject.AddDebugInfo strName1, strValue1
    End If

    ' Add debug record for second name/value pair
    If strName2 <> "" Then
        ExceptionObject.AddDebugInfo strName2, strValue2
    End If

    ' Add debug record for third name/value pair
    If strName3 <> "" Then
        ExceptionObject.AddDebugInfo strName3, strValue3
    End If

    ' Display the exception if desired
    If GBoolLogErrorsOnly = False Then
        ExceptionObject.Display
    End If

    ' Always log the exception
    ExceptionObject.Log
    
    ' Clear the error object
    Err.Clear
End Sub
'--------------------------------------------------------------------------------------------------
' Logs specified error information by wrapping it in a COMUCLIDException object and 
' logging it.
' Requires:
'    ExceptionObject - UCLIDExceptionMgmt.COMUCLIDException object
' Notes:
'    Several variations of LogScriptError are defined, providing different levels of support 
'       for debug information.
'    LogScriptError0 - supports only strELI, strText
'    LogScriptError1 - adds strName1, strValue1 for debug info
'    LogScriptError2 - adds strName2, strValue2 for debug info
'    LogScriptError3 - adds strName3, strValue3 for debug info
'    LogScriptError_Internal - uses strELI, strText and three sets of name/value pairs 
'       for debug info
'--------------------------------------------------------------------------------------------------
Sub LogScriptError0(strELI,strText)
    ' Do not define error handler

    ' Call the internal LogScriptError subroutine w/o debug info
    Call LogScriptError_Internal(strELI,strText,"","","","","","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub LogScriptError1(strELI,strText,strName1,strValue1)
    ' Do not define error handler

    ' Call the internal LogScriptError subroutine with 1 name/value pair for debug info
    Call LogScriptError_Internal(strELI,strText,strName1,strValue1,"","","","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub LogScriptError2(strELI,strText,strName1,strValue1,strName2,strValue2)
    ' Do not define error handler

    ' Call the internal LogScriptError subroutine with 2 name/value pairs for debug info
    Call LogScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,"","")
End Sub
'--------------------------------------------------------------------------------------------------
Sub LogScriptError3(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
    ' Do not define error handler

    ' Call the internal LogScriptError subroutine with 3 name/value pairs for debug info
    Call LogScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
End Sub
'--------------------------------------------------------------------------------------------------
Sub LogScriptError_Internal(strELI,strText,strName1,strValue1,strName2,strValue2,strName3,strValue3)
    ' Do not define error handler

    ' Create script exception from string
    ExceptionObject.CreateFromString "ELI20331", "Logged Script Exception"

    ' Add a history record with provided information
    ExceptionObject.AddHistoryRecord strELI, strText

    ' Add debug record for first name/value pair
    If strName1 <> "" Then
        ExceptionObject.AddDebugInfo strName1, strValue1
    End If

    ' Add debug record for second name/value pair
    If strName2 <> "" Then
        ExceptionObject.AddDebugInfo strName2, strValue2
    End If

    ' Add debug record for third name/value pair
    If strName3 <> "" Then
        ExceptionObject.AddDebugInfo strName3, strValue3
    End If

    ' Log the exception
    ExceptionObject.Log

    ' Clear the error object
    Err.Clear
End Sub

'--------------------------------------------------------------------------------------------------
' Central handler for Debug information where nothing is displayed to the user unless 
' "-debug" was included on the command line
' Will display a message box with "Script Debug Information" title
' and 
'    Label = strLabel
'    Value = strValue ( if non-empty )
'--------------------------------------------------------------------------------------------------
Sub HandleDebug(strLabel,strValue)
    ' Do not define error handler
    
    ' Check for unexpected error before call to HandleDebug
    HandleScriptError2 "ELI15894", "Unexpected error found at beginning of HandleDebug()!", "Label Parameter", strLabel, "Value Parameter", strValue

    ' Check global setting and return if debugging is disabled
    If GBoolShowDebugInfo = False Then 
        Exit Sub
    End If

    ' Prepare the message box
    If strValue <> "" Then
        ' Both Label and Value are defined
        MsgBox "LABEL = " + strLabel + vbCrLf + "VALUE = " + strValue,,"Script Debug Information"
    Else
        ' Value is not defined
        MsgBox "LABEL = " + strLabel,,"Script Debug Information"
    End If
End Sub
'--------------------------------------------------------------------------------------------------
