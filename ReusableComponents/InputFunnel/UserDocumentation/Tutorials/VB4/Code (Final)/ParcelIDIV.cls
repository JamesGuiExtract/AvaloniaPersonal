VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "ParcelIDIV"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Implements UCLID_INPUTFUNNELLib.IInputValidator

Private Sub Error_Handler(strErrNum As String)
    Dim ex As ICOMUCLIDException
    Set ex = New COMUCLIDException
    ex.CreateFromString strErrNum, Err.Description
    ex.AddDebugInfo "Error Number", Err.Number

    ' convert the UCLIDException to a string and throw as a COM error with
    ' description
    Dim strMsg As String
    strMsg = ex.AsStringizedByteStream
    
    ' throw the exception to the caller
    Err.Raise vbObjectError, "", strMsg
End Sub
    
Private Function IInputValidator_GetInputType() As String
    'This is an input validator for Parcel ID's
    IInputValidator_GetInputType = "Parcel ID"
End Function

Private Function IInputValidator_ValidateInput(ByVal pTextInput As UCLID_INPUTFUNNELLib.ITextInput) As Boolean
    ' Set error handler
    On Error GoTo errHandler
    
    'Default to invalid
    Dim bResult As Boolean
    bResult = False
    
    'Retrieve text string
    Dim txtInput As String
    txtInput = pTextInput.GetText
    
    'String must have exactly 9 characters
    If Len(txtInput) = 9 Then
        'Decompose the input
        Dim txtLeft As String
        txtLeft = Left(txtInput, 4)
        
        Dim txtRight As String
        txtRight = Right(txtInput, 4)
        
        Dim txtMid As String
        txtMid = Mid(txtInput, 5, 1)
        
        'Check for XXXX-XXXX format
        If IsNumeric(txtLeft) And IsNumeric(txtRight) And txtMid = "-" Then
            bResult = True
        End If
    End If
    
    'Return
    IInputValidator_ValidateInput = bResult
    Exit Function
errHandler:
    Error_Handler 14000
End Function
