// NOTE: This code was downloaded from:
// http://www.codeproject.com/cpp/zipunzip.asp

// Zipper.cpp: implementation of the CZipper class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "Zipper.h"
#include "filemisc.h"
#include "UCLIDException.h"

#include <zip.h>
#include <iowin32.h>

#include <stdlib.h>

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////
const UINT BUFFERSIZE = 2048;

CZipper::CZipper(LPCTSTR szFilePath, LPCTSTR szRootFolder, bool bAppend, bool bCompress) :
m_uzFile(0),
m_bCompress(bCompress)
{
	CloseZip();

	if (szFilePath)
		OpenZip(szFilePath, szRootFolder, bAppend);
}

CZipper::~CZipper()
{
	try
	{
		CloseZip();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16550");
}

bool CZipper::CloseZip()
{
	int nRet = m_uzFile ? zipClose(m_uzFile, NULL) : ZIP_OK;

	m_uzFile = NULL;
	m_szRootFolder[0] = 0;
	ZeroMemory(&m_info, sizeof(m_info));

	return (nRet == ZIP_OK);
}

void CZipper::GetFileInfo(Z_FileInfo& info)
{
	info = m_info;
}

// simple interface
bool CZipper::ZipFile(LPCTSTR szFilePath)
{
	// make zip path
	char szDrive[_MAX_DRIVE] = {0};
	char szFolder[MAX_PATH] = {0};
	char szName[_MAX_FNAME] = {0};
	_splitpath(szFilePath, szDrive, szFolder, szName, NULL);

	char szZipPath[MAX_PATH] = {0};
	_makepath(szZipPath, szDrive, szFolder, szName, "zip");

	CZipper zip;

	if (zip.OpenZip(szZipPath, false))
		return zip.AddFileToZip(szFilePath, false);

	return FALSE;
}
	
bool CZipper::ZipFolder(LPCTSTR szFilePath, bool bIgnoreFilePath)
{
	// make zip path
	char szDrive[_MAX_DRIVE] = {0};
	char szFolder[MAX_PATH] = {0};
	char szName[_MAX_FNAME] = {0};
	_splitpath(szFilePath, szDrive, szFolder, szName, NULL);

	char szZipPath[MAX_PATH] = {0};
	_makepath(szZipPath, szDrive, szFolder, szName, "zip");

	CZipper zip;

	if (zip.OpenZip(szZipPath, FALSE))
		return zip.AddFolderToZip(szFilePath, bIgnoreFilePath);

	return FALSE;
}
	
// works with prior opened zip
bool CZipper::AddFileToZip(LPCTSTR szFilePath, bool bIgnoreFilePath)
{
	if (!m_uzFile)
		return FALSE;

	// we don't allow paths beginning with '..\' because this would be outside
	// the root folder
	if (!bIgnoreFilePath && strstr(szFilePath, "..\\") == szFilePath)
		return false;

	bool bFullPath = (strchr(szFilePath, ':') != NULL);

	// if the file is relative then we need to append the root before opening
	char szFullFilePath[MAX_PATH] = {0};
	
	lstrcpy(szFullFilePath, szFilePath);
	PrepareSourcePath(szFullFilePath);

	// if the file is a fullpath then remove the root path bit
	char szFileName[MAX_PATH] = "";

	if (bIgnoreFilePath)
	{
		char szName[_MAX_FNAME] = {0};
		char szExt[_MAX_EXT] = {0};
		_splitpath(szFilePath, NULL, NULL, szName, szExt);

		_makepath(szFileName, NULL, NULL, szName, szExt);
	}
	else if (bFullPath)
	{
		// check the root can be found
		if (0 != _strnicmp(szFilePath, m_szRootFolder, lstrlen(m_szRootFolder)))
			return false;

		// else
		lstrcpy(szFileName, szFilePath + lstrlen(m_szRootFolder));
	}
	else // relative path
	{
		// if the path begins with '.\' then remove it
		if (strstr(szFilePath, ".\\") == szFilePath)
			lstrcpy(szFileName, szFilePath + 2);
		else
			lstrcpy(szFileName, szFilePath);
	}

	// save file attributes
	zip_fileinfo zfi;

	zfi.internal_fa = 0;
	zfi.external_fa = ::GetFileAttributes(szFilePath);
	
	// save file time
	SYSTEMTIME st;

	GetLastModified(szFullFilePath, st, TRUE);

	zfi.dosDate = 0;
	zfi.tmz_date.tm_year = st.wYear;
	zfi.tmz_date.tm_mon = st.wMonth - 1;
	zfi.tmz_date.tm_mday = st.wDay;
	zfi.tmz_date.tm_hour = st.wHour;
	zfi.tmz_date.tm_min = st.wMinute;
	zfi.tmz_date.tm_sec = st.wSecond;
	
	// load input file
	HANDLE hInputFile = ::CreateFile(szFullFilePath, 
									GENERIC_READ,
									0,
									NULL,
									OPEN_EXISTING,
									FILE_ATTRIBUTE_READONLY,
									NULL);

	if (hInputFile == INVALID_HANDLE_VALUE)
		return FALSE;

	int nRet = zipOpenNewFileInZip(m_uzFile, 
									szFileName,
									&zfi, 
									NULL, 
									0,
									NULL,
									0, 
									NULL,
									Z_DEFLATED,
									m_bCompress ? Z_DEFAULT_COMPRESSION : Z_NO_COMPRESSION);

	if (nRet == ZIP_OK)
	{
		m_info.nFileCount++;

		// read the file and output to zip
		char pBuffer[BUFFERSIZE];
		DWORD dwBytesRead = 0, dwFileSize = 0;

		while (nRet == ZIP_OK && ::ReadFile(hInputFile, pBuffer, BUFFERSIZE, &dwBytesRead, NULL))
		{
			dwFileSize += dwBytesRead;

			if (dwBytesRead)
				nRet = zipWriteInFileInZip(m_uzFile, pBuffer, dwBytesRead);
			else
				break;
		}

		m_info.dwUncompressedSize += dwFileSize;
	}

	zipCloseFileInZip(m_uzFile);
	::CloseHandle(hInputFile);

	return (nRet == ZIP_OK);
}

bool CZipper::AddFileToZip(LPCTSTR szFilePath, LPCTSTR szRelFolderPath)
{
	if (!m_uzFile)
		return FALSE;

	// szRelFolderPath cannot contain drive info
	if (szRelFolderPath && strchr(szRelFolderPath, ':'))
		return FALSE;

	// if the file is relative then we need to append the root before opening
	char szFullFilePath[MAX_PATH] = {0};
	
	lstrcpy(szFullFilePath, szFilePath);
	PrepareSourcePath(szFullFilePath);

	// save file attributes and time
	zip_fileinfo zfi;

	zfi.internal_fa = 0;
	zfi.external_fa = ::GetFileAttributes(szFilePath);
	
	// save file time
	SYSTEMTIME st;

	GetLastModified(szFullFilePath, st, TRUE);

	zfi.dosDate = 0;
	zfi.tmz_date.tm_year = st.wYear;
	zfi.tmz_date.tm_mon = st.wMonth - 1;
	zfi.tmz_date.tm_mday = st.wDay;
	zfi.tmz_date.tm_hour = st.wHour;
	zfi.tmz_date.tm_min = st.wMinute;
	zfi.tmz_date.tm_sec = st.wSecond;

	// load input file
	HANDLE hInputFile = ::CreateFile(szFullFilePath, 
									GENERIC_READ,
									0,
									NULL,
									OPEN_EXISTING,
									FILE_ATTRIBUTE_READONLY,
									NULL);

	if (hInputFile == INVALID_HANDLE_VALUE)
		return FALSE;

	// strip drive info off filepath
	char szName[_MAX_FNAME] = {0};
	char szExt[_MAX_EXT] = {0};
	_splitpath(szFilePath, NULL, NULL, szName, szExt);

	// prepend new folder path 
	char szFileName[MAX_PATH] = {0};
	_makepath(szFileName, NULL, szRelFolderPath, szName, szExt);

	// open the file in the zip making sure we remove any leading '\'
	int nRet = zipOpenNewFileInZip(m_uzFile, 
									szFileName + (szFileName[0] == '\\' ? 1 : 0),
									&zfi, 
									NULL, 
									0,
									NULL,
									0, 
									NULL,
									Z_DEFLATED,
									m_bCompress ? Z_DEFAULT_COMPRESSION : Z_NO_COMPRESSION);

	if (nRet == ZIP_OK)
	{
		m_info.nFileCount++;

		// read the file and output to zip
		char pBuffer[BUFFERSIZE];
		DWORD dwBytesRead = 0, dwFileSize = 0;

		while (nRet == ZIP_OK && ::ReadFile(hInputFile, pBuffer, BUFFERSIZE, &dwBytesRead, NULL))
		{
			dwFileSize += dwBytesRead;

			if (dwBytesRead)
				nRet = zipWriteInFileInZip(m_uzFile, pBuffer, dwBytesRead);
			else
				break;
		}

		m_info.dwUncompressedSize += dwFileSize;
	}

	zipCloseFileInZip(m_uzFile);
	::CloseHandle(hInputFile);

	return (nRet == ZIP_OK);
}

// This method is a simplified version of the above, AddFileToZip(LPCTSTR szFilePath, bool bIgnoreFilePath),
// that writes the file to the supplied path inside the zip instead of building a path based on the source file name
bool CZipper::AddFileToPathInZip(LPCTSTR szFilePath, LPCTSTR szPathInZip)
{
	if (!m_uzFile)
		return FALSE;

	// szPathInZip cannot contain drive info
	if (strchr(szPathInZip, ':'))
		return FALSE;

	// if the file is relative then we need to append the root before opening
	char szFullFilePath[MAX_PATH] = {0};
	
	lstrcpy(szFullFilePath, szFilePath);
	PrepareSourcePath(szFullFilePath);

	// save file attributes and time
	zip_fileinfo zfi;

	zfi.internal_fa = 0;
	zfi.external_fa = ::GetFileAttributes(szFilePath);
	
	// save file time
	SYSTEMTIME st;

	GetLastModified(szFullFilePath, st, TRUE);

	zfi.dosDate = 0;
	zfi.tmz_date.tm_year = st.wYear;
	zfi.tmz_date.tm_mon = st.wMonth - 1;
	zfi.tmz_date.tm_mday = st.wDay;
	zfi.tmz_date.tm_hour = st.wHour;
	zfi.tmz_date.tm_min = st.wMinute;
	zfi.tmz_date.tm_sec = st.wSecond;

	// load input file
	HANDLE hInputFile = ::CreateFile(szFullFilePath, 
									GENERIC_READ,
									0,
									NULL,
									OPEN_EXISTING,
									FILE_ATTRIBUTE_READONLY,
									NULL);

	if (hInputFile == INVALID_HANDLE_VALUE)
		return FALSE;

	// open the file in the zip making sure we remove any leading '\'
	int nRet = zipOpenNewFileInZip(m_uzFile, 
									szPathInZip,
									&zfi, 
									NULL, 
									0,
									NULL,
									0, 
									NULL,
									Z_DEFLATED,
									m_bCompress ? Z_DEFAULT_COMPRESSION : Z_NO_COMPRESSION);

	if (nRet == ZIP_OK)
	{
		m_info.nFileCount++;

		// read the file and output to zip
		char pBuffer[BUFFERSIZE];
		DWORD dwBytesRead = 0, dwFileSize = 0;

		while (nRet == ZIP_OK && ::ReadFile(hInputFile, pBuffer, BUFFERSIZE, &dwBytesRead, NULL))
		{
			dwFileSize += dwBytesRead;

			if (dwBytesRead)
				nRet = zipWriteInFileInZip(m_uzFile, pBuffer, dwBytesRead);
			else
				break;
		}

		m_info.dwUncompressedSize += dwFileSize;
	}

	zipCloseFileInZip(m_uzFile);
	::CloseHandle(hInputFile);

	return (nRet == ZIP_OK);
}


bool CZipper::AddFolderToZip(LPCTSTR szFolderPath, bool bIgnoreFilePath)
{
	if (!m_uzFile)
		return FALSE;

	m_info.nFolderCount++;

	// if the path is relative then we need to append the root before opening
	char szFullPath[MAX_PATH] = {0};
	
	lstrcpy(szFullPath, szFolderPath);
	PrepareSourcePath(szFullPath);

	// always add folder first
	// save file attributes
	zip_fileinfo zfi;
	
	zfi.internal_fa = 0;
	zfi.external_fa = ::GetFileAttributes(szFullPath);
	
	SYSTEMTIME st;
	
	GetLastModified(szFullPath, st, TRUE);
	
	zfi.dosDate = 0;
	zfi.tmz_date.tm_year = st.wYear;
	zfi.tmz_date.tm_mon = st.wMonth - 1;
	zfi.tmz_date.tm_mday = st.wDay;
	zfi.tmz_date.tm_hour = st.wHour;
	zfi.tmz_date.tm_min = st.wMinute;
	zfi.tmz_date.tm_sec = st.wSecond;
	
	// if the folder is a fullpath then remove the root path bit
	char szFolderName[MAX_PATH] = "";
	
	if (bIgnoreFilePath)
	{
		_splitpath(szFullPath, NULL, NULL, szFolderName, NULL);
	}
	else
	{
		// check the root can be found
		if (0 != _strnicmp(szFullPath, m_szRootFolder, lstrlen(m_szRootFolder)))
			return false;
		
		// else
		lstrcpy(szFolderName, szFullPath + lstrlen(m_szRootFolder));
	}
	
	// folders are denoted by a trailing '\\'
	lstrcat(szFolderName, "\\");
	
	// open the file in the zip making sure we remove any leading '\'
	int nRet = zipOpenNewFileInZip(m_uzFile, 
		szFolderName,
		&zfi, 
		NULL, 
		0,
		NULL,
		0, 
		NULL,
		Z_DEFLATED,
		m_bCompress ? Z_DEFAULT_COMPRESSION : Z_NO_COMPRESSION);
	
	zipCloseFileInZip(m_uzFile);

	// build searchspec
	char szDrive[_MAX_DRIVE] = {0};
	char szFolder[MAX_PATH] = {0};
	char szName[_MAX_FNAME] = {0};
	_splitpath(szFullPath, szDrive, szFolder, szName, NULL);
	lstrcat(szFolder, szName);

	char szSearchSpec[MAX_PATH] = {0};
	_makepath(szSearchSpec, szDrive, szFolder, "*", "*");

	WIN32_FIND_DATA finfo;
	HANDLE hSearch = FindFirstFile(szSearchSpec, &finfo);

	if (hSearch != INVALID_HANDLE_VALUE) 
	{
		do 
		{
			if (finfo.cFileName[0] != '.') 
			{
				char szItem[MAX_PATH] = {0};
				_makepath(szItem, szDrive, szFolder, finfo.cFileName, NULL);
				
				if (finfo.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
				{
					AddFolderToZip(szItem, bIgnoreFilePath);
				}
				else 
					AddFileToZip(szItem, bIgnoreFilePath);
			}
		} 
		while (FindNextFile(hSearch, &finfo));
		
		FindClose(hSearch);
	}

	return TRUE;
}

// extended interface
bool CZipper::OpenZip(LPCTSTR szFilePath, LPCTSTR szRootFolder, bool bAppend)
{
	CloseZip();

	if (!szFilePath || !lstrlen(szFilePath))
		return false;

	// convert szFilePath to fully qualified path 
	char szFullPath[MAX_PATH] = {0};

	if (!GetFullPathName(szFilePath, MAX_PATH, szFullPath, NULL))
		return false;

	// zipOpen will fail if bAppend is TRUE and zip does not exist
	if (bAppend && ::GetFileAttributes(szFullPath) == 0xffffffff)
		bAppend = false;

	m_uzFile = zipOpen(szFullPath, bAppend ? APPEND_STATUS_ADDINZIP : APPEND_STATUS_CREATE);

	if (m_uzFile)
	{
		if (!szRootFolder)
		{
			char szDrive[_MAX_DRIVE] = {0};
			char szFolder[MAX_PATH] = {0};
			_splitpath(szFullPath, szDrive, szFolder, NULL, NULL);

			_makepath(m_szRootFolder, szDrive, szFolder, NULL, NULL);
		}
		else if (lstrlen(szRootFolder))
		{
			_makepath(m_szRootFolder, NULL, szRootFolder, "", NULL);
		}
	}

	return (m_uzFile != NULL);
}

void CZipper::PrepareSourcePath(LPTSTR szPath)
{
	bool bFullPath = (strchr(szPath, ':') != NULL);

	// if the file is relative then we need to append the root before opening
	if (!bFullPath)
	{
		char szTemp[MAX_PATH] = {0};
		lstrcpy(szTemp, szPath);

		_makepath(szPath, NULL, m_szRootFolder, szTemp, NULL);
	}
}
