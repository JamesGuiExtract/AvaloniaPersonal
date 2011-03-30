
#include "stdafx.h"
#include "TemporaryFileName.h"
#include "UCLIDException.h"
#include "cpputil.h"
#include "MutexUtils.h"

#include <io.h>
#include <fstream>
using namespace std;

//--------------------------------------------------------------------------------------------------
// Static data members
//--------------------------------------------------------------------------------------------------

// String for the named mutex to make file creation globally thread safe
const string gmutMUTEX_NAME = "Global\\260BA215-4090-4172-B696-FC86B52269B4";

//--------------------------------------------------------------------------------------------------
// Public functions
//--------------------------------------------------------------------------------------------------
TemporaryFileName::TemporaryFileName(const char *pszPrefix, 
									 const char *pszSuffix, bool bAutoDelete) :
m_strFileName(""),
m_bAutoDelete(bAutoDelete)
{
	try
	{
		init("", pszPrefix, pszSuffix, true);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20700");
}
//--------------------------------------------------------------------------------------------------
TemporaryFileName::TemporaryFileName(const string& strDir, const char *pszPrefix, 
									 const char *pszSuffix, bool bAutoDelete) :
m_strFileName(""),
m_bAutoDelete(bAutoDelete)
{
	try
	{
		init(strDir, pszPrefix, pszSuffix, true);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20701");
}
//--------------------------------------------------------------------------------------------------
TemporaryFileName::TemporaryFileName(const string& strFileName, bool bAutoDelete/* = true*/) :
m_strFileName(""),
m_bAutoDelete(bAutoDelete)
{
	try
	{
		init(getDirectoryFromFullPath(strFileName), getFileNameWithoutExtension(strFileName).c_str(),
			 getExtensionFromFullPath(strFileName).c_str(), false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30201");
}
//--------------------------------------------------------------------------------------------------
TemporaryFileName::~TemporaryFileName()
{
	try
	{
		// If auto delete, there is a file name, and the file still exists, delete it
		if (m_bAutoDelete && !m_strFileName.empty() && isValidFile(m_strFileName))
		{
			// Attempt to delete the file
			deleteFile(m_strFileName, true);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16407");
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
// moved from constuctor as per [p13 #4951] - JDS 04/09/2008
// along with this change changed from using _tempnam to GetTempFileName
// which allows specification of a directory (and is more secure than _tempnam)
void TemporaryFileName::init(string strDir, const char *pszPrefix, const char* pszSuffix,
							 bool bRandomFileName)
{
	string strSuffix = ".tmp";
	string strPrefix = (pszPrefix != __nullptr ? pszPrefix : "");

	// check for NULL suffix
	if (pszSuffix != __nullptr)
	{
		strSuffix = pszSuffix;
		makeLowerCase(strSuffix);

		// Ensure the suffix starts with a '.'
		if (strSuffix[0] != '.')
		{
			strSuffix.insert(0, ".");
		}
	}

	// if no directory provided then create files in the temp directory
	if (strDir.empty())
	{
		// create buffer to hold the path to the temp directory
		char szDir[BUFSIZ] = {0};

		// get the temp directory
		DWORD dwRetVal = GetTempPath(BUFSIZ, szDir);
		if (dwRetVal == 0 || dwRetVal > BUFSIZ)
		{
			UCLIDException ue("ELI20703", "Unable to determine system TEMP folder!");
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// set the strDir to point to temp directory
		strDir = szDir;
	}
	// If a directory is provided, ensure it ends in a backslash
	else if (strDir[strDir.length()-1] != '\\')
	{
		strDir.append("\\");
	}

	// ensure the directory exists
	validateFileOrFolderExistence(strDir);

	string strFileName = "";

	// lock this section of code while we create and open the file
	// modified as per [p13 #4970] changed CSingleLock(&mutex, TRUE)
	// to CSingleLock lock(&mutex, TRUE) - JDS - 04/30/2008
	// Modified as per [LegacyRCAndUtils #4975] changed to use a global named
	// mutex rather than a static mutex which is only thread safe for a specific
	// process.  This should make this thread safe system wide. - JDS - 10/03/2008
	unique_ptr<CMutex> pMutex;
	pMutex.reset(getGlobalNamedMutex(gmutMUTEX_NAME));
	ASSERT_RESOURCE_ALLOCATION("ELI29992", pMutex.get() != __nullptr);

	CSingleLock lock(pMutex.get(), TRUE);

	if (bRandomFileName)
	{
		do
		{
			// Build a random file name
			strFileName = strDir + strPrefix + m_Rand.getRandomString(8, true, false, true) + strSuffix;
		}
		while (isValidFile(strFileName));
	}
	else
	{
		strFileName = strDir + strPrefix + strSuffix;
	}

	HANDLE hFileHandle = CreateFile(strFileName.c_str(), GENERIC_WRITE | GENERIC_READ,
		0, NULL, CREATE_NEW, FILE_ATTRIBUTE_NORMAL |
		FILE_FLAG_NO_BUFFERING | FILE_FLAG_WRITE_THROUGH, NULL);

	// check for failed file creation
	if (hFileHandle == INVALID_HANDLE_VALUE)
	{
		UCLIDException ue("ELI21032", "Unable to create new temporary file!");
		ue.addDebugInfo("TempFileName", m_strFileName);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// CreateFile was successful, need to close the handle
	CloseHandle(hFileHandle);

	// Set the file name
	m_strFileName = strFileName;

	// ensure file is created before releasing lock
	waitForFileToBeReadable(m_strFileName);
	lock.Unlock();
}
//--------------------------------------------------------------------------------------------------
