'--------------------------------------------------------------------------------------------------
' Script commands specific for CA - Sacramento
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a multiple-page TIF
' where:
'     GStrImg = H:\Verification\Input\O--1992-12-31-1-00774.TIF
'     GStrDetailed = H:\Verification\DetailedReportOutputFile.txt
'     GStrSummary = H:\Verification\SummaryReportOutputFile.txt
'--------------------------------------------------------------------------------------------------
Dim GObjArgs, GStrImg, GStrDetailed, GStrSummary
Set GObjArgs = WScript.Arguments
GStrImg = GObjArgs(0)
GStrDetailed = GObjArgs(1)
GStrSummary = GObjArgs(2)
Call ParseCommandLineOptions(3)
HandleDebug "Input Image From Command-Line", GStrImg
HandleDebug "Output File For Detailed Report From Command-Line", GStrDetailed
HandleDebug "Output File For Summary Report From Command-Line", GStrSummary

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI21531", "Unable to create File System Object!"

' Create Page Count object and get page count for this file
Dim PageCountObject, GNPageCount
Set PageCountObject = CreateObject("VBScriptUtils.ImageData")
HandleScriptError0 "ELI21532", "Unable to create Page Count Object!"
GNPageCount = PageCountObject.GetImagePageCount(GStrImg)
HandleScriptError1 "ELI21533", "Unable to GetImagePageCount()!", "Image Name", GStrImg
HandleDebug "Page Count For Input Image", CStr(GNPageCount)

' Get XML filename from command-line argument
' where:
'     XMLPath( H:\Verification\Input\O--1992-12-31-1-00774.TIF ) = 
'       "H:\Verification\Input\O--1992-12-31-1-00774.TIF.xml"
Dim GStrXMLPath
GStrXMLPath = strGetXMLPath(GStrImg)
HandleDebug "XML Path For Input Image", GStrXMLPath

