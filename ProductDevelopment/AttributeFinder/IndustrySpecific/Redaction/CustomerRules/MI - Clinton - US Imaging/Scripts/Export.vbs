'--------------------------------------------------------------------------------------------------
' Script commands specific for MI - Clinton - US Imaging
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a multiple or single-page TIF
' where:
'     GStrImg = H:\Images\3036412-0001.tif
'     GStrOut = H:\Redaction\OutputFile.txt
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
HandleScriptError0 "ELI23285", "Unable to create File System Object!"

' Create Page Count object and get page count for this file
Dim GNPageCount, GNPageNumber
GNPageNumber = getPageNumber(GStrImg)
HandleScriptError1 "ELI23287", "Unable to getPageNumber()!", "Image Name", GStrImg
HandleDebug "Page Number Of Input Image", CStr(GNPageNumber)

GNPageCount = getImagePageCount(GStrImg)
HandleScriptError1 "ELI24873", "Unable to getImagePageCount()!", "Image Name", GStrImg
HandleDebug "Page Count For Input Image", CStr(GNPageCount)

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Images\3036412-0001.tif ) = "H:\Images\3036412-0001.tif.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

' Append redaction information from parsed XML file to the specified output file
' DEFINE 'redaction': a rectangular bounding box corresponding to SpatialLineBounds nodes in the xml
' as:
'     3036412|none
' if no redactions were found in the image with this internal ID number
' OR:
'     3036412|6|10|476|1731|188|26
'     3036412|7|10|122|1122|140|55
' if two redactions were found in the image with this internal ID number
' where:
'     3036412|6|10|476|1731|188|26
' is interpreted as:
'     3036412 - internal ID number for this document
'     6 - page number where this redaction is located
'     10 - total number of pages in the document
'     476 - the X coordinate of the top left corner of the redaction in pixels
'     1731 - the Y coordinate of the top left corner of the redaction in pixels
'     188 - the width of the redaction in pixels
'     26 - the height of the redaction in pixels
ProcessXMLFile GStrXMLPath, GStrOut

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for MI - Clinton - US Imaging
'--------------------------------------------------------------------------------------------------
' Get XML filename from specified image file path
' where:
'     XMLPath( H:\Images\3036412-0001.tif ) = "H:\Images\3036412-0001.tif.xml"
'--------------------------------------------------------------------------------------------------
Function strGetXMLPath(strImagePath)
    ' Append the XML file extension
    strGetXMLPath = strImagePath + ".xml"
End Function

'-------------------------------------------------------------------------------------------------
' Retrieve number of image files in the image directory that share an 'internal' ID
' where:
' IDNumber( H:\Images\3036412-0001.tif.xml ) = "3036412"
'-------------------------------------------------------------------------------------------------
Function getImagePageCount(strImagePath)
  ' Define error handler
    On Error Resume Next

    Dim next_page, page_count, LHS, RHS, strPage
    strPage = CStr(GNPageNumber+1)
    page_count = GNPageNumber
    LHS = Left(strImagePath, InStrRev(strImagePath,"-") )
    RHS = Mid (strImagePath, InStrRev(strImagePath,"-") + 5)
    next_page = LHS & string(4 - Len(strPage),"0") & strPage & RHS

    Do While fso.FileExists(next_page)
        page_count = page_count + 1
        strPage = CStr(page_count + 1)
        next_page = LHS & string(4 - Len(strPage),"0") & strPage & RHS
    Loop
    
  getImagePageCount = page_count
End Function

