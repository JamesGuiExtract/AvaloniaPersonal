'--------------------------------------------------------------------------------------------------
' OH - Secretary of State Utility Subroutines
'--------------------------------------------------------------------------------------------------
' Retrieves various items from a page-level filename
'    * Document Folder
'    * Document Number
'    * File Name
'    * SubFolder Name
'    * Roll Or Din Name
'    * Page Count - searches for subsequent page-level files for the specified document
' 
' Requires:
'    fso - File System Object to test file existence
'    Debug support via functions in Debug_And_Error_Handling.vbs
' 
' Notes:
'    The File System Object is declared in each customer-specific VBS file
' 
'--------------------------------------------------------------------------------------------------
' Retrieve Document Folder from page-level filename
' where:
'     DocFolder( C:\Images\0123-0001.tif ) = "C:\Images"
'--------------------------------------------------------------------------------------------------
Function strGetDocFolder(strFileName)
    ' Define error handler
    On Error Resume Next

    ' Extract folder portion of filename by finding last backslash
    ' where:
    '    "C:\Images\0123-0001.tif" ====> "C:\Images"
    Dim FolderChar, p
    FolderChar = "\"
    p = InStrRev(strFileName,FolderChar)
    If p > 0 Then
        strGetDocFolder = Left(strFileName, p-1)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Document Number from page-level filename
' where:
'     DocNumber( C:\Images\0123-0001.tif ) = "0123"
'--------------------------------------------------------------------------------------------------
Function strGetDocNumber(strFileName)
    ' Define error handler
    On Error Resume Next

    ' Remove folder portion of filename by finding last backslash
    ' where:
    '    "C:\Images\0123-0001.tif" ====> "0123-0001.tif"
    Dim FolderChar, n, p, strFile
    FolderChar = "\"
    p = InStrRev(strFileName,FolderChar)
    If p > 0 Then
        n = Len(strFileName)
        strFile = Right(strFileName, n-p)
        HandleDebug "GetDocNumber::strFile", strFile
    End If

    ' Get document number portion of name by finding first dash
    ' where:
    '    "0123-0001.tif" ====> "0123"
    ' or:
    '    "0123.tif" ====> "0123"
    Dim SearchChar
    SearchChar = "-"
    p = InStr(strFile,SearchChar)
    If p > 0 Then
        HandleDebug "Testing If Doc Number portion of filename contains a dash", "True"
        ' Found dash, return portion before the dash
        strGetDocNumber = Left(strFile, p-1)
    Else
        ' Did not find dash, return portion before the file extension
        HandleDebug "Testing If Doc Number portion of filename contains a dash", "False"
        SearchChar = "."
        p = InStr(strFile,SearchChar)
        If p > 0 Then
            strGetDocNumber = Left(strFile, p-1)
        End If
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Filename from page-level filename
' where:
'     DocFile( C:\Images\0123-0001.tif ) = "0123-0001.tif"
'--------------------------------------------------------------------------------------------------
Function strGetDocFile(strFileName)
    ' Define error handler
    On Error Resume Next

    ' Remove folder portion of filename by finding last backslash
    ' where:
    '    "C:\Images\0123-0001.tif" ====> "0123-0001.tif"
    Dim FolderChar, n, p
    FolderChar = "\"
    p = InStrRev(strFileName,FolderChar)
    If p > 0 Then
        n = Len(strFileName)
        strGetDocFile = Right(strFileName, n-p)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Subfolder name from folder portion of full path
' where:
'     SubfolderName( C:\Images\Roll ) = "Images"
'--------------------------------------------------------------------------------------------------
Function strSubfolderName(strFolder)
    ' Define error handler
    On Error Resume Next

    ' Remove Roll or Din portion of filename by finding last backslash
    ' where:
    '    "C:\Images\Roll" ====> "C:\Images"
    Dim FolderChar, n, p, strParent
    FolderChar = "\"
    p = InStrRev(strFolder,FolderChar)
    If p > 0 Then
        strParent = Left(strFolder, p-1)
        HandleDebug "SubfolderName::strParent", strParent
    End If

    ' Get subfolder portion of name by finding last remaining backslash
    ' where:
    '    "C:\Images" ====> "Images"
    p = InStrRev(strParent,FolderChar)
    If p > 0 Then
        ' Found backslash, return subsequent portion
        n = Len(strParent)
        strSubfolderName = Right(strParent, n-p)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve Rool or Din name from folder portion of full path
' where:
'     strRollOrDinName( C:\Images\Roll ) = "Roll"
'--------------------------------------------------------------------------------------------------
Function strRollOrDinName(strText)
    ' Define error handler
    On Error Resume Next

    ' Remove Roll or Din portion of filename by finding last backslash
    ' where:
    '    "C:\Images\Roll" ====> "Roll"
    Dim FolderChar, n, p
    FolderChar = "\"
    p = InStrRev(strText,FolderChar)
    If p > 0 Then
        ' Found backslash, return subsequent portion
        n = Len(strText)
        strRollOrDinName = Right(strText, n-p)
    End If
End Function

'--------------------------------------------------------------------------------------------------
' Retrieve integer Page Count from page-level filename by checking file existence for 
' subsequent pages
'--------------------------------------------------------------------------------------------------
Function nGetPageCount(strFileName)
    ' Define error handler
    On Error Resume Next

    ' Get entire path before Page Number
    '   i.e. "C:\Images\0123-0001.tif" ====> "C:\Images\0123-"
    Dim SearchChar, p, strBegin
    SearchChar = "-"
    p = InStrRev(strFileName,SearchChar)   ' find last dash
    If p > 0 Then
        strBegin = Left(strFileName, p)
        HandleDebug "GetPageCount::strBegin", strBegin
    End If
    
    ' Get entire path after Page Number
    '   i.e. "C:\Images\0123-0001.tif" ====> ".tif"
    Dim n, strEnd
    SearchChar = "."
    p = InStr(strFileName,SearchChar)   ' find first period
    If p > 0 Then
        n = Len(strFileName)
        strEnd = Right(strFileName, n-p+1)
        HandleDebug "GetPageCount::strEnd", strEnd
    End If
    
    ' At least one page is available
    Dim nLastPageFound
    nLastPageFound = 1
    
    ' Check file existence for subsequent pages
    Do
        ' Build new page number by zero-padding to 4 characters
        Dim nNewPage, strTempNewPage, nLen, strNewPage
        nNewPage = nLastPageFound + 1
        strTempNewPage = CStr(nNewPage)
        nLen = Len(strTempNewPage)
        strNewPage = String(4 - nLen, "0") + strTempNewPage
        HandleDebug "GetPageCount::strNewPage", strNewPage

        ' Build file name for test as:
        ' strBegin + strNewPage + strEnd
        Dim strTest
        strTest = strBegin + strNewPage + strEnd
        HandleDebug "GetPageCount::strTest", strTest

        ' Test file existence
        If fso.FileExists(strTest) Then
            ' Found the file, increment nLastPageFound
            HandleDebug "GetPageCount() - strTest file found", "True"
            nLastPageFound = nNewPage
        Else
            ' File not found, exit from loop
            HandleDebug "GetPageCount() - strTest file found", "False"
            Exit Do
        End If
    Loop

    ' Return last page found
    nGetPageCount = nLastPageFound
End Function

