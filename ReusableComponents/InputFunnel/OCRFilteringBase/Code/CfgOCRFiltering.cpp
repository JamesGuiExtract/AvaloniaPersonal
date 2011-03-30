#include "stdafx.h"
#include "CfgOCRFiltering.h"

#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <RegConstants.h>

using namespace std;

const std::string DEFAULT_SCHEME = "<Default Scheme>";
const string CfgOCRFiltering::LAST_USED_SCHEME = "LastUsedScheme";

//--------------------------------------------------------------------------------------------------
CfgOCRFiltering::CfgOCRFiltering()
{
	// root folder 
	string strRootFolder = gstrREG_ROOT_KEY + "\\InputFunnel";
	ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr(HKEY_CURRENT_USER, strRootFolder));
}
//---------------------------------------------------------------------------
string CfgOCRFiltering::getLastUsedOCRFilteringScheme()
{
	// since the scheme name is stored per-user, per-application base
	string strAppKey(getApplicationSpecificSchemeRoot());
	// Check for existence 
	if (!ma_pUserCfgMgr->keyExists(strAppKey, LAST_USED_SCHEME))
	{
		// Not found, just default to DEFAULT_SCHEME
		string strDefaultScheme(DEFAULT_SCHEME);
		ma_pUserCfgMgr->createKey(strAppKey, LAST_USED_SCHEME, strDefaultScheme);
		return strDefaultScheme;
	}

	return ma_pUserCfgMgr->getKeyValue(strAppKey, LAST_USED_SCHEME);	
}
//---------------------------------------------------------------------------
void CfgOCRFiltering::setLastUsedOCRFilteringScheme(const string& strSchemeName)
{
	ma_pUserCfgMgr->setKeyValue(getApplicationSpecificSchemeRoot(), LAST_USED_SCHEME, strSchemeName);
}

/////////////////////////////////////////////////////////////////////////////
// Helper functions
//---------------------------------------------------------------------------
string CfgOCRFiltering::getApplicationSpecificSchemeRoot()
{
	string strRes("\\ApplicationSpecificSettings");
	string strAppName = ::getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
	strRes = strRes + "\\" + strAppName;

	return strRes;
}
//---------------------------------------------------------------------------
