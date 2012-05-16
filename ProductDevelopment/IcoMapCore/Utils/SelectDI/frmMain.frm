VERSION 5.00
Begin VB.Form frmMain 
   BorderStyle     =   1  'Fixed Single
   Caption         =   "Select Feature Attribute Data Interpreter"
   ClientHeight    =   2715
   ClientLeft      =   45
   ClientTop       =   360
   ClientWidth     =   6645
   Icon            =   "frmMain.frx":0000
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   2715
   ScaleWidth      =   6645
   StartUpPosition =   3  'Windows Default
   Begin VB.CommandButton cmdFindComponents 
      Caption         =   "Find interpreters"
      Height          =   375
      Left            =   4440
      TabIndex        =   5
      Top             =   1200
      Width           =   2055
   End
   Begin VB.CommandButton cmdSaveComponent 
      Caption         =   "Save selection"
      Height          =   375
      Left            =   4440
      TabIndex        =   4
      Top             =   2160
      Width           =   2055
   End
   Begin VB.CommandButton cmdClose 
      Caption         =   "Close"
      Height          =   375
      Left            =   4440
      TabIndex        =   2
      Top             =   360
      Width           =   2055
   End
   Begin VB.CommandButton cmdTestComponent 
      Caption         =   "Test selection"
      Height          =   375
      Left            =   4440
      TabIndex        =   1
      Top             =   1680
      Width           =   2055
   End
   Begin VB.ListBox lstComponents 
      Height          =   2205
      Left            =   120
      TabIndex        =   0
      Top             =   360
      Width           =   4215
   End
   Begin VB.Label Label2 
      Caption         =   "Available Feature Attribute Data Interpreters"
      Height          =   255
      Left            =   120
      TabIndex        =   3
      Top             =   120
      Width           =   4215
   End
End
Attribute VB_Name = "frmMain"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit
Private m_pDescToProgIDMap As IStrToStrMap

Private Sub cmdClose_Click()
    End
End Sub

Private Sub cmdFindComponents_Click()
    On Error GoTo myErrHandler
    lstComponents.Clear
    
    Dim pCatMgr As New CategoryManager
    Set m_pDescToProgIDMap = pCatMgr.GetDescriptionToProgIDMap1("UCLID IFeatureAttributeDataInterpreter")
    
    Dim pvecDescriptions As IVariantVector
    Set pvecDescriptions = m_pDescToProgIDMap.GetKeys()
    Dim i As Integer
    For i = 0 To pvecDescriptions.Size() - 1
        lstComponents.AddItem pvecDescriptions.Item(i)
    Next i

    If (pvecDescriptions.Size() > 0) Then
        lstComponents.Selected(0) = True
        cmdSaveComponent.Enabled = True
        cmdTestComponent.Enabled = True
    Else
        cmdSaveComponent.Enabled = False
        cmdTestComponent.Enabled = False
    End If
    Exit Sub
    
myErrHandler:
    Dim pEx As New COMUCLIDException
    pEx.CreateFromString "ELI04271", Err.Description
    pEx.Display
End Sub

Private Sub cmdSaveComponent_Click()
    Dim lKey As Long
    Dim lRet As Long
    
    Dim strKey As String
    strKey = "SOFTWARE\UCLID Software\IcoMap for ArcGIS\Options\General"
    
    SaveSettingString HKEY_LOCAL_MACHINE, strKey, "DataInterpreter", lstComponents.Text
    MsgBox "Your selection has been saved."
    
End Sub

Private Sub cmdTestComponent_Click()
    
    If (lstComponents.SelCount = 0) Then
        MsgBox "Please select a component!"
        Exit Sub
    End If
    
    Dim pDataInterpreter As IFeatureAttributeDataInterpreter
    Set pDataInterpreter = CreateObject(m_pDescToProgIDMap.GetValue(lstComponents.Text))
    
    Dim pFeature As UCLIDFEATUREMGMTLib.IFeature
    Set pFeature = GetRandomFeature
    
    MsgBox pDataInterpreter.getAttributeDataFromFeature(pFeature)
End Sub

Private Function GetRandomFeature() As UCLIDFEATUREMGMTLib.IFeature
    ' create a simple feature object
    Dim pFeature As UCLIDFEATUREMGMTLib.IFeature
    Set pFeature = New UCLIDFEATUREMGMTLib.Feature
    Dim pPart As UCLIDFEATUREMGMTLib.IPart
    Set pPart = New UCLIDFEATUREMGMTLib.Part
    
    Dim pLine As UCLIDFEATUREMGMTLib.ILineSegment
    Set pLine = New UCLIDFEATUREMGMTLib.LineSegment
    pLine.setBearingDistance "North", "300 feet"
    Dim pSegment As UCLIDFEATUREMGMTLib.ISegment
    Set pSegment = pLine
    
    Dim pPoint As ICartographicPoint
    Set pPoint = New CartographicPoint
    pPoint.InitPointInXY 10, 10
    pPart.addSegment pSegment
    pPart.setStartingPoint pPoint
    
    pFeature.addPart pPart
    pFeature.setFeatureType kPolygon

    Set GetRandomFeature = pFeature
End Function


Private Sub Form_Load()
    cmdSaveComponent.Enabled = False
    cmdTestComponent.Enabled = False
    
    Dim pLM As New UCLIDComponentLM
    pLM.Initialize "AW247YHUG8"
End Sub
