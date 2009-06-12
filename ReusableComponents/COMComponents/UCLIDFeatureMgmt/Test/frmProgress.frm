VERSION 5.00
Begin VB.Form frmProgress 
   AutoRedraw      =   -1  'True
   Caption         =   "UCLIDFeatureMgmt Automated Testing"
   ClientHeight    =   1815
   ClientLeft      =   60
   ClientTop       =   375
   ClientWidth     =   7530
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   1815
   ScaleWidth      =   7530
   StartUpPosition =   3  'Windows Default
   Begin VB.Label lblTotalTests 
      Caption         =   "0"
      ForeColor       =   &H00FF0000&
      Height          =   255
      Left            =   1560
      TabIndex        =   7
      Top             =   600
      Width           =   5775
   End
   Begin VB.Label Label4 
      Caption         =   "Total tests:"
      ForeColor       =   &H000000C0&
      Height          =   255
      Left            =   240
      TabIndex        =   6
      Top             =   600
      Width           =   1215
   End
   Begin VB.Label lblNumPasses 
      Caption         =   "0"
      ForeColor       =   &H00FF0000&
      Height          =   255
      Left            =   1560
      TabIndex        =   5
      Top             =   960
      Width           =   5775
   End
   Begin VB.Label Label3 
      Caption         =   "Tests failed:"
      ForeColor       =   &H000000C0&
      Height          =   255
      Left            =   240
      TabIndex        =   4
      Top             =   1320
      Width           =   1215
   End
   Begin VB.Label lblNumFails 
      Caption         =   "0"
      ForeColor       =   &H00FF0000&
      Height          =   255
      Left            =   1560
      TabIndex        =   3
      Top             =   1320
      Width           =   5895
   End
   Begin VB.Label Label2 
      Caption         =   "Tests passed:"
      ForeColor       =   &H000000C0&
      Height          =   255
      Left            =   240
      TabIndex        =   2
      Top             =   960
      Width           =   1215
   End
   Begin VB.Label Label1 
      Caption         =   "Status:"
      ForeColor       =   &H000000C0&
      Height          =   255
      Left            =   240
      TabIndex        =   1
      Top             =   240
      Width           =   1215
   End
   Begin VB.Label lblStatus 
      Caption         =   "Testing ..."
      ForeColor       =   &H00FF0000&
      Height          =   255
      Left            =   1560
      TabIndex        =   0
      Top             =   240
      Width           =   5775
      WordWrap        =   -1  'True
   End
End
Attribute VB_Name = "frmProgress"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

Public Sub recordFailedTest(strTestName As String, strELICode As String)
    ' increment the total number of tests
    lblTotalTests = lblTotalTests + 1
    
    ' incerement the fail count
    lblNumFails = lblNumFails + 1
    
    ' update the status
    lblStatus = "Completed test " & strTestName
    
    'refresh the window
    RefreshWindow
    
    'update data on screen
    Dim strMsg As String
    strMsg = "Test case failed!" & vbCrLf
    strMsg = strMsg + strTestName & vbCrLf
    strMsg = strMsg + strELICode
    MsgBox strMsg
End Sub
Public Sub recordPassedTest(strTestName As String)
    ' increment the total number of tests
    lblTotalTests = lblTotalTests + 1
    
    ' incerement the success count
    lblNumPasses = lblNumPasses + 1
    
    ' update the status
    lblStatus = "Completed test " & strTestName
    
    'refresh the window
    RefreshWindow
End Sub
Private Sub RefreshWindow()
    Refresh
    Sleep 100
End Sub
