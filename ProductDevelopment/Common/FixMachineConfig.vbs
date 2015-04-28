Option Explicit

Main

Sub Main
	
	Dim strFileName, strBackupFile
	Dim xmlDoc
	Dim dbProviderNodes, emptyProviderNodes
	Dim sqlCEProviderNode
	
	'File names for the machine.config and backup files
	strFileName = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\machine.config"
	strBackupFile = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\Config\machine.config.backup"
	
	' Load the machine.config file as XML
	Set xmlDoc = CreateObject("Microsoft.XMLDOM")
	xmlDoc.async = False
	xmlDoc.load(strFileName)
	
	' Look for problems in the machine.config file
	' Get the DbProviderFactories nodes
	Set dbProviderNodes = xmlDoc.selectNodes("//configuration/system.data/DbProviderFactories")
	
	' Get the Sql Server Ce provider node
	Set sqlCEProviderNode = xmlDoc.selectSingleNode("//configuration/system.data/DbProviderFactories/add[@invariant='System.Data.SqlServerCe.3.5']")
	
	' if either there is more than one DbProviderFactories node or the Sql Server Ce provider is missing will need to fix
	' the problems
	if ((dbProviderNodes.length > 1) OR (sqlCEProviderNode is nothing)) then
		' Make sure the script is running elevated
		elevateIfNeeded
		
		' Remove the extra DbProviderFactories entries if they exist
		Dim objNode
		Dim emptyNodeRemoved
		Dim firstSaved
		
		emptyNodeRemoved = false
		
		' If there is more than one DbProviderFactories node need to remove the extra
		if (dbProviderNodes.length > 1) then
			' Find all of the empty nodes
			Set emptyProviderNodes = xmlDoc.selectNodes("//configuration/system.data/DbProviderFactories[not(*)]")
			
			' If the there are any empty nodes they may need to be removed
			if (emptyProviderNodes.length > 0) then

				' If the total number of empty DbProviderFactories found is the same as the number of DbProviderFactories
				' then don't remove the first one
				if (dbProviderNodes.length = emptyProviderNodes.length) then
					firstsaved = false
					' Step thru all of the empty provider nodes
					for each objNode in emptyProviderNodes
						if (firstSaved) then
							' remove the node
							objNode.parentNode.removeChild(objNode)
						else
							' Don't remove the node but flag that the first node has been visited
							firstSaved = true
						end if
					next
				else
					' Remove all the empty nodes found
					emptyProviderNodes.RemoveAll()
				end if
				' Flag that the empty nodes have been removed
				emptyNodeRemoved = true
			end if
		end if
		
		Dim dbProvider
		Dim sqlCENodeAdded
		
		sqlCENodeAdded = false
		
		' if sqlCEProviderNode is nothing, need to add the sql ce provider node
		if  (sqlCEProviderNode is nothing) then
			' Create the new sql ce provider node
			Set sqlCEProviderNode = xmlDoc.createNode(1, "add", "")
			
			' Add the attributes 
			sqlCEProviderNode.setAttribute("name"), "Microsoft SQL Server Compact Data Provider"
			sqlCEProviderNode.setAttribute("invariant"), "System.Data.SqlServerCe.3.5"
			sqlCEProviderNode.setAttribute("description"), ".NET Framework Data Provider for Microsoft SQL Server Compact"
			sqlCEProviderNode.setAttribute("type"), "System.Data.SqlServerCe.SqlCeProviderFactory, System.Data.SqlServerCe, Version=3.5.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91"
			
			' Get the provider node to add the new sql CE provider node to 
			Set dbProvider = xmlDoc.selectSingleNode("//configuration/system.data/DbProviderFactories")
			
			if (dbProvider is nothing) then
				WScript.Echo "Unable to add provider. No changes were made to " + strFileName + vbCrLf
				Wscript.Quit
			else
				' Add the node to the DbProviderFactories node
				dbProvider.appendChild(sqlCEProviderNode)
				sqlCENodeAdded = true
			end if
		end if
		
		' Format and save the updated machine.config file
		Dim xsl
		Dim formattedDoc
		if ( sqlCENodeAdded or emptyNodeRemoved ) then
		
			' Make a backup copy of the machine.config file
			backupFile strFileName, strBackupFile

			Set xsl = CreateObject("Microsoft.XMLDOM")
			xsl.async = False
			xsl.loadXml("<xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">" & vbCrLf &_
							"<xsl:output method=""xml"" version=""1.0"" encoding=""utf-8"" indent=""yes""/>" & vbCrLf &_
							"<xsl:template match=""@* | node()"">" & vbCrLf &_
								"<xsl:copy>" & vbCrLf &_
									"<xsl:apply-templates select=""@* | node()""/>" & vbCrLf &_
								"</xsl:copy>" & vbCrLf &_
							"</xsl:template>" & vbCrLf &_
							"<xsl:template match=""*[count(node())=0]"">" & vbCrLf &_
								"<xsl:copy>" & vbCrLf &_
									"<xsl:apply-templates select=""@*""/>" & vbCrLf &_
								"</xsl:copy>" & vbCrLf &_
							"</xsl:template>" & vbCrLf &_
						"</xsl:stylesheet>")
			
			Set formattedDoc = CreateObject("Microsoft.XMLDOM")
			formattedDoc.async = False
			xmlDoc.transformNodeToObject(xsl), formattedDoc
			
			On Error Resume Next
			formattedDoc.save(strFileName)
			if (Err.Number > 0) then
				WScript.Echo "There was an error saving " + strFileName + ". ", vbCrLf + _
					"Error description: " + Err.Description + vbCrLf
				WScript.Echo "No changes were made.", vbCrLf
				Wscript.Quit
			end if
			
			Dim finalPrompt
			
			finalPrompt = "Problem(s) fixed:" + vbCrLf
			if (emptyNodeRemoved) then
				finalPrompt = finalPrompt + "Removed empty DbProviderFactories entries." + vbCrLf 
			end if
			if (sqlCENodeAdded) then
				finalPrompt = finalPrompt + "Added DbProviderFactories entry for SQL CE 3.5." + vbCrLf
			end if
			wScript.Echo finalPrompt
		end if
	else
		WScript.Echo "No problems found with " + strFileName + vbCrLf
	end if
end sub

' Backups the source file to the destFile
function backupFile(sourceFile, destFile)
	Dim objFSO

	' Make a backup copy of the machine.config file
	'Create the FileSystemObject
	Set objFSO = CreateObject("Scripting.FileSystemObject")
	
	'Check for the backup file
	if (objFSO.FileExists(destFile)) then
		'Backup file exists so change cannot be made
		WScript.Echo "Backup file " + destFile + " already exists. " + vbCrLf
		WScript.Quit
	end if
	
	On Error Resume Next
	'Copy the machine.config file to machine.config.backup
	objFSO.CopyFile sourceFile, destFile, false
	if (Err.Number > 0) then
		WScript.Echo "There was an error creating the backup. You must have administrator rights to update file." + _
			vbCrLf + vbCrLf + "Error description: " + Err.Description + vbCrLf
		WScript.Quit
	end if
end function

' Elevates the running of the script if needed
sub elevateIfNeeded
	' Attempt to elevate if newer than Windows XP
	if (isAfterWin52) then
		if not isElevated then 
			'Reopens script with uacPrompt
			uacPrompt
		end if
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