'--------------------------------------------------------------------------------------------------
' Script commands specific for OH - Secretary of State
' - to update Verified.csv file
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that image is a single-page TIF that is part of a multi-page Document
' where:
'     GStrCmd = C:\Images\0123-0001.tif
Dim GObjArgs, GStrCmd
Set GObjArgs = WScript.Arguments
GStrCmd = GObjArgs(0)
Call ParseCommandLineOptions(1)
HandleDebug "Input Image From Command-Line", GStrCmd

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI15849", "Unable to create File System Object!"

' Get folder & page number from command-line argument
' where:
'     DocFolder( C:\Images\0123-0001.tif ) = "C:\Images"
'    PageNumber( C:\Images\0123-0001.tif ) = 1
Dim GStrDocFolder
GStrDocFolder = strGetDocFolder(GStrCmd)
HandleScriptError1 "ELI15850", "Unable to GetDocFolder()!", "Image Name", GStrCmd
HandleDebug "Doc Folder For Input Image", CStr(GStrDocFolder)

Dim GNPageNumber
GNPageNumber = nGetPageNumber(GStrCmd)
HandleScriptError1 "ELI15851", "Unable to GetPageNumber()!", "Image Name", GStrCmd
HandleDebug "Page Number For Input Image", CStr(GNPageNumber)

