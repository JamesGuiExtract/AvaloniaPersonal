// THIS CODE WAS DOWNLOADED BY ARVIND ON 11/07/2003 FROM
// http://www.codeproject.com/dialog/XBrowseForFolder.asp

// XBrowseForFolder.h  Version 1.0
//
// Author:  Hans Dietrich
//          hdietrich2@hotmail.com
//
// This software is released into the public domain.  You are free to use it 
// in any way you like.
//
// This software is provided "as is" with no expressed or implied warranty.  
// I accept no liability for any damage or loss of business that this software 
// may cause.
//
///////////////////////////////////////////////////////////////////////////////

#pragma once

#include "BaseUtils.h"

EXPORT_BaseUtils BOOL XBrowseForFolder(HWND hWnd,
									   LPCTSTR lpszInitialFolder,
									   LPTSTR lpszBuf,
									   DWORD dwBufSize);
