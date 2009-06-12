Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics

Public Module GuidUtils

    Sub GetNewGUID()
        Dim myGUID As String = Guid.NewGuid.ToString("D").ToUpper()
        DTE.ActiveDocument.Selection.text = myGUID
    End Sub

End Module
