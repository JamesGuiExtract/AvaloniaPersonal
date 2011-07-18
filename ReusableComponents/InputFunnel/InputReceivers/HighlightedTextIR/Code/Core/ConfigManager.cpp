#include "stdafx.h"
#include "ConfigManager.h"

#include <cpputil.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;
// keys
const string ConfigManager::LAST_FILE_OPEN_DIR = "LastFileOpenDirectory";
const string ConfigManager::TEXT_FONT = "TextFont";
const string ConfigManager::TEXT_SIZE = "TextSize";
const string ConfigManager::WINDOW_POS_X = "WindowPositionX";
const string ConfigManager::WINDOW_POS_Y = "WindowPositionY";
const string ConfigManager::WINDOW_SIZE_X = "WindowSizeX";
const string ConfigManager::WINDOW_SIZE_Y = "WindowSizeY";
const string ConfigManager::LAST_INPUT_FINDER = "LastInputFinder";
const string ConfigManager::TEXT_PROCESSING_SCOPE = "TextProcessingScope";

const string ConfigManager::DEFAULT_LAST_FILE_OPEN_DIR = ".";
const string ConfigManager::DEFAULT_TEXT_FONT = "Times New Roman";
const string ConfigManager::DEFAULT_TEXT_SIZE = "10";
const string ConfigManager::DEFAULT_WINDOW_POS_X = "10";
const string ConfigManager::DEFAULT_WINDOW_POS_Y = "10";
const string ConfigManager::DEFAULT_WINDOW_SIZE_X = "280";
const string ConfigManager::DEFAULT_WINDOW_SIZE_Y = "170";
const string ConfigManager::DEFAULT_LAST_INPUT_FINDER = "";
const int gnDEFAULT_TEXT_PROCESSING_SCOPE = 0;

// minimum width and height for the dialog
const int giMIN_WIDTH	= 245;
const int giMIN_HEIGHT = 115;

