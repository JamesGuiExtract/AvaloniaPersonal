'--------------------------------------------------------------------------------------------------
' Script commands specific for CA - Yuba
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a multiple-page TIF
' where:
'     GStrImg = H:\Verification\Input\O--1992-12-31-1-00774.TIF
'     GStrOutput = H:\Verification\OutputFile.txt
'--------------------------------------------------------------------------------------------------
Dim GObjArgs, GStrImg, GStrOutput
Set GObjArgs = WScript.Arguments
GStrImg = GObjArgs(0)
GStrOutput = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input Image From Command-Line", GStrImg
HandleDebug "Output File From Command-Line", GStrOutput

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI23268", "Unable to create File System Object!"

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Verification\Input\O--1992-12-31-1-00774.TIF ) = 
'       "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

ProcessXMLFile GStrXMLPath, GStrOutput

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for CA - Yuba
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
' Append information from parsed XML file, one line per redaction, to the output file
' as:
'     "Filename","XXX1:YYY1","XXX2:YYY2"
' where:
'     "Filename" is the filename of the input image
'     "XXX1:YYY1" is the top-left point of the redaction
'     "XXX2:YYY2" is the bottom-right point of the redaction
'--------------------------------------------------------------------------------------------------
Function ProcessXMLFile(strXMLPath,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessXMLFile()", strXMLPath + ", " + strOutputPath
    
    ' Create output string, etc.
    Dim CommaChar, DoubleQuoteChar, strOutputText
    CommaChar = ","
    DoubleQuoteChar = Chr(34)

    ' Create the XML DOM Document object
    Dim xmlDoc
    Set xmlDoc = CreateObject("MSXML.DOMDocument")
    HandleScriptError0 "ELI23269", "Unable to create XML DOM Document Object!"

    ' Load the XML output file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI23270", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

    ' Get the collection of redacted items
    Dim xmlList
    Set xmlList = xmlDoc.selectNodes("/FlexData/( HCData | MCData | LCData )/SpatialLine/SpatialLineBounds")
    HandleScriptError1 "ELI23271", "Unable to select Redaction nodes!", "XML Path", strXMLPath

    ' Check each Node in the collection
    Dim nCount, I
    nCount = xmlList.length
    HandleDebug "Redactions Node Count", CStr(nCount)
    For I = 1 To nCount

        ' Reset the Output String
        strOutputText = ""

        ' Retrieve this Node
        Dim xmlThisNode
        Set xmlThisNode = xmlList.item(I-1)
        HandleScriptError1 "ELI23272", "Unable to retrieve Redaction Node #" + CStr(I) + " of " +_
                                  CStr(nCount) + "!", "XML Path", strXMLPath

        ' Add image filename to Output String
        strOutputText = DoubleQuoteChar + GStrImg + DoubleQuoteChar + CommaChar

        ' Add redaction coordinates to Output String
        Dim Left, Top, Right, Bottom
        Left = xmlThisNode.selectSingleNode("@Left").text
        Top = xmlThisNode.selectSingleNode("@Top").text
        Right = xmlThisNode.selectSingleNode("@Right").text
        Bottom = xmlThisNode.selectSingleNode("@Bottom").text
        strOutputText = strOutputText + DoubleQuoteChar + Left + ":" + Top + DoubleQuoteChar + CommaChar +_
                                    DoubleQuoteChar + Right + ":" + Bottom + DoubleQuoteChar

        ' Append output string to the output file
        AppendRedactionInfo strOutputText, GStrOutput
        HandleScriptError1 "ELI23273", "Error appending line to file", strOutputText, strXMLPath
    Next
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
    MutexObject.CreateNamed "Export_Output"
    HandleScriptError0 "ELI23274", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI23275", "Unable to acquire named Mutex!"

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
                HandleScriptError1 "ELI23276", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI23277", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI23278", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI23279", "Unable to release named Mutex!"
End Function

