'--------------------------------------------------------------------------------------------------
' Script commands specific for OH - Secretary of State
' - to create .REDACTED file
' - to update Redacted.csv file
'--------------------------------------------------------------------------------------------------
' Retrieve command-line argument 
' Expecting that image is a single-page TIF that is part of a multi-page Document
' where:
'     GStrCmd = C:\Images\0123-0001.tif
Dim GObjArgs, GStrCmd
Set GObjArgs = WScript.Arguments
GStrCmd = GObjArgs(0)
Call ParseCommandLineOptions(1)
HandleDebug "Input Image From Command-Line", GStrCmd

' Create File System Object and Wait Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI15865", "Unable to create File System Object!"

' Get folder & document number from command-line argument
' where:
'     DocFolder( C:\Images\0123-0001.tif ) = "C:\Images"
'     DocNumber( C:\Images\0123-0001.tif ) = "0123"
Dim GStrDocFolder, GStrDocNumber
GStrDocFolder = strGetDocFolder(GStrCmd)
HandleScriptError1 "ELI15866", "Unable to GetDocFolder()!", "Image Name", GStrCmd
HandleDebug "Doc Folder For Input Image", CStr(GStrDocFolder)

GStrDocNumber = strGetDocNumber(GStrCmd)
HandleScriptError1 "ELI15867", "Unable to GetDocFolder()!", "Image Name", GStrCmd
HandleDebug "Doc Number For Input Image", CStr(GStrDocNumber)

' Retrieve Roll or Din subfolder
Dim strSubsubfolder, strSubfolder
strSubsubfolder = strRollOrDinName(GStrDocFolder)
HandleScriptError1 "ELI15868", "Unable to GetRollOrDinName()!", "Document Folder", GStrDocFolder
HandleDebug "Roll or Din Name for Doc Folder", CStr(strSubsubfolder)

' Retrieve Folder Name
strSubfolder = strSubfolderName(GStrDocFolder)
HandleScriptError1 "ELI15869", "Unable to GetSubFolderName()!", "Document Folder", GStrDocFolder
HandleDebug "SubFolder Name for Doc Folder", CStr(strSubfolder)

' Build path to .REDACTED file to see if this document is already 
' listed in the Redacted.csv file
Dim strRedactedFile
strRedactedFile = "G:\Redacted\" + strSubfolder + "\" + strSubsubfolder + "\" + GStrDocNumber + ".redacted"
HandleDebug "Path to .REDACTED File", CStr(strRedactedFile)

' Do nothing if .REDACTED file already exists
If Not fso.FileExists(strRedactedFile) Then
    HandleDebug "Testing If .REDACTED File Exists", "False"

    ' Create named Mutex Object
    Dim MutexObject 
    Set MutexObject = CreateObject( "UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Redacted"
    HandleScriptError0 "ELI15870", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI15871", "Unable to acquire named mutex!"

    ' Double check file non-existence
    If Not fso.FileExists(strRedactedFile) Then
        ' Create the .REDACTED file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strRedactedFile)
        HandleScriptError1 "ELI15872", "Unable to create .REDACTED file!", "Filename", strRedactedFile

        ' Write the new text to the file
        NewFile.WriteLine("File now exists!")
        HandleScriptError1 "ELI15873", "Unable to write to .REDACTED file!", "Filename", strRedactedFile
        NewFile.Close

        ' Get Page Count for this document
        Dim nPageCount
        nPageCount = nGetPageCount(GStrCmd)
        HandleDebug "Page Count For Document", CStr(nPageCount)

        ' Update the Redacted.csv file (in the G:\Redacted\... subfolder)
        Call UpdateRedactedCSV(GStrDocFolder,GStrDocNumber,nPageCount)
    End If
    
    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI15874", "Unable to release named Mutex!"
Else
    HandleDebug "Testing If .REDACTED File Exists", "True"
End If

'--------------------------------------------------------------------------------------------------
' Appends to Redacted.csv file a line with the following format
'   strDocNumber,R_UCC,strDocNumber-01-0001,nPageCount,OK,IDSHIELD
'--------------------------------------------------------------------------------------------------
Sub UpdateRedactedCSV(strFolder,strDocNumber,nPageCount)
    ' Retrieve Roll or Din subfolder, Folder name
    Dim strRollOrDin, strSubFolder
    strRollOrDin = strRollOrDinName(strFolder)
    HandleDebug "Roll or Din Name from strFolder", CStr(strRollOrDin)
    
    strSubFolder = strSubfolderName(strFolder)
    HandleDebug "Sub Folder Name from strFolder", CStr(strSubFolder)

    ' Build path to Redacted.csv
    Dim strDstFolder, strCSVFile
    strDstFolder = "G:\Redacted\" + strSubFolder + "\" + strRollOrDin + "\"
    HandleDebug "Destination Folder", CStr(strDstFolder)

    strCSVFile = strDstFolder + "Redacted.csv"
    HandleDebug "Path To CSV File", CStr(strCSVFile)

    ' Build line of text to be appended to CSV file
    Dim strNewLine
    strNewLine = strDocNumber + ",R_UCC," + strDocNumber + "-01-0001," + CStr(nPageCount) + ",OK,IDSHIELD"
    HandleDebug "Line Of Text For CSV File", CStr(strNewLine)

    ' Create the file if not found
    If Not fso.FileExists(strCSVFile) Then
        HandleDebug "Testing Redacted.csv File Existence", "False"

        ' Check for folder existence
        Dim strNewFolder
        strNewFolder = strGetDocFolder(strCSVFile)
        HandleDebug "UpdateRedactedCSV::strNewFolder", strNewFolder
        If Not fso.FolderExists(strNewFolder) Then
            HandleDebug "Testing Redacted.csv Folder Existence", "False"
            fso.CreateFolder(strNewFolder)
            HandleScriptError1 "ELI15875", "Unable to create Folder for Redacted.csv!", "Folder Name", strNewFolder
        End If
        
        ' Create the .CSV file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strCSVFile)
        HandleScriptError1 "ELI15876", "Unable to create Redacted.csv!", "File Path", strCSVFile

        ' Write the first line to the file
        NewFile.WriteLine(strNewLine)
        HandleScriptError1 "ELI15877", "Unable to write to Redacted.csv!", "Text", strNewLine
        NewFile.Close
    Else
        HandleDebug "Testing Redacted.csv File Existence", "True"

        ' Open the CSV file for append
        Dim RedCSVFile
        Set RedCSVFile = fso.OpenTextFile(strCSVFile,ForAppending,True)
        HandleScriptError1 "ELI15878", "Unable to open Redacted.csv for append!", "File Path", strCSVFile

        ' Write the new text to the file
        RedCSVFile.WriteLine(strNewLine)
        HandleScriptError1 "ELI15879", "Unable to write to Redacted.csv!", "Text", strNewLine
        RedCSVFile.Close
    End If
End Sub

