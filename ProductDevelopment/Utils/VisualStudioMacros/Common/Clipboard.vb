Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Class Clipboard
    Private cliptext As String

    Property Text() As String
        Get
            ' To get clipboard text in a Visual Studio macro, a new thread must be kicked off.
            ' Based on code from http://www.helixoft.com/blog/archives/12 . Retrieved March 13, 2008.
            Dim clipboardThread As New System.Threading.Thread(AddressOf GetClipboardTextThread)
            With clipboardThread
                .ApartmentState = System.Threading.ApartmentState.STA
                .IsBackground = True
                .Start()
                .Join()
            End With
            clipboardThread = Nothing

            Return cliptext
        End Get

        Set(ByVal value As String)
            ' added 04/10/2008 to allow pasting of a string into the clipboard
            Dim clipboardThread As New System.Threading.Thread(AddressOf SetClipboardTextThread)
            With clipboardThread
                .ApartmentState = System.Threading.ApartmentState.STA
                .IsBackground = True
                .Start(value)
                .Join()
            End With
            clipboardThread = Nothing

        End Set
    End Property

    ' To get clipboard text in a Visual Studio macro, a new thread must be kicked off.
    ' Based on code from http://www.helixoft.com/blog/archives/12 . Retrieved March 13, 2008.
    Private Sub GetClipboardTextThread()
        Dim dataObject As System.Windows.Forms.IDataObject = _
            System.Windows.Forms.Clipboard.GetDataObject()
        cliptext = (CType(dataObject.GetData(System.Windows.Forms.DataFormats.Text), String))
    End Sub

    Private Sub SetClipboardTextThread(ByVal cliptext As Object)
        ' set text to clipboard
        System.Windows.Forms.Clipboard.SetData( _
            System.Windows.Forms.DataFormats.StringFormat, cliptext)
    End Sub
End Class
