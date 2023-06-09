Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module CollapseProjects
    Public Sub CollapseProjects()

        ' get the status bar
        Dim statusbar As EnvDTE.StatusBar = DTE.StatusBar

        ' get the solution hierarchy item
        Dim solutionItem As UIHierarchyItem = DTE.ToolWindows.SolutionExplorer.UIHierarchyItems.Item(1)

        ' collapse the solution so that changes made won't be distracting
        Dim projItems As UIHierarchyItems = solutionItem.UIHierarchyItems
        projItems.Expanded = False

        ' iterate through each project hierarchy item
        Dim projCount As Integer = projItems.Count
        Dim i As Integer = 1
        For Each projItem As UIHierarchyItem In projItems

            ' update the progress status
            statusbar.Progress(True, "Collapsing " & projItem.Name, i, projCount)

            ' iterate through each project folder
            Dim projFolders As UIHierarchyItems = projItem.UIHierarchyItems
            For Each projFolder As UIHierarchyItem In projFolders

                ' collapse this project folder hierarchy item
                projFolder.UIHierarchyItems.Expanded = False
            Next

            ' collapse this project hierarchy item
            projFolders.Expanded = False

            ' increment count
            i += 1
        Next

        ' uncollapse the solution so project items are visible again
        projItems.Expanded = True

        ' reset progress status updates
        statusbar.Clear()
        statusbar.Progress(False)
    End Sub

End Module
