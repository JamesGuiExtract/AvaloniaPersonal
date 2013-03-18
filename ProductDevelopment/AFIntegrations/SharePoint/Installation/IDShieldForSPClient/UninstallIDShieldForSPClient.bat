@echo Uninstalling Exception Service
@IF "C:\Program Files (x86)"=="%ProgramFiles(x86)%" (

	del "C:\Program Files (x86)\Extract Systems\CommonComponents\Extract.SharePoint.Redaction.dll"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\IDShieldForSPClient.exe"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\Microsoft.SharePoint.Client.dll"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\Microsoft.SharePoint.Client.Runtime.dll"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\Extract.ExtensionMethods.dll"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\Extract.SharePoint.Redaction.Utilities.dll"
	del "C:\Program Files (x86)\Extract Systems\CommonComponents\RemoveExtractSPColumns.exe"
) ELSE (
	del "C:\Program Files\Extract Systems\CommonComponents\Extract.SharePoint.Redaction.dll"
	del "C:\Program Files\Extract Systems\CommonComponents\IDShieldForSPClient.exe"
	del "C:\Program Files\Extract Systems\CommonComponents\Microsoft.SharePoint.Client.dll"
	del "C:\Program Files\Extract Systems\CommonComponents\Microsoft.SharePoint.Client.Runtime.dll"
	del "C:\Program Files\Extract Systems\CommonComponents\Extract.ExtensionMethods.dll"
	del "C:\Program Files\Extract Systems\CommonComponents\Extract.SharePoint.Redaction.Utilities.dll"
	del "C:\Program Files\Extract Systems\CommonComponents\RemoveExtractSPColumns.exe"
)

