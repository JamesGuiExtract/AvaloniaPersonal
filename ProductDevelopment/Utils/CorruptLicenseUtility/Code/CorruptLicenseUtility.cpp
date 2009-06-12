// CorruptLicenseUtility.cpp : Defines the entry point for the application.
//

#include "stdafx.h"

#include <shlobj.h>
#include <io.h>
#include <string>

// This path is also defined in COMLMCore\Code\TimeRollbackPreventer.cpp
const char gpszDateTimeSubfolderFile[] = "\\Windows\\tlsuuw_DO_NOT_DELETE.dll";

//-------------------------------------------------------------------------------------------------
std::string getDateTimeFilePath()
{
	std::string	strDTFile;

	// Get path to special user-specific folder
	char pszDir[MAX_PATH];
	if (SUCCEEDED (SHGetSpecialFolderPath( NULL, pszDir, CSIDL_APPDATA, 0 ))) 
	{
		// Add path and filename for DT file
		strDTFile = std::string( pszDir ) + std::string( gpszDateTimeSubfolderFile );
	}

	// Return results
	return strDTFile;
}
//-------------------------------------------------------------------------------------------------
int APIENTRY WinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPSTR     lpCmdLine,
                     int       nCmdShow)
{
 	// Get path to local TRP file
	std::string strFile = getDateTimeFilePath();
	if (strFile.length() == 0)
	{
		MessageBox( NULL, "Unable to determine path to file", "Error", MB_OK );
	}
	else
	{
		// Check for file presence
		if (_access( strFile.c_str(), 00 ) != 0)
		{
			MessageBox( NULL, "File not found, but operation succeeded!", "Success", MB_OK );
			return 0;
		}

		// Remove Hidden attribute
		if (!SetFileAttributes( strFile.c_str(), FILE_ATTRIBUTE_NORMAL ))
		{
			// Removing Hidden attribute failed
			MessageBox( NULL, "Unable to modify file attribute, operation failed!", "Error", MB_OK );
			return 0;
		}

		// Test for retained Hidden attribute
		DWORD dwAttr = GetFileAttributes( strFile.c_str() );
		if (dwAttr != FILE_ATTRIBUTE_NORMAL)
		{
			MessageBox( NULL, "File attribute unchanged, operation failed!", "Error", MB_OK );
			return 0;
		}

		MessageBox( NULL, "Operation succeeded!", "Success", MB_OK );
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
