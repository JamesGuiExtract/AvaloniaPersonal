#include "stdafx.h"
#include "ENSConfigMgr.h"

#include <cpputil.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// Define keys
const string ENSConfigMgr::MOVE_NAMES = "MoveLastNameToFront";
const string ENSConfigMgr::DEFAULT_MOVE_NAMES = "0";

//-------------------------------------------------------------------------------------------------
// ENSConfigMgr
//-------------------------------------------------------------------------------------------------
ENSConfigMgr::ENSConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
}
//-------------------------------------------------------------------------------------------------
long ENSConfigMgr::getMoveNames()
{
	long lResult = 0;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, MOVE_NAMES ))
	{
		// Key does not exist, set and return default (NO MOVE)
		m_pCfgMgr->createKey( m_strSectionFolderName, MOVE_NAMES, DEFAULT_MOVE_NAMES );
	}
	else
	{
		// Key found - return its value
		string strResult = m_pCfgMgr->getKeyValue( m_strSectionFolderName, MOVE_NAMES, DEFAULT_MOVE_NAMES );
		lResult = ::asLong( strResult );
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
void ENSConfigMgr::setMoveNames(long lValue)
{
	// Set the flag
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, MOVE_NAMES, asString(lValue) );
}
//-------------------------------------------------------------------------------------------------