'-------------------------------------------------------------------------------------------------
' Retrieve the page number from an image file name
' where:
' PageNumber( H:\Images\3036412-0001.tif.xml ) = "1"
'-------------------------------------------------------------------------------------------------
Function getPageNumber(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract page number plus extension portion of path by finding last dash
    Dim p
    p = InStrRev(strPath,"-")
    If p > 0 Then
        ' Extract the page number and extension
        Dim n, strFileName
        n = Len(strPath)
        strFileName = Right(strPath, n-p)

        ' Extract page number portion of filename by finding the first period
        ' where:
        '    "3036412-0001.tif.xml" ====> "3036412"
        p = InStr(strFileName,".")
        If p > 0 Then
            getPageNumber = CInt(Mid(strFileName, 1, p-1))
        End If
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Internal ID Number from filename of image or XML file
' where:
'     IDNumber( H:\Images\3036412-0001.tif.xml ) = "3036412"
'--------------------------------------------------------------------------------------------------
Function strGetIDNumber(strPath)
    ' Define error handler
    On Error Resume Next

    ' Extract filename portion of path by finding last backslash
    ' where:
    '    "H:\Images\3036412-0001.tif.xml" ====> "3036412-0001.tif.xml"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        ' Extract the filename
        Dim n, strFileName
        n = Len(strPath)
        strFileName = Right(strPath, n-p)
        HandleDebug "GetIDNumber::strFileName", strFileName

        ' Extract ID portion of filename by finding the first dash
        ' where:
        '    "3036412-0001.tif.xml" ====> "3036412"
        p = InStr(strFileName,"-")
        If p > 0 Then
            strGetIDNumber = Mid(strFileName, 1, p-1)
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
' Computes width as difference between left and right sides of a rectangle
' where:
'     Width( left, right ) = Abs( left - right )
' Note that all input and output parameters are strings
'--------------------------------------------------------------------------------------------------
Function strGetZoneWidth(strLeft, strRight)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering strGetZoneWidth()", strLeft + "," + strRight

    ' Convert input parameters to integers
    Dim nX1, nX2
    nX1 = CInt(strLeft)
    nX2 = CInt(strRight)
    
    ' Perform calculation and round to nearest integer
    Dim nWidth
    nWidth = Abs(nX1-nX2)
    
    ' Return result converted to string
    strGetZoneWidth = CStr(nWidth)
End Function

'--------------------------------------------------------------------------------------------------
' Computes height as difference between top and bottom of a rectangle
' where:
'     Width( top, bottom ) = Abs( top - bottom )
' Note that all input and output parameters are strings
'--------------------------------------------------------------------------------------------------
Function strGetZoneHeight(strTop, strBottom)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering strGetZoneHeight()", strTop + "," + strBottom

    ' Convert input parameters to integers
    Dim nY1, nY2
    nY1 = CInt(strTop)
    nY2 = CInt(strBottom)
    
    ' Perform calculation and round to nearest integer
    Dim nHeight
    nHeight = Abs(nY1-nY2)
    
    ' Return result converted to string
    strGetZoneHeight = CStr(nHeight)
End Function

'--------------------------------------------------------------------------------------------------
' Append redaction information from parsed XML file to the specified output file
' as:
'     3036412|none
' if no redactions were found in the image with this internal ID number
' OR:
'     3036412|6|10|476|1731|188|26
'     3036412|7|10|122|1122|140|55
' if two redactions were found in the image with this internal ID number
' where:
'     3036412|6|10|476|1731|188|26
' is interpreted as:
'     3036412 - internal ID number for this document
'     6 - page number where this redaction is located
'     10 - total number of pages in the document
'     476 - the X coordinate of the top left corner of the redaction in pixels
'     1731 - the Y coordinate of the top left corner of the redaction in pixels
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
    HandleScriptError0 "ELI23288", "Unable to create XML DOM Document Object!"

    ' Load the XML metadata file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI23289", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

	' Get the collection of SpatialLine nodes
	Dim xmlList
	Set xmlList = xmlDoc.selectNodes("/FlexData//(HCData|MCData|LCData)//SpatialLine")
    HandleScriptError1 "ELI23290", "Unable to select SpatialLine nodes!", "XML Path", strXMLPath
	
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
    HandleDebug "SpatialLine Node Count", CStr(nCount)

        If nCount > 0 Then
          boolFoundRedaction = True
        End If

	For I = 1 To nCount
	    ' Retrieve this Node
	    Dim xmlThisNode
	    Set xmlThisNode = xmlList.item(I-1)
        HandleScriptError1 "ELI23291", "Unable to retrieve SpatialLine Node #" + CStr(I) + " of " + CStr(nCount) + "!", "XML Path", strXMLPath


            Dim xmlSingleZoneNode, strTop, strLeft, strBottom, strRight, strWidth, strHeight, strPage

            ' Retrieve the Page Number item
            strPage = CStr(GNPageNumber)

            ' Set xmlThisNode to be the SpatialLineBounds node
            Set xmlThisNode = xmlThisNode.selectSingleNode("SpatialLineBounds")

            ' Retrieve the Top item
            Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@Top")
            HandleScriptError1 "ELI23293", "Unable to select Top attribute!", "XML Path", strXMLPath
            strTop = xmlSingleZoneNode.text

            ' Retrieve the Left item
            Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@Left")
            HandleScriptError1 "ELI23294", "Unable to select Left attribute!", "XML Path", strXMLPath
            strLeft = xmlSingleZoneNode.text

            ' Retrieve the Right item
            Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@Right")
            HandleScriptError1 "ELI23295", "Unable to select Right attribute!", "XML Path", strXMLPath
            strRight = xmlSingleZoneNode.text

            ' Retrieve the Bottom item
            Set xmlSingleZoneNode = xmlThisNode.selectSingleNode("@Bottom")
            HandleScriptError1 "ELI23296", "Unable to select Bottom attribute!", "XML Path", strXMLPath
            strBottom = xmlSingleZoneNode.text
            
            ' Compute the Width
            strWidth = strGetZoneWidth(strLeft, strRight)

            ' Compute the Height
            strHeight = strGetZoneHeight(strTop, strBottom)

            ' Provide various Debug items
            HandleDebug "Line#: Page Number, Page Count", CStr(I) + ": " + strPage + "," + CStr(GNPageCount)
            HandleDebug "Line#: Zone Start X,Y", CStr(I) + ": " + strLeft + "," + strTop
            HandleDebug "Line#: Zone Width,Height", CStr(I) + ": " + strWidth + "," + strHeight

            ' Add Page Number and Page Count
            strOutputText = strOutputText + PipeChar + strPage + PipeChar + CStr(GNPageCount)

            ' Add X and Y
            strOutputText = strOutputText + PipeChar + strLeft + PipeChar + strTop

            ' Add Width and Height
            strOutputText = strOutputText + PipeChar + strWidth + PipeChar + strHeight

            ' Append the final output string for this redaction to output file
            AppendRedactionInfo strOutputText, GStrOut

            ' Reset the output string to prepare for a next redaction
            strOutputText = strIDNumber
	Next

	' Append "none" if no redactions found, this is the last page of the image, and no previous
        ' entries have been made.

	If boolFoundRedaction = False and GNPageNumber = GNPageCount Then
            If HasNoPreviousEntry(strIDNumber, GStrOut) Then
                strOutputText = strOutputText + PipeChar + "none"
                
                HandleDebug "Result text for output file", strOutputText
	
        	' Append this string to the output file
        	AppendRedactionInfo strOutputText, GStrOut
            End If
	End If

End Function

'

'
Function HasNoPreviousEntry(strIDNumber, strOutputPath)
    ' Define error handler
    On Error Resume Next
    HandleDebug "Entering HasNoPreviousEntry()", strIDNumber + ", " + strOutputPath
    Dim hasPreviousEntry
    hasPreviousEntry = False

    ' Check for existence of the output file
    If Not fso.FileExists(strOutputPath) Then
        HandleDebug "Testing Output File Existence", "False"
        hasPreviousEntry = False
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for read
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForReading,True)
        HandleScriptError1 "ELI23444", "Unable to open output file for reading!", "Output File", strOutputPath

        ' Read each line of the file and look for strIDNumber
        Dim strLine
        Do Until (OutFile.AtEndOfStream or hasPreviousEntry)
            strLine=OutFile.readline
            If InStr(strLine, strIDNumber) Then
                hasPreviousEntry = True
            End If
        Loop
        OutFile.Close
    End If

    HasNoPreviousEntry = Not hasPreviousEntry
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
    HandleScriptError0 "ELI23297", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI23298", "Unable to acquire named Mutex!"

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
                HandleScriptError1 "ELI23299", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI23300", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI23301", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI23302", "Unable to release named Mutex!"
End Function