//--------------------------------------------------------------------------------------------------
ConfigManager::ConfigManager(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
}
//--------------------------------------------------------------------------------------------------
string ConfigManager::getLastFileOpenDirectory(void)
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_FILE_OPEN_DIR))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, LAST_FILE_OPEN_DIR, DEFAULT_LAST_FILE_OPEN_DIR);
		return ".";
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR,
		DEFAULT_LAST_FILE_OPEN_DIR);
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setLastFileOpenDirectory(const string& strFileDir)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR, strFileDir);
}
//--------------------------------------------------------------------------------------------------
string ConfigManager::getTextFont()
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, TEXT_FONT))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, TEXT_FONT, DEFAULT_TEXT_FONT);
		return DEFAULT_TEXT_FONT;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, TEXT_FONT, DEFAULT_TEXT_FONT);
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setTextFont(const string& strFontName)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, TEXT_FONT, strFontName);
}
//--------------------------------------------------------------------------------------------------
string ConfigManager::getTextSizeInTwips()
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, TEXT_SIZE))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, TEXT_SIZE, DEFAULT_TEXT_SIZE);
		return DEFAULT_TEXT_SIZE;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, TEXT_SIZE, DEFAULT_TEXT_SIZE);
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setTextSizeInTwips(const string& strTextSize)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, TEXT_SIZE, strTextSize);
}
//--------------------------------------------------------------------------------------------------
int ConfigManager::getTextProcessingScope()
{
	unsigned long iResult = gnDEFAULT_TEXT_PROCESSING_SCOPE;

	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, TEXT_PROCESSING_SCOPE))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, TEXT_PROCESSING_SCOPE,
			::asString(gnDEFAULT_TEXT_PROCESSING_SCOPE));
	}
	else
	{
		// key found - return its value
		string strResult = m_pCfgMgr->getKeyValue(m_strSectionFolderName, TEXT_PROCESSING_SCOPE,
			asString(gnDEFAULT_TEXT_PROCESSING_SCOPE));
		iResult = ::asLong(strResult);
	}

	// if the value is within valid range, then reset the value to the default
	// value.
	if (iResult < 0 || iResult > 2)
	{
		iResult = gnDEFAULT_TEXT_PROCESSING_SCOPE;
		setTextProcessingScope(iResult);
	}

	return iResult;
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setTextProcessingScope(int iValue)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, TEXT_PROCESSING_SCOPE, asString(iValue) );
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::getWindowPos(long &lPosX, long &lPosY)
{
	string	strX;
	string	strY;

	// Check for existence of X position
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strSectionFolderName, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
		lPosX = asLong(DEFAULT_WINDOW_POS_X);
	}
	else
	{
		// Retrieve X position
		strX = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_X,
			DEFAULT_WINDOW_POS_X);
		lPosX = asLong( strX );
	}

	// Check for existence of Y position
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strSectionFolderName, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
		lPosY = asLong(DEFAULT_WINDOW_POS_Y);
	}
	else
	{
		// Retrieve Y position
		strY = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_Y,
			DEFAULT_WINDOW_POS_Y);
		lPosY = asLong( strY );
	}
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setWindowPos(long lPosX, long lPosY)
{
	CString	pszX, pszY;

	// Format strings
	pszX.Format( "%ld", lPosX );
	pszY.Format( "%ld", lPosY );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_X, (LPCTSTR)pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_Y, (LPCTSTR)pszY );
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::getWindowSize(long &lSizeX, long &lSizeY)
{
	string	strX;
	string	strY;

	// Check for existence of width
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_SIZE_X))
	{
		// Not found, just default to 280
		m_pCfgMgr->createKey( m_strSectionFolderName, WINDOW_SIZE_X, DEFAULT_WINDOW_SIZE_X );
		lSizeX = asLong(DEFAULT_WINDOW_SIZE_X);
	}
	else
	{
		// Retrieve width
		strX = m_pCfgMgr->getKeyValue( m_strSectionFolderName, WINDOW_SIZE_X, DEFAULT_WINDOW_SIZE_X );
		lSizeX = asLong( strX );
	}

	// Check for existence of height
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_SIZE_Y))
	{
		// Not found, just default to 170
		m_pCfgMgr->createKey( m_strSectionFolderName, WINDOW_SIZE_Y, DEFAULT_WINDOW_SIZE_Y );
		lSizeY = asLong(DEFAULT_WINDOW_SIZE_Y);
	}
	else
	{
		// Retrieve height
		strY = m_pCfgMgr->getKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y, DEFAULT_WINDOW_SIZE_Y );
		lSizeY = asLong( strY );
	}

	// Check for minimum width
	if (lSizeX < giMIN_WIDTH)
	{
		lSizeX = giMIN_WIDTH;
	}

	// Check for minimum height
	if (lSizeY < giMIN_HEIGHT)
	{
		lSizeY = giMIN_HEIGHT;
	}
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setWindowSize(long lSizeX, long lSizeY)
{
	long	lActualX = lSizeX;
	long	lActualY = lSizeY;

	// Check for minimum width
	if (lActualX < giMIN_WIDTH)
	{
		lActualX = giMIN_WIDTH;
	}

	// Check for minimum height
	if (lActualY < giMIN_HEIGHT)
	{
		lActualY = giMIN_HEIGHT;
	}

	CString pszX, pszY;
	pszX.Format("%ld", lActualX);
	pszY.Format("%ld", lActualY);

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_X, (LPCTSTR)pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y, (LPCTSTR)pszY );
}
//--------------------------------------------------------------------------------------------------
string ConfigManager::getLastInputFinderName()
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_INPUT_FINDER))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, LAST_INPUT_FINDER, DEFAULT_LAST_INPUT_FINDER);
		return DEFAULT_LAST_INPUT_FINDER;
	}

	string strName(m_pCfgMgr->getKeyValue(m_strSectionFolderName, LAST_INPUT_FINDER,
		DEFAULT_LAST_INPUT_FINDER));
	return strName;
}
//--------------------------------------------------------------------------------------------------
void ConfigManager::setLastInputFinderName(const string& strInputFinderName)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, LAST_INPUT_FINDER, strInputFinderName);
}
//--------------------------------------------------------------------------------------------------
