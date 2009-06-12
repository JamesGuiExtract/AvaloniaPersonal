'--------------------------------------------------------------------------------------------------
' Script commands specific for MI - Ingham County
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is an XML file
' where:
'     GStrIn  = H:\Images\20059019862200.tif.xml
'     GStrOut = H:\Images\20059019862200.tif.ocr
Dim GObjArgs, GStrIn
Set GObjArgs = WScript.Arguments
GStrIn = GObjArgs(0)
Call ParseCommandLineOptions(1)
HandleDebug "Input File From Command-Line", GStrIn

' Create output filename from input filename
' where:
'     GStrIn  = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.TIFXXXN
'     GStrOut = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.FTRXXXN
Dim GStrOut
GStrOut = strBuildOutputFilename(GStrIn)
HandleDebug "Output File Name", GStrOut

' COMMENTED OUT THE DELAY
' Add a 5-second delay to manage potential problems caused by delayed-write disk caching
' WScript.Sleep(5000)
' END COMMENT

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI17980", "Unable to create File System Object!"

' Create XML filename from input filename
' where:
'     GStrIn = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.TIFXXXN
'     strXML = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.TIFXXXN.xml
Dim strXMLPath
strXMLPath = GStrIn + ".xml"
HandleDebug "XML File Name", strXMLPath

' Create clear-text output file with FullText data from XML file
' Retry parsing up to three times with a 5-second delay between retries
Dim bContinue, bSuccess, nFailureCounter
bContinue = True
bSuccess = False
nFailureCount = 0
Do
    ' Attempt to parse the XML file
    bSuccess = ProcessXMLFile(strXMLPath, GStrOut)

    ' Check status of parsing
    If bSuccess = False Then
        ' Update the failure counter
        nFailureCounter = nFailureCounter + 1

        ' Log an exception
        LogScriptError2 "ELI20332", "ProcessXMLFile Failed", "XML File", strXMLPath, "Number of Failures", CStr(nFailureCounter)

        ' Quit retrying if parsing has failed three times
        If nFailureCounter >= 3 Then
            bContinue = False
        End If
    Else
        ' XML File parsed successfully
        bContinue = False
    End If

    ' Sleep for 5 seconds before trying again
    If bContinue = True Then
        WScript.Sleep(5000)
    End If
Loop Until bContinue = False


'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for CA - ContraCosta County
'--------------------------------------------------------------------------------------------------
' Create output filename from input filename
' where:
'     strInput  = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.TIFXXXN
'     strOutput = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.FTRXXXN
'--------------------------------------------------------------------------------------------------
Function strBuildOutputFilename(strInputName)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering BuildOutputFilename() - ", strInputName

    ' Find beginning of file extension ".TIF" from input filename
    ' where:
    '     strInputName  = H:\Images\CCCCOTBU00000119980101CCC ... 0000000087000016.TIFXXXN
    Dim PeriodChar, p, strFinal
    PeriodChar = "."
    p = InStrRev(strInputName,PeriodChar)
    If p > 0 Then
        ' Extract the characters to the left and to the right
        Dim n, strLeft, strRight
        n = Len(strInputName)
        strLeft = Left(strInputName, p-1)
        strRight = Right(strInputName, n-p)
        HandleDebug "BuildOutputFilename::strLeft", strLeft
        HandleDebug "BuildOutputFilename::strRight", strRight

        ' Find the characters that follow the F in TIF
        ' where:
        '    "TIFXXXN" ====> "XXXN"
        Dim FChar, q, r, strFarRight
        FChar = "F"
        q = InStr(strRight,FChar)
        If q > 0 Then
            r = Len(strRight)
            strFarRight = Right(strRight, r-q)
            HandleDebug "BuildOutputFilename::strFarRight", strFarRight
        End If
        
        ' Construct the final string
        ' where:
        '    strFinal = strLeft + ".FTR" + strFarRight
        strFinal = strLeft + ".FTR" + strFarRight
        HandleDebug "BuildOutputFilename::strFinal", strFinal
    End If
    
    ' Return result converted to string
    strBuildOutputFilename = strFinal
End Function

