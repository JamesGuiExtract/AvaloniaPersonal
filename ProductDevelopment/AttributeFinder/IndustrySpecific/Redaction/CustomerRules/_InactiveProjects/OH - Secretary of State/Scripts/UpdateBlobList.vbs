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

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI15880", "Unable to create File System Object!"

' Get folder & document number & document filename from command-line argument
' where:
'     DocFolder( C:\Images\0123-0001.tif ) = "C:\Images"
'     DocNumber( C:\Images\0123-0001.tif ) = "0123"
'       DocFile( C:\Images\0123-0001.tif ) = "0123-0001.tif"
Dim GStrDocFolder, GStrDocNumber, GStrDocFile
GStrDocFolder = strGetDocFolder(GStrCmd)
HandleDebug "Document Folder From Input Image", GStrDocFolder
GStrDocNumber = strGetDocNumber(GStrCmd)
HandleDebug "Document Number From Input Image", GStrDocNumber
GStrDocFile   = strGetDocFile(GStrCmd)
HandleDebug "Document File From Input Image", GStrDocFile

' Retrieve Roll or Din subfolder
Dim strSubsubfolder, strSubfolder
strSubsubfolder = strRollOrDinName(GStrDocFolder)
HandleDebug "Roll Or Din Name From Doc Folder", strSubsubfolder

' Retrieve Folder name
strSubfolder = strSubfolderName(GStrDocFolder)
HandleDebug "Folder Name From Doc Folder", strSubfolder

' Build path to .BLOB file to see if pages from this document are already 
' listed in the Uccblob.csv file
Dim strBlobFile
strBlobFile = "G:\Redacted\" + strSubfolder + "\" + strSubsubfolder + "\" + GStrDocNumber + ".blob"
HandleDebug "Path to .BLOB File", strBlobFile

