
#pragma once

#include "BaseUtils.h"

#include <string>

using namespace std;

class EXPORT_BaseUtils FileRecoveryManager
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: if strPrefix == "", a default prefix for the recovery file
	//			will be computed.  Part of this default prefix will include
	//			the current application's name.
	//			strSuffix must include the period and extension 
	//			such as ".txt", ".rsd", etc
	//			hEXEModule should be the module handle for the EXE
	//			using this class
	FileRecoveryManager(const string& strSuffix, 
		const string& strPrefix = "");
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if a file exists that needs to be recovered.
	//			If true is returned, strFile will contain the full path
	//			to the file that needs to be recovered
	bool recoveryFileExists(string& strFile);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To delete the recovery file associated with this process
	void deleteRecoveryFile();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To delete the specified recovery file, which is from another
	//			process that terminated prematurely
	void deleteRecoveryFile(const string& strFile);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the recovery filename associated with this process
	string getRecoveryFileName();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return true if the current user has file creation and write privileges in 
	//			the recovery folder.
	bool isRecoveryFolderWritable();
	//---------------------------------------------------------------------------------------------

private:
	string m_strRecoveryFileFolder;
	string m_strThisEXEName;
	string m_strRecoveryFilePrefix;
	string m_strRecoveryFileSuffix;
	string m_strRecoveryFileName;

	bool isCurrentlyExecutingSimilarProcess(DWORD dwProcessID);
};

