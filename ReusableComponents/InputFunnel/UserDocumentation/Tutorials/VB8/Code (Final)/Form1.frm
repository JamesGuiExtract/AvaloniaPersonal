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
   Begin VB.CommandButton Command1 
      Caption         =   "New Image Window"
      Height          =   495
      Left            =   120
      TabIndex        =   2
      Top             =   240
      Width           =   1815
   End
   Begin VB.TextBox Text1 
      Height          =   495
      Left            =   2040
      TabIndex        =   0
      Top             =   1080
      Width           =   2535
   End
   Begin VB.Label Label1 
      Caption         =   "Enter telephone number:"
      Height          =   255
      Left            =   0
      TabIndex        =   1
      Top             =   1200
      Width           =   1935
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private WithEvents m_pFunnel As UCLID_INPUTFUNNELLib.InputManager
Attribute m_pFunnel.VB_VarHelpID = -1
Private m_pPhoneIV As UCLID_REGEXPRIVLib.IRegExprInputValidator
Private m_pContext As UCLID_INPUTFUNNELLib.IInputContext

Private Sub Command1_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Spot Recognition Window"
    Exit Sub
    
errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "18002", Err.Description
    
    ' Display the error
    pEx.Display
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
    
    ' Create the input manager
    Set m_pFunnel = New InputManager
    
    ' Set visibility of Input Receivers
    m_pFunnel.ShowWindows True
    
    ' Create the regular expression Input Validator
    Set m_pPhoneIV = New RegExprInputValidator
    
    ' Set the regular expression and input type for phone numbers
    Dim strPhone As String
    strPhone = "\d{3}\s*-\s*\d{4}"
    m_pPhoneIV.Pattern = strPhone
    m_pPhoneIV.SetInputType "Phone Number"
    
    ' Enable Phone number input
    m_pFunnel.EnableInput2 m_pPhoneIV, "Phone Number", m_pContext
    Exit Sub
    
errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "18000", Err.Description
    
    ' Display the error
    pEx.Display
End Sub

Private Sub Form_Unload(Cancel As Integer)
    ' Specify error handler
    On Error GoTo errHandler
    
    ' Cleanup the input funnel
    m_pFunnel.Destroy
    Exit Sub
    
errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "18001", Err.Description
    
    ' Display the error
    pEx.Display
End Sub

Private Sub m_pFunnel_NotifyInputReceived(ByVal pTextInput As UCLID_INPUTFUNNELLib.ITextInput)
    ' Set error handler
    On Error GoTo errHandler
    
    ' Display received text in text box
    Text1 = pTextInput.GetText
    Exit Sub
    
errHandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "18003", Err.Description
    
    ' Display the error
    pEx.Display
End Sub