' Do nothing if .BLOB file already exists
If Not fso.FileExists(strBlobFile) Then
    HandleDebug "Testing If .BLOB File Exists", "False"

    ' Create named Mutex Object
    Dim MutexObject 
    Set MutexObject = CreateObject( "UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Blob"
    HandleScriptError0 "ELI15881", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI15882", "Unable to acquire named mutex!"

    ' Double check file non-existence
    If Not fso.FileExists(strBlobFile) Then
        ' Create the .BLOB file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strBlobFile)
        HandleScriptError0 "ELI15883", "Unable to create .BLOB file!", "Filename", strBlobFile

        ' Write the new text to the file
        NewFile.WriteLine("File now exists!")
        HandleScriptError0 "ELI15884", "Unable to write to .BLOB file!", "Filename", strBlobFile
        NewFile.Close

        ' Get Page Count for this document
        Dim nPageCount
        nPageCount = nGetPageCount(GStrCmd)
        HandleDebug "Page Count For Document", CStr(nPageCount)

        ' Update the Uccblob.csv file (in the G:\Redacted\... subfolder)
        Call UpdateBlobCSV(GStrDocFolder,GStrDocNumber,nPageCount)

        ' Copy all original TIFs to the G:\Redacted\... subfolder
        Call CopyOriginalTIFs(GStrDocFolder,GStrDocNumber,nPageCount)
    End If
    
    ' Build path to Source and Destination files
    Dim strSrcFile, strDstFile
    strSrcFile = "G:\Unredacted\" + strSubfolder + "\" + strSubsubfolder + "\" + GStrDocFile
    HandleDebug "Source File", strSrcFile

    strDstFile = "G:\Redacted\"   + strSubfolder + "\" + strSubsubfolder + "\" + GStrDocFile
    HandleDebug "Destination File", strDstFile

    ' Copy the redacted page to the Destination folder - overwriting the existing file
    fso.CopyFile strSrcFile, strDstFile, true
    HandleScriptError2 "ELI15885", "Unable to copy redacted page to destination folder!", "Source File", strSrcFile, "Destination File", strDstFile
    
    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI15886", "Unable to release named Mutex!"
Else
    HandleDebug "Testing If .BLOB File Exists", "True"
End If

'--------------------------------------------------------------------------------------------------
' Appends to Uccblob.csv file one or more lines with the following format
'   strDocNumber,strDocNumber-01-0001
'   strDocNumber,strDocNumber-01-0002
'   strDocNumber,strDocNumber-01-0003
'   strDocNumber,strDocNumber-01-0004
'   ... so that one line is added for each of nPageCount pages in the Document
'--------------------------------------------------------------------------------------------------
Sub UpdateBlobCSV(strFolder,strDocNumber,nPageCount)
    ' Retrieve Roll or Din subfolder
    Dim strRollOrDin
    strRollOrDin = strRollOrDinName(strFolder)
    HandleDebug "UpdateBlobCSV::strRollOrDin", strRollOrDin

    ' Retrieve Folder name
    Dim strSubFolder
    strSubFolder = strSubfolderName(strFolder)
    HandleDebug "UpdateBlobCSV::strSubFolder", strSubFolder

    ' Build path to Uccblob.csv
    Dim strDstFolder, strCSVFile
    strDstFolder = "G:\Redacted\" + strSubFolder + "\" + strRollOrDin + "\"
    strCSVFile = strDstFolder + "Uccblob.csv"
    HandleDebug "Path to Uccblob.csv file", strCSVFile

    ' Build first line of text to be appended to CSV file
    Dim nCurrentPage, strNewLine
    nCurrentPage = 1
    strNewLine = strDocNumber + "," + strDocNumber + "-01-0001"
    HandleDebug "First line of text for Uccblob.csv file", strNewLine

    ' Check CSV file existence
    Dim CSVFile
    If Not fso.FileExists(strCSVFile) Then
        HandleDebug "Testing If Uccblob.csv File Exists", "False"

        ' Create CSV file
        Set CSVFile = fso.CreateTextFile(strCSVFile,ForWriting)
        HandleScriptError1 "ELI15887", "Unable to create Uccblob.csv!", "File Path", strCSVFile
    Else
        HandleDebug "Testing If Uccblob.csv File Exists", "True"

        ' Open the file for append
        Set CSVFile = fso.OpenTextFile(strCSVFile,ForAppending,True)
        HandleScriptError1 "ELI15888", "Unable to open Uccblob.csv for append!", "File Path", strCSVFile
    End If

    ' Write the first line of text to the file
    CSVFile.WriteLine(strNewLine)
    HandleScriptError1 "ELI15889", "Unable to write to Uccblob.csv!", "Text", strNewLine

    ' Add lines for subsequent pages
    While nCurrentPage < nPageCount
        ' Build new page number by zero-padding to 4 characters
        Dim nNewPage, strTempNewPage, nLen, strNewPage
        nNewPage = nCurrentPage + 1
        strTempNewPage = CStr(nNewPage)
        nLen = Len(strTempNewPage)
        strNewPage = String(4 - nLen, "0") + strTempNewPage
        HandleDebug "UpdateBlobCSV::strNewPage", strNewPage

        ' Build next string
        strNewLine = strDocNumber + "," + strDocNumber + "-01-" + strNewPage
        HandleDebug "Next line of text for Uccblob.csv file", strNewLine

        ' Write the new text to the file
        CSVFile.WriteLine(strNewLine)
        HandleScriptError1 "ELI15890", "Unable to write to Uccblob.csv!", "Text", strNewLine

        ' Update current page
        nCurrentPage = nNewPage
    Wend

    ' Close the file
    CSVFile.Close
    HandleScriptError1 "ELI15891", "Unable to close Uccblob.csv!", "File Path", strCSVFile
End Sub

'--------------------------------------------------------------------------------------------------
' Copies each original page of strDocNumber to appropriate G:\Redacted subfolder
'   strDocNumber-01-0001.tif
'   strDocNumber-01-0002.tif
'   strDocNumber-01-0003.tif
'   strDocNumber-01-0004.tif
'   ... 
'   where Source location (within strFolder) is 
'     G:\Unredacted\Subfolder\Roll OR
'     G:\Unredacted\Subfolder\Din
'--------------------------------------------------------------------------------------------------
Sub CopyOriginalTIFs(strFolder,strDocNumber,nPageCount)
    ' Retrieve Roll or Din subfolder
    Dim strRollOrDin
    strRollOrDin = strRollOrDinName(strFolder)
    HandleDebug "CopyOriginalTIFs::strRollOrDin", strRollOrDin

    ' Retrieve Folder name
    Dim strSubFolder
    strSubFolder = strSubfolderName(strFolder)
    HandleDebug "CopyOriginalTIFs::strSubFolder", strSubFolder

    ' Build path to Source and Destination folders
    Dim strSrcFolder, strDstFolder
    strSrcFolder = "G:\Unredacted\" + strSubFolder + "\" + strRollOrDin + "\"
    HandleDebug "CopyOriginalTIFs::strSrcFolder", strSrcFolder
    strDstFolder = "G:\Redacted\" + strSubFolder + "\" + strRollOrDin + "\"
    HandleDebug "CopyOriginalTIFs::strDstFolder", strDstFolder

    ' Check Destination folder existence
    If Not fso.FolderExists(strDstFolder) Then
        HandleDebug "Testing Destination Folder '" + strDstFolder + "' Existence", "False"

        ' Create the folder
        Dim f
        Set f = fso.CreateFolder(strDstFolder)
        HandleScriptError1 "ELI15892", "Unable to create Destination folder!", "Folder Path", strDstFolder
    End If

    ' Start with first page
    Dim nCurrentPage
    nCurrentPage = 1

    ' Copy each page to Destination folder
    While nCurrentPage <= nPageCount
        ' Build page number string
        Dim strTempNewPage, nLen, strNewPage
        strTempNewPage = CStr(nCurrentPage)
        nLen = Len(strTempNewPage)
        strNewPage = String(4 - nLen, "0") + strTempNewPage
        HandleDebug "CopyOriginalTIFs::strNewPage", strNewPage

        ' Build Source and Destination filenames
        Dim strSrcFile, strDstFile
        strSrcFile = strSrcFolder + strDocNumber + "-01-" + strNewPage + ".tif"
        HandleDebug "CopyOriginalTIFs::strSrcFile", strSrcFile

        strDstFile = strDstFolder + strDocNumber + "-01-" + strNewPage + ".tif"
        HandleDebug "CopyOriginalTIFs::strDstFile", strDstFile

        ' Copy the file
        fso.CopyFile strSrcFile, strDstFile, false
        HandleScriptError2 "ELI15893", "Unable to copy source file!", "Source File", strSrcFile, "Destination File", strDstFile

        ' Update current page
        nCurrentPage = nCurrentPage + 1
    Wend
End Sub

