'--------------------------------------------------------------------------------------------------
' Script commands specific for CA - Kings
'--------------------------------------------------------------------------------------------------
' Retrieve and parse command-line arguments
' where:
'     strPdfPath = Path of output file from processing, e.g. \\server\path\abc.pdf
'     numTifPathIndex = The number of the OriginalImage token in CSV file, e.g. 1
'     numDocNumIndex = The number of the DocNumber token in the CSV file, e.g. 2
'
' Script will rename file strPdfPath to <DocNumber of OriginalImage>.pdf
'    E.g., \\server\path\abc.pdf -> \\server\path\123.pdf
'       Where 123 is the document number of abc.tif
'--------------------------------------------------------------------------------------------------
Dim GObjArgs, fso, strPdfPath, strTifPath, reExtension, numTifPathIndex, numDocNumIndex, strFolder, strRecDoc
Set GObjArgs = WScript.Arguments

strPdfPath = GObjArgs(0)
numTifPathIndex = CInt(GObjArgs(1))
numDocNumIndex = CInt(GObjArgs(2))
HandleScriptError0 "ELI24251", "Unable to get args from command-line"

strRecDoc = "RecDoc.txt"
Call ParseCommandLineOptions(3)
HandleDebug "Output Image From Command-Line", strPdfPath
HandleDebug "CSV Index Of Original Image From Command-Line", CStr(numTifPathIndex)
HandleDebug "CSV Index Of Document Number From Command-Line", CStr(numDocNumIndex)

Set fso = CreateObject("Scripting.FileSystemObject")
HandleScriptError0 "ELI24252", "Unable to create File System Object!"

' Get the absolute name for the output file
strPdfPath = fso.GetAbsolutePathName(GObjArgs(0))
HandleScriptError1 "ELI24253", "Unable to get full path to output file!", "Path", strPdfPath

' Create regex to change file extension to 'tif'
Set reExtension = New RegExp
reExtension.Pattern = "\.pdf$"
reExtension.IgnoreCase = True

' Get path of original image
strTifPath = reExtension.Replace(strPdfPath, ".tif")
HandleDebug "Path of original image", strTifPath

' Get path to the text document that contains the image name to document number mapping
strFolder = strGetFolder(strPdfPath)
strRecDoc = strFolder & "\" & strRecDoc
HandleDebug "Path of RecDoc.txt", strRecDoc

Dim fileRecDoc, arrTokens, numReqSize, strNewPdfName

If numTifPathIndex > numDocNumIndex Then
  numReqSize = numTifPathIndex
Else
  numReqSize = numDocNumIndex
End If

' Open the text doc mapping file
Set fileRecDoc = fso.OpenTextFile(strRecDoc)
HandleScriptError1 "ELI24254", "Unable to open file for reading!", "Path", strRecDoc

' Get the basename for the original image to match with RecDoc.txt line
Dim strTifBaseName
strTifBaseName = strGetBaseName(strTifPath)

' Assemble the new name for the output file from the mapping
Do Until fileRecDoc.AtEndOfStream
  arrTokens = Split(fileRecDoc.readline, ",")

  ' Require enough tokens
  If UBound(arrTokens)+1 >= numReqSize Then

    Dim strImageToken
    strImageToken = Replace(arrTokens(numTifPathIndex-1), Chr(34), "")

    ' Compare the basename of the image name token to strTifBaseName
    If StrComp(strGetBaseName(strImageToken), strTifBaseName, vbTextCompare) = 0 Then

      ' Construct the file name from the doc number
      strNewPdfName = strFolder & "\" & Replace(arrTokens(numDocNumIndex-1), Chr(34), "") & ".pdf"
      Exit Do

    End If
  End If
Loop
fileRecDoc.close

Dim objFile
Set objFile = fso.GetFile(strPdfPath)
HandleScriptError1 "ELI24255", "Output file doesn't exist!", "Path", strPdfPath

HandleDebug "File to be renamed", strPdfPath
HandleDebug "New name", strNewPdfName

' Rename the file
If Not isEmpty(objFile) Then
  objFile.Move(strNewPdfName)
  HandleScriptError2 "ELI24256", "Unable to rename output file!", "Path", strPdfPath, "NewPath", strNewPdfName
End If
