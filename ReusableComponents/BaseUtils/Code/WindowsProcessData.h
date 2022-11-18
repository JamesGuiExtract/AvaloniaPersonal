#pragma once

#include "BaseUtils.h"
#include <string>

// This class is used by UCLIDException in the constructor and should never
// create UCLIDException methods or method calls(not even if they are logged
// this can cause a stack overflow.
class EXPORT_BaseUtils WindowsProcessData
{
public:
	WindowsProcessData();
	WindowsProcessData(const WindowsProcessData& wpd);

	string m_strProcessName;
	DWORD m_PID;
	string m_strComputerName;
	string m_strVersion;
	string m_strUserName;
private:
	void Initialize();
	string getProcessName();
	string getVersion();
	
	// There is a global function by this name but it can create UCLIDException
	// which causes stack overflows since this is used by UCLIDException
	// This function ignores the errror and returns "" 
	string getFileVersion(string strFileFullName);
};

