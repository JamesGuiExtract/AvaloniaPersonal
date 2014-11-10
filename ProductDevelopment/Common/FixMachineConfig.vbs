Option Explicit

Main

Sub Main

	' Constants for the OpenTextFile method
	Const ForReading = 1
	Const ForWriting = 2
	
	Dim strFileName, strBackupFile
	Dim objFSO
	Dim objFile
	Dim strText, newStrText, replacementText
	
	'File names for the machine.config and backup files
	strFileName = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\machine.config"
	strBackupFile = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\machine.config.backup"
	
	'Create the FileSystemObject
	Set objFSO = CreateObject("Scripting.FileSystemObject")
	
	'Open the machine.config file 
	Set objFile = objFSO.OpenTextFile(strFileName, ForReading)

	' Read the contents from the file
	strText = objFile.ReadAll
	objFile.Close

	'Create RegExp object for searching the contents
    Dim RegExOb
    Set RegExOb = New RegExp
	
	'Check for the beginning tag for DbProviderFactories
    RegExOb.Pattern = "<DbProviderFactories>"
    RegExOb.IgnoreCase = True
    RegExOb.Global = True
    
	'Find matches for the beginning tag 
    Dim Matches
    Set Matches = RegExOb.Execute(strText)
 
	' if the Beginning tag for DbProviderFactories was found check for the invalid tag 
    if (Matches.Count > 0) then
		'Search for the invalid tag (since the beginning tag was found
		RegExOb.Pattern = "\s*<DbProviderFactories\s*/>"
		Set Matches = RegExOb.Execute(strText)
		
		'If invalid tag was found attempt to remove it 
		if (Matches.Count > 0) then
			'Replace the invalid tag with an empty string
			replacementText = ""
			newStrText = RegExOb.Replace(strText, replacementText)
			
			'if the change was made create a backup of the machine.config file and save the changes
			if (newStrText <> strText ) then
				' Attempt to elevate if newer than Windows XP
				if (isAfterWin52) then
					if not isElevated then 
						'Reopens script with uacPrompt
						uacPrompt
					end if
				end if

				'Check for the backup file
				if (objFSO.FileExists(strBackupFile)) then
					'Backup file exists so change cannot be made
					WScript.Echo "No changes made to Machine.Config file. Unable to create " + strBackupFile
					WScript.Quit
				end if
				
				On Error Resume Next
				'Copy the machine.config file to machine.config.backup
				objFSO.CopyFile strFileName, strBackupFile, false
				if (Err.Number > 0) then
					WScript.Echo "There was an error creating the backup. You must have administrator rights to update file."
					WScript.Quit
				end if
				
				'Save the changed text to the machine.config file
				Set objFile = objFSO.OpenTextFile(strFileName, ForWriting)
				objFile.Write newStrText
				if (Err.Number > 0) then
					WScript.Echo "There was an error creating the backup. You must have administrator rights to update file."
					WScript.Quit
				end if
				objFile.Close
				
				WScript.Echo "Removed <DbProviderFactories/> from Machine.Config file."
			else
				WScript.Echo "No changes made to Machine.Config file"
			end if
		else
			WScript.Echo "Problem not found. No changes made to Machine.Config file"
		end if
	else
		WScript.Echo "Problem not found. No changes made to Machine.Config file"
    end if

end sub

'Modified code from http://www.kellestine.com/self-elevate-vbscript/
'Checks if the script is running elevated (UAC)
function isElevated
	On Error Resume Next
	Dim shell
	Dim whoami
	Dim whoamiOutput
	Dim strWhoamiOutput
	
	Set shell = CreateObject("WScript.Shell")
	Set whoami = shell.Exec("whoami /groups")
	'If the whoami command is unavailable, return true - the whoami app is not always installed on XP
	'so mark is isElevated to prevent an infinite loop.
	if Err.Number <> 0 then	
		isElevated = True
		exit function
	end if
	
	Set whoamiOutput = whoami.StdOut
	strWhoamiOutput = whoamiOutput.ReadAll

	If InStr(1, strWhoamiOutput, "S-1-16-12288", vbTextCompare) Then 
		isElevated = True
	Else
		isElevated = False
	End If
end function

'Modified code from http://www.kellestine.com/self-elevate-vbscript/
'Re-runs the process prompting for priv elevation on re-run
sub uacPrompt
	Dim interpreter
	Dim shellApp
	
	'Check if we need to run in C or W script
	interpreter = "wscript.exe"
	If Instr(1, WScript.FullName, "CScript", vbTextCompare) = 0 Then
	interpreter = "wscript.exe"
	else
	interpreter = "cscript.exe"
	end if
	
	'Start a new instance with an elevation prompt first
	Set shellApp = CreateObject("Shell.Application")
	shellApp.ShellExecute interpreter, Chr(34) & WScript.ScriptFullName & Chr(34) & " RunAsAdministrator", "", "runas", 1
	
	'End the non-elevated instance
	WScript.Quit
end sub

'Code from http://www.kellestine.com/self-elevate-vbscript/ from JohnLBevan's comment
function isAfterWin52() 'see http://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions#Server_versions for full list
	dim objWMIService, objOSs, objOS, version, result
	Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}!\\.\root\cimv2")
	Set objOSs = objWMIService.ExecQuery ("Select * from Win32_OperatingSystem")
	result = false
	for each objOS in objOSs 
		version = Split(objOS.Version, ".", -1, vbTextCompare)
		result = result or CDbl(version(0) & "." & version(1)) > 5.2
	next
	isAfterWin52 = result
	Set objOS = Nothing
	Set objOSs = Nothing
	Set objWMIService = Nothing
end function