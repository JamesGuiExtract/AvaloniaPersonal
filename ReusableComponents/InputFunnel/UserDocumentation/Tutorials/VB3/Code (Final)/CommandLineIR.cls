VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "CommandLineIR"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True
Implements UCLID_INPUTFUNNELLib.IInputReceiver
Private m_pDialog As frmCLDialog
Private m_lParentWindowHandle As Long

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
    
Private Sub Class_Initialize()
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Create the dialog object
    Set m_pDialog = New frmCLDialog
    m_pDialog.Show False
    
    ' Disable input
    m_pDialog.m_bEnabled = False
    Exit Sub
    
ErrorHandler:
    Error_Handler 11000
End Sub

Private Sub IInputReceiver_DisableInput()
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Disable input
    m_pDialog.m_bEnabled = False
    Exit Sub
    
ErrorHandler:
    Error_Handler 11001
End Sub

Private Sub IInputReceiver_EnableInput(ByVal strInputType As String, ByVal strPrompt As String)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Set prompt
    m_pDialog.lblPrompt = strPrompt
    
    ' Set enabled flag
    m_pDialog.m_bEnabled = True
    Exit Sub
    
ErrorHandler:
    Error_Handler 11002
End Sub

Private Property Get IInputReceiver_HasWindow() As Boolean
    ' This input receiver has a window
    IInputReceiver_HasWindow = True
End Property

Private Property Get IInputReceiver_InputIsEnabled() As Boolean
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Return flag from dialog object
    IInputReceiver_InputIsEnabled = m_pDialog.m_bEnabled
    Exit Sub
    
ErrorHandler:
    Error_Handler 11003
End Property

Private Property Let IInputReceiver_ParentWndHandle(ByVal RHS As Long)
    ' Store the handle
    m_lParentWindowHandle = RHS
End Property

Private Property Get IInputReceiver_ParentWndHandle() As Long
    ' Return data member
    IInputReceiver_ParentWndHandle = m_lParentWindowHandle
End Property

Private Sub IInputReceiver_SetEventHandler(ByVal pEventHandler As UCLID_INPUTFUNNELLib.IIREventHandler)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Store
    Set m_pDialog.m_pHandler = pEventHandler
    Exit Sub
    
ErrorHandler:
    Error_Handler 11004
End Sub

Private Sub IInputReceiver_SetOCREngine(ByVal pEngine As UCLID_RASTERANDOCRMGMTLib.IOCREngine)
    ' This method should never be called by the input funnel
    ' because UsesOCR = False
End Sub

Private Sub IInputReceiver_SetOCRFilter(ByVal pFilter As UCLID_INPUTFUNNELLib.IOCRFilter)
    ' This method should never be called by the input funnel
    ' because UsesOCR = False
End Sub

Private Sub IInputReceiver_ShowWindow(ByVal bShow As Boolean)
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Display or hide
    m_pDialog.Visible = bShow
    Exit Sub
    
ErrorHandler:
    Error_Handler 11005
End Sub

Private Property Get IInputReceiver_UsesOCR() As Boolean
    ' Return data member
    IInputReceiver_UsesOCR = False
End Property

Private Property Get IInputReceiver_WindowHandle() As Long
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    IInputReceiver_WindowHandle = m_pDialog.hWnd
    Exit Sub
    
ErrorHandler:
    Error_Handler 11006
End Property

Private Property Get IInputReceiver_WindowShown() As Boolean
    ' Specify error handler
    On Error GoTo ErrorHandler
    
    ' Return flag from dialog object
    IInputReceiver_WindowShown = m_pDialog.Visible
    Exit Sub
    
ErrorHandler:
    Error_Handler 11007
End Property

