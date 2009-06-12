'--------------------------------------------------------------------------------------------------
' Script commands specific for LA - East Baton Rouge
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a single-page TIF
' where:
'     GStrImg = H:\Images\December_00012345_0001.tif
'     GStrOut = H:\Images\OutputFile.txt
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
HandleScriptError0 "ELI16214", "Unable to create File System Object!"

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Images\December_00012345_0001.tif ) = 
'       "H:\Images\December_00012345_0001.tif.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

' Append redaction information for input image from parsed XML file to the specified output file
' as:
'     December_00012345_0001.tif|100|10|20|78
'     December_00012345_0001.tif|300|120|22|111
' if two redactions were found in the image
' where:
'     December_00012345_0001.tif|300|120|22|111
' is interpreted as:
'     December_00012345_0001.tif - filename of original image before redaction
'     300 - the top edge of the redaction rectangle in pixels
'     120 - the left edge of the redaction rectangle in pixels
'     22  - the height of the redaction rectangle in pixels
'     111 - the width of the redaction rectangle in pixels
ProcessXMLFile GStrImg, GStrXMLPath, GStrOut

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for LA - East Baton Rouge
'--------------------------------------------------------------------------------------------------
' Get XML filename from specified image file path
' where:
'     XMLPath( H:\Images\December_00012345_0001.tif ) = 
'       "H:\Images\December_00012345_0001.tif.xml"
'--------------------------------------------------------------------------------------------------
Function strGetXMLPath(strImagePath)
    ' Append the XML file extension
    strGetXMLPath = strImagePath + ".xml"
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Folder from path
' where:
'     Folder( H:\Images\December_00012345_0001.tif ) = "H:\Images"
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
' Return smallest of specified double-precision inputs
' where:
'     GetMin( 1.0, 2.0, 3.0, 4.0 ) = 1.0
'--------------------------------------------------------------------------------------------------
Function dGetMin(d1,d2,d3,d4)
    ' Start with d1
    Dim dTemp
    dTemp = d1
    
    ' Check d2
    If d2 < dTemp Then
       dTemp = d2
    End If

    ' Check d3
    If d3 < dTemp Then
       dTemp = d3
    End If

    ' Check d4
    If d4 < dTemp Then
       dTemp = d4
    End If
    
    ' Return smallest value
    dGetMin = dTemp
End Function

'--------------------------------------------------------------------------------------------------
' Return largest of specified double-precision inputs
' where:
'     GetMax( 1.0, 2.0, 3.0, 4.0 ) = 4.0
'--------------------------------------------------------------------------------------------------
Function dGetMax(d1,d2,d3,d4)
    ' Start with d1
    Dim dTemp
    dTemp = d1
    
    ' Check d2
    If d2 > dTemp Then
       dTemp = d2
    End If

    ' Check d3
    If d3 > dTemp Then
       dTemp = d3
    End If

    ' Check d4
    If d4 > dTemp Then
       dTemp = d4
    End If
    
    ' Return largest value
    dGetMax = dTemp
End Function

