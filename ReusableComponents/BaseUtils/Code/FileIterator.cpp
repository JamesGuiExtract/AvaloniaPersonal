#include "stdafx.h"
#include "FileIterator.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const ULONGLONG gullHIGH_DWORD = (ULONGLONG)(MAXDWORD) + 1;

//-------------------------------------------------------------------------------------------------
// Constructor/Destructor
//-------------------------------------------------------------------------------------------------
FileIterator::FileIterator(const string& strPath)
: m_strPath(strPath),
  m_hCurrent(NULL)
{
	memset(&m_findData, 0, sizeof(m_findData));
}
//-------------------------------------------------------------------------------------------------
FileIterator::~FileIterator()
{
	try
	{
		reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25685")
}

//-------------------------------------------------------------------------------------------------
// Methods
//-------------------------------------------------------------------------------------------------
void FileIterator::reset()
{
	if (m_hCurrent != NULL)
	{
		if (m_hCurrent != INVALID_HANDLE_VALUE)
		{
			BOOL bSuccess = FindClose(m_hCurrent);
			if (bSuccess == FALSE)
			{
				UCLIDException ue("ELI25686", "Unable to close file.");
				ue.addDebugInfo("File name", m_findData.cFileName);
				ue.addDebugInfo("Find path", m_strPath);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}
		m_hCurrent = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
void FileIterator::reset(const string& strPath)
{
	reset();
	m_strPath = strPath;
}
//-------------------------------------------------------------------------------------------------
bool FileIterator::moveNext()
{
	bool bFirstTime = m_hCurrent == NULL;
	bool bSuccess = false;

	// Check whether iteration has started
	if (bFirstTime)
	{
		// Get the first file
		m_hCurrent = FindFirstFile(m_strPath.c_str(), &m_findData);
		bSuccess = m_hCurrent != INVALID_HANDLE_VALUE;
	}
	else
	{
		// Get the next file
		bSuccess = asCppBool( FindNextFile(m_hCurrent, &m_findData) );
	}

	// Check for errors
	if (!bSuccess)
	{
		// If no files were found, it is not considered an error condition.
		DWORD dwError = GetLastError();
		DWORD dwNotError = bFirstTime ? ERROR_FILE_NOT_FOUND : ERROR_NO_MORE_FILES;
		if (dwError != dwNotError)
		{
			UCLIDException ue("ELI25687", "Unable to get next file.");
			ue.addDebugInfo("Find path", m_strPath);
			ue.addWin32ErrorInfo(dwError);
			throw ue;
		}
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
string FileIterator::getFileName()
{
	return m_findData.cFileName;
}
//-------------------------------------------------------------------------------------------------
ULONGLONG FileIterator::getFileSize()
{
	return m_findData.nFileSizeHigh * gullHIGH_DWORD + m_findData.nFileSizeLow;
}
//-------------------------------------------------------------------------------------------------
bool FileIterator::isDirectory()
{
	return (m_findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0;
}
//-------------------------------------------------------------------------------------------------
bool FileIterator::isSystemFile()
{
	return (m_findData.dwFileAttributes & FILE_ATTRIBUTE_SYSTEM) != 0;
}