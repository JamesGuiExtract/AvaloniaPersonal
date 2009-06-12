'--------------------------------------------------------------------------------------------------
' Script commands specific for MI - Clinton - US Imaging
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' where:
'     GStrIn = H:\Redaction\InputFile.txt
'     GStrOut = H:\Redaction\OutputFile.txt
Dim GObjArgs, GStrIn, GStrOut
Set GObjArgs = WScript.Arguments
GStrIn = GObjArgs(0)
GStrOut = GObjArgs(1)
Call ParseCommandLineOptions(2)
HandleDebug "Input File From Command-Line", GStrIn
HandleDebug "Output File From Command-Line", GStrOut

' Create File System Object
Dim fso
Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI23445", "Unable to create File System Object!"

' Check for existence of the input file and ensure that it is not the same as the output file
If (fso.FileExists(GStrIn) And StrComp(GStrIn, GStrOut, vbTextCompare) <> 0) Then
    
    HandleDebug "Testing Input File Existence", "True"

    ' Open the input file for read
    Dim InFile, OutFile
    Set InFile = fso.OpenTextFile(GStrIn,ForReading,True)
   
    HandleScriptError1 "ELI23446", "Unable to open input file for reading!", "Input File", GStrIn
    

    ' Open output file for appending

    ' Create named Mutex Object to protect access to the output file
    Dim MutexObject 
    Set MutexObject = CreateObject("UCLIDCOMUtils.COMMutex")
    MutexObject.CreateNamed "Cleanup"
    HandleScriptError0 "ELI23447", "Unable to create COMMutex object!"
    MutexObject.Acquire
    HandleScriptError0 "ELI23448", "Unable to acquire named Mutex!"

    ' Check for existence of the output file

    If Not fso.FileExists(GStrOut) Then
        HandleDebug "Testing Output File Existence", "False"
        ' Get folder
        Dim strOutputFolder
        strOutputFolder = strGetFolder(GStrOut)
        HandleDebug "Output Folder", strOutputFolder

        ' Check for existence of hardcoded folder 
        If Not strOutputFolder = "" Then
            If Not fso.FolderExists(strOutputFolder) Then
                HandleDebug "Testing Output Folder Existence", "False"
                ' Create the folder
                fso.CreateFolder(strOutputFolder)
                HandleScriptError1 "ELI23449", "Unable to create output folder!", "Folder Name", strOutputFolder
            Else
                HandleDebug "Testing Output Folder Existence", "True"
            End If
        End If

        ' Create the output file
        Set OutFile = fso.CreateTextFile(GStrOut)
        HandleScriptError1 "ELI23450", "Unable to create output file!", "Output File", GStrOut
    Else
        HandleDebug "Testing Output File Existence", "True"
        ' Open the output file for writing
        Set OutFile = fso.OpenTextFile(GStrOut,ForWriting,True)
        HandleScriptError1 "ELI23451", "Unable to open output file for write!", "Output File", GStrOut
    End If


    ' Read each line of the file, fix and output if contains redactions
    Dim l
    Do Until InFile.AtEndOfStream
        l = InFile.Readline

        ' If this is an entry describing redactions then fix page count number and output
        If InStr(l, "none") = 0 Then
            OutFile.WriteLine strFixPageCount(l) 
        End If
    Loop

    ' Close the input/output files
    InFile.Close
    OutFile.Close

    ' Release the Mutex
    MutexObject.ReleaseNamedMutex
    HandleScriptError0 "ELI23452", "Unable to release named Mutex!"

End If

'--------------------------------------------------------------------------------------------------
' Decrement page count in line unless it is the same as page number
' where:
'     strLine is something like: 2995147|6|7|1801|1082|229|102
'     page number = 6     
'     page count = 7
'--------------------------------------------------------------------------------------------------

Function strFixPageCount(strLine)
    Dim strPage, strPageCount

    ' Regexes to match page number/count
    Dim rPage, rPageCount
    Set rPage = New RegExp
    Set rPageCount = New RegExp

    ' First capture group will be page number
    rPage.Pattern = "^\d+\|(\d+)\|.*$"
    ' Second capture group will be page count
    rPageCount.Pattern = "(^\d+\|\d+\|)(\d+)(\|.*$)"
    strPage = rPage.Replace(strLine, "$1")
    strPageCount = rPageCount.Replace(strLine, "$2")
    If strPageCount <> strPage Then
        strPageCount = (CStr)((CInt)(strPageCount)-1)
        strFixPageCount = rPageCount.Replace(strLine, "$1"&strPageCount&"$3")
    Else
        strFixPageCount = strLine
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
