'--------------------------------------------------------------------------------------------------
' Script commands specific for NV - Douglas County
'--------------------------------------------------------------------------------------------------
' Constants for locations of input and output files ( TIFs, VOAs, XMLs )
Const InputFile = "\\10.64.21.51\Redact\Redact_In\index.txt"
Const OutputFolder = "\\10.64.21.51\Redact\Redact_Out\"
Const OutputFileName = "index.txt"

' Constant for searching a string for a double-quote character
Const DoubleQuote = """"

' Retrieve and parse command-line arguments
' Expecting that image is a multiple-page TIF
' where:
'     GStrImg = \\10.64.21.51\Redact\Redact_In\0123-0001.tif
Dim GObjArgs, GStrImg, GStrSubfolder
Set GObjArgs = WScript.Arguments
GStrImg = GObjArgs(0)
GStrSubfolder = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input Image From Command-Line", GStrImg
HandleDebug "Optional Subfolder From Command-Line", GStrSubfolder

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI15793", "Unable to create File System Object!"

' Get filename from command-line argument
' where:
'     FileName( \\10.64.21.51\Redact\Redact_In\0123-0001.tif ) = "0123-001.tif"
Dim GStrFileName
GStrFileName = strGetFileName(GStrImg)
HandleDebug "File Name From Input Path", GStrFileName

' Get document number from filename and index file
' where:
'     DocNumber( 0123-0001.tif ) = "1234567"
' when the appropriate line of InputFile is:
'     "Recorded Documents - Redaction","Recorded Document - Redaction","Doc #","1234567","\\10.64.21.51\Redact_In\0123-0001.tif"
Dim GStrDocNumber
GStrDocNumber = strGetDocNumber(GStrFileName)
HandleDebug "Document Number From File Name", GStrDocNumber

' Build the line of text for the output file
' as:
'    GStrDocNumber @@.\ GStrFileName
' Note that whitespace in the above example will be removed
Dim GStrOutputText
GStrOutputText = GStrDocNumber + "@@.\" + GStrFileName
HandleDebug "Output Text", GStrOutputText

' Construct name of output file including optional subfolder from the command line
Dim strOutputPath, strOutputFolder
strOutputFolder = OutputFolder
If GStrSubfolder <> "" Then
    ' Append subfolder
    strOutputFolder = strOutputFolder + GStrSubfolder + "\"
End If
' Append the actual filename
strOutputPath = strOutputFolder + OutputFileName
HandleDebug "Output File Path", strOutputPath

' Create named Mutex Object to protect access to the output file
Dim MutexObject 
Set MutexObject = CreateObject("UCLIDCOMUtils.COMMutex")
MutexObject.CreateNamed "Recorder"
HandleScriptError0 "ELI15795", "Unable to create COMMutex Object!"
MutexObject.Acquire
HandleScriptError0 "ELI15796", "Unable to acquire named Mutex!"

' Check for existence of the output file
If Not fso.FileExists(strOutputPath) Then
    HandleDebug "Testing Output File Existence", "False"
    ' Check for existence of hardcoded folder 
    If Not fso.FolderExists(strOutputFolder) Then
        HandleDebug "Testing Output Folder Existence", "False"
        ' Create the folder
        fso.CreateFolder(strOutputFolder)
        HandleScriptError1 "ELI15797", "Unable to create output folder!", "Folder Name", strOutputFolder
    Else
        HandleDebug "Output Folder Exists", strOutputFolder
    End If

    ' Create the output file
    Dim NewFile
    Set NewFile = fso.CreateTextFile(strOutputPath)
    HandleScriptError1 "ELI15798", "Unable to create output file!", "Output File", strOutputPath
    HandleDebug "Created Output File", strOutputPath

    ' Write the first line to the file
    NewFile.WriteLine(GStrOutputText)
    NewFile.Close
Else
    HandleDebug "Testing Output File Existence", "True"
    ' Output file exists, open it for append
    Dim OutFile
    Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
    HandleScriptError1 "ELI15799", "Unable to open output file for append!", "Output File", strOutputPath
    HandleDebug "Opened Output File", strOutputPath

    ' Write the new text to the file
    OutFile.WriteLine(GStrOutputText)
    OutFile.Close
End If

' Release the Mutex
MutexObject.ReleaseNamedMutex
HandleScriptError0 "ELI15800", "Unable to release named Mutex!"

'--------------------------------------------------------------------------------------------------
' Retrieve Filename from path
' where:
'     Filename( \\10.64.21.51\Redact\Redact_In\0123-0001.tif ) = "0123-0001.tif"
'--------------------------------------------------------------------------------------------------
Function strGetFileName(strPath)
    ' Extract filename portion of path by finding last backslash
    ' where:
    '    "\\10.64.21.51\Redact\Redact_In\0123-0001.tif" ====> "0123-0001.tif"
    Dim FolderChar, n, p
    FolderChar = "\"
    p = InStrRev(strPath,FolderChar)
    If p > 0 Then
        n = Len(strPath)
        strGetFileName = Right(strPath, n-p)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Document Number from filename of image and index file based on the parsing of text 
'     within the index file
' where:
'     DocNumber( 0123-0001.tif ) = "1234567"
' when the appropriate line of the InputFile is:
'     "Recorded Documents - Redaction","Recorded Document - Redaction","Doc #","1234567","\\10.64.21.51\Redact_In\0123-0001.tif"
'--------------------------------------------------------------------------------------------------
Function strGetDocNumber(strFileName)
    ' Check index file existence
    Set fso = CreateObject("Scripting.FileSystemObject")
    HandleScriptError0 "ELI15808", "Unable to create File System Object!"
    If fso.FileExists(InputFile) Then
        ' Open the index file for reading
        Dim IndexFile
        Set IndexFile = fso.OpenTextFile(InputFile,ForReading,False)
        HandleScriptError1 "ELI15801", "Unable to open index file for reading!", "Index File", InputFile

        ' Read each line of the index file to find document number for strFileName
        Dim strLine, strLinePath, strLineFile, strFirstChar, strLastChar, strNumber
        Dim n, nFileLength, nLength, CommaChar, CommaLast, CommaSecondToLast, bFilesMatch
        CommaChar = ","
        bFilesMatch = False
        Do While IndexFile.AtEndOfStream <> True
            ' Read this line of the index file
            strLine = IndexFile.ReadLine
            HandleDebug "Line From Index File", strLine
            n = Len(strLine)

            ' Find last and second-to-last commas in string
            CommaLast = InStrRev(strLine,CommaChar)
            CommaSecondToLast = InStrRev(strLine,CommaChar,CommaLast-1)
            
            ' Retrieve associated filename between last comma and end-of-string
            strLinePath = Right(strLine,n-CommaLast-1)
            strLineFile = strGetFileName(strLinePath)
            
            ' Remove trailing double quote if present
            nFileLength = Len(strLineFile)
            strLastChar = Right(strLineFile,1)
            If strLastChar = DoubleQuote Then
                strLineFile = Left(strLineFile,nFileLength-1)
            End If
            HandleDebug "Filename From Line Of Index File", strLineFile

            ' Compare filenames
            If strFileName = strLineFile Then
                ' Filenames match, set flag
                bFilesMatch = True
            End If

            ' Continue processing this line if file names match
            If bFilesMatch = True Then
                HandleDebug "Filename Comparison Between Index File & Source", "Match"
                ' Extract substring between last comma and second-to-last comma 
                ' This will be the document number surrounded by double-quotes
                If CommaLast > 0 And CommaSecondToLast > 0 Then
                    strNumber = Mid(strLine, CommaSecondToLast+1, CommaLast-CommaSecondToLast-1)
                End If

                ' Remove a leading double quote
                nLength = Len(strNumber)
                strFirstChar = Left(strNumber,1)
                If strFirstChar = DoubleQuote Then
                    strNumber = Right(strNumber,nLength-1)
                End If

                ' Remove a trailing double quote
                nLength = Len(strNumber)
                strLastChar = Right(strNumber,1)
                If strLastChar = DoubleQuote Then
                    strNumber = Left(strNumber,nLength-1)
                End If
                
                ' Return the trimmed document number
                strGetDocNumber = strNumber
                Exit Do
            Else
                HandleDebug "Filename Comparison Between Index File & Source", "No Match"
            End If
        Loop
        
        ' File was not found in index file, this is an error
        If bFilesMatch <> True Then
            ExceptionObject.CreateFromString "ELI15749", "Source document not found in index file!"

            ' Add debug info
            ExceptionObject.AddDebugInfo "Source Document", strFileName
            ExceptionObject.AddDebugInfo "Index File", InputFile
            
            ' Log the exception
            ExceptionObject.Log
        End If
        
        ' Close the index file
        IndexFile.Close()
    Else
        ' Create and log an error
        ExceptionObject.CreateFromString "ELI15802", "Index file does not exist!"

        ' Add debug info
        ExceptionObject.AddDebugInfo "Missing Index File", InputFile
        
        ' Log the exception
        ExceptionObject.Log
    End If
End Function

