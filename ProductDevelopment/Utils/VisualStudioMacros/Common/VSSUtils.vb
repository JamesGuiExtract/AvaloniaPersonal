Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics
Imports Microsoft

Public Module VSSUtils

    Function CheckoutVSSItemFromVSSPath(ByVal vssDB As SourceSafeTypeLib.VSSDatabase, ByVal vssPath As String) As SourceSafeTypeLib.VSSItem

        ' get the vss item for this vss path
        CheckoutVSSItemFromVSSPath = vssDB.VSSItem(vssPath)

        ' get the LI code dat file from Source Safe
        If CheckoutVSSItemFromVSSPath Is Nothing Then
            Throw New System.Exception("Could not find file in VSS: " & vssPath)
        Else
            With CheckoutVSSItemFromVSSPath
                ' check if the file is already checked out
                If .Checkouts.Count <= 0 Then

                    ' the file is not checked out, check out this item from SourceSafe
                    Dim filename As String = .LocalSpec
                    .Checkout("", filename, SourceSafeTypeLib.VSSFlags.VSSFLAG_REPREPLACE)

                ElseIf .Checkouts(1).Username <> vssDB.Username Then

                    ' the file is checked out to someone else, throw an exception
                    Throw New System.Exception(.Checkouts(1).Username & " has already checked out " & vssPath & ". Please try again later.")
                End If
            End With
        End If
    End Function

    Function OpenVSSDatabase() As SourceSafeTypeLib.VSSDatabase

        ' Get the source control bindings for the solution file
        Dim ssc As SourceControl2 = CType(DTE.SourceControl, SourceControl2)
        Dim bindings As SourceControlBindings = ssc.GetBindings(DTE.Solution.FullName)

        ' Open the Visual Source Safe (tm) database
        OpenVSSDatabase = New SourceSafeTypeLib.VSSDatabase
        If bindings Is Nothing Then

            ' This solution is not bound to SourceSafe,
            ' let VSS determine the correct database to open.
            OpenVSSDatabase.Open()            
        Else

            ' This solution is bound to SourceSafe, open the 
            ' VSS database to which the solution is bound.
            OpenVSSDatabase.Open(bindings.ServerName & "\srcsafe.ini")
        End If


    End Function

    ' Gets the physical directory of the Visual Source Safe root
    Function GetVssRootDir() As String

        Dim vssDB As SourceSafeTypeLib.VSSDatabase = OpenVSSDatabase()
        Return vssDB.VSSItem("$/").LocalSpec
    End Function

    ' Gets the physical directory of the SourceSafe Control server. For example, on many machines
    ' it would return: "C:\Program Files\Microsoft Visual Studio\Common\VSS\win32"
    Function GetSccServerDir() As String

        ' get the Visual SourceSafe (tm) registry key
        Dim vssKey As Win32.RegistryKey = Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SourceSafe")

        ' get the full path to SSSCC.DLL
        Dim sccServerPath As String = vssKey.GetValue("SCCServerPath").ToString()

        ' return the directory of the SourceSafe server
        Return System.IO.Path.GetDirectoryName(sccServerPath)
    End Function

End Module