'--------------------------------------------------------------------------------------------------
' Create clear-text output file with FullText data from XML file
'--------------------------------------------------------------------------------------------------
Function ProcessXMLFile(strXMLPath,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessXMLFile()", strXMLPath + ", " + strOutputPath

    Dim bSuccess
    bSuccess = True
    
    ' Create the XML DOM Document object
    Dim xmlDoc
    Set xmlDoc = CreateObject("MSXML.DOMDocument")
    HandleScriptError0 "ELI17981", "Unable to create XML DOM Document Object!"

    ' Load the XML metadata file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI17982", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

	' Get the collection of DocLine nodes
	Dim xmlDocLineList
	Set xmlDocLineList = xmlDoc.selectNodes("/FlexData/DocLine")
    HandleScriptError1 "ELI17983", "Unable to retrieve DocLine collection!", "XML Path", strXMLPath
	
    ' Log an error and return if an error occurred
    If xmlDocLineList Is Nothing Then
        ' Log an exception
        LogScriptError1 "ELI20333", "ProcessXMLFile Failed - no DocLine list", "XML File", strXMLPath

        ' Parsing the XML file has failed, return False
        bSuccess = False
    End If
    
    ' Check node count, log an exception and return if count = 0
	Dim nCount
	nCount = xmlDocLineList.length
    HandleDebug "DocLine Node Count", CStr(nCount)
    If nCount = 0 Then
        ' Log an exception
        LogScriptError1 "ELI20334", "ProcessXMLFile Failed - DocLine Count = 0", "XML File", strXMLPath

        ' Parsing the XML file has failed, return False
        bSuccess = False
    End If
    
    ' Create the output file, clearing any existing content
    ' Do not create the file if a parsing error has occurred
    If bSuccess = True Then
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath, True, False)
        HandleScriptError1 "ELI17985", "Unable to create output file!", "Output File", strOutputPath
        NewFile.Close
    End If
    
	' Begin building the output string
	Dim strOutputText

	' Check each Node in the collection
	Dim mCount, I
    Dim xmlDocLineNode
	For Each xmlDocLineNode in xmlDocLineList
        ' Retrieve Line information
        Dim xmlFullTextList
        Set xmlFullTextList = xmlDocLineNode.selectNodes("FullText")

        ' Log an exception if an error occurred
        If xmlFullTextList Is Nothing Then
            ' Log an exception
            LogScriptError1 "ELI20336", "ProcessXMLFile Error - DocLine node without FullText nodes", "XML File", strXMLPath
        End If
        
        ' Check each FullText item - there should only be one
        ' If count = 0, log an exception and continue to the next DocLine node
    	mCount = xmlFullTextList.length
        HandleDebug "FullText Node Count", CStr(mCount)
        If mCount = 0 Then
            ' Log an exception
            LogScriptError2 "ELI20337", "ProcessXMLFile Error - DocLine without FullText", "XML File", strXMLPath, "Node Text", xmlDocLineNode.xml
        End If
    
        ' Retrieve the FullText item
        Dim xmlThisText
        For Each xmlThisText in xmlFullTextList
            ' Check if text is present
            Dim bNotFound
            bNotFound = xmlThisText Is Nothing
            
            ' Continue only if text was found
            If bNotFound = False Then
                ' Get the text
                Dim strValue
                strValue = xmlThisText.text
                HandleDebug "Node #" + CStr(I) + ": Output Value: " + strValue

                ' Write text to output file
                strOutputText = strValue
      	        AppendOutput strOutputText, strOutputPath
  	        End If
       	Next
   	Next

    ' Function completed successfully unless a specific error condition was noted
    ProcessXMLFile = bSuccess
End Function

'--------------------------------------------------------------------------------------------------
' Writes the specified text to the specified output file
'--------------------------------------------------------------------------------------------------
Function AppendOutput(strInfoLine,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering AppendOutput()", strInfoLine + ", " + strOutputPath

    ' Open the output file that was previously created and cleared
    Dim OutFile
    Set OutFile = fso.OpenTextFile(strOutputPath,8,False,0)
    HandleScriptError1 "ELI17987", "Unable to open output file!", "Output File", strOutputPath

    ' Write the text to the file
    OutFile.WriteLine(strInfoLine)

    ' Close the output file
    OutFile.Close
End Function

