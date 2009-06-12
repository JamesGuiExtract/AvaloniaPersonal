VERSION 5.00
Begin VB.Form frmCLDialog 
   Caption         =   "Command Line Input Receiver"
   ClientHeight    =   870
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   3945
   LinkTopic       =   "Form1"
   ScaleHeight     =   870
   ScaleWidth      =   3945
   StartUpPosition =   3  'Windows Default
   Begin VB.TextBox txtInput 
      Height          =   495
      Left            =   0
      TabIndex        =   0
      Top             =   360
      Width           =   3975
   End
   Begin VB.Label lblPrompt 
      Caption         =   "Prompt:"
      Height          =   375
      Left            =   0
      TabIndex        =   1
      Top             =   0
      Width           =   3855
   End
End
Attribute VB_Name = "frmCLDialog"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Public m_pHandler As UCLID_INPUTFUNNELLib.IIREventHandler
Public m_bEnabled As Boolean

Private Sub HandleError(strErrNum As String)
    Dim ex As ICOMUCLIDException
    Set ex = New COMUCLIDException
    ex.CreateFromString strErrNum, Err.Description
    ex.AddDebugInfo "Error Number", Err.Number

    ' Convert the UCLIDException to a string and
    ' throw as a COM error with description
    Dim strMsg As String
    strMsg = ex.AsStringizedByteStream
    
    ' Throw the exception to the caller
    Err.Raise vbObjectError, "", strMsg
End Sub

Private Sub txtInput_KeyDown(KeyCode As Integer, Shift As Integer)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Ignore the return if disabled
    If Not m_bEnabled Then
        Exit Sub
    End If
    
    ' Check for return
    If KeyCode = 13 Then
        ' Send text event to event handler
        If Not m_pHandler Is Nothing Then
            ' Create a TextInput object
            Dim pInput As UCLID_INPUTFUNNELLib.TextInput
            Set pInput = New UCLID_INPUTFUNNELLib.TextInput
            
            ' Initialize the TextInput with text from edit box
            pInput.InitTextInput Nothing, txtInput
            
            ' Send TextInput to event handler
            m_pHandler.NotifyInputReceived pInput
                
            ' Clear the edit box
            txtInput = ""
        End If
    End If
    Exit Sub
    
ErrorHandler:
    HandleError 11008
End Sub

