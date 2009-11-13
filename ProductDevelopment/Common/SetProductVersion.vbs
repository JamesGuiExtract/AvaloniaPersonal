Option Explicit
' This file is to be used to set the product version of the Install shield project before it is built with the automated build scripts
' It should be ran using cscript so that the output goes to the command window instead of displaying windows dialogs

Main

Sub Main
	Dim objArgs
	Set objArgs = WScript.Arguments

	Dim stdout
	set stdout = WScript.StdOut

	'Verify that there are 2 arguments
	if ( objArgs.Count <> 2 ) then
		stdout.WriteLine "Expects 2 arguments: <Project File Name> <Product Version>"
		stdout.WriteLine "   <Product Version> is expected to have the string ""Ver. "" followed by the Numeric version "
		exit sub
	end if
	
	Dim sProjectName
	Dim sVersionNumber
	
	'First arguement is the project name
	sProjectName = objArgs(0)

	'Extract the numeric version from the version string
	Dim nVerPos
	nVerPos = InStr(1, objArgs(1), "Ver. ")
	if ( nVerPos = 0 ) then
		stdout.WriteLine "Version not fould " + objArgs(1)
		exit sub
	end if
	sVersionNumber = Mid( objArgs(1), nVerPos + 5)
	
	'Separate the version number from any suffix that follows (ie, patch letter) by creating a RegEx search
	'for a version number
    Dim RegExOb
    Set RegExOb = New RegExp
    RegExOb.Pattern = "[\d]+\.[\d]+\.[\d]+\.[\d]+"
    RegExOb.IgnoreCase = True
    RegExOb.Global = True
    
    Dim Matches
    Set Matches = RegExOb.Execute(sVersionNumber)
 
    if (Matches.Count > 0) then
        sVersionNumber = Matches(0).Value
    else
        stdout.WriteLine "Unexpected version format: " + objArgs(1)
        exit sub
    end if
	
	Dim pProject
	Set pProject = CreateObject ("IswiAuto16.ISWiProject")

	'Open Project
	pProject.OpenProject sProjectName, False

	'Set the product version
	pProject.ProductVersion = sVersionNumber
	
	'Import the version strings
	'Create the path by using the path to the project files
	Dim sVersionFile
	RegExOb.Pattern = "[\s\S]+Engineering"
	Set Matches = RegExOb.Execute(sProjectName)
	if ( Matches.Count > 0 ) then
		sVersionFile = Matches(0).Value + "\ProductDevelopment\Common\ExtractSystemsVersions.txt"
		stdout.WriteLine(sVersionFile)
		pProject.ImportStrings sVersionFile, 1033, 0
	end if
	
	'Save project with the updated product version
	pProject.SaveProject
	'Close project
	pProject.CloseProject

end sub
