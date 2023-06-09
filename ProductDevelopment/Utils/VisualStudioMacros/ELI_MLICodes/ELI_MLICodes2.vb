Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics
Imports System.Runtime.InteropServices


Public Module ELIMLICodes

    Sub InsertELICode()

        InsertLICode(LITypeNum.ELI)
    End Sub

    Sub InsertMLICode()

        InsertLICode(LITypeNum.MLI)
    End Sub

    Sub PasteWithNewLICodes()

        ' get the clipboard text
        Dim clipboard As New Clipboard
        Dim clipboardText As String = clipboard.Text

        ' ensure that there is some clipboard text to paste
        If clipboardText Is Nothing Or clipboardText = "" Then
            Return
        End If

        ' get the selection text
        Dim selection As EnvDTE.TextSelection = CType(DTE.ActiveDocument.Selection, EnvDTE.TextSelection)

        ' instantiate an LICodeHandler to replace LI codes
        Using LIHandler As New LICodeHandler

            With LIHandler
                ' retrieve the clipboard text with LI codes replaced
                Dim output As String = .ReplaceLICodesInText(clipboardText)

                With selection

                    ' replace the selected text with the next LI codes
                    .Insert(output, vsInsertFlags.vsInsertFlagsContainNewText)

                    ' deselect the inserted text and set the active cursor to the right of the recently inserted text
                    .CharRight()
                End With

                ' commit the changes to LI code files
                .CommitChanges()
            End With
        End Using

    End Sub

    Sub ReplaceLICodesInSelection()

        ' get the selection text
        Dim selection As EnvDTE.TextSelection = CType(DTE.ActiveDocument.Selection, EnvDTE.TextSelection)

        ' ensure that some text has been selected
        If selection.Text Is Nothing Or selection.Text = "" Then
            Return
        End If

        ' instantiate an LICodeHandler to replace LI codes
        Using LIHandler As New LICodeHandler

            With LIHandler
                ' retrieve the selected text with LI codes replaced
                Dim output As String = .ReplaceLICodesInText(selection.Text)

                With selection

                    ' replace the selected text with the next LI codes
                    .Insert(output, vsInsertFlags.vsInsertFlagsContainNewText)

                    ' deselect the inserted text and set the active cursor to the right of the recently inserted text
                    .CharRight()
                End With

                ' commit the changes to LI code files
                .CommitChanges()
            End With
        End Using
    End Sub

    Sub InsertLICode(ByVal typeNum As Integer)

        ' get the selected text
        Dim selection As EnvDTE.TextSelection = CType(DTE.ActiveDocument.Selection, EnvDTE.TextSelection)

        ' instantiate an LICodeHandler to retrieve LI code
        Using LIHandler As New LICodeHandler
            With LIHandler
                ' get the next LI code
                Dim LICode As String = CType(IIf(typeNum = LITypeNum.ELI, .NextELICode, .NextMLICode), String)

                With selection

                    ' insert the LI code, replacing text if any is selected
                    .Insert(LICode, vsInsertFlags.vsInsertFlagsContainNewText)

                    ' deselect the inserted text and set the active cursor to the right of the recently inserted text
                    .CharRight()
                End With

                ' commit the changes to LI dat file
                .CommitChanges()
            End With
        End Using
    End Sub
End Module

