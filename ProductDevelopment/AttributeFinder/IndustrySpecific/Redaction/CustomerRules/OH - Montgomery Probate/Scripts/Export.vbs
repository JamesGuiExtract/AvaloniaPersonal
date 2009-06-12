'--------------------------------------------------------------------------------------------------
' Script commands specific for OH - Montgomery County
'--------------------------------------------------------------------------------------------------
' Define constants used within this script for
' - script output text filename
' - prefix used to name redacted output files from the filename of the input image
' - suffix used to name redacted output files from the filename of the input image
' where:
'     RedactedOutputFilename = GStrOutputFolder + "\" + GStrRedactedImagePrefix + "\" + 
'         $FileNoExtOf(<SourceDocName>) + GStrRedactedImageSuffix + $ExtOf(<SourceDocName>)
Dim GStrOutputTextFile, GStrRedactedImagePrefix, GStrRedactedImageSuffix
GStrOutputTextFile = "Redacted_Output_TEMP.txt"
GStrRedactedImagePrefix = ""
' Per customer usage, changed suffix from "R" to ""
GStrRedactedImageSuffix = ""
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' Expecting that 
' - image is a multiple-page TIF and 
' - output folder will contain redacted images
' where:
'     GStrInputImage   = D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF
'     GStrOutputFolder = D:\ImageOutputVerify
' and:
'     OutputImage      = D:\ImageOutputVerify\Redact_EST239778-1101788-20001218-I0000774.TIF
Dim GObjArgs, GStrOutputFolder, GStrInputImage
Set GObjArgs = WScript.Arguments
GStrInputImage = GObjArgs(0)
GStrOutputFolder = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input Image From Command-Line", GStrInputImage
HandleDebug "Output Folder From Command-Line", GStrOutputFolder

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI20931", "Unable to create File System Object!"

' Get filename of error file
' where:
'     ErrorFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       "D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF.uex"
Dim GStrErrorFilePath
GStrErrorFilePath = strGetErrorFilePath(GStrInputImage)
HandleDebug "Error File Path For Input Image", GStrErrorFilePath

' Get filename of Redacted output file
' where:
'     RedactedFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       GStrOutputFolder + "\" + GStrRedactedImagePrefix + 
'       "EST239778-1101788-20001218-I0000774" + GStrRedactedImagePrefix + ".TIF"
Dim GStrRedactedFilePath
GStrRedactedFilePath = strGetRedactedFilePath(GStrInputImage)
HandleDebug "Redacted File Path For Input Image", GStrRedactedFilePath

' Get filename of VOA file
' where:
'     VOAFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       "D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF.voa"
Dim GStrVOAFilePath
GStrVOAFilePath = strGetVOAFilePath(GStrInputImage)
HandleDebug "VOA File Path For Input Image", GStrVOAFilePath

' Get timestamp of VOA file in YYYYMMDD format
' where:
'     VOAFileTime( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF.voa ) = 
'       "20080422"
' If the VOA file cannot be found or the modification date cannot be determined, 
' the return value will be "00000000".
Dim GStrVOAFileTime
GStrVOAFileTime = strGetFileTime(GStrVOAFilePath)
HandleDebug "VOA File Time", GStrVOAFileTime

' Build filename of output text file
' where:
'     OutputFilePath = GStrOutputFolder + "\" + GStrOutputTextFile
Dim GStrOutputFilePath
GStrOutputFilePath = GStrOutputFolder + "\" + GStrOutputTextFile
HandleDebug "Output File Path", GStrOutputFilePath

' Append redaction information from the specified input file to the output file
' as:
'     filename|processingdate|info
' where:
'     filename       = the filename of the input image
'     processingdate = the modification time of the VOA file
'     info           = 0 if no redactions were made
'                    = 1 if 1 or more redactions were made
'                    = 2 if an error occurred during processing
ProcessRedactionResult GStrInputImage, GStrVOAFileTime, GStrOutputFilePath

