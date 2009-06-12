VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   3195
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   4680
   LinkTopic       =   "Form1"
   ScaleHeight     =   3195
   ScaleWidth      =   4680
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdImage 
      Caption         =   "New Image Window"
      Height          =   375
      Left            =   120
      TabIndex        =   3
      Top             =   1680
      Width           =   2055
   End
   Begin VB.CommandButton cmdText 
      Caption         =   "New Text Window"
      Height          =   375
      Left            =   120
      TabIndex        =   2
      Top             =   1080
      Width           =   2055
   End
   Begin VB.TextBox txtInput 
      Height          =   375
      Left            =   120
      TabIndex        =   1
      Top             =   480
      Width           =   4095
   End
   Begin VB.Label Label1 
      Caption         =   "Specify input:"
      Height          =   255
      Left            =   120
      TabIndex        =   0
      Top             =   120
      Width           =   1815
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit
Private WithEvents m_pFunnel As UCLID_INPUTFUNNELLib.InputManager
Attribute m_pFunnel.VB_VarHelpID = -1
Private m_pContext As UCLID_INPUTFUNNELLib.IInputContext

Private Sub Error_Handler(strErrNum As String)
    ' Create a COMUCLIDException object
    Dim pEx As New UCLIDEXCEPTIONMGMTLib.COMUCLIDException
    pEx.CreateFromString strErrNum, Err.Description
    
    ' Display the error to the user
    pEx.Display
End Sub
    
Private Sub cmdImage_Click()
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Spot Recognition Window"
    Exit Sub
    
ErrorHandler:
    Error_Handler 10000
End Sub

Private Sub cmdText_Click()
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Highlighted Text Window"
    Exit Sub
    
ErrorHandler:
    Error_Handler 10001
End Sub

Private Sub Form_Load()
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Create License Manager
    Dim pLM As New UCLID_COMLMLib.UCLIDComponentLM
    
    ' Initialize License Manager with license file and 4 passwords
    pLM.InitializeFromFile "INSERT FILENAME HERE", 1, 2, 3, 4
    
    ' Create the Input Context object
    Set m_pContext = New LandRecordsIC
    
    ' Create the Input Manager object
    Set m_pFunnel = New InputManager
    
    ' Set visibility of Input Receivers
    m_pFunnel.ShowWindows True
    
    ' Enable Text input
    m_pFunnel.EnableInput1 "Text", "Please specify value for X", m_pContext
    
    ' Create a C-Pen Input Receiver
'    m_pFunnel.CreateNewInputReceiver "C-Pen"
    
    Exit Sub
    
ErrorHandler:
    Error_Handler 10002
End Sub

Private Sub Form_Unload(Cancel As Integer)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Cleanup the input funnel
    m_pFunnel.Destroy
    Exit Sub
    
ErrorHandler:
    Error_Handler 10003
End Sub

Private Sub m_pFunnel_NotifyInputReceived(ByVal pTextInput As UCLID_INPUTFUNNELLib.ITextInput)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Display received text in text box
    txtInput = pTextInput.GetText
    Exit Sub
    
ErrorHandler:
    Error_Handler 10004
End Sub
