'--------------------------------------------------------------------------------------------------
' Script commands specific for MI - Ingham County
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a multiple-page TIF
' where:
'     GStrImg = H:\Verification\Input\DOCC04092078-200515554.tif
'     GStrOut = H:\Verification\OutputFile.txt
Dim GObjArgs, GStrImg, GStrOut
Set GObjArgs = WScript.Arguments
GStrImg = GObjArgs(0)
GStrOut = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input Image From Command-Line", GStrImg
HandleDebug "Output File From Command-Line", GStrOut

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI15773", "Unable to create File System Object!"

' Create Page Count object and get page count for this file
Dim PageCountObject, GNPageCount
Set PageCountObject = CreateObject("VBScriptUtils.ImageData")
HandleScriptError0 "ELI15774", "Unable to create Page Count Object!"
GNPageCount = PageCountObject.GetImagePageCount(GStrImg)
HandleScriptError1 "ELI15775", "Unable to GetImagePageCount()!", "Image Name", GStrImg
HandleDebug "Page Count For Input Image", CStr(GNPageCount)

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Verification\Input\DOCC04092078-200515554.tif ) = 
'       "H:\Verification\Input\DOCC04092078-200515554.tif.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

' Append redaction information from parsed XML file to the specified output file
' as:
'     DOCC04092078|none
' if no redactions were found in the image with this internal ID number
' OR:
'     DOCC04092078|6|10|476|1731|188|26
'     DOCC04092078|7|10|122|1122|140|55
' if two redactions were found in the image with this internal ID number
' where:
'     DOCC04092078|6|10|476|1731|188|26
' is interpreted as:
'     DOCC04092078 - internal ID number for this document
'     6 - page number where this redaction is located
'     10 - total number of pages in the document
'     476 - the X coordinate of the redaction in pixels
'     1731 - the Y coordinate of the redaction in pixels
'     188 - the width of the redaction in pixels
'     26 - the height of the redaction in pixels
ProcessXMLFile GStrXMLPath, GStrOut

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for MI - Ingham County
'--------------------------------------------------------------------------------------------------
' Get XML filename from specified image file path
' where:
'     XMLPath( H:\Verification\Input\DOCC04092078-200515554.tif ) = 
'       "H:\Verification\Input\DOCC04092078-200515554.tif.xml"
'--------------------------------------------------------------------------------------------------
Function strGetXMLPath(strImagePath)
    ' Append the XML file extension
    strGetXMLPath = strImagePath + ".xml"
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Internal ID Number from filename of image or XML file
' where:
'     IDNumber( H:\Verification\Input\200515554-DOCC04092078.tif.xml ) = "DOCC04092078"
'     IDNumber( H:\Verification\Input\BF-69502-DOCC69502.A0.tif ) = "DOCC69502"
'--------------------------------------------------------------------------------------------------
Function strGetIDNumber(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract filename portion of path by finding last backslash
    ' where:
    '    "H:\Verification\Input\200515554-DOCC04092078.tif.xml" ====> "200515554-DOCC04092078.tif.xml"
    '    "H:\Verification\Input\BF-69502-DOCC69502.A0.tif" ====> "BF-69502-DOCC69502.A0.tif"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        ' Extract the filename
        Dim n, strFileName
        n = Len(strPath)
        strFileName = Right(strPath, n-p)
        HandleDebug "GetIDNumber::strFileName", strFileName

        ' Extract ID portion of filename by finding the last dash and the first period
        ' where:
        '    "200515554-DOCC04092078.tif.xml" ====> "DOCC04092078"
        '    "BF-69502-DOCC69502.A0.tif" ====> "DOCC69502"
        Dim DashChar, PeriodChar, q
        DashChar = "-"
        PeriodChar = "."
        p = InStrRev(strFileName,DashChar)
        q = InStr(strFileName,PeriodChar)
        If p > 0 Then
            strGetIDNumber = Mid(strFileName, p+1, q-p-1)
        End If
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Folder from path
' where:
'     Folder( H:\Verification\Input\DOCC04092078-200515554.tif.xml ) = "H:\Verification\Input"
'--------------------------------------------------------------------------------------------------
Function strGetFolder(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract folder portion of filename by finding last backslash
    ' where:
    '    "H:\Verification\Input\DOCC04092078-200515554.tif.xml" ====> "H:\Verification\Input"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        strGetFolder = Left(strPath, p-1)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Reverse the start/end coordinates if the start X coordinate is right of the end X coordinate
'--------------------------------------------------------------------------------------------------
Sub NormalizeCoordinates(strX1,strY1,strX2,strY2)
    ' Define error handler
    On Error Resume Next
    
    HandleDebug "Entering NormalizeCoordinates()", strX1 + "," + strY1 + " ; " + strX2 + "," + strY2

    ' Convert input parameters to integers
    Dim intX1, intX2, strTemp
    intX1 = CInt(strX1)
    intX2 = CInt(strX2)

    ' Swap Start/End Coordinates if needed
    If intX1 > intX2 Then
        strX1 = CStr(intX2)
        strX2 = CStr(intX1)
        strTemp = strY1
        strY1 = strY2
        strY2 = strTemp
    End If

End Sub
'--------------------------------------------------------------------------------------------------
' Computes width or distance between two points
' where:
'     Width( X1,Y1,X2,Y2 ) = SQRT( (X2-X1)**2 + (Y2-Y1)**2 )
' Note that all input and output parameters are strings
'--------------------------------------------------------------------------------------------------
Function strGetZoneWidth(strX1,strY1,strX2,strY2)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering strGetZoneWidth()", strX1 + "," + strY1 + " ; " + strX2 + "," + strY2

    ' Convert input parameters to integers
    Dim nX1, nY1, nX2, nY2
    nX1 = CInt(strX1)
    nY1 = CInt(strY1)
    nX2 = CInt(strX2)
    nY2 = CInt(strY2)
    
    ' Perform calculation and round to nearest integer
    Dim dDistance, nDistance
    dDistance = Sqr( (nX2-nX1)*(nX2-nX1) + (nY2-nY1)*(nY2-nY1) )
    nDistance = Round(dDistance)
    
    ' Return result converted to string
    strGetZoneWidth = CStr(nDistance)
End Function

'--------------------------------------------------------------------------------------------------
' Append redaction information from parsed XML file to the specified output file
' as:
'     DOCC04092078|none
' if no redactions were found in the image with this internal ID number
' OR:
'     DOCC04092078|6|10|476|1731|188|26
'     DOCC04092078|7|10|122|1122|140|55
' if two redactions were found in the image with this internal ID number
' where:
'     DOCC04092078|6|10|476|1731|188|26
' is interpreted as:
'     DOCC04092078 - internal ID number for this document
'     6 - page number where this redaction is located
'     10 - total number of pages in the document
'     476 - the X coordinate of the redaction in pixels
'     1731 - the Y coordinate of the redaction in pixels
'     188 - the width of the redaction in pixels
'     26 - the height of the redaction in pixels
'--------------------------------------------------------------------------------------------------
Function ProcessXMLFile(strXMLPath,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessXMLFile()", strXMLPath + ", " + strOutputPath

    ' Get internal ID number from XML file path
    Dim strIDNumber
    strIDNumber = strGetIDNumber(strXMLPath)
    HandleDebug "IDNumber", strIDNumber

    ' Create the XML DOM Document object
    Dim xmlDoc
    Set xmlDoc = CreateObject("MSXML.DOMDocument")
    HandleScriptError0 "ELI15756", "Unable to create XML DOM Document Object!"

    ' Load the XML metadata file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI15757", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

	' Get the collection of Redaction nodes
	Dim xmlList
	Set xmlList = xmlDoc.selectNodes("/IDShieldMetaData/Redactions/Redaction")
    HandleScriptError1 "ELI15758", "Unable to select Redaction nodes!", "XML Path", strXMLPath
	
	' Begin building the output string
	Dim PipeChar, strOutputText
	PipeChar = "|"
	strOutputText = strIDNumber
	
	' Clear flag used to determine whether default output string will be used
	Dim boolFoundRedaction
	boolFoundRedaction = False
	
	' Check each Node in the collection
	Dim nCount, I, J, K
	nCount = xmlList.length
    HandleDebug "Redactions Node Count", CStr(nCount)
	For I = 1 To nCount
	    ' Retrieve this Node
	    Dim xmlThisNode
	    Set xmlThisNode = xmlList.item(I-1)
        HandleScriptError1 "ELI15759", "Unable to retrieve Redaction Node #" + CStr(I) + " of " + CStr(nCount) + "!", "XML Path", strXMLPath

	    ' Retrieve the Output flag
	    Dim xmlSingleNode
	    Set xmlSingleNode = xmlThisNode.selectSingleNode("@Output")
        HandleScriptError1 "ELI15760", "Unable to select Output attribute!", "XML Path", strXMLPath

	    ' Get the text for the Output flag
	    Dim strValue
	    strValue = xmlSingleNode.text
        HandleDebug "Node#, Output Value", CStr(I) + ", " + strValue

	    ' Check text
	    If strValue = "1" Then
	        ' This redaction item is ON, set flag
	        boolFoundRedaction = True
	        
	        ' Retrieve Line information
	        Dim xmlLineList
	        Set xmlLineList = xmlThisNode.selectNodes("Line")
            HandleScriptError1 "ELI15776", "Unable to select Line nodes!", "XML Path", strXMLPath

	        ' Check each Node in the collection
	        Dim nLineCount
	        nLineCount = xmlLineList.length
            HandleDebug "Line Count", CStr(nLineCount)
	        For J = 1 To nLineCount
	            ' Retrieve this Node
	            Dim xmlThisLine
	            Set xmlThisLine = xmlLineList.item(J-1)
                HandleScriptError1 "ELI15777", "Unable to retrieve Line #" + CStr(J) + " of " + CStr(nLineCount) + "!", "XML Path", strXMLPath

	            ' Retrieve Zone information
	            Dim xmlZoneList
	            Set xmlZoneList = xmlThisLine.selectNodes("Zone")
                HandleScriptError1 "ELI15778", "Unable to select Zone nodes!", "XML Path", strXMLPath

	            ' Check each Node in the collection
	            Dim nZoneCount
	            nZoneCount = xmlZoneList.length
                HandleDebug "Zone Count", CStr(nZoneCount)
	            For K = 1 To nZoneCount
	                ' Retrieve the Zone information
	                Dim xmlThisZone
	                Set xmlThisZone = xmlZoneList.item(K-1)
                    HandleScriptError1 "ELI15779", "Unable to retrieve Zone #" + CStr(K) + " of " + CStr(nZoneCount) + "!", "XML Path", strXMLPath

                    ' Retrieve the StartX item
                    Dim strStartX, strStartY, strEndX, strEndY, strWidth, strHeight, strPage
                    Dim xmlSingleZoneNode
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@StartX")
                    HandleScriptError1 "ELI15780", "Unable to select StartX attribute!", "XML Path", strXMLPath
                    strStartX = xmlSingleZoneNode.text

                    ' Retrieve the StartY item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@StartY")
                    HandleScriptError1 "ELI15781", "Unable to select StartY attribute!", "XML Path", strXMLPath
                    strStartY = xmlSingleZoneNode.text

                    ' Retrieve the EndX item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@EndX")
                    HandleScriptError1 "ELI15782", "Unable to select EndX attribute!", "XML Path", strXMLPath
                    strEndX = xmlSingleZoneNode.text

                    ' Retrieve the EndY item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@EndY")
                    HandleScriptError1 "ELI15783", "Unable to select EndY attribute!", "XML Path", strXMLPath
                    strEndY = xmlSingleZoneNode.text
                    
                    ' Normalize Coordinate values
                    Call NormalizeCoordinates(strStartX, strStartY, strEndX, strEndY)

                    ' Compute the Width
                    strWidth = strGetZoneWidth(strStartX, strStartY, strEndX, strEndY)

                    ' Retrieve the Height item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@Height")
                    HandleScriptError1 "ELI15784", "Unable to select Height attribute!", "XML Path", strXMLPath
                    strHeight = xmlSingleZoneNode.text

                    ' Retrieve the Page Number item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@PageNumber")
                    HandleScriptError1 "ELI15785", "Unable to select PageNumber attribute!", "XML Path", strXMLPath
                    strPage = xmlSingleZoneNode.text

                    ' Provide various Debug items
                    HandleDebug "Line#: Page Number, Page Count", CStr(J) + ": " + strPage + "," + CStr(GNPageCount)
                    HandleDebug "Line#: Zone Start X,Y", CStr(J) + ": " + strStartX + "," + strStartY
                    HandleDebug "Line#: Zone Width,Height", CStr(J) + ": " + strWidth + "," + strHeight

                    ' Add Page Number and Page Count
                    strOutputText = strOutputText + PipeChar + strPage + PipeChar + CStr(GNPageCount)

                    ' Compute Top
                    Dim nTop, nHalfHeight
                    Dim strTop
                    ' Round up so that region is not too small
                    nHalfHeight = CInt(strHeight) / 2 + 0.5
                    nTop = CInt(strStartY) - CInt(nHalfHeight)
                    strTop = CStr(nTop)

                    ' Add X and Top
                    strOutputText = strOutputText + PipeChar + strStartX + PipeChar + strTop

                    ' Add Width and Height
                    strOutputText = strOutputText + PipeChar + strWidth + PipeChar + strHeight

                    ' Append the final output string for this redaction to output file
                	AppendRedactionInfo strOutputText, GStrOut

                    ' Reset the output string to prepare for a next redaction
                    strOutputText = strIDNumber
            	Next
        	Next
	    End If
	Next

	' Append "none" if no redactions found
	If boolFoundRedaction = False Then
	    strOutputText = strOutputText + PipeChar + "none"
        HandleDebug "Result text for output file", strOutputText
	
    	' Append this string to the output file
    	AppendRedactionInfo strOutputText, GStrOut
	End If

End Function

'--------------------------------------------------------------------------------------------------
' Appends the specified redaction information to the specified output file
'--------------------------------------------------------------------------------------------------
Function AppendRedactionInfo(strInfoLine,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering AppendRedactionInfo()", strInfoLine + ", " + strOutputPath

    ' Create named Mutex Object to protect access to the output file
    Dim MutexObject 
    Set MutexObject = CreateObject("UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Export"
    HandleScriptError0 "ELI15761", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI15762", "Unable to acquire named Mutex!"

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
                HandleScriptError1 "ELI15763", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI15764", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI15765", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI15766", "Unable to release named Mutex!"
End Function

