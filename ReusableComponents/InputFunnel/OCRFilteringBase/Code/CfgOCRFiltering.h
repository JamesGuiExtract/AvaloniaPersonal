#pragma once

#include <IConfigurationSettingsPersistenceMgr.h>

#include <string>
#include <memory>

class CfgOCRFiltering
{
public:
	CfgOCRFiltering();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns last used scheme name
	//
	// PROMISE:	
	//
	std::string getLastUsedOCRFilteringScheme();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  sets last used scheme name for ocr filtering
	//
	// PROMISE:	
	//
	void setLastUsedOCRFilteringScheme(const std::string& strSchemeName);

private:
	// key name
	static const std::string LAST_USED_SCHEME;

	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	// gets the current application name. The storage for current ocr
	// filtering scheme is per-user, per-application base
	std::string getApplicationSpecificSchemeRoot();
};