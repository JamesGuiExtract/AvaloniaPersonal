Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module CustomDiff

    Sub CustomDiff()

        ' iterate through each selected item
        Dim item As SelectedItem
        Dim filename As String
        For Each item In DTE.SelectedItems

            ' get the filename associated with this item
            filename = GetFilenameForItem(item)

            ' run the custom diff
            DTE.ExecuteCommand("Tools.Shell", """I:\Common\Engineering\Tools\Utils\CompareToVssWithCustomDiffTool\CompareToVssWithCustomDiffTool.exe"" """ & filename & """")

        Next

    End Sub

    Function GetFilenameForItem(ByRef item As SelectedItem) As String

        ' check if this item's name is a file url (pending checkin)
        If item.Name.StartsWith("file:") Then
            ' drop the "file:" prefix
            GetFilenameForItem = item.Name.Remove(0, 5)
        ElseIf item.ProjectItem Is Nothing Then
            ' this item is does not have an associated project item (solution or project file)
            If item.Project Is Nothing Then
                ' this item is the solution file
                GetFilenameForItem = DTE.Solution.FullName
            Else
                ' this item is a project file
                GetFilenameForItem = item.Project.FullName
            End If
        Else
            ' this item has an associated project item
            GetFilenameForItem = item.ProjectItem.FileNames(0)
        End If
    End Function

End Module


