On Error Resume Next
Randomize
Dim objShell, objFSO, objSourceFolder, objTargetFolder, objSourceFile, objTargetFile
Set objShell = WScript.CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objSourceFolder = objFSO.GetFolder(WScript.Arguments.Item(0))
targetFolderName = WScript.Arguments.Item(1)
Set objTargetFolder = objFSO.GetFolder(targetFolderName)

While true
	If objTargetFolder.Files.Count> 2000 Then
		WScript.Sleep 30000
	Else
		For Each objSourceFile in objSourceFolder.Files
			If Len(lastFileName) = 0 Then 
				lastFileName = objSourceFile.Path
			End If
			Randomize
			randomNum = Int((sourceCount - 1) * Rnd)
			If randomNum < targetCopyCount Then
				destFile = targetFolderName & "\" & GetRandomName(10) & ".pdf"
				If Not objFSO.FileExists(destFile) Then
					objFSO.CopyFile lastFileName, destFile, True
					WScript.Sleep 50
				End If
			End If
			lastFileName = objSourceFile.Path
		Next
	End If
Wend

Function GetRandomName(Count)

	GetRandomName = Now
	GetRandomName = Replace(GetRandomName, "/", "-")
	GetRandomName = Replace(GetRandomName, ":", "-")
	GetRandomName = Replace(GetRandomName, "AM", "")
	GetRandomName = Replace(GetRandomName, "PM", "")

    For i = 1 To Count
        If (Int((1 - 0 + 1) * Rnd + 0)) Then
            GetRandomName = GetRandomName & Chr(Int((90 - 65 + 1) * Rnd + 65))
        Else
            GetRandomName = GetRandomName & Chr(Int((57 - 48 + 1) * Rnd + 48))
        End If
    Next

End Function