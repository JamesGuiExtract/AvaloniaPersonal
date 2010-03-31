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
FileIterator::FileIterator(const string& strPathAndSpec)
: m_strPathAndSpec(strPathAndSpec),
  m_hCurrent(NULL)
{
	memset(&m_findData, 0, sizeof(m_findData));
	m_strSpec = getFileNameFromFullPath(m_strPathAndSpec);
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
				ue.addDebugInfo("Find path", m_strPathAndSpec);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}
		m_hCurrent = NULL;
	}
	memset(&m_findData, 0, sizeof(m_findData));
}
//-------------------------------------------------------------------------------------------------
void FileIterator::reset(const string& strPathAndSpec)
{
	reset();
	m_strPathAndSpec = strPathAndSpec;
	m_strSpec = getFileNameFromFullPath(m_strPathAndSpec);
}
//-------------------------------------------------------------------------------------------------
bool FileIterator::moveNext()
{
	// Loop until either a file is not found or a file that matches the specified pattern
	// is found
	bool bSuccess = false;
	do
	{
		// Check whether iteration has started
		if (m_hCurrent == NULL)
		{
			// Get the first file
			m_hCurrent = FindFirstFile(m_strPathAndSpec.c_str(), &m_findData);
			bSuccess = m_hCurrent != INVALID_HANDLE_VALUE;
		}
		else
		{
			// Get the next file
			bSuccess = asCppBool( FindNextFile(m_hCurrent, &m_findData) );
		}
	}
	while (bSuccess && !doesFileMatchPattern(m_strSpec, m_findData.cFileName));

	// Check for errors
	if (!bSuccess)
	{
		// If no files were found, it is not considered an error condition.
		DWORD dwError = GetLastError();
		if (dwError != ERROR_NO_MORE_FILES && dwError != ERROR_FILE_NOT_FOUND)
		{
			UCLIDException ue("ELI25687", "Unable to get next file.");
			ue.addDebugInfo("Find path", m_strPathAndSpec);
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
//-------------------------------------------------------------------------------------------------