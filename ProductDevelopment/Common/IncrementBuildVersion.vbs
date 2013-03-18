Option Explicit
' This file is to be used to set the product version of the Install shield project before it is built with the automated build scripts
' It should be ran using cscript so that the output goes to the command window instead of displaying windows dialogs

Main

Sub Main
	Dim objArgs
	Set objArgs = WScript.Arguments

	Dim stdout
	set stdout = WScript.StdOut

	'Open LatestComponentVersions file
	dim objFSO
	dim objOldLatestComponentVersions
	dim objNewLatestComponentVersions
	
	Set objFSO = CreateObject("Scripting.FileSystemObject")
	
	Set objOldLatestComponentVersions = objFSO.OpenTextFile("LatestComponentVersions.mak", 1)
	Set objNewLatestComponentVersions = objFSO.CreateTextFile("LatestComponentVersions.new.mak", true)
	
	Dim sLine
	
	'Setup RegEx to look for last number
    Dim RegExOb
    Set RegExOb = New RegExp
	RegExOb.Pattern = "\d+$"
    RegExOb.IgnoreCase = True
    RegExOb.Global = True
	
	Dim Matches
	Dim value
	Dim strValue
	
	'Load each line and if it has a number at the end of a line increment the number
	Do Until objOldLatestComponentVersions.AtEndOfStream
		'Get the line
		sLine = objOldLatestComponentVersions.ReadLine
		
		' Get the last number in the version
		Set Matches = RegExOb.Execute(sLine) 
		
		'If there was a match increment it
		if (Matches.Count > 0 ) then
			value = CInt(Matches(0).Value) + 1
			strValue = CStr(value)
			objNewLatestComponentVersions.WriteLine( RegExOb.Replace(sLine, strValue))
		else
			objNewLatestComponentVersions.WriteLine(sLine)
		end if
	Loop
	
	objOldLatestComponentVersions.Close()
	objNewLatestComponentVersions.Close()
	
	'Delete the old latest components
	objFSO.DeleteFile "LatestComponentVersions.mak"
	
	'Rename the new
	objFSO.MoveFile "LatestComponentVersions.new.mak", "LatestComponentVersions.mak"
end sub