'--------------------------------------------------------------------------------------------------
' Local functions and subroutines specific for OH - Montgomery County
'--------------------------------------------------------------------------------------------------
' Get filename from specified image file path
' where:
'     Filename( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       "EST239778-1101788-20001218-I0000774.TIF"
'--------------------------------------------------------------------------------------------------
Function strGetFileNameWithoutExtension(strImagePath)
    ' Find last backslash in input filename
    ' Also find last backslash in input filename
    ' where:
    '     strImagePath  = D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF
    Dim BackslashChar, PeriodChar, p, q
    BackslashChar = "\"
    PeriodChar = "."
    p = InStrRev(strImagePath,BackslashChar)
    q = InStrRev(strImagePath,PeriodChar)
    If p > 0 Then
        ' Extract the characters to right
        Dim n
        n = Len(strImagePath)
        If q > 0 Then
            ' Extract characters after backslash and before period
            strGetFileNameWithoutExtension = Mid(strImagePath, p+1, q-p-1)
        End If
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Get file extension from specified image file path
' where:
'     FileExtension( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       ".TIF"
'--------------------------------------------------------------------------------------------------
Function strGetFileExtension(strImagePath)
    ' Find last backslash in input filename
    ' where:
    '     strImagePath  = D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF
    Dim PeriodChar, p
    PeriodChar = "."
    p = InStrRev(strImagePath,PeriodChar)
    If p > 0 Then
        ' Extract the period and the characters to right
        Dim n
        n = Len(strImagePath)
        strGetFileExtension = Right(strImagePath, n-p+1)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Get last-modified-time timestamp of specified file as YYYYMMDD
'--------------------------------------------------------------------------------------------------
Function strGetFileTime(strPath)
    ' First test file existence
    Dim strFinal
    If Not fso.FileExists(strPath) Then
        ' File does not exist, use default time string
        strFinal = "00000000"
    Else
        ' File exists, get the File object
        Dim f
        Set f = fso.GetFile(strPath)
        HandleScriptError1 "ELI20993", "Failed to GetFile()!", "Filename", strPath
    
        ' Get last-modified time from File object
        Dim time
        time = f.DateLastModified
    
        ' Parse time string to retrieve YYYY
        Dim SlashChar, p, q, strYear, strMonth, strDay
        SlashChar = "/"
        p = InStrRev(time,SlashChar)
        If p > 0 Then
            ' Extract four characters
            strYear = Mid(time, p+1, 4)
        End If
        
        ' Parse time string to retrieve DD
        q = InStr(time,SlashChar)
        If q > 0 Then
            ' Extract characters between slashes and zero-pad to two characters
            strDay = Mid(time, q+1, p-q-1)
            If Len(strDay) < 2 Then
                strDay = "0" + strDay
            End If
        End If
        
        ' Parse time string to retrieve MM
        ' Extract characters before first slash and zero-pad to two characters
        strMonth = Left(time, q-1)
        If Len(strMonth) < 2 Then
            strMonth = "0" + strMonth
        End If
        
        ' Build final string and set to "00000000" if not already 8 characters
        strFinal = strYear + strMonth + strDay
        If Len(strFinal) <> 8 Then
            strFinal = "00000000"
        End If
    End If
    
    strGetFileTime = strFinal
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Folder from path
' where:
'     Folder( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = "D:\Probate\Input\"
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
' Get associated UEX error filename from specified image file path
' where:
'     ErrorFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       "D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF.uex"
'--------------------------------------------------------------------------------------------------
Function strGetErrorFilePath(strImagePath)
    ' Append the UEX file extension
    strGetErrorFilePath = strImagePath + ".uex"
End Function

'--------------------------------------------------------------------------------------------------
' Get path to VOA file from specified image file path
' where:
'     VOAFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       "D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF.voa"
'--------------------------------------------------------------------------------------------------
Function strGetVOAFilePath(strImagePath)
    ' Append the VOA file extension
    strGetVOAFilePath = strImagePath + ".voa"
End Function

'--------------------------------------------------------------------------------------------------
' Get redacted output filename from specified image file path
' where:
'     RedactedFilePath( D:\Probate\Input\EST239778-1101788-20001218-I0000774.TIF ) = 
'       GStrOutputFolder + "\" + GStrRedactedImagePrefix + 
'       "EST239778-1101788-20001218-I0000774" + GStrRedactedImageSuffix + ".TIF"
'--------------------------------------------------------------------------------------------------
Function strGetRedactedFilePath(strImagePath)
    ' Get the filename without extension
    Dim strFileNameWOExt
    strFileNameWOExt = strGetFileNameWithoutExtension(strImagePath)
    
    ' Get the file extension
    Dim strFileExtension
    strFileExtension = strGetFileExtension(strImagePath)
    
    ' Prepend the output folder and the redacted image prefix
    strGetRedactedFilePath = GStrOutputFolder + "\" + GStrRedactedImagePrefix + strFileNameWOExt + GStrRedactedImageSuffix + strFileExtension
