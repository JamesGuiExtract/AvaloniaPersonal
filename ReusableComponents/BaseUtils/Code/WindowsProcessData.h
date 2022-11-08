#pragma once

#include "BaseUtils.h"
#include <string>


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
	string getProcessName();
	string getVersion();
};