'--------------------------------------------------------------------------------------------------
' Computes Top, Left, Height, Width of the bounding rectangle defined by the specified zone
' where:
'     Theta = atan2( y2 - y1, x2 - x1 )
' 
'     P1x = x1 - h/2 * sin( theta )
'     P1y = y1 + h/2 * cos( theta )
' 
'     P2x = x2 - h/2 * sin( theta )
'     P2y = y2 + h/2 * cos( theta )
' 
'     P3x = x2 + h/2 * sin( theta )
'     P3y = y2 - h/2 * cos( theta )
' 
'     P4x = x1 + h/2 * sin( theta )
'     P4y = y1 - h/2 * cos( theta )
' 
' and:
'     Top    = min( P1y, P2y, P3y, P4y )
'     Left   = min( P1x, P2x, P3x, P4x )
'     Height = max( P1y, P2y, P3y, P4y ) - Top
'     Width  = max( P1x, P2x, P3x, P4x ) - Left
' Notes: 
'     (x1, y1) is the start point of the zone
'     (x2, y2) is the end point of the zone
'     Theta is the orientation angle of the zone
'     h is the height of the zone
'     (P1, P2, P3, P4) are the points that define the bounding rectangle
'     The 4 output values are rounded to the nearest integer 
'     The returned string uses strDelimiter as the delimiter
'--------------------------------------------------------------------------------------------------
Function strGetZoneBounds(strX1,strY1,strX2,strY2,strH,strDelimiter)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering strGetZoneBounds()", strX1 + "," + strY1 + " ; " + strX2 + "," + strY2+ " ; " + strH

    ' Convert input parameters to integers
    Dim nX1, nY1, nX2, nY2, nH
    nX1 = CInt(strX1)
    nY1 = CInt(strY1)
    nX2 = CInt(strX2)
    nY2 = CInt(strY2)
    nH  = CInt(strH)
    
    ' Compute angle and sin(), cos() of angle
    Dim dDeltaY, dDeltaX, dTheta, dCosTheta, dSinTheta
    dDeltaY = nY2 - nY1
    dDeltaX = nX2 - nX1
    dTheta = Atn( dDeltaY / dDeltaX )
    dCosTheta = Cos( dTheta )
    dSinTheta = Sin( dTheta )
    
    ' Calculate the four points of the bounding rectangle
    Dim P1x, P1y, P2x, P2y, P3x, P3y, P4x, P4y
    P1x = nX1 - nH / 2 * dSinTheta
    P1y = nY1 + nH / 2 * dCosTheta

    P2x = nX2 - nH / 2 * dSinTheta
    P2y = nY2 + nH / 2 * dCosTheta

    P3x = nX2 + nH / 2 * dSinTheta
    P3y = nY2 - nH / 2 * dCosTheta

    P4x = nX1 + nH / 2 * dSinTheta
    P4y = nY1 - nH / 2 * dCosTheta

    ' Compute Top and Left and round to nearest integer
    Dim dTop, dLeft, nTop, nLeft
    dTop    = dGetMin( P1y, P2y, P3y, P4y )
    nTop    = Round( dTop )
    dLeft   = dGetMin( P1x, P2x, P3x, P4x )
    nLeft   = Round( dLeft )

    ' Compute Height, Width and round to nearest integer
    Dim nHeight, nWidth
    nHeight = Round( dGetMax( P1y, P2y, P3y, P4y ) - dTop )
    nWidth  = Round( dGetMax( P1x, P2x, P3x, P4x ) - dLeft )
    
    ' Build and return the output string
    strGetZoneBounds = CStr(nTop) + strDelimiter + CStr(nLeft) + strDelimiter + CStr(nHeight) + strDelimiter + CStr(nWidth)
End Function

