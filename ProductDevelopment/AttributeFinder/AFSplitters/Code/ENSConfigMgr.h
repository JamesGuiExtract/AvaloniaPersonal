#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

class ENSConfigMgr
{
public:
	ENSConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the Move Names To Front flag
	long	getMoveNames();
	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the Move Names To Front flag
	void	setMoveNames(long lValue);
	//---------------------------------------------------------------------------------------------

private:
	///////
	// Data
	///////
	static const std::string MOVE_NAMES;
	static const std::string DEFAULT_MOVE_NAMES;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string		m_strSectionFolderName;

	//////////
	// Methods
	//////////
};