ProcessXMLFile GStrXMLPath, GStrDetailed

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for CA - Sacramento
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
' Append information from parsed XML file, one line per redaction, to the detailed report file
' as:
'     Recording date|Sequential Document Number|Page within document number
'         |Automated/Verified|Number of redactions on that page
' Compute redactions applied/not applied and add to numbers in summary report file
'--------------------------------------------------------------------------------------------------
Function ProcessXMLFile(strXMLPath,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessXMLFile()", strXMLPath + ", " + strOutputPath
    
    ' Create output string, etc.
    Dim PipeChar, strFileNumber, strOutputText
    PipeChar = "|"

    ' Get Recording Date and File Number from XML file path
    ' File Format:
    ' 0--1992-12-31-r-nnnnn.tif
    '   0-- = "Official" rendition of Recorded Document
    '   1992-12-31 = Recording Date (yyyy-mm-dd)
    '   r = film reel number for that date
    '   nnnnn = Sequential Document Number for that Recording Date
    Dim matchDateNum, dateNum
    Set matchDateNum = New RegExp
    matchDateNum.Pattern = "\bO--(\d+-\d+-\d+)-\d+-(\d+)[^\\]+$"
    Set dateNum = matchDateNum.Execute(strXMLPath)(0).subMatches
    strFileNumber = Cstr(dateNum(0)) + PipeChar + Cstr(dateNum(1))

    HandleScriptError1 "ELI21534", "Unable to get info from XML Path!", "XML Path", strXMLPath
    HandleDebug "Recording Date and File Number", strFileNumber

    ' Create the XML DOM Document object
    Dim xmlDoc
    Set xmlDoc = CreateObject("MSXML.DOMDocument")
    HandleScriptError0 "ELI21535", "Unable to create XML DOM Document Object!"

    ' Load the XML metadata file
    xmlDoc.load(strXMLPath)
    HandleScriptError1 "ELI21536", "Unable to load XML file into DOM Document!", "XML Path", strXMLPath

    ' Get the collection of applied Redaction nodes
    Dim xmlList
    Set xmlList = xmlDoc.selectNodes("/IDShieldMetaData/Redactions/Redaction[@Output='1']")
    HandleScriptError1 "ELI21537", "Unable to select Redaction nodes!", "XML Path", strXMLPath

    ' Check each Node in the collection
    Dim nCount, I
    nCount = xmlList.length
    HandleDebug "Redactions Node Count", CStr(nCount)
    For I = 1 To nCount
        
        ' Reset the Output String
        strOutputText = strFileNumber

        ' Retrieve this Node
        Dim xmlThisNode
        Set xmlThisNode = xmlList.item(I-1)
        HandleScriptError1 "ELI21538", "Unable to retrieve Redaction Node #" + CStr(I) + " of " +_
                                  CStr(nCount) + "!", "XML Path", strXMLPath

        ' Add Page Number to Output String
        Dim xmlSingleNode, strPageNumber
        Set xmlSingleNode = xmlThisNode.selectSingleNode("Line/Zone/@PageNumber")
        strPageNumber = xmlSingleNode.text
        strOutputText = strOutputText + PipeChar + strPageNumber
        HandleScriptError1 "ELI21539", "Unable to select PageNumber attribute!", "XML Path", strXMLPath

        ' Retrieve the Category
        Dim strCategory
        Set xmlSingleNode = xmlThisNode.selectSingleNode("@Category")
        strCategory = xmlSingleNode.text
        HandleScriptError1 "ELI21540", "Unable to select Category attribute!", "XML Path", strXMLPath

        ' Add "Verified" or "Automated" to output string
        If strCategory = "Man" Then
            strOutputText = strOutputText + PipeChar + "Verified"
        Else
        ' Category is High, Medium, or Low
            strOutputText = strOutputText + PipeChar + "Automated"
        End If

        ' Add Redactions on This Page to Output String
        Dim numRedactions        
        numRedactions = xmlDoc.selectNodes("//Redaction[@Output='1'][Line/Zone/@PageNumber='"+strPageNumber+"']").length
        strOutputText = strOutputText + PipeChar + Cstr(numRedactions)
        HandleDebug "Page#, Redactions", strPageNumber + ", " + Cstr(numRedactions)
        HandleScriptError1 "ELI21541", "Unable to get number of redactions on page!", "XML Path", strXMLPath

        ' Append output string to the output file
        AppendRedactionInfo strOutputText, GStrDetailed
        HandleScriptError1 "ELI21542", "Error appending line to file", strOutputText, strXMLPath
    Next

    ' Add info to summary output file
    Dim numReviewed, numApplied, numNotApplied, numManual

    ' Query for High/Medium/Low Confidence redactions presented
    numReviewed = xmlDoc.selectNodes("//Redaction[@Category!='Clue'][@Category!='Man']").length
    HandleDebug "High/Medium/Low Confidence redactions presented", Cstr(numReviewed)

    ' Query for High/Medium/Low Confidence redactions applied
    numApplied = xmlDoc.selectNodes("//Redaction[@Category!='Clue'][@Category!='Man'][@Output='1']").length
    HandleDebug "High/Medium/Low Confidence redactions applied", Cstr(numApplied)

    ' Query for High/Medium/Low Confidence redactions not applied
    numNotApplied = xmlDoc.selectNodes("//Redaction[@Category!='Clue'][@Category!='Man'][@Output='0']").length
    HandleDebug "High/Medium/Low Confidence redactions not applied", Cstr(numNotApplied)

    ' Query for Manual redactions applied
    numManual = xmlDoc.selectNodes("//Redaction[@Category='Man'][@Output='1']").length
    HandleDebug "Manual redactions applied", Cstr(numManual)
    
    ' Add to running totals
    AddAccuracyInfo numReviewed, numApplied, numNotApplied, numManual, GStrSummary
    HandleScriptError1 "ELI21543", "Error adding accuracy info to file", GStrSummary
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
    MutexObject.CreateNamed "Export_Detailed"
    HandleScriptError0 "ELI21544", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI21545", "Unable to acquire named Mutex!"

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
                HandleScriptError1 "ELI21546", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI21547", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI21548", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI21549", "Unable to release named Mutex!"
End Function

'--------------------------------------------------------------------------------------------------
' Adds the specified accuracy information to the existing information in specified output file.
' Output file is formatted as:
'               Total Documents Processed: #
'               Total Pages Processed: #
'                 Total Redactions Reviewed: #
'                   Total Applied: #
'                   Total Not Applied: #
'                 Total Redactions Applied: #
'                   Total From Automated Process: #
'                   Total From Review Process: #
'--------------------------------------------------------------------------------------------------
Function AddAccuracyInfo(numReviewed, numApplied, numNotApplied, numManual, strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering AddAccuracyInfo()", "Output file: " + strOutputPath

    ' Create named Mutex Object to protect access to the output file
    Dim MutexObject 
    Set MutexObject = CreateObject("UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Export_Summary"
    HandleScriptError0 "ELI21550", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI21551", "Unable to acquire named Mutex!"

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
                HandleScriptError1 "ELI21552", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI21553", "Unable to create output file!", "Output File", strOutputPath

        ' Write the outline and initial values to the file
        NewFile.Write(FormatData(0,0,0,0,0,0))       

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
    End If

    ' Open the input file for reading
    Dim OutFile, InFile, strData, findData, numDocuments, numPages
    Set findData = New RegExp
    Set InFile = fso.OpenTextFile(strOutputPath, ForReading)
    HandleScriptError1 "ELI21554", "Unable to open input file for read!", "Input File", strOutputPath

    'Read data
    strData = InFile.ReadAll
    HandleScriptError1 "ELI21555", "Unable to read from file!", "Input File", strOutputPath

    ' Close the input file
    InFile.Close

    ' Combine values from this run with values in the summary report output file

    ' Get previous total documents processed and add 1
    findData.Pattern = "Total\sDocuments\sProcessed:\s(\d+)"
    numDocuments = 1 + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21556", "Unable to read number of documents processed from file!", "Input File", strOutputPath

    ' Get previous total pages processed and add this document's page count
    findData.Pattern = "Total\sPages\sProcessed:\s(\d+)"
    numPages = GNPageCount + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21557", "Unable to read number of pages processed from file!", "Input File", strOutputPath

    ' Get previous total redactions presented and add those from this document
    findData.Pattern = "Total\sRedactions\sReviewed:\s(\d+)"
    numReviewed = numReviewed + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21558", "Unable to read number of redactions reviewed from file!", "Input File", strOutputPath

    ' Get previous total of presented redactions applied and add those from this document
    findData.Pattern = "Total\sApplied:\s(\d+)"
    numApplied = numApplied + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21559", "Unable to read number of redactions applied from file!", "Input File", strOutputPath

    ' Get previous total of presented redactions NOT applied and add those from this document
    findData.Pattern = "Total\sNot\sApplied:\s(\d+)"
    numNotApplied = numNotApplied + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21560", "Unable to read number of redactions not applied from file!", "Input File", strOutputPath

    ' Get previous total of manually added redactions and add those from this document
    findData.Pattern = "Total\sFrom\sReview\sProcess:\s(\d+)"
    numManual = numManual + Cint(findData.Execute(strData)(0).subMatches(0))
    HandleScriptError1 "ELI21561", "Unable to read number of manual redactions from file!", "Input File", strOutputPath
   
    ' Open the output file for writing
    Set OutFile = fso.OpenTextFile(strOutputPath, ForWriting)
    HandleScriptError1 "ELI21562", "Unable to open output file for write!", "Output File", strOutputPath

    ' Write the formatted values to the file
    OutFile.Write(FormatData(numDocuments, numPages, numReviewed, numApplied, numNotApplied, numManual))
    OutFile.Close

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI21563", "Unable to release named Mutex!"
End Function

'--------------------------------------------------------------------------------------------------
' Formats the specified accuracy information plus current time
'--------------------------------------------------------------------------------------------------
Function FormatData(numDocuments, numPages, numReviewed, numApplied, numNotApplied, numManual)
   FormatData = "Total Documents Processed: "&Cstr(numDocuments) & vbCrLf &_
                "Total Pages Processed: "&Cstr(numPages) & vbCrLf &_
                "  Total Redactions Reviewed: "&Cstr(numReviewed) & vbCrLf &_
                "    Total Applied: "&Cstr(numApplied) & vbCrLf &_
                "    Total Not Applied: "&Cstr(numNotApplied) & vbCrLf &_
                "  Total Redactions Applied: "&Cstr(numApplied+numManual) & vbCrLf &_
                "    Total From Automated Process: "&Cstr(numApplied)& vbCrLf &_
                "    Total From Review Process: "&Cstr(numManual)
End Function