'--------------------------------------------------------------------------------------------------
' Append redaction information from the specified single-page image and associated XML file to 
' the specified output file
'     H:\Images\December_00012345_0001.tif|100|10|20|78
'     H:\Images\December_00012345_0001.tif|300|120|22|111
' if two redactions were found in the image
' where:
'     H:\Images\December_00012345_0001.tif|300|120|22|111
' is interpreted as:
'     H:\Images\December_00012345_0001.tif - filename of original image before redaction
'     300 - the top edge of the redaction rectangle in pixels
'     120 - the left edge of the redaction rectangle in pixels
'     22  - the height of the redaction rectangle in pixels
'     111 - the width of the redaction rectangle in pixels
'--------------------------------------------------------------------------------------------------
Function ProcessXMLFile(strImage,strXMLPath,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessXMLFile() ", strImage + ", " + strXMLPath + ", " + strOutputPath

    ' Create the XML DOM Document object
    Dim xmlDoc
    Set xmlDoc = CreateObject("MSXML.DOMDocument")
    HandleScriptError0 "ELI16217", "Unable to create XML DOM Document Object!"

    ' Load the XML metadata file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI16218", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

	' Get the collection of Redaction nodes
	Dim xmlList
	Set xmlList = xmlDoc.selectNodes("/IDShieldMetaData/Redactions/Redaction")
    HandleScriptError1 "ELI16219", "Unable to select Redaction nodes!", "XML Path", strXMLPath
	
	' Begin building the output string
	Dim DelimiterChar, strOutputText
	DelimiterChar = ","
	strOutputText = strImage
	
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
        HandleScriptError1 "ELI16220", "Unable to retrieve Redaction Node #" + CStr(I) + " of " + CStr(nCount) + "!", "XML Path", strXMLPath

	    ' Retrieve the Output flag
	    Dim xmlSingleNode
	    Set xmlSingleNode = xmlThisNode.selectSingleNode("@Output")
        HandleScriptError1 "ELI16221", "Unable to select Output attribute!", "XML Path", strXMLPath

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
            HandleScriptError1 "ELI16222", "Unable to select Line nodes!", "XML Path", strXMLPath

	        ' Check each Node in the collection
	        Dim nLineCount
	        nLineCount = xmlLineList.length
            HandleDebug "Line Count", CStr(nLineCount)
	        For J = 1 To nLineCount
	            ' Retrieve this Node
	            Dim xmlThisLine
	            Set xmlThisLine = xmlLineList.item(J-1)
                HandleScriptError1 "ELI16223", "Unable to retrieve Line #" + CStr(J) + " of " + CStr(nLineCount) + "!", "XML Path", strXMLPath

	            ' Retrieve Zone information
	            Dim xmlZoneList
	            Set xmlZoneList = xmlThisLine.selectNodes("Zone")
                HandleScriptError1 "ELI16224", "Unable to select Zone nodes!", "XML Path", strXMLPath

	            ' Check each Node in the collection
	            Dim nZoneCount
	            nZoneCount = xmlZoneList.length
                HandleDebug "Zone Count", CStr(nZoneCount)
	            For K = 1 To nZoneCount
	                ' Retrieve the Zone information
	                Dim xmlThisZone
	                Set xmlThisZone = xmlZoneList.item(K-1)
                    HandleScriptError1 "ELI16225", "Unable to retrieve Zone #" + CStr(K) + " of " + CStr(nZoneCount) + "!", "XML Path", strXMLPath

                    ' Retrieve the StartX item
                    Dim strStartX, strStartY, strEndX, strEndY, strHeight, strBounds
                    Dim xmlSingleZoneNode
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@StartX")
                    HandleScriptError1 "ELI16226", "Unable to select StartX attribute!", "XML Path", strXMLPath
                    strStartX = xmlSingleZoneNode.text

                    ' Retrieve the StartY item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@StartY")
                    HandleScriptError1 "ELI16227", "Unable to select StartY attribute!", "XML Path", strXMLPath
                    strStartY = xmlSingleZoneNode.text

                    ' Retrieve the EndX item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@EndX")
                    HandleScriptError1 "ELI16228", "Unable to select EndX attribute!", "XML Path", strXMLPath
                    strEndX = xmlSingleZoneNode.text

                    ' Retrieve the EndY item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@EndY")
                    HandleScriptError1 "ELI16229", "Unable to select EndY attribute!", "XML Path", strXMLPath
                    strEndY = xmlSingleZoneNode.text

                    ' Retrieve the Height item
                    Set xmlSingleZoneNode = xmlThisZone.selectSingleNode("@Height")
                    HandleScriptError1 "ELI16230", "Unable to select Height attribute!", "XML Path", strXMLPath
                    strHeight = xmlSingleZoneNode.text

                    ' Compute the Zone bounds
                    strBounds = strGetZoneBounds(strStartX, strStartY, strEndX, strEndY, strHeight, DelimiterChar)

                    ' Provide various Debug items
                    HandleDebug "Line#: Zone Start X,Y", CStr(J) + ": " + strStartX + "," + strStartY
                    HandleDebug "Line#: Zone End X,Y", CStr(J) + ": " + strEndX + "," + strEndY
                    HandleDebug "Line#: Zone Height", CStr(J) + ": " + strHeight

                    ' Add the Zone Bounds string
                    strOutputText = strOutputText + DelimiterChar + strBounds

                    ' Append the final output string for this redaction to output file
                	AppendRedactionInfo strOutputText, GStrOut

                    ' Reset the output string to prepare for a next redaction
                    strOutputText = strImage
            	Next
        	Next
	    End If
	Next

	' Do nothing for this image and XML file if no redactions found

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
    HandleScriptError0 "ELI16231", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI16232", "Unable to acquire named Mutex!"

    ' Check for existence of the output file
    If Not fso.FileExists(strOutputPath) Then
        HandleDebug "Testing Output File Existence", "False"
        ' Get folder
        Dim strOutputFolder
        strOutputFolder = strGetFolder(strOutputPath)
        HandleDebug "Output Folder", strOutputFolder

        ' Check for existence of hardcoded folder 
        If Not fso.FolderExists(strOutputFolder) Then
            HandleDebug "Testing Output Folder Existence", "False"
            ' Create the folder
            fso.CreateFolder(strOutputFolder)
            HandleScriptError1 "ELI16233", "Unable to create output folder!", "Folder Name", strOutputFolder
        Else
            HandleDebug "Testing Output Folder Existence", "True"
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI16234", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI16235", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI16236", "Unable to release named Mutex!"
End Function

