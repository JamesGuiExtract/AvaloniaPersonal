Dim targetApplications(5)
targetApplications(0) = "ProcessFiles.exe"
targetApplications(1) = "DataEntryApplication.exe"
targetApplications(2) = "RunFPSFile.exe"
targetApplications(3) = "RuleSetEditor.exe"
targetApplications(4) = "RuleTester.exe"
targetApplications(5) = "FAMProcess.exe"

Dim dependentAssemblies(16)
dependentAssemblies(0)  = "Extract"
dependentAssemblies(1)  = "Extract.AttributeFinder"
dependentAssemblies(2)  = "Extract.DataEntry"
dependentAssemblies(3)  = "Extract.DataEntry.LabDE"
dependentAssemblies(4)  = "Extract.Database"
dependentAssemblies(5)  = "Extract.Drawing"
dependentAssemblies(6)  = "Extract.FileActionManager.FAMFileInspector"
dependentAssemblies(7)  = "Extract.FileActionManager.Forms"
dependentAssemblies(8)  = "Extract.Imaging"
dependentAssemblies(9)  = "Extract.Imaging.Forms"
dependentAssemblies(10) = "Extract.Interop"
dependentAssemblies(11) = "Extract.Licensing"
dependentAssemblies(12) = "Extract.Utilities"
dependentAssemblies(13) = "Extract.Utilities.Forms"
dependentAssemblies(14) = "FAMFileInspector"
dependentAssemblies(15) = "LeadTools.WinForms"
dependentAssemblies(16) = "PaginationUtility"
                                                                      
Const namespaceURI = "urn:schemas-microsoft-com:asm.v1"

Set wshShell = CreateObject("WScript.Shell")
Set fileSys = CreateObject("Scripting.FileSystemObject")
commonComponents = "C:\Engineering\binaries\debug"

resultMessage = Empty

