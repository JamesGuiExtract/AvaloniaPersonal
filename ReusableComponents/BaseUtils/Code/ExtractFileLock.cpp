#include "stdafx.h"
#include "ExtractFileLock.h"
#include "cpputil.h"
#include "UCLIDException.h"
#include "StringTokenizer.h"
#include "LicenseUtils.h"

#include <io.h>

#include <memory>
#include <fstream>
using namespace std;

const char* gszLOCK_FILE_EXTENSION = ".ExtractLock";

CMutex ExtractFileLock::ms_Mutex;
bool ExtractFileLock::ms_bCheckedInternal = false;
bool ExtractFileLock::ms_bIsInternal = false;

//-------------------------------------------------------------------------------------------------
// ExtractFileLock
//-------------------------------------------------------------------------------------------------
ExtractFileLock::ExtractFileLock(std::string strFileName, bool bThrowIfLocked, 
	string strContext /*= ""*/)
: m_strFileName(strFileName)
, m_bIsReadOnly(false)
, m_bHaveLock(false)
, m_bIsLockedByAnotherProcess(false)
, m_hLockFileHandle(INVALID_HANDLE_VALUE)
{
	try
	{
		try
		{
			// Since the initial identified need for locking is for internal purposes, for now
			// locking will only be done for software running internally at Extract Systems.
			if (!isInternal())
			{
				setAsValid(true);
				return;
			}

			// NOTE:
			// It is valid that a non-existent file be locked since a process may wish to ensure a
			// file it is about to write not be accessible via another process via a race condition.

			// If a file exists but is read-only, don't attempt to lock the file (whether behavior
			// otherwise existed for read-only files will continue to exist).
			if ((_access_s(strFileName.c_str(), giMODE_FILE_EXISTS) == 0) &&
				(_access_s(strFileName.c_str(), giMODE_WRITE_ONLY) != 0))
			{
				m_bIsReadOnly = true;
				setAsValid(true);
				return;
			}

			m_strLockFileName = strFileName + gszLOCK_FILE_EXTENSION;

			// Before creating the files need to make sure the directory exists
			// Fixes https://extract.atlassian.net/browse/ISSUE-13870
			string strDirectory = getDirectoryFromFullPath(m_strLockFileName);
			if (!directoryExists(strDirectory))
			{
				createDirectory(strDirectory);
			}

			// Creates lock file if it doesn't already exist
			// FILE_SHARE_READ allows other extract processes to read metadata info
			// FILE_SHARE_DELETE must be specified with FILE_FLAG_DELETE_ON_CLOSE for other extract processes to open.
			// FILE_FLAG_DELETE_ON_CLOSE helps ensure the lock file gets deleted when this instance is
			// done with it.
			m_hLockFileHandle = CreateFile(m_strLockFileName.c_str(), GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_DELETE, NULL,
				CREATE_NEW, FILE_ATTRIBUTE_HIDDEN | FILE_FLAG_DELETE_ON_CLOSE | FILE_FLAG_WRITE_THROUGH, 
				NULL);

			// If able to create the lock file
			if (m_hLockFileHandle != INVALID_HANDLE_VALUE)
			{
				writeLockInfo(m_hLockFileHandle, strContext);
				m_bHaveLock = true;
			}
			// If unable to create the lock file
			else
			{
				// Before doing anything else, initialize an exception with the win32 error info
				// that resulted from the attempt to create the lock file (this info may be lost by
				// calling getExternalLockInfo).
				UCLIDException ueLockError("ELI39243", "Failed to lock data file for writing.");
				ueLockError.addWin32ErrorInfo();

				// Find out if another context has the file locked, and if so 
				m_strExternalLockInfo = getExternalLockInfo(m_strLockFileName, &m_bIsLockedByAnotherProcess);

				if (!m_bIsLockedByAnotherProcess)
				{
					throw ueLockError;
				}
				else if (bThrowIfLocked)
				{
					UCLIDException ueLockInfo("ELI39244", "File is being used by another process.");
					addExternalLockInfo(ueLockInfo);
					throw ueLockInfo;
				}
			}

			setAsValid(true);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39245");
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Filename", strFileName, false);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
ExtractFileLock::~ExtractFileLock(void)
{
	if (m_hLockFileHandle != INVALID_HANDLE_VALUE)
	{
		CloseHandle(m_hLockFileHandle);
	}

	setAsValid(false);
}
//-------------------------------------------------------------------------------------------------
bool ExtractFileLock::IsForFile(const std::string& strFileName)
{ 
	return (_strcmpi(strFileName.c_str(), m_strFileName.c_str()) == 0);
}
//-------------------------------------------------------------------------------------------------
string ExtractFileLock::GetExternalLockInfo()
{
	if (m_strExternalLockInfo.empty())
	{
		return "";
	}

	string strExternalLockInfo = Util::Format(
		"The file \"%s\"\r\n"
		"is being used by another process.\r\n\r\n"
		"%s", m_strFileName.c_str(), m_strExternalLockInfo.c_str());

	return strExternalLockInfo;
}
//-------------------------------------------------------------------------------------------------
void ExtractFileLock::addExternalLockInfo(UCLIDException &rue)
{
	if (m_strExternalLockInfo.empty())
	{
		rue.addDebugInfo("Context", "Unknown", false);
		rue.addDebugInfo("User", "Unknown", false);
		rue.addDebugInfo("Computer", "Unknown", false);
		rue.addDebugInfo("Time", "Unknown", false);
	}
	else
	{
		vector<string> vecLines;
		StringTokenizer st('\n');
		st.parse(m_strExternalLockInfo, vecLines);

		for each (const string& strLine in vecLines)
		{
			auto divider = strLine.find(':');
			string key = trim(strLine.substr(0, divider), " \r\n", " \r\n");
			if (!key.empty())
			{
				string value = trim(strLine.substr(divider + 1), " \r\n", " \r\n");
				rue.addDebugInfo(key, value, false);
			}
		}
	}
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool ExtractFileLock::isInternal()
{
	if (!ms_bCheckedInternal)
	{
		CSingleLock lg(&ms_Mutex, TRUE);

		if (!ms_bCheckedInternal)
		{
			ms_bIsInternal = isInternalToolsLicensed();
			ms_bCheckedInternal = true;
		}
	}

	return ms_bIsInternal;
}
//-------------------------------------------------------------------------------------------------
void ExtractFileLock::setAsValid(bool bValid)
{
	if (bValid)
	{
		m_validator.reset(new HANDLE*(&m_hLockFileHandle));
	}
	else
	{
		m_validator.reset();
	}
}
//-------------------------------------------------------------------------------------------------
void ExtractFileLock::writeLockInfo(HANDLE hInfoFileHandle, string strContext)
{
	if (strContext.empty())
	{
		// Use the name of the currently running application as the context of the lock if a
		// context is not already specified.
		strContext = getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
	}

	string strInfoFileData = Util::Format(
		"Context: %s\r\n"
		"User: %s\r\n"
		"Computer: %s\r\n"
		"Time: %s %s",
		strContext.c_str(), getFullUserName(false).c_str(), getComputerName().c_str(),
		getDateAsString().c_str(), getTimeAsString().c_str());

	DWORD dwLen = strInfoFileData.length();
	DWORD dwWritten = 0;
	if (!asCppBool(WriteFile(m_hLockFileHandle, strInfoFileData.c_str(), dwLen, &dwWritten, NULL)))
	{
		throw UCLIDException("ELI39246", "Failed to write lock file");
	}
}
//-------------------------------------------------------------------------------------------------
string ExtractFileLock::getExternalLockInfo(const string& strLockFileName, bool* pbLockExists)
{
	ASSERT_ARGUMENT("ELI39247", pbLockExists != nullptr)

	*pbLockExists = (_access_s(strLockFileName.c_str(), giMODE_FILE_EXISTS) == 0);
	
	if (!*pbLockExists)
	{
		return "";
	}

	string strLockInfo;

	try
	{
		// FILE_SHARE_DELETE must be specified to open. Documentation doesn't mention
		// FILE_SHARE_WRITE, but testing seems to indicate it is necessary as well. Since the original
		// locking process isn't sharing write access, this shouldn't risk allowing another process to
		// write to it.
		HANDLE hFileInfoHandle = CreateFile(strLockFileName.c_str(), GENERIC_READ,
			FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, NULL, OPEN_EXISTING, 0, NULL);
			
		if (hFileInfoHandle != INVALID_HANDLE_VALUE)
		{
			try
			{
				DWORD dwBufferSize = GetFileSize(hFileInfoHandle, NULL);
		
				unique_ptr<char> upCharBuffer(new char[dwBufferSize]);
				ZeroMemory(upCharBuffer.get(), dwBufferSize);

				DWORD dwBytesRead = 0;
				if (ReadFile(hFileInfoHandle, upCharBuffer.get(), dwBufferSize - 1, &dwBytesRead, NULL))
				{
					strLockInfo = string(upCharBuffer.get());
				}

				CloseHandle(hFileInfoHandle);
			}
			catch (...)
			{
				CloseHandle(hFileInfoHandle);
			}

			return strLockInfo;
		}
	}
	// Don't allow any issue reading the lock info (which could come about via race condition with
	// the process that locked the file) to become an issue. Simply report "unknown" lock info.
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI39267")

	return "";
}