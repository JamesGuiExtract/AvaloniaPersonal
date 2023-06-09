Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module CloseMiscFiles

    Private Const MiscFilesName As String = "Miscellaneous Files"

    Sub CloseMiscFiles()

        ' create a queue to hold the miscellaneous file documents
        Dim miscDocs As New Collections.Generic.Queue(Of Document)

        ' iterate through each open document
        For Each doc As Document In DTE.Documents

            ' check if the document is a miscellaneous file
            If doc.ProjectItem.ContainingProject.Name = MiscFilesName Then

                ' add this document to the queue
                miscDocs.Enqueue(doc)
            End If
        Next

        ' iterate through each miscellaneous file document
        For Each doc As Document In miscDocs
            With doc

                ' check if the document was saved
                If .Saved Then

                    ' it was saved, just close it
                    .Windows.Item(1).Close(vsSaveChanges.vsSaveChangesNo)
                Else
                    Try

                        ' it wasn't saved, prompt to save before closing
                        .Windows.Item(1).Close(vsSaveChanges.vsSaveChangesPrompt)
                    Catch ex As Exception

                        ' the user selected cancel, stop here
                        Exit Sub
                    End Try
                End If
            End With
        Next
    End Sub

End Module
