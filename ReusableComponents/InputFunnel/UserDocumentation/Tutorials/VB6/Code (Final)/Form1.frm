VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   2880
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   3180
   LinkTopic       =   "Form1"
   ScaleHeight     =   2880
   ScaleWidth      =   3180
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdTest 
      Caption         =   "Test"
      Height          =   495
      Left            =   360
      TabIndex        =   3
      Top             =   2160
      Width           =   2415
   End
   Begin VB.Frame Frame1 
      Caption         =   "Exception Display Format"
      Height          =   1455
      Left            =   360
      TabIndex        =   0
      Top             =   360
      Width           =   2415
      Begin VB.OptionButton btnMessage 
         Caption         =   "MsgBox Style"
         Height          =   495
         Left            =   240
         TabIndex        =   2
         Top             =   840
         Width           =   1935
      End
      Begin VB.OptionButton btnUCLID 
         Caption         =   "UCLID Style"
         Height          =   375
         Left            =   240
         TabIndex        =   1
         Top             =   360
         Width           =   1935
      End
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private Sub cmdTest_Click()
    On Error GoTo errHandler
    
    'Create a UCLID IUnknownVector object
    Dim pVec As New UCLIDCOMUTILSLib.IUnknownVector
    
    ' Attempt object retrieval from empty vector
    ' This will throw a UCLID Exception
    Dim pUnknown As IUnknown
    Set pUnknown = pVec.At(0)
    
    Exit Sub

errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "16000", Err.Description
    
    ' Check chosen display method
    If btnUCLID.Value = True Then
        ' Just display the exception with UCLID dialog
        pEx.Display
    ElseIf btnMessage.Value = True Then
        ' Display the topmost error number and text in
        ' a MsgBox
        MsgBox "Error Code = " + pEx.GetTopELICode + vbCrLf + "Error Description = " + pEx.GetTopText
    Else
        ' Display an error message to the user
        MsgBox "Please select a display method"
    End If
End Sub

Private Sub Form_Load()
    On Error GoTo errHandler
    
    ' Create License Manager
    Dim pLM As New UCLID_COMLMLib.UCLIDComponentLM
    
    ' Initialize License Manager with license file and 4 passwords
    pLM.InitializeFromFile "INSERT FILENAME HERE", 1, 2, 3, 4
    Exit Sub

errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "16001", Err.Description
    
    ' Display the error
    pEx.Display
End Sub
