VERSION 5.00
Begin VB.Form Form1 
   Caption         =   "Form1"
   ClientHeight    =   5115
   ClientLeft      =   60
   ClientTop       =   345
   ClientWidth     =   8010
   LinkTopic       =   "Form1"
   ScaleHeight     =   5115
   ScaleWidth      =   8010
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton Command1 
      Caption         =   "Search"
      Height          =   375
      Left            =   120
      TabIndex        =   3
      Top             =   840
      Width           =   1095
   End
   Begin VB.ListBox List1 
      Height          =   1815
      Left            =   1560
      TabIndex        =   4
      Top             =   3120
      Width           =   5055
   End
   Begin VB.CheckBox Check1 
      Caption         =   "Do case-sensitive checking of the search string"
      Height          =   375
      Left            =   1560
      TabIndex        =   1
      Top             =   840
      Width           =   5055
   End
   Begin VB.TextBox Text1 
      Height          =   375
      Left            =   1560
      TabIndex        =   0
      Top             =   240
      Width           =   5055
   End
   Begin VB.TextBox Text2 
      Height          =   1575
      Left            =   1560
      MultiLine       =   -1  'True
      TabIndex        =   2
      Top             =   1320
      Width           =   5055
   End
   Begin VB.Label Label3 
      Caption         =   "Result"
      Height          =   375
      Left            =   0
      TabIndex        =   7
      Top             =   3120
      Width           =   1455
   End
   Begin VB.Label Label1 
      Caption         =   "Pattern"
      Height          =   375
      Left            =   0
      TabIndex        =   5
      Top             =   240
      Width           =   1455
   End
   Begin VB.Label Label2 
      Caption         =   "Search String"
      Height          =   375
      Left            =   0
      TabIndex        =   6
      Top             =   1320
      Width           =   1455
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Private m_regExp As UCLIDCOMUTILSLib.IRegularExprParser
Private m_vecPos As UCLIDCOMUTILSLib.IIUnknownVector

Private Sub Check1_Click()
    ' Specify error handler
    On Error GoTo errhandler
    
    ' Pass new setting on to Parser
    If Check1.Value = 0 Then
        ' Ignore case
        m_regExp.IgnoreCase = True
    Else
        ' Do not ignore case
        m_regExp.IgnoreCase = False
    End If
    Exit Sub
    
errhandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "17002", Err.Description
    
    ' Display the error
    pEx.Display
End Sub

Private Sub Command1_Click()
    ' Specify error handler
    On Error GoTo errhandler
    
    ' Set the regular expression pattern
    m_regExp.Pattern = Text1.Text

    ' Clear any existing items out of the vector and the list
    m_vecPos.Clear
    List1.Clear
    
    ' Find any substrings within the input string
    If Not m_regExp Is Nothing Then
        Set m_vecPos = m_regExp.Find(Text2.Text, False)
    End If

    ' Add lines to output list
    For i = 0 To m_vecPos.Size - 1 ' Loop through vector
        Dim ipObjPair As IObjectPair
        ' Get this token
        Dim ipToken As UCLIDCOMUTILSLib.Token
        Set ipObjPair = m_vecPos.At(i)
        Set ipToken = ipObjPair.Object1

        ' Get character positions
        Dim lStart As Long
        Dim lEnd As Long
        Dim strType As String
        Dim strText As String
        ipToken.GetTokenInfo lStart, lEnd, strType, strText

        ' Add string to list
        List1.AddItem strText
    Next
    Exit Sub
    
errhandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "17001", Err.Description
    
    ' Display the error
    pEx.Display
End Sub

Private Sub Form_Load()
    ' Specify error handler
    On Error GoTo errhandler
    
    ' Create License Manager
    Dim pLM As New UCLID_COMLMLib.UCLIDComponentLM
    
    ' Initialize License Manager with license file and 4 passwords
    pLM.InitializeFromFile "INSERT FILENAME HERE", 1, 2, 3, 4
    
    ' Create the object to hold substring positions
    Set m_vecPos = New UCLIDCOMUTILSLib.IUnknownVector

    ' Create the regular expression parser
    Set m_regExp = New UCLID_IEVBSCRIPTPARSERLib.VBScriptParser
    
    ' Set checkbox to appropriate initial state
    Check1.Value = IIf(m_regExp.IgnoreCase, 0, 1)
    Exit Sub
    
errhandler:
    ' Create a COMUCLIDException object
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "17000", Err.Description
    
    ' Display the error
    pEx.Display
End Sub

