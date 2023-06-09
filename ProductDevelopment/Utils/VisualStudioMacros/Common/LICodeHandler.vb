Option Strict On
Option Explicit On
Option Compare Binary
Imports System
Imports EnvDTE
Imports EnvDTE80
Imports System.Diagnostics
Imports System.Text.RegularExpressions

Enum LITypeNum
    ELI = 0
    MLI = 1
End Enum

' Handles the retrieval and modification of the Location Identifier dat files
Class LICodeHandler
    Implements IDisposable

    ' Location identifier number format string (five digits)
    Const LI_NUM_FORMAT As String = "D5"

    ' Location identifier dat file data type
    Private Class LIDatFileData

        ' Source safe path to the dat file (eg. $\Engineering\file.ext)
        Public vssPath As String

        ' Source safe item corresponding to dat file
        Public vssItem As SourceSafeTypeLib.VSSItem

        ' String builder containing lines to append to dat file for new LI codes
        Public builder As Text.StringBuilder

        ' The prefix of the location identifier code (ELI or MLI)
        Public LIPrefix As String

        ' The location identifier number (eg. 12345 in ELI12345)
        Public LINum As Integer = -1

        ' Source safe path and location identifier prefix must be specified at instantiation
        Sub New(ByVal newVssPath As String, ByVal newLIPrefix As String)
            vssPath = newVssPath
            LIPrefix = newLIPrefix
        End Sub
    End Class

    ' Visual SourceSafe database
    Private vssDB As SourceSafeTypeLib.VSSDatabase

    ' Array of location identifier dat file data objects
    Private LIDatFiles() As LIDatFileData = { _
        New LIDatFileData("$/Engineering/ProductDevelopment/Common/UCLIDExceptionLocationIdentifiers.dat", "ELI"), _
        New LIDatFileData("$/Engineering/ProductDevelopment/Common/ExtractMethodLocationIdentifiers.dat", "MLI")}

    ' Whether this object has been disposed
    Protected disposed As Boolean = False

    Sub New()
        ' do nothing of consequence
    End Sub

    Sub New(ByVal vssDatabase As SourceSafeTypeLib.VSSDatabase)

        ' store the VSS database
        vssDB = vssDatabase
    End Sub

    ReadOnly Property NextELICode() As String
        Get
            ' open the Visual SourceSafe database if not already opened
            If vssDB Is Nothing Then
                vssDB = OpenVSSDatabase()
            End If

            ' retrieve the ELI dat file if it has not already been retrieved
            RetrieveLIDatFileFromVSS(LIDatFiles(LITypeNum.ELI))

            Return GetNextLICode(LITypeNum.ELI)
        End Get
    End Property

    ReadOnly Property NextMLICode() As String
        Get
            ' open the Visual SourceSafe database if not already opened
            If vssDB Is Nothing Then
                vssDB = OpenVSSDatabase()
            End If

            ' retrieve the MLI dat file if it has not already been retrieved
            RetrieveLIDatFileFromVSS(LIDatFiles(LITypeNum.MLI))

            Return GetNextLICode(LITypeNum.MLI)
        End Get
    End Property

    Function ReplaceLICodesInText(ByVal textToReplace As String) As String

        ' open the Visual SourceSafe database if not already opened
        If vssDB Is Nothing Then
            vssDB = OpenVSSDatabase()
        End If

        ' retrieve both dat files
        For Each datFile As LIDatFileData In LIDatFiles
            ' get the vss item for the LI dat file if not already retrieved
            RetrieveLIDatFileFromVSS(datFile)
        Next

        ' replace the selected text with new LI codes
        Dim regex As New Regex("""(M|E)LI\d+""")
        Return regex.Replace(textToReplace, AddressOf ReplaceLI)
    End Function

    Sub CommitChanges()

        For Each datFile As LIDatFileData In LIDatFiles

            ' check if the LIBuilder exists
            If datFile.builder IsNot Nothing Then
                With datFile

                    ' check if any changes to the LI dat file need to be committed
                    If .builder.Length > 0 Then

                        ' add newly added LI codes to the dat file and check it in
                        My.Computer.FileSystem.WriteAllText(.vssItem.LocalSpec, .builder.ToString(), True)
                        .vssItem.Checkin()
                    Else

                        ' undo the checkout on the LI dat file
                        .vssItem.UndoCheckout()
                    End If

                    ' reset the values of the current LI number and the LI string builder
                    .LINum = -1
                    .builder = Nothing
                End With
            End If
        Next

    End Sub

    Private Function GetLastLICodeInDatFile(ByVal filename As String) As Integer

        ' open the LI dat file as a comma-delimited file
        Using parser As New FileIO.TextFieldParser(filename)
            With parser
                .TextFieldType = FileIO.FieldType.Delimited
                .Delimiters = New String() {","}

                ' iterate through each entry until the end of the file
                Dim row() As String
                While Not .EndOfData
                    row = .ReadFields()
                End While

                ' get the LI number of the last entry
                Return CInt(row(0).Remove(0, 3))
            End With
        End Using

    End Function

    Private Function GetNextLICode(ByVal index As Integer) As String

        With LIDatFiles(index)

            ' increment the LI number
            .LINum += 1

            ' create a new line for this LI code
            .builder.Append(.LIPrefix & .LINum.ToString(LI_NUM_FORMAT) & "," _
              & Microsoft.VisualBasic.Environ("USERNAME") & "," _
              & Microsoft.VisualBasic.Environ("COMPUTERNAME") & "," _
              & Date.Now.ToString("MM/dd/yyyy,HH:mm:ss") & ",," & vbCrLf)

            ' return this LI number
            Return """" & .LIPrefix & .LINum.ToString(LI_NUM_FORMAT) & """"
        End With

    End Function

    Private Function ReplaceLI(ByVal match As Match) As String

        ' TODO: add progress status

        ' get the LI dat file data associated with this match
        Dim i As Integer = CInt(IIf(match.Value.Chars(1) = "E"c, 0, 1))

        ' get increment and return the LI code number
        Return GetNextLICode(i)

    End Function

    Private Sub RetrieveLIDatFileFromVSS(ByVal datFile As LIDatFileData)
        With datFile
            If .LINum = -1 Then
                .vssItem = CheckoutVSSItemFromVSSPath(vssDB, .vssPath)
                .builder = New Text.StringBuilder

                ' get the last LI code from the dat file
                .LINum = GetLastLICodeInDatFile(.vssItem.LocalSpec)

            End If
        End With
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not disposed Then

            ' undo the check out of the ELI dat file if it is currently held
            For Each datFile As LIDatFileData In LIDatFiles
                With datFile
                    If .vssItem IsNot Nothing And .vssItem.IsCheckedOut = SourceSafeTypeLib.VSSFileStatus.VSSFILE_CHECKEDOUT_ME Then
                        .vssItem.UndoCheckout()
                    End If
                End With
            Next

            disposed = True
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub

End Class

