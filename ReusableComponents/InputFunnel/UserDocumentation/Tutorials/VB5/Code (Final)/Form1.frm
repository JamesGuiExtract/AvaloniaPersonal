VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   3195
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   5040
   LinkTopic       =   "Form1"
   ScaleHeight     =   3195
   ScaleWidth      =   5040
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdRetrieve 
      Caption         =   "Retrieve Parcel Information"
      Height          =   375
      Left            =   120
      TabIndex        =   5
      Top             =   1320
      Width           =   2295
   End
   Begin VB.CommandButton cmdText 
      Caption         =   "New Text Window"
      Height          =   375
      Left            =   2880
      TabIndex        =   4
      Top             =   480
      Width           =   2055
   End
   Begin VB.CommandButton cmdImage 
      Caption         =   "New Image Window"
      Height          =   375
      Left            =   2880
      TabIndex        =   3
      Top             =   1080
      Width           =   2055
   End
   Begin VB.TextBox txtOutput 
      Height          =   975
      Left            =   120
      Locked          =   -1  'True
      MultiLine       =   -1  'True
      TabIndex        =   2
      Top             =   1920
      Width           =   2295
   End
   Begin VB.TextBox txtInput 
      Height          =   375
      Left            =   120
      TabIndex        =   1
      Top             =   480
      Width           =   2295
   End
   Begin VB.Label Label1 
      Caption         =   "Parcel ID:"
      Height          =   255
      Left            =   240
      TabIndex        =   0
      Top             =   120
      Width           =   855
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
    ' Set error handler
    On Error GoTo errHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Spot Recognition Window"
    Exit Sub
    
errHandler:
    Error_Handler 15000
End Sub

Private Sub cmdRetrieve_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Clear the output and return if no parcel ID available
    If txtInput = "" Then
        txtOutput = ""
        Exit Sub
    End If
    
    ' Create database connection
    
    ' Send parcel ID to database and retrieve associated info
    
    ' Display parcel data
    If txtInput = "1234-2345" Then
        txtOutput = "Parcel ID: 1234-2345" + vbCrLf + "Information: Known"
    Else
        txtOutput = "Parcel ID: " + txtInput + vbCrLf + "Information: Unknown"
    End If
    Exit Sub
    
errHandler:
    Error_Handler 15001
End Sub

Private Sub cmdText_Click()
    ' Set error handler
    On Error GoTo errHandler
    
    ' Ask Input Funnel to create the Input Receiver
    m_pFunnel.CreateNewInputReceiver "Highlighted Text Window"
    Exit Sub
    
errHandler:
    Error_Handler 15002
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
    
    ' Create a Parcel ID Validator object
    Dim pIV As UCLID_INPUTFUNNELLib.IInputValidator
    Set pIV = New ParcelIDValidator.ParcelIDIV
    
    ' Enable Parcel ID input
    m_pFunnel.EnableInput2 pIV, "Specify parcel ID", m_pContext
    Exit Sub
    
errHandler:
    Error_Handler 15003
End Sub

Private Sub Form_Unload(Cancel As Integer)
    On Error GoTo errHandler
    m_pFunnel.Destroy
    Exit Sub
errHandler:
    Error_Handler 15005
End Sub

Private Sub m_pFunnel_NotifyInputReceived(ByVal pTextInput As UCLID_INPUTFUNNELLib.ITextInput)
    ' Set error handler
    On Error GoTo errHandler
    
    ' Display received text in text box
    txtInput = pTextInput.GetText
    Exit Sub
    
errHandler:
    Error_Handler 15004

End Sub
