#include "stdafx.h"
#include "EntityFinderConfigMgr.h"

#include <cpputil.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// Define keys
const string EntityFinderConfigMgr::LOGGING_ENABLED = "LoggingEnabled";
const string EntityFinderConfigMgr::DEFAULT_LOGGING_ENABLED = "0";

//-------------------------------------------------------------------------------------------------
// EntityFinderConfigMgr
//-------------------------------------------------------------------------------------------------
EntityFinderConfigMgr::EntityFinderConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
}
//-------------------------------------------------------------------------------------------------
long EntityFinderConfigMgr::getLoggingEnabled()
{
	long lResult = 0;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, LOGGING_ENABLED ))
	{
		// Key does not exist, set and return default (NOT ENABLED)
		m_pCfgMgr->createKey( m_strSectionFolderName, LOGGING_ENABLED, DEFAULT_LOGGING_ENABLED );
	}
	else
	{
		// Key found - return its value
		string strResult = m_pCfgMgr->getKeyValue( m_strSectionFolderName, LOGGING_ENABLED,
			DEFAULT_LOGGING_ENABLED);
		lResult = ::asLong( strResult );
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
void EntityFinderConfigMgr::setLoggingEnabled(long lValue)
{
	// Set the flag
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, LOGGING_ENABLED, asString(lValue) );
}
//-------------------------------------------------------------------------------------------------
