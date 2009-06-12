#include "stdafx.h"
#include "PersistenceMgr.h"

#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry keys
const string PersistenceMgr::FEEDBACK_FOLDER = "FeedbackFolder";
const string PersistenceMgr::FEEDBACK_ENABLED = "FeedbackEnabled";
const string PersistenceMgr::AUTO_TURNOFF_ENABLED = "TurnOffEnabled";
const string PersistenceMgr::AUTO_TURNOFF_DATE = "TurnOffDate";
const string PersistenceMgr::AUTO_TURNOFF_COUNT = "TurnOffCount";
const string PersistenceMgr::SKIP_COUNT = "SkipCount";
const string PersistenceMgr::DOCUMENT_COLLECTION = "SourceDocCollection";
const string PersistenceMgr::CONVERT_TO_TEXT = "ConvertToText";
const string PersistenceMgr::ATTRIBUTE_SELECTION = "AttributeSelection";
const string PersistenceMgr::ATTRIBUTE_NAME = "Attribute_";
const string PersistenceMgr::PACKAGE_FILE = "PackageFile";
const string PersistenceMgr::CLEAR_AFTER_PACKAGE = "ClearAfterPackage";

//-------------------------------------------------------------------------------------------------
// PersistenceMgr
//-------------------------------------------------------------------------------------------------
PersistenceMgr::PersistenceMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
}
//-------------------------------------------------------------------------------------------------
bool PersistenceMgr::getFeedbackEnabled()
{
	string	strValue;
	bool	bEnabled = false;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, FEEDBACK_ENABLED ))
	{
		// Not found, just default to 0 - False
		m_pCfgMgr->createKey( m_strSectionFolderName, FEEDBACK_ENABLED, 
			"0" );
	}
	else
	{
		// Retrieve value
		strValue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			FEEDBACK_ENABLED );
		bEnabled = asLong( strValue ) ? true : false;
	}

	return bEnabled;
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setFeedbackEnabled(bool bEnabled)
{
	CString	zValue;

	// Set string
	if (bEnabled)
	{
		zValue = "1";
	}
	else
	{
		zValue = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, FEEDBACK_ENABLED, (LPCTSTR)zValue );
}
//-------------------------------------------------------------------------------------------------
string PersistenceMgr::getFeedbackFolder(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, FEEDBACK_FOLDER))
	{
		// Not found, just default to empty string
		m_pCfgMgr->createKey(m_strSectionFolderName, FEEDBACK_FOLDER, "");
		return "";
	}

	return m_pCfgMgr->getKeyValue( m_strSectionFolderName, FEEDBACK_FOLDER );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setFeedbackFolder(const string& strFolder)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, FEEDBACK_FOLDER, strFolder );
}
//-------------------------------------------------------------------------------------------------
bool PersistenceMgr::getAutoTurnOffEnabled()
{
	string	strValue;
	bool	bEnabled = false;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, AUTO_TURNOFF_ENABLED ))
	{
		// Not found, just default to 0 - False
		m_pCfgMgr->createKey( m_strSectionFolderName, AUTO_TURNOFF_ENABLED, 
			"0" );
	}
	else
	{
		// Retrieve value
		strValue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			AUTO_TURNOFF_ENABLED );
		bEnabled = asLong( strValue ) ? true : false;
	}

	return bEnabled;
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setAutoTurnOffEnabled(bool bEnabled)
{
	CString	zValue;

	// Set string
	if (bEnabled)
	{
		zValue = "1";
	}
	else
	{
		zValue = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, AUTO_TURNOFF_ENABLED, (LPCTSTR)zValue );
}
//-------------------------------------------------------------------------------------------------
string PersistenceMgr::getTurnOffDate(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, AUTO_TURNOFF_DATE))
	{
		// Not found, just default to empty string
		m_pCfgMgr->createKey(m_strSectionFolderName, AUTO_TURNOFF_DATE, "");
		return "";
	}

	return m_pCfgMgr->getKeyValue( m_strSectionFolderName, AUTO_TURNOFF_DATE );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setTurnOffDate(const string& strDate)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, AUTO_TURNOFF_DATE, strDate );
}
//-------------------------------------------------------------------------------------------------
long PersistenceMgr::getTurnOffCount(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, AUTO_TURNOFF_COUNT))
	{
		// Not found, just default to 100
		m_pCfgMgr->createKey(m_strSectionFolderName, AUTO_TURNOFF_COUNT, "100");
		return 100;
	}

	return asLong( m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
		AUTO_TURNOFF_COUNT ) );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setSkipCount(const long lCount)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, SKIP_COUNT, 
		asString( lCount ) );
}
//-------------------------------------------------------------------------------------------------
long PersistenceMgr::getSkipCount(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, SKIP_COUNT))
	{
		// Not found, just default to 0
		m_pCfgMgr->createKey(m_strSectionFolderName, SKIP_COUNT, "0");
		return 0;
	}

	return asLong( m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
		SKIP_COUNT ) );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setTurnOffCount(const long lCount)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, AUTO_TURNOFF_COUNT, 
		asString( lCount ) );
}
//-------------------------------------------------------------------------------------------------
long PersistenceMgr::getDocumentCollection(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, DOCUMENT_COLLECTION))
	{
		// Not found, just default to 0 - No Collection
		m_pCfgMgr->createKey(m_strSectionFolderName, DOCUMENT_COLLECTION, "0");
		return 0;
	}

	return asLong( m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
		DOCUMENT_COLLECTION ) );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setDocumentCollection(const long lCount)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, DOCUMENT_COLLECTION, 
		asString( lCount ) );
}
//-------------------------------------------------------------------------------------------------
bool PersistenceMgr::getDocumentConversion()
{
	string	strValue;
	bool	bConvert = false;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, CONVERT_TO_TEXT ))
	{
		// Not found, just default to 0 - False
		m_pCfgMgr->createKey( m_strSectionFolderName, CONVERT_TO_TEXT, "0" );
	}
	else
	{
		// Retrieve value
		strValue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, CONVERT_TO_TEXT );
		bConvert = asLong( strValue ) ? true : false;
	}

	return bConvert;
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setDocumentConversion(bool bConvert)
{
	CString	zValue;

	// Set string
	if (bConvert)
	{
		zValue = "1";
	}
	else
	{
		zValue = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, CONVERT_TO_TEXT, (LPCTSTR)zValue );
}
//-------------------------------------------------------------------------------------------------
bool PersistenceMgr::getAttributeSelection()
{
	string	strValue;
	bool	bNamed = false;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, ATTRIBUTE_SELECTION ))
	{
		// Not found, just default to 0 - All Attributes
		m_pCfgMgr->createKey( m_strSectionFolderName, ATTRIBUTE_SELECTION, "0" );
	}
	else
	{
		// Retrieve value
		strValue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, ATTRIBUTE_SELECTION );
		bNamed = asLong( strValue ) ? true : false;
	}

	return bNamed;
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setAttributeSelection(bool bNamed)
{
	CString	zValue;

	// Set string
	if (bNamed)
	{
		zValue = "1";
	}
	else
	{
		zValue = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ATTRIBUTE_SELECTION, (LPCTSTR)zValue );
}
//-------------------------------------------------------------------------------------------------
string PersistenceMgr::getPackageFile(void)
{
	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, PACKAGE_FILE))
	{
		// Not found, just default to empty string
		m_pCfgMgr->createKey(m_strSectionFolderName, PACKAGE_FILE, "");
		return "";
	}

	return m_pCfgMgr->getKeyValue( m_strSectionFolderName, PACKAGE_FILE );
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setPackageFile(const string& strFile)
{
	// Store setting
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, PACKAGE_FILE, strFile );
}
//-------------------------------------------------------------------------------------------------
bool PersistenceMgr::getClearAfterPackage()
{
	string	strValue;
	bool	bClear = true;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, CLEAR_AFTER_PACKAGE ))
	{
		// Not found, just default to 1 - True
		m_pCfgMgr->createKey( m_strSectionFolderName, CLEAR_AFTER_PACKAGE, "1" );
	}
	else
	{
		// Retrieve value
		strValue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, CLEAR_AFTER_PACKAGE );
		bClear = asLong( strValue ) ? true : false;
	}

	return bClear;
}
//-------------------------------------------------------------------------------------------------
void PersistenceMgr::setClearAfterPackage(bool bClear)
{
	CString	zValue;

	// Set string
	if (bClear)
	{
		zValue = "1";
	}
	else
	{
		zValue = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, CLEAR_AFTER_PACKAGE, (LPCTSTR)zValue );
}
//-------------------------------------------------------------------------------------------------