End Function

'--------------------------------------------------------------------------------------------------
' Append redaction information from the specified input file to the output file
' as:
'     filename|processingdate|info
' where:
'     filename       = the filename of the input image
'     processingdate = the modification time of the VOA file
'     info           = 0 if no redactions were made
'                    = 1 if 1 or more redactions were made
'                    = 2 if an error occurred during processing
'--------------------------------------------------------------------------------------------------
Function ProcessRedactionResult(strInputImage,strVOAFileTime,strOutputPath)
    ' Define error handler
    On Error Resume Next

    HandleDebug "Entering ProcessRedactionResult()", strInputImage + ", " + strVOAFileTime + ", " + strOutputPath

	' Begin building the output string
	' Start with input filename + Pipe
	Dim PipeChar, strOutputText
	PipeChar = "|"
	strOutputText = strGetFileNameWithoutExtension(strInputImage) + strGetFileExtension(strInputImage)+ PipeChar
	
	' Add the processing date + Pipe
	strOutputText = strOutputText + strVOAFileTime + PipeChar

	' Use ErrorChar if any error occurred during processing of this input file
	Dim ErrorChar
	ErrorChar = "2"

	' Use RedactionsFoundChar if one or more redactions were applied to this input file
	Dim RedactionsFoundChar
	RedactionsFoundChar = "1"

	' Use RedactionsNotFoundChar if no redactions were applied to this input file
	Dim RedactionsNotFoundChar
	RedactionsNotFoundChar = "0"

    ' Get path to error file generated if an error occurred during processing	
	Dim strErrorFile
	strErrorFile = strGetErrorFilePath(strInputImage)
	
	' Check for existence of this error file
    If fso.FileExists(strErrorFile) Then
        HandleDebug "Testing Error File Existence", "True"
        
        ' Append ErrorChar
    	strOutputText = strOutputText + ErrorChar
    Else
        HandleDebug "Testing Error File Existence", "False"
        
        ' No error, now check for redactions

        ' Get path to redacted output file
	    Dim strRedactedFile
	    strRedactedFile = strGetRedactedFilePath(strInputImage)
        HandleDebug "Name of Redacted File", strRedactedFile
	
	    ' Check for existence of this error file
        If fso.FileExists(strRedactedFile) Then
            HandleDebug "Testing Redacted File Existence", "True"
            
            ' Append RedactionsFoundChar
    	    strOutputText = strOutputText + RedactionsFoundChar
        Else
            HandleDebug "Testing Redacted File Existence", "False"
            
            ' Append RedactionsNotFoundChar
    	    strOutputText = strOutputText + RedactionsNotFoundChar
        End If
    End If
	
	' Append completed string to the output file
    AppendRedactionInfo strOutputText, strOutputPath

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
    HandleScriptError0 "ELI20987", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI20988", "Unable to acquire named Mutex!"

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
            HandleScriptError1 "ELI20989", "Unable to create output folder!", "Folder Name", strOutputFolder
        Else
            HandleDebug "Testing Output Folder Existence", "True"
        End If

        ' Create the output file
        Dim NewFile
        Set NewFile = fso.CreateTextFile(strOutputPath)
        HandleScriptError1 "ELI20990", "Unable to create output file!", "Output File", strOutputPath

        ' Write the first line to the file
        NewFile.WriteLine(strInfoLine)

        ' Close the output file
        NewFile.Close
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for append
        Dim OutFile
        Set OutFile = fso.OpenTextFile(strOutputPath,ForAppending,True)
        HandleScriptError1 "ELI20991", "Unable to open output file for append!", "Output File", strOutputPath

        ' Write the new text to the file
        OutFile.WriteLine(strInfoLine)

        ' Close the output file
        OutFile.Close
    End If

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI20992", "Unable to release named Mutex!"
End Function

