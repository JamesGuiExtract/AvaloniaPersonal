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
   Begin VB.CommandButton cmdCLIR 
      Caption         =   "New Command Line IR"
      Height          =   375
      Left            =   2400
      TabIndex        =   5
      Top             =   1080
      Width           =   2175
   End
   Begin VB.CommandButton cmdCalculate 
      Caption         =   "Calculate distance x 2 (in feet)"
      Height          =   375
      Left            =   120
      TabIndex        =   4
      Top             =   2280
      Width           =   2415
   End
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
    
Private Sub cmdCalculate_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Create a Distance object
    Dim pDistance As New UCLIDLANDRECORDSIVLib.Distance
    
    ' Set units to feet
    pDistance.GlobalDefaultDistanceUnit = kFeet
    
    ' Apply the received text
    pDistance.InitDistance txtInput
    
    ' Compute the new distance
    Dim dTwiceDistanceInFeet As Double
    dTwiceDistanceInFeet = pDistance.GetDistanceInUnit(kFeet) * 2
    
    ' Display new distance in MsgBox
    MsgBox dTwiceDistanceInFeet
    Exit Sub
    
errHandler:
    Error_Handler 12000
End Sub

Private Sub cmdCLIR_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Create the Input Receiver
    Dim pIR As UCLID_INPUTFUNNELLib.IInputReceiver
    Set pIR = New CLTestIR.CommandLineIR
    
    ' Connect the new input receiver to Input Funnel
    m_pFunnel.ConnectInputReceiver pIR
    Exit Sub
    
errHandler:
    Error_Handler 12001
End Sub

Private Sub cmdImage_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Spot Recognition Window"
    Exit Sub
    
errHandler:
    Error_Handler 12002
End Sub

Private Sub cmdText_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Highlighted Text Window"
    Exit Sub
    
errHandler:
    Error_Handler 12003
End Sub

Private Sub Form_Load()
    ' Set error handler
    On Error GoTo errHandler
    
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
    
errHandler:
    Error_Handler 12004
End Sub

Private Sub Form_Unload(Cancel As Integer)
    On Error GoTo errHandler
        m_pFunnel.Destroy
    Exit Sub
errHandler:
    Error_Handler 12005

End Sub

Private Sub m_pFunnel_NotifyInputReceived(ByVal pTextInput As UCLID_INPUTFUNNELLib.ITextInput)
    ' Set error handler
    On Error GoTo errHandler
    
    ' Display received text in text box
    txtInput = pTextInput.GetText
    Exit Sub
    
errHandler:
    Error_Handler 12006

End Sub
