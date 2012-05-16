Attribute VB_Name = "Helpers"
Option Explicit

Public Function LongToString(ByVal lLong As Long) As String
On Error GoTo ErrHandler
   LongToString = lLong
Exit Function

ErrHandler:
    MsgBox "Error: " & Err.Number & " " & Err.Description & " " & "LongToString"
End Function

Public Function StringToLong(ByVal str As String) As Long
On Error GoTo ErrHandler
    StringToLong = str
Exit Function

ErrHandler:
    MsgBox "Error: " & Err.Number & " " & Err.Description & " " & "StringToLong"
End Function
