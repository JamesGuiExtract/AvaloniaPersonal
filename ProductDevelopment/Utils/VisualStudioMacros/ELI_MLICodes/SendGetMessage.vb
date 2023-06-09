Option Strict On
Option Explicit On
Option Compare Text
Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml
Imports EnvDTE
Imports EnvDTE80

Public Module SendGetMessage

    ReadOnly DEVELOPERS_EMAILS() As String = { _
        "Nathan Figueroa <nathan_figueroa@extractsystems.com>", _
        "Arvind Ganesan <ag@extractsystems.com>", _
        "Steve Kurth <steve_kurth@extractsystems.com>", _
        "Wayne Lenius <wayne_lenius@extractsystems.com>", _
        "William Parr <william@extractsystems.com>", _
        "Jeff Shergalis <jeff_shergalis@extractsystems.com>"}

    Sub SendGetMessage()

        ' Create the subject heading and body of the email message
        Dim subject As String = "get"
        Dim message As New StringBuilder()

        ' Check if a checkin file exists
        Dim checkinsXml As String = Path.Combine(GetSccServerDir(), "checkins.xml")
        Dim checkinXmlExists As Boolean = File.Exists(checkinsXml)
        If checkinXmlExists Then

            ' load the checkins xml file
            Dim xmlDoc As New XmlDocument()
            xmlDoc.Load(checkinsXml)

            ' Get the display directories from the file tags
            Dim checkinDirs As String() = GetDirsFromXmlTagValues(xmlDoc, "File")
            With message

                ' Add the directories to the body of the email message
                For Each checkinDir As String In checkinDirs
                    .AppendLine(checkinDir)
                Next

                ' Add the comments to the body of the email message
                Dim comments As Queue(Of String) = GetXmlTagValues(xmlDoc, "Comment")
                For Each comment As String In comments
                    .Append("- ")
                    .AppendLine(comment)
                Next
                .AppendLine()

                ' Add the reviewers to the body of the email message
                Dim reviewers As Queue(Of String) = GetXmlTagValues(xmlDoc, "Reviewer")
                If reviewers.Count > 0 Then
                    .AppendLine("Reviewed by:")
                    .AppendLine()
                End If
                For Each reviewer As String In reviewers
                    .AppendLine(reviewer)
                Next
            End With
        Else
            ' There were no checkins, clear the subject heading
            subject = ""
        End If

        ' Send the email
        Dim mapi As New SimpleMAPI
        mapi.SendMail(subject, message.ToString(), DEVELOPERS_EMAILS)

        ' Check if the checkin file exists
        If checkinXmlExists Then

            ' Delete the old backup file if it exists
            Dim backupCheckinsXml As String = checkinsXml + ".bak"
            If File.Exists(backupCheckinsXml) Then
                File.Delete(backupCheckinsXml)
            End If

            ' Make a backup of the checkins file before deleting it
            File.Move(checkinsXml, backupCheckinsXml)
        End If
    End Sub

    Function GetDirsFromXmlTagValues(ByVal xmlDoc As XmlDocument, ByVal tagName As String) As String()

        ' Create a sorted dictionary to store directories
        Dim sortedDirectories As New SortedDictionary(Of String, String)()

        ' Get all the directories from the xml tags
        Dim xmlNodes As XmlNodeList = xmlDoc.GetElementsByTagName(tagName)
        For Each xmlNode As XmlNode In xmlNodes
            Dim dir As String = System.IO.Path.GetDirectoryName(xmlNode.InnerText).ToUpperInvariant()
            If Not sortedDirectories.ContainsKey(dir) Then
                sortedDirectories.Add(dir, "")
            End If
        Next

        ' Convert the directory names to their display format
        Dim directoryManager As New DisplayDirectoryManager()
        Return directoryManager.GetDisplayDirectories(sortedDirectories)
    End Function

    Function GetXmlTagValues(ByVal xmlDoc As XmlDocument, ByVal tagName As String) As Queue(Of String)
        GetXmlTagValues = New Queue(Of String)
        Dim xmlNodes As XmlNodeList = xmlDoc.GetElementsByTagName(tagName)
        For Each xmlNode As XmlNode In xmlNodes
            Dim tagValue As String = xmlNode.InnerText.Trim()
            If Not GetXmlTagValues.Contains(tagValue) Then
                GetXmlTagValues.Enqueue(tagValue)
            End If
        Next
    End Function

    Class DisplayDirectoryManager

        Private displayDirectoryCollection As New LinkedList(Of Queue(Of DisplayDirectory))()
        Private rootDirectory As String

        Public Sub New()
            rootDirectory = System.IO.Path.Combine(GetVssRootDir(), "Engineering")
        End Sub

        Public Function GetDisplayDirectories(ByVal directories As SortedDictionary(Of String, String)) As String()

            ' Iterate through each directory
            Dim keys As System.Collections.Generic.SortedDictionary(Of String, String).KeyCollection = directories.Keys
            Dim displayDirectories(keys.Count) As String
            Dim i As Integer = 0
            For Each directory As String In keys

                ' Construct and store the display directory name for this directory
                displayDirectories(i) = GetDisplayDirectory(directory)

                ' Iterate to the next directory
                i += 1
            Next

            Return displayDirectories

        End Function

        Private Function GetDisplayDirectory(ByVal physicalDirectory As String) As String

            ' If this directory is not under the VSS root, just return the physical directory
            If Not physicalDirectory.StartsWith(rootDirectory, StringComparison.CurrentCultureIgnoreCase) Then

                Return physicalDirectory
            End If

            Dim displayDirectory As New StringBuilder()
            Dim startDirectoryIndex As Integer = rootDirectory.Length + 1
            Dim nextDirectorySlashIndex As Integer
            Dim directoryQueueNode As LinkedListNode(Of Queue(Of DisplayDirectory)) = displayDirectoryCollection.First

            ' Iterate through each subdirectory of the specified physical directory
            While startDirectoryIndex < physicalDirectory.Length

                ' Find the slash of the next directory
                nextDirectorySlashIndex = physicalDirectory.IndexOf("\", startDirectoryIndex)

                ' Find the name of the current subdirectory
                Dim subdirectory As String
                If nextDirectorySlashIndex < 0 Then

                    ' This is the last subdirectory
                    subdirectory = physicalDirectory.Substring(startDirectoryIndex)
                Else

                    ' Get the name of the next subdirectory
                    subdirectory = physicalDirectory.Substring(startDirectoryIndex, nextDirectorySlashIndex - startDirectoryIndex)
                End If

                ' Check if the display directory collection has gone this deep yet
                If directoryQueueNode Is Nothing Then

                    ' Create the new queue
                    directoryQueueNode = AddDirectoryQueueNode(Left(physicalDirectory, startDirectoryIndex))
                End If

                ' Queue up the current subdirectory in the directory queue
                Dim directoryQueue As Queue(Of DisplayDirectory) = directoryQueueNode.Value
                With directoryQueue
                    If Not .Peek().Physical = subdirectory Then

                        ' Remove child directories from the linked list
                        While directoryQueueNode IsNot displayDirectoryCollection.Last
                            displayDirectoryCollection.RemoveLast()
                        End While

                        ' Iterate through the subdirectory queue
                        While .Count > 0 AndAlso .Peek().Physical < subdirectory

                            ' remove directories until the directory is found
                            .Dequeue()
                        End While

                        ' If the directory wasn't found return the physical directory
                        ' NOTE: If the directory wasn't found, it means it's not currently in the directory tree on disk
                        ' or that the sorted dictionary of directories was not alphabetized in the same way as the 
                        ' linked list directory tree from disk. The latter would be an internal logic error.
                        If .Count = 0 OrElse .Peek().Physical <> subdirectory Then

                            ' If this directory queue is empty, remove it from the display directory collection
                            If .Count = 0 Then
                                displayDirectoryCollection.RemoveLast()
                                directoryQueueNode = displayDirectoryCollection.Last
                            End If

                            ' Return the original directory
                            Return physicalDirectory
                        End If

                    End If

                    ' Add the display subdirectory
                    displayDirectory.Append(.Peek().Display)
                End With

                ' If this is the last directory, we are done.
                If nextDirectorySlashIndex = -1 Then

                    Exit While
                End If

                ' Iterate stuff
                startDirectoryIndex = nextDirectorySlashIndex + 1
                directoryQueueNode = directoryQueueNode.Next()

            End While

            Return displayDirectory.ToString()
        End Function

        Private Function AddDirectoryQueueNode(ByVal parentDirectory As String) As LinkedListNode(Of Queue(Of DisplayDirectory))

            ' Get and sort an array of the subdirectories of the specified directory
            Dim subdirectories As String() = System.IO.Directory.GetDirectories(parentDirectory)
            System.Array.Sort(subdirectories)

            Dim displayDirectories(subdirectories.Length - 1) As DisplayDirectory
            Dim i As Integer = 0

            For Each directory As String In subdirectories

                ' Add this directory to the array
                displayDirectories(i) = New DisplayDirectory(directory.Substring(directory.LastIndexOf("\") + 1))

                ' Iterate to the next directory
                i += 1
            Next

            ' Ensure the uniqueness of each display directory name
            For i = 0 To displayDirectories.Length() - 2

                Dim isUnique As Boolean = True
                For j As Integer = i + 1 To displayDirectories.Length() - 1

                    ' Check if a preceding display name matches this one
                    If displayDirectories(i).Display = displayDirectories(j).Display Then

                        ' change both display names to their physical directory name
                        displayDirectories(j).Display = "/" + displayDirectories(j).Physical
                        isUnique = False
                    End If
                Next

                If Not isUnique Then
                    displayDirectories(i).Display = "/" + displayDirectories(i).Physical
                End If
            Next

            ' Add and return the resultant queue node
            Return displayDirectoryCollection.AddLast(New Queue(Of DisplayDirectory)(displayDirectories))

        End Function

    End Class

    Class DisplayDirectory
        Private physicalDir As String
        Private displayDir As String

        Sub New(ByVal physicalDirectory As String)
            physicalDir = physicalDirectory
            displayDir = "/"

            ' Construct the display directory from the first character and each subsequent 
            ' capital letter, number, or punctuation mark in the physical directory.
            Dim capLetterIndex As Integer = 0
            Dim capLetters As Char() = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890._-".ToCharArray()
            While capLetterIndex < physicalDir.Length

                ' Add this capital letter
                displayDir += physicalDir.Chars(capLetterIndex)

                ' Find the next capital letter
                capLetterIndex = physicalDir.IndexOfAny(capLetters, capLetterIndex + 1)
                If capLetterIndex < 0 Then
                    Exit While
                End If
            End While

            ' Replace UCLID with U
            displayDir = displayDir.Replace("UCLID", "U")

            ' If the result length is two or less, don't use the abbreviated form
            If displayDir.Length <= 2 Then
                displayDir = "/" + physicalDir
            End If

        End Sub

        Property Physical() As String
            Get
                Return physicalDir
            End Get
            Set(ByVal value As String)
                physicalDir = value
            End Set
        End Property

        Property Display() As String
            Get
                Return displayDir
            End Get
            Set(ByVal value As String)
                displayDir = value
            End Set
        End Property

    End Class

End Module
