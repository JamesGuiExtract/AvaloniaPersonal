
#include "stdafx.h"
#include "TemporaryFileName.h"
#include "UCLIDException.h"
#include "cpputil.h"

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
		init("", pszPrefix, pszSuffix);
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
		init(strDir, pszPrefix, pszSuffix);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20701");
}
//--------------------------------------------------------------------------------------------------
TemporaryFileName::~TemporaryFileName()
{
	try
	{
		if (m_bAutoDelete && !m_strFileName.empty())
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
void TemporaryFileName::init(string strDir, const char *pszPrefix, const char* pszSuffix)
{
	string strSuffix = "";

	// check for NULL suffix
	if (pszSuffix != NULL)
	{
		strSuffix.assign(pszSuffix);
		makeLowerCase(strSuffix);

		// if suffix is .tmp, then just ignore it (GetTempFile adds .tmp automatically)
		if (strSuffix == ".tmp")
		{
			strSuffix = "";
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

	// ensure the directory exists
	validateFileOrFolderExistence(strDir);

	// also check to be sure the path is not too long for GetTempFileName
	// path must not be longer than MAX_PATH - 14
	// see: http://msdn2.microsoft.com/en-us/library/aa364991.aspx
	if (strDir.length() > (MAX_PATH - 14))
	{
		UCLIDException ue("ELI20705", "Path for temporary file is too large!");
		ue.addDebugInfo("Path", strDir);
		ue.addDebugInfo("PathLength", strDir.length());
		ue.addDebugInfo("MaxPathLength", MAX_PATH - 14);
		throw ue;
	}

	// lock this section of code while we create and open the file
	// modified as per [p13 #4970] changed CSingleLock(&mutex, TRUE)
	// to CSingleLock lock(&mutex, TRUE) - JDS - 04/30/2008
	// Modified as per [LegacyRCAndUtils #4975] changed to use a global named
	// mutex rather than a static mutex which is only thread safe for a specific
	// process.  This should make this thread safe system wide. - JDS - 10/03/2008
	CMutex mutex(FALSE, gmutMUTEX_NAME.c_str());
	CSingleLock lock(&mutex, TRUE);

	// if strSuffix is empty then any temp file will do.  set uiUnique to
	// 0 which causes GetTempFileName to generate a unique file name
	// and create the file to ensure the uniqueness
	if (strSuffix.empty())
	{
		char szTemp[BUFSIZ] = {0};

		// call GetTempFileName with uUinque = 0 so function will generate
		// unique file and create it to ensure uniqueness, if user
		// does not have write permissions to strDir then GetTempFileName
		// will fail and set GetLastError().  
		UINT uRetVal = GetTempFileName(strDir.c_str(), pszPrefix, 0, szTemp);
		if (uRetVal == 0)
		{
			UCLIDException ue("ELI20704", "Unable to create temporary file!");
			ue.addDebugInfo("Directory", strDir);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		m_strFileName = szTemp;
	}
	// strSuffix is non-empty, need to generate a unique file name ending in
	// strSuffix
	else
	{
		// set uiUnique to 0 since it will be incremented as first step in the loop
		// TODO: 5/12/07 SNK There seems to be threading issues with the function.
		// See [LegacyRCAndUtils:4975] 
		UINT uiUnique = 0;
		do
		{
			// increment the unique file number (will generate a temp file name
			// ending in uiUnique.tmp, it will not check for file existence,
			// nor will it create the file, we must do this manually)
			uiUnique++;

			// create buffer to hold file name
			char szTemp[BUFSIZ] = {0};

			// attempt to create temporary file
			UINT uRetVal = GetTempFileName(strDir.c_str(), pszPrefix, uiUnique, szTemp);
			if (uRetVal == 0)
			{
				UCLIDException ue("ELI20706", "Unable to create temporary file!");
				ue.addDebugInfo("Directory", strDir);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			// assign temp file name
			m_strFileName = szTemp;

			// add the suffix before checking for file existence
			m_strFileName += strSuffix;

		}
		while (isValidFile(m_strFileName));

		HANDLE hFileHandle = CreateFile(m_strFileName.c_str(), GENERIC_WRITE | GENERIC_READ,
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

		// ensure file is created before releasing lock
		waitForFileToBeReadable(m_strFileName);
	}
}
//--------------------------------------------------------------------------------------------------
