On Error Resume Next
Dim objShell, objFSO, objSourceFolder, objTargetFolder, objSourceFile, objTargetFile
Set objShell = WScript.CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
Set objSourceFolder = objFSO.GetFolder(WScript.Arguments.Item(0))

While true
	If objSourceFolder.Files.Count > 2000 Then
		WScript.Sleep 10000
	Else
		For Each objSourceFile in objSourceFolder.Files
			If Len(lastFileName) = 0 Then 
				lastFileName = objSourceFile.Path
			Else
				objFSO.CopyFile lastFileName, objSourceFile.Path, True
			End If
			WScript.Sleep 50
			lastFileName = objSourceFile.Path
		Next
	End If
Wend
