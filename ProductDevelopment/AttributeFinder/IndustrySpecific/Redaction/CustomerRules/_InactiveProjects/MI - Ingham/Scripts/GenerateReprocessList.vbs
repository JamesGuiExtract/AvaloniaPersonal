'--------------------------------------------------------------------------------------------------
' Script commands specific for MI - Ingham
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' where:
'     GStrImg = H:\Verification\Input\O--1992-12-31-1-00774.TIF
'     GStrReport = H:\Verification\DetailedReportOutputFile.txt
'--------------------------------------------------------------------------------------------------

Dim GObjArgs, GStrImg, GStrReport
Set GObjArgs = WScript.Arguments
GStrImg = GObjArgs(0)
GStrReport = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input Image From Command-Line", GStrImg
HandleDebug "Output File For Report From Command-Line", GStrReport

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI21990", "Unable to create File System Object!"

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Verification\Input\O--1992-12-31-1-00774.TIF ) = 
'       "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

if hasBackwardsRedaction(GStrXMLPath) Then
  AppendToFile GStrImg, GStrReport
End If


'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for MI - Ingham
'--------------------------------------------------------------------------------------------------
'Check for right-to-left redaction zone
'--------------------------------------------------------------------------------------------------
Function hasBackwardsRedaction(strXMLPath)
  ' Define error handler
  On Error Resume Next

  ' Create the XML DOM Document object
  Dim xmlDoc
  Set xmlDoc = CreateObject("MSXML.DOMDocument")
  HandleScriptError0 "ELI21991", "Unable to create XML DOM Document Object!"

  ' Load the XML metadata file
  xmlDoc.load(strXMLPath)
  HandleScriptError1 "ELI21992", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

  ' Get the collection of applied, manual, redaction nodes
  Dim xmlList
  Set xmlList = xmlDoc.selectNodes("/IDShieldMetaData/Redactions/Redaction[@Output='1' and @Type='Man']/Line/Zone")
  HandleScriptError1 "ELI21993", "Unable to select Redaction nodes!", "XML Path", strXMLPath

  ' Check each node in the collection
  Dim nCount, I
  nCount = xmlList.length
  HandleDebug "Redactions Node Count", CStr(nCount)
  For I = 1 To nCount
    ' Retrieve this Node
    Dim xmlThisNode
    Set xmlThisNode = xmlList.item(I-1)
    HandleScriptError1 "ELI21994", "Unable to retrieve Redaction Node #" + CStr(I) + " of " +_
                              CStr(nCount) + "!", "XML Path", strXMLPath
    ' Retrieve the StartX item
    Dim intX1, intX2
    Dim xmlSingleZoneNode
    Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@StartX")
    HandleScriptError1 "ELI21995", "Unable to select StartX attribute!", "XML Path", strXMLPath
    intX1 = CInt(xmlSingleZoneNode.text)
    HandleDebug "StartX", CStr(intX1)

    ' Retrieve the EndX item
    Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@EndX")
    HandleScriptError1 "ELI21996", "Unable to select EndX attribute!", "XML Path", strXMLPath
    intX2 = CInt(xmlSingleZoneNode.text)
    HandleDebug "EndX", CStr(intX2)

    If intX1 > intX2 Then
      hasBackwardsRedaction = true
      Exit Function
    End If
  Next
  hasBackwardsRedaction = false
End Function

'--------------------------------------------------------------------------------------------------
' Get XML filename from specified image file path
' where:
'     XMLPath( H:\Verification\Input\O--1992-12-31-1-00774.TIF ) = 
'       "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml"
'--------------------------------------------------------------------------------------------------
Function strGetXMLPath(strImagePath)
    ' Append the XML file extension
    strGetXMLPath = strImagePath + ".xml"
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Folder from path
' where:
'     Folder( H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml ) = "H:\Verification\Input"
'--------------------------------------------------------------------------------------------------
Function strGetFolder(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract folder portion of filename by finding last backslash
    ' where:
    '    "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml" ====> "H:\Verification\Input"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        strGetFolder = Left(strPath, p-1)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Appends the specified information to the specified output file
'--------------------------------------------------------------------------------------------------
Function AppendToFile(strInfoLine, strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering AppendToFile()", strInfoLine + ", " + strOutputPath

    ' Create named Mutex Object to protect access to the output file
    Dim MutexObject 
    Set MutexObject = CreateObject("UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "GenerateReprocessList"
    HandleScriptError0 "ELI21997", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI21998", "Unable to acquire named Mutex!"

    ' Check for existence of the output file
    If Not fso.FileExists(strOutputPath) Then
        HandleDebug "Testing Output File Existence", "False"
        ' Get folder
        Dim strOutputFolder
        strOutputFolder = strGetFolder(strOutputPath)
        HandleDebug "Output Folder", strOutputFolder

        ' Check for existence of hardcoded folder
        If Not strOutputFolder = "" Then
            If Not fso.FolderExists(strOutputFolder) Then
                HandleDebug "Testing Output Folder Existence", "False"
                ' Create the folder
                fso.CreateFolder(strOutputFolder)
                HandleScriptError1 "ELI21999", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI22000", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI22001", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI22002", "Unable to release named Mutex!"
End Function
