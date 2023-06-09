Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module MakeWriteable
    Private Const gstrMARK_STRING As String = "// WRITEABLE_FILE: - "

    Public Sub MarkAndMakeFileWriteable()
        Dim item As SelectedItem
        Dim strFileName As String

        ' now get the file name
        strFileName = DTE.ActiveDocument.FullName

        ' attempt to make file writeable
        If MakeFileWriteable(strFileName) Then
            ' since we now have a writeable file, lets add a mark that we can search for
            Dim editPoint As EditPoint
            Dim crntSelection As TextSelection = DTE.ActiveDocument.Selection

            ' create an undo context so all edits can be undone with a single Undo call
            DTE.UndoContext.Open("MarkWriteableFile")

            Try
                ' define the mark string
                Dim strMark As String = gstrMARK_STRING & Today().ToShortDateString()

                Dim iCol As Integer = crntSelection.AnchorPoint.DisplayColumn
                Dim iRow As Integer = crntSelection.AnchorPoint.Line

                ' move to the end of the document
                crntSelection.EndOfDocument()

                ' create an edit point at the end of the document
                editPoint = crntSelection.TopPoint.CreateEditPoint()

                ' insert a blank line and move to the beginning of it
                editPoint.Insert(vbCrLf)
                editPoint.StartOfLine()

                ' insert the writeable mark
                editPoint.Insert(strMark)

                crntSelection.MoveToDisplayColumn(iRow, iCol)

            Finally
                ' if an error occurred make sure the undo context is closed
                DTE.UndoContext.Close()

            End Try

        End If

    End Sub

    Public Sub SearchForWriteableFiles()

        'DTE.ActiveDocument.Activate()
        DTE.Find.FindWhat = gstrMARK_STRING ' search for the writeable file mark
        DTE.Find.Target = vsFindTarget.vsFindTargetSolution
        DTE.Find.MatchCase = True
        DTE.Find.MatchWholeWord = False
        DTE.Find.Backwards = False
        DTE.Find.MatchInHiddenText = True
        DTE.Find.PatternSyntax = vsFindPatternSyntax.vsFindPatternSyntaxLiteral
        DTE.Find.Action = vsFindAction.vsFindActionFind
        DTE.Find.ResultsLocation = vsFindResultsLocation.vsFindResults2 ' display results in the 2nd find list
        DTE.Find.SearchPath = "Entire Solution"
        DTE.Find.SearchSubfolders = True
        DTE.Find.FilesOfType = "*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hpp;*.hxx;*.hh;*.inl;*.rc;*.resx;*.idl;*.asm;*.inc"
        DTE.Find.Action = vsFindAction.vsFindActionFindAll

        Try
            If (DTE.Find.Execute() = vsFindResult.vsFindResultNotFound) Then
                Throw New System.Exception("vsFindResultNotFound")
            End If
        Catch ex As Exception
            MsgBox(ex.Message & vbCrLf & ex.StackTrace, MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly, "Exception Occurred")
        End Try

    End Sub

    Private Function MakeFileWriteable(ByVal strFileName As String) As Boolean
        Try
            FileSystem.SetAttr(strFileName, FileAttribute.Normal)
        Catch ex As Exception
            Dim strErrorMsg As String = "Exception thrown while attempting to make file " _
                & "writeable!" & vbCrLf & "FileName: " & strFileName & vbCrLf _
                & "Exception: " & vbCrLf & ex.Message & vbCrLf _
                & "StackTrace: " & ex.StackTrace
            MsgBox(strErrorMsg, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly, "Exception!")

            Return False
        End Try

        Return True
    End Function

End Module