For Each targetApplication In targetApplications
	ApplyConfig(commonComponents & "\" & targetApplication & ".config")
Next

showMessageBox = 1 
If WScript.Arguments.Count = 1 Then
	showMessageBox = StrComp(WScript.Arguments.Item(0), "/c", vbTextCompare)
End If
	
If showMessageBox = 1 Then
	MsgBox(resultMessage), vbOKOnly, "Config file changes for DEP Compatibility"
Else
	Wscript.Echo resultMessage
End If

Function ApplyConfig(filename)
	createdConfig = False
	modifiedConfig = False
	
	Set xmlDoc = CreateObject("Microsoft.XMLDOM")
	xmlDoc.async = False
	
	If filesys.FileExists(filename) Then
		xmlDoc.load(filename)
	Else
		Set xmlDoc.documentElement = xmlDoc.createElement("configuration")
		createdConfig = True
	End If
		
	Set runtimeNode = FindNode(xmlDoc.documentElement, "runtime", Empty, Empty, False)

	If runtimeNode Is Nothing Then
		Set runtimeNode = xmlDoc.createElement("runtime")
		xmlDoc.documentElement.appendChild(runtimeNode)
	End If

	Set remoteSourcesNode = FindNode(runtimeNode, "loadFromRemoteSources", Empty, Empty, False)
	If remoteSourcesNode Is Nothing Then
		Set remoteSourcesNode = xmlDoc.createNode(1, "loadFromRemoteSources", namespaceURI)
		runtimeNode.appendChild(remoteSourcesNode)
		remoteSourcesNode.setAttribute("enabled"), "true"
		modifiedConfig = True
	ElseIf remoteSourcesNode.getAttribute("enabled") <> "true" Then
		remoteSourcesNode.setAttribute("enabled"), "true"
		modifiedConfig = True
	End If

	Set assemblyBindingNode = FindNode(runtimeNode, "assemblyBinding", Empty, Empty, False)
	If assemblyBindingNode Is Nothing Then
		Set assemblyBindingNode = xmlDoc.createNode(1, "assemblyBinding", namespaceURI)
		runtimeNode.appendChild(assemblyBindingNode)
	End If
	
	For Each dependentAssembly in dependentAssemblies
		Set dependentAssemblyNode = FindNode(assemblyBindingNode, "assemblyIdentity", "name", dependentAssembly, True)
		If dependentAssemblyNode Is Nothing Then
			Set dependentAssemblyNode = xmlDoc.createNode(1, "dependentAssembly", namespaceURI)
			assemblyBindingNode.appendChild(dependentAssemblyNode)
			
			Set assemblyIdentityNode = xmlDoc.createNode(1, "assemblyIdentity", namespaceURI)
			assemblyIdentityNode.setAttribute("name"), dependentAssembly
			assemblyIdentityNode.setAttribute("publicKeyToken"), "329544a1499f0564"
			assemblyIdentityNode.setAttribute("culture"), "neutral"
			dependentAssemblyNode.appendChild(assemblyIdentityNode)
		End If
		
		installedVersion = Empty
		If fileSys.FileExists(commonComponents & "\" & dependentAssembly & ".dll") Then
			installedVersion = fileSys.GetFileVersion(commonComponents & "\" & dependentAssembly & ".dll")
		ElseIf fileSys.FileExists(commonComponents & "\" & dependentAssembly & ".exe") Then
			installedVersion = fileSys.GetFileVersion(commonComponents & "\" & dependentAssembly & ".exe")
		End If
		
		If Not IsEmpty(installedVersion) Then
			Set bindingRedirectNode = FindNode(dependentAssemblyNode, "bindingRedirect", "newVersion", installedVersion, False)
			If Not bindingRedirectNode Is Nothing Then
				oldVersion = bindingRedirectNode.getAttribute("oldVersion")
				If Not oldVersion = "1.0.0.0-100.0.0.0" Then
					dependentAssemblyNode.removeChild(bindingRedirectNode)
					Set bindingRedirectNode = Nothing
				End If
			End If
			
			If bindingRedirectNode Is Nothing Then
				Set bindingRedirectNode = xmlDoc.createNode(1, "bindingRedirect", namespaceURI)
				bindingRedirectNode.setAttribute("oldVersion"), "1.0.0.0-100.0.0.0"
				bindingRedirectNode.setAttribute("newVersion"), installedVersion
				dependentAssemblyNode.appendChild(bindingRedirectNode)
				
				modifiedConfig = True
			End If
		End If
	Next
	
	If modifiedConfig = True Then
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
		formattedDoc.save(filename)
		
		If createdConfig = True Then
			resultMessage = resultMessage & "Created " & targetApplication & ".config" & vbCrLf
		Else
			resultMessage = resultMessage & "Updated " & targetApplication & ".config" & vbCrLf
		End If
	Else
		resultMessage = resultMessage & "No modifications necessary for " & targetApplication & ".config" & vbCrLf
	End If
End Function

Function FindNode(xmlNode, name, attributeName, attributeValue, recursive)
	For Each child In xmlNode.childNodes
		compareResult = StrComp(child.nodeName, name, vbTextCompare)
		If compareResult = 0 Then
			If IsEmpty(attributeName) Then
				Set FindNode = child
				Exit Function
			End If
			For Each attribute in child.attributes
				nameCompareResult = StrComp(attribute.name, attributeName, vbTextCompare)
				valueCompareResult = StrComp(attribute.value, attributeValue, vbTextCompare)
				If nameCompareResult = 0 And valueCompareResult = 0 Then
					Set FindNode = child
					Exit Function
				End If
			Next
		End If
		If recursive = True And Not child Is Nothing Then
			Set FindNode = FindNode(child, name, attributeName, attributeValue, recursive)
			If Not FindNode Is Nothing Then
				Set FindNode = child
				Exit Function
			End If
		End If
	Next
	Set FindNode = Nothing
End Function




