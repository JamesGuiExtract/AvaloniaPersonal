'--------------------------------------------------------------------------------------------------
' Extract Systems Script Header
' Copyright 2007 onwards
'--------------------------------------------------------------------------------------------------
' Define error handler
Option Explicit
On Error Resume Next
Err.Clear

' Create COMUCLIDException object if needed to log errors
Dim ExceptionObject
Set ExceptionObject = CreateObject("UCLIDExceptionMgmt.COMUCLIDException")

' Define default error display and debug settings
Dim GBoolLogErrorsOnly, GBoolShowDebugInfo
GBoolLogErrorsOnly = False
GBoolShowDebugInfo = False

' Constants for file access
Const ForWriting = 2
Const ForReading = 1
Const ForAppending = 8

' Helper functions

Function strGetFolder(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract folder portion of filename by finding last backslash
    ' where:
    '    "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml" ====> "H:\Verification\Input"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        strGetFolder = Left(strPath, p-1)
    End If
End Function

Function strGetBaseName(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract basename portion of filename by finding last backslash
    ' where:
    '    "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml" ====> "O--1992-12-31-1-00774.TIF.xml"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        strGetBaseName = Right(strPath, Len(strPath)-p)
    End If
End Function
