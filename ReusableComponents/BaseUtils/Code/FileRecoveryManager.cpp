#include "stdafx.h"
#include "FileRecoveryManager.h"
#include "cpputil.h"
#include "FileIterator.h"
#include "UCLIDException.h"
#include "Win32Util.h"

#include <tlhelp32.h>
#include <io.h>

//-------------------------------------------------------------------------------------------------
FileRecoveryManager::FileRecoveryManager(const string& strSuffix, 
										 const string& strPrefix) :
	m_strRecoveryFileSuffix(strSuffix),
	m_strRecoveryFilePrefix(strPrefix),
	m_strRecoveryFileName("")
{
	// Get the user app data folder
	getSpecialFolderPath(CSIDL_LOCAL_APPDATA, m_strRecoveryFileFolder);
	m_strRecoveryFileFolder += "\\Extract Systems\\RecoveryFiles\\";

	// get this EXE's full path
	char pszThisModuleFullPath[MAX_PATH + 1] = {0};
	GetModuleFileName(NULL, pszThisModuleFullPath, 
		sizeof(pszThisModuleFullPath));
	m_strThisEXEName = getFileNameFromFullPath(pszThisModuleFullPath);

	// compute the prefix if it is an empty string
	if (m_strRecoveryFilePrefix.empty())
	{
		// set the prefix = some temp file indicator followed by this EXE's name
		m_strRecoveryFilePrefix = "$RecoveryFile$ ";
		string strEXENameWithoutExtension = m_strThisEXEName.substr(0, 
			m_strThisEXEName.length() - 4);
		m_strRecoveryFilePrefix += strEXENameWithoutExtension;
		m_strRecoveryFilePrefix += string("_");
	}
}
//-------------------------------------------------------------------------------------------------
bool FileRecoveryManager::isCurrentlyExecutingSimilarProcess(DWORD dwProcessID)
{
	// get a snapshot of the currently running processes
	const int MAX_PROCESSES = 500;
	TASK_LIST arrTaskList[MAX_PROCESSES];
	int iNumProcesses = getTaskList(arrTaskList, MAX_PROCESSES);

	// iterate through the currently running processes and check whether
	// a process with the same name as this process is running.
	for (int i = 0; i < iNumProcesses; i++)
	{
		// if the process's EXE is the same as this EXE, then
		// return true
		if (arrTaskList[i].dwProcessId == dwProcessID &&
			_strcmpi(arrTaskList[i].ProcessName, m_strThisEXEName.c_str()) == 0)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool FileRecoveryManager::recoveryFileExists(string& strFile)
{
	// Ensure the recovery directory exists
	if (!isValidFolder(m_strRecoveryFileFolder))
	{
		return false;
	}

	// create a file search specification that looks like
	// c:\somedir\SomePrefix*SomeSuffix"
	string strFileSearchSpec = m_strRecoveryFileFolder + m_strRecoveryFilePrefix;
	strFileSearchSpec += "*";
	strFileSearchSpec += m_strRecoveryFileSuffix;
	
	// search for the first file that meets our recovery file specifications
	FileIterator iter(strFileSearchSpec);
	
	// iterate through all the files that meet the file spec
	while (iter.moveNext())
	{
		// Check that the file name is not "." or ".."
		string strFileName = iter.getFileName();
		if (strFileName != "." && strFileName != "..")
		{
			// check if the returned fileinfo is for a file or a directory
			if (!iter.isDirectory())
			{
				// we found a recovery file, but that recovery file
				// may be one saved by a currently running and well-behaved
				// instance of this application...check to see if a process of
				// the same name as this process exists with the same
				// process ID that's part of the recovery filename.
				// the recovery file is in the form {prefix}{processid}{suffix}
				// such as "$RecoveryFile$ RuleSetEditor_2220.rsd"
				//          ^prefix                      ^pid^siffix
				// we want to extract the processid from this filename
				string strTemp = getFileNameFromFullPath(strFileName);
				strTemp = strTemp.substr(m_strRecoveryFilePrefix.length(),
					strTemp.length() - m_strRecoveryFilePrefix.length() -
					m_strRecoveryFileSuffix.length());
				DWORD dwProcessID = asLong(strTemp);

				if (!isCurrentlyExecutingSimilarProcess(dwProcessID))
				{
					// we found a file that needs to be recovered
					strFile = m_strRecoveryFileFolder + strFileName;
					return true;
				}
			}
		}
	}

	// no files found that need to be recovered
	return false;
}
//-------------------------------------------------------------------------------------------------
void FileRecoveryManager::deleteRecoveryFile()
{
	// delete the recovery file associated with the current process
	deleteRecoveryFile(getRecoveryFileName());
}
//-------------------------------------------------------------------------------------------------
void FileRecoveryManager::deleteRecoveryFile(const string& strFile)
{
	try
	{
		// Ensure the file exists
		if (isValidFile(strFile))
		{
			deleteFile(strFile, true);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25126");
}
//-------------------------------------------------------------------------------------------------
string FileRecoveryManager::getRecoveryFileName()
{
	// if we have not yet computed the recovery file name, compute it 
	if (m_strRecoveryFileName.empty())
	{
		m_strRecoveryFileName = m_strRecoveryFileFolder;
		m_strRecoveryFileName += m_strRecoveryFilePrefix;
		m_strRecoveryFileName += asString(GetCurrentProcessId());
		m_strRecoveryFileName += m_strRecoveryFileSuffix;
	}

	return m_strRecoveryFileName;
}
//-------------------------------------------------------------------------------------------------
bool FileRecoveryManager::isRecoveryFolderWritable()
{
	// Check if the folder exists
	if (!isValidFolder(m_strRecoveryFileFolder))
	{
		// Attempt to create it
		try
		{
			createDirectory(m_strRecoveryFileFolder);
		}
		catch(...)
		{
			// If a directory can't be created than a file can't be written
			// so just return false
			return false;
		}
	}

	// Test file creation
	return canCreateFile(getRecoveryFileName());
}
//-------------------------------------------------------------------------------------------------