' Continue only if this TIF is the first page of the document
If GNPageNumber = 1 Then
    HandleDebug "Testing If Page 1", "True"

    ' Create named Mutex Object
    Dim MutexObject 
    Set MutexObject = CreateObject( "UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Verified"
    HandleScriptError0 "ELI15852", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI15853", "Unable to acquire named mutex!"

    ' Get document number from command-line argument
    ' where:
    '     DocNumber( C:\Images\0123-0001.tif ) = "0123"
    Dim GStrDocNumber
    GStrDocNumber = strGetDocNumber(GStrCmd)
    HandleScriptError1 "ELI15854", "Unable to GetDocNumber()!", "Image Name", GStrCmd
    HandleDebug "Doc Number For Input Image", CStr(GStrDocNumber)

    ' Get page count for this document by checking for available 
    ' (still unprocessed) TIFs
    Dim nPageCount
    nPageCount = nGetPageCount(GStrCmd)
    HandleScriptError1 "ELI15855", "Unable to GetPageCount()!", "Image Name", GStrCmd
    HandleDebug "Page Count For Input Image", CStr(nPageCount)

    ' Update the Verified.csv file (in the G:\Redacted\... subfolder)
    Call UpdateVerifiedCSV(GStrDocFolder,GStrDocNumber,nPageCount)

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI15856", "Unable to release named Mutex!"
Else
    HandleDebug "Testing If Page 1", "False"
End If

'--------------------------------------------------------------------------------------------------
' Retrieve integer Page Number from page-level filename
' where:
'    PageNumber( C:\Images\0123-0001.tif ) = 1
'--------------------------------------------------------------------------------------------------
Function nGetPageNumber(strFileName)
    ' Define error handler
    On Error Resume Next

    ' Remove page number and file extension portion of filename by finding last dash
    ' where:
    '    "C:\Images\0123-0001.tif" ====> "0001.tif"
    Dim SearchChar, n, p, strPart
    SearchChar = "-"
    p = InStrRev(strFileName,SearchChar)
    If p > 0 Then
        n = Len(strFileName)
        strPart = Right(strFileName, n-p)
        HandleDebug "GetPageNumber::strPart", strPart
    End If

    ' Get Page Number by finding first period
    ' where:
    '    "0001.tif" ====> "0001"
    Dim strPageNumber
    SearchChar = "."
    p = InStr(strPart,SearchChar)
    If p > 0 Then
        ' Found period, get preceding portion
        strPageNumber = Left(strPart, p-1)
        ' Convert page number text to integer
        nGetPageNumber = CInt(strPageNumber)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Appends to Verified.csv file a line with the following format
'   strDocNumber,R_UCC,strDocNumber-01-0001,nPageCount,OK,IDSHIELD
'--------------------------------------------------------------------------------------------------
Sub UpdateVerifiedCSV(strFolder,strDocNumber,nPageCount)
    ' Define error handler
    On Error Resume Next

    ' Retrieve Roll or Din subfolder and Folder name
    Dim strRollOrDin, strSubFolder
    strRollOrDin = strRollOrDinName(strFolder)
    HandleDebug "UpdateVerifiedCSV::strRollOrDin", strRollOrDin

    strSubFolder = strSubfolderName(strFolder)
    HandleDebug "UpdateVerifiedCSV::strSubFolder", strSubFolder

    ' Build path to Verified.csv
    Dim strDstFolder, strCSVFile
    strDstFolder = "G:\Redacted\" + strSubFolder + "\" + strRollOrDin + "\"
    strCSVFile = strDstFolder + "Verified.csv"
    HandleDebug "UpdateVerifiedCSV::strCSVFile", strCSVFile

    ' Build line of text to be appended to CSV file
    Dim strNewLine
    strNewLine = strDocNumber + ",R_UCC," + strDocNumber + "-01-0001," + CStr(nPageCount) + ",OK,IDSHIELD"
    HandleDebug "UpdateVerifiedCSV::strNewLine", strNewLine

    ' Create the file if not found
    If Not fso.FileExists(strCSVFile) Then
        HandleDebug "UpdateVerifiedCSV - Testing File '" + strCSVFile + "' Existence", "False"

        ' Check for folder existence
        Dim strNewFolder
        strNewFolder = strGetDocFolder(strCSVFile)
        HandleDebug "UpdateVerifiedCSV::strNewFolder", strNewFolder
        If Not fso.FolderExists(strNewFolder) Then
            HandleDebug "UpdateVerifiedCSV - Testing Folder '" + strNewFolder + "' Existence", "False"

            ' Check for subfolder presence
            Dim strTemp
            strTemp = "G:\Redacted\" + strSubFolder
            If Not fso.FolderExists(strTemp) Then
                HandleDebug "UpdateVerifiedCSV - Testing Folder '" + strTemp + "' Existence", "False"
                fso.CreateFolder(strTemp)
                HandleScriptError1 "ELI15857", "Unable to create subfolder!", "Folder Name", strTemp
            End If

            ' Create the Roll / Din folder
            fso.CreateFolder(strNewFolder)
            HandleScriptError1 "ELI15858", "Unable to create folder!", "Folder Name", strNewFolder
        End If

        ' Create the .CSV file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strCSVFile)
        HandleScriptError1 "ELI15859", "Unable to create CSV file!", "CSV File Name", strCSVFile

        ' Write the first line to the file
        NewFile.WriteLine(strNewLine)
        HandleScriptError2 "ELI15860", "Unable to write line to CSV file!", "CSV File", strCSVFile, "Line", strNewLine
        NewFile.Close
        HandleScriptError1 "ELI15861", "Unable to close CSV file!", "CSV File", strCSVFile
    Else
        HandleDebug "UpdateVerifiedCSV - Testing File '" + strCSVFile + "' Existence", "True"

        ' Open the CSV file for append
        Dim CSVFile
        Set CSVFile = fso.OpenTextFile(strCSVFile,ForAppending,True)
        HandleScriptError1 "ELI15862", "Unable to open CSV file for append!", "CSV File Name", strCSVFile

        ' Write the new text to the file
        CSVFile.WriteLine(strNewLine)
        HandleScriptError2 "ELI15863", "Unable to write line to CSV file!", "CSV File", strCSVFile, "Line", strNewLine
        CSVFile.Close
        HandleScriptError1 "ELI15864", "Unable to close CSV file!", "CSV File", strCSVFile
    End If
End Sub

