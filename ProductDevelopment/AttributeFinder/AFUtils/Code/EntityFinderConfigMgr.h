#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

class EntityFinderConfigMgr
{
public:
	EntityFinderConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the logging enabled flag
	long	getLoggingEnabled();
	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the logging enabled flag
	void	setLoggingEnabled(long lValue);
	//---------------------------------------------------------------------------------------------

private:
	///////
	// Data
	///////
	static const std::string LOGGING_ENABLED;
	static const std::string DEFAULT_LOGGING_ENABLED;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string		m_strSectionFolderName;

	//////////
	// Methods
	//////////
};
