#include "stdafx.h"
#include "TesterConfigMgr.h"

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
const string TesterConfigMgr::LAST_FILE_OPEN_DIR = "LastFileOpenDirectory";
const string TesterConfigMgr::LAST_FILE_NAME = "LastFileName";
const string TesterConfigMgr::WINDOW_POS_X = "WindowPositionX";
const string TesterConfigMgr::WINDOW_POS_Y = "WindowPositionY";
const string TesterConfigMgr::WINDOW_SIZE_X = "WindowSizeX";
const string TesterConfigMgr::WINDOW_SIZE_Y = "WindowSizeY";
const string TesterConfigMgr::ALL_ATTRIBUTES_TEST_SCOPE = "AllAttributesTestScope";
const string TesterConfigMgr::LAST_INPUT_TYPE = "LastInputType";
const string TesterConfigMgr::NAME_COLUMN_WIDTH = "NameColumnWidth";
const string TesterConfigMgr::SHOW_ONLY_VALID_ENTRIES = "ShowOnlyValidEntries";
const string TesterConfigMgr::SPLITTER_POS_Y = "SplitterPositionY";
const string TesterConfigMgr::TYPE_COLUMN_WIDTH = "TypeColumnWidth";
const string TesterConfigMgr::LAST_FILE_SAVE_DIR = "LastFileSaveDirectory";
const string TesterConfigMgr::AUTO_EXPAND_ATTRIBUTES = "AutoExpandAttributes";

const string TesterConfigMgr::DEFAULT_LAST_FILE_OPEN_DIR = ".";
const string TesterConfigMgr::DEFAULT_LAST_FILE_NAME = "";
const string TesterConfigMgr::DEFAULT_WINDOW_POS_X = "10";
const string TesterConfigMgr::DEFAULT_WINDOW_POS_Y = "10";
const string TesterConfigMgr::DEFAULT_WINDOW_SIZE_X = "280";
const string TesterConfigMgr::DEFAULT_WINDOW_SIZE_Y = "170";
const string TesterConfigMgr::DEFAULT_ALL_ATTRIBUTES_TEST_SCOPE = "0";
const string TesterConfigMgr::DEFAULT_NAME_COLUMN_WIDTH = "125";
const string TesterConfigMgr::DEFAULT_SPLITTER_POS_Y = "205";
const string TesterConfigMgr::DEFAULT_SHOW_ONLY_VALID_ENTRIES = "0";
const string TesterConfigMgr::DEFAULT_TYPE_COLUMN_WIDTH = "125";
const string TesterConfigMgr::DEFAULT_LAST_FILE_SAVE_DIR = ".";
const string TesterConfigMgr::DEFAULT_AUTO_EXPAND_ATTRIBUTES = "0";

// Minimum width and height for the dialog
const int	TesterConfigMgr::giRTDLG_MIN_WIDTH = 380;
const int	TesterConfigMgr::giRTDLG_MIN_HEIGHT = 410;

// Strings for Test Input combo box
const string	TesterConfigMgr::gstrTEXTFROMIMAGEWINDOW	= "Most recently OCR'd text from Image Window";
const string	TesterConfigMgr::gstrTEXTFROMFILE	= "Most recently processed text from file";
const string	TesterConfigMgr::gstrMANUALTEXT		= "Text from manual input";

//-------------------------------------------------------------------------------------------------
// TesterConfigMgr
//-------------------------------------------------------------------------------------------------
TesterConfigMgr::TesterConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
}
//-------------------------------------------------------------------------------------------------
string TesterConfigMgr::getLastFileOpenDirectory(void)
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_FILE_OPEN_DIR))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, LAST_FILE_OPEN_DIR, DEFAULT_LAST_FILE_OPEN_DIR);
		return DEFAULT_LAST_FILE_OPEN_DIR;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR, DEFAULT_LAST_FILE_OPEN_DIR);
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setLastFileOpenDirectory(const string& strFileDir)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR, strFileDir);
}
//-------------------------------------------------------------------------------------------------
string TesterConfigMgr::getLastFileName(void)
{
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, LAST_FILE_NAME ))
	{
		m_pCfgMgr->createKey( m_strSectionFolderName, LAST_FILE_NAME, DEFAULT_LAST_FILE_NAME );
		return DEFAULT_LAST_FILE_NAME;
	}

	return m_pCfgMgr->getKeyValue( m_strSectionFolderName, LAST_FILE_NAME, DEFAULT_LAST_FILE_NAME );
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setLastFileName(const string& strFileName)
{
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, LAST_FILE_NAME, 
		strFileName );
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::getWindowPos(long &lPosX, long &lPosY)
{
	string	strX;
	string	strY;

	// Check for existence of X position
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strSectionFolderName, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
		lPosX = 10;
	}
	else
	{
		// Retrieve X position
		strX = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
		lPosX = asLong( strX );
	}

	// Check for existence of Y position
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(m_strSectionFolderName, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
		lPosY = 10;
	}
	else
	{
		// Retrieve Y position
		strY = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
		lPosY = asLong( strY );
	}
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setWindowPos(long lPosX, long lPosY)
{
	CString	pszX, pszY;

	// Format strings
	pszX.Format( "%ld", lPosX );
	pszY.Format( "%ld", lPosY );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_X, (LPCTSTR)pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_Y, (LPCTSTR)pszY );
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::getWindowSize(long &lSizeX, long &lSizeY)
{
	string	strX;
	string	strY;

	// Check for existence of width
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, WINDOW_SIZE_X))
	{
		// Not found, just default to 280
		m_pCfgMgr->createKey( m_strSectionFolderName, WINDOW_SIZE_X, DEFAULT_WINDOW_SIZE_X );
		lSizeX = 280;
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
		lSizeY = 170;
	}
	else
	{
		// Retrieve height
		strY = m_pCfgMgr->getKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y, DEFAULT_WINDOW_SIZE_Y );
		lSizeY = asLong( strY );
	}

	// Check for minimum width
	if (lSizeX < giRTDLG_MIN_WIDTH)
	{
		lSizeX = giRTDLG_MIN_WIDTH;
	}

	// Check for minimum height
	if (lSizeY < giRTDLG_MIN_HEIGHT)
	{
		lSizeY = giRTDLG_MIN_HEIGHT;
	}
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setWindowSize(long lSizeX, long lSizeY)
{
	long	lActualX = lSizeX;
	long	lActualY = lSizeY;

	// Check for minimum width
	if (lActualX < giRTDLG_MIN_WIDTH)
	{
		lActualX = giRTDLG_MIN_WIDTH;
	}

	// Check for minimum height
	if (lActualY < giRTDLG_MIN_HEIGHT)
	{
		lActualY = giRTDLG_MIN_HEIGHT;
	}

	CString pszX, pszY;
	pszX.Format("%ld", lActualX);
	pszY.Format("%ld", lActualY);

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_X, 
		(LPCTSTR)pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y, 
		(LPCTSTR)pszY );
}
//-------------------------------------------------------------------------------------------------
bool TesterConfigMgr::getAllAttributesTestScope()
{
	string	strScope;
	bool	bAllAttributes = false;

	// Check for existence of scope
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, ALL_ATTRIBUTES_TEST_SCOPE ))
	{
		// Not found, just default to 0 - False
		m_pCfgMgr->createKey( m_strSectionFolderName, ALL_ATTRIBUTES_TEST_SCOPE, 
			DEFAULT_ALL_ATTRIBUTES_TEST_SCOPE );
	}
	else
	{
		// Retrieve scope
		strScope = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			ALL_ATTRIBUTES_TEST_SCOPE, DEFAULT_ALL_ATTRIBUTES_TEST_SCOPE );
		bAllAttributes = asCppBool( strScope );
	}

	return bAllAttributes;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setAllAttributesTestScope(bool bAllAttributes)
{
	CString	zScope;

	// Set string
	if (bAllAttributes)
	{
		zScope = "1";
	}
	else
	{
		zScope = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ALL_ATTRIBUTES_TEST_SCOPE, 
		(LPCTSTR)zScope );
}
//-------------------------------------------------------------------------------------------------
string TesterConfigMgr::getLastInputType(void)
{
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, LAST_INPUT_TYPE ))
	{
		// Not found, just default to Manual input
		m_pCfgMgr->createKey( m_strSectionFolderName, LAST_INPUT_TYPE, 
			gstrMANUALTEXT.c_str() );
		return gstrMANUALTEXT;
	}

	return m_pCfgMgr->getKeyValue( m_strSectionFolderName, LAST_INPUT_TYPE, gstrMANUALTEXT );
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setLastInputType(const string& strType)
{
	// Store type
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, LAST_INPUT_TYPE, 
		strType );
}
//-------------------------------------------------------------------------------------------------
long TesterConfigMgr::getNameColumnWidth()
{
	string	strWidth;
	long	lColumnWidth = giRTDLG_MIN_WIDTH / 3;

	// Check for existence of column width
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, NAME_COLUMN_WIDTH ))
	{
		// Not found, just default to one-third of minimum width
		m_pCfgMgr->createKey( m_strSectionFolderName, NAME_COLUMN_WIDTH, 
			DEFAULT_NAME_COLUMN_WIDTH );
	}
	else
	{
		// Retrieve column width
		strWidth = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			NAME_COLUMN_WIDTH, DEFAULT_NAME_COLUMN_WIDTH );
		lColumnWidth = asLong( strWidth );
	}

	return lColumnWidth;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setNameColumnWidth(long lColumnWidth)
{
	CString	zWidth;

	// Format string
	zWidth.Format( "%ld", lColumnWidth );

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, NAME_COLUMN_WIDTH, 
		(LPCTSTR)zWidth );
}
//-------------------------------------------------------------------------------------------------
long TesterConfigMgr::getSplitterPosition()
{
	string	strPosition;
	long	lPosition = giRTDLG_MIN_HEIGHT / 2;

	// Check for existence of splitter position
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, SPLITTER_POS_Y ))
	{
		// Not found, just default to half of minimum height
		m_pCfgMgr->createKey( m_strSectionFolderName, SPLITTER_POS_Y, 
			DEFAULT_SPLITTER_POS_Y );
	}
	else
	{
		// Retrieve splitter position
		strPosition = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			SPLITTER_POS_Y, DEFAULT_SPLITTER_POS_Y );
		lPosition = asLong( strPosition );
	}

	return lPosition;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setSplitterPosition(long lSplitterPosition)
{
	CString	zPosition;

	// Format string
	zPosition.Format( "%ld", lSplitterPosition );

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, SPLITTER_POS_Y, 
		(LPCTSTR)zPosition );
}
//-------------------------------------------------------------------------------------------------
bool TesterConfigMgr::getShowOnlyValidEntries()
{
	string	strValid;
	bool	bShowValid = false;

	// Check for existence of show only valid entries setting
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, SHOW_ONLY_VALID_ENTRIES ))
	{
		// Not found, just default to 0 - False ---> All entries will be shown
		m_pCfgMgr->createKey( m_strSectionFolderName, SHOW_ONLY_VALID_ENTRIES, 
			DEFAULT_SHOW_ONLY_VALID_ENTRIES );
	}
	else
	{
		// Retrieve setting
		strValid = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			SHOW_ONLY_VALID_ENTRIES, DEFAULT_SHOW_ONLY_VALID_ENTRIES );
		bShowValid = asCppBool( strValid );
	}

	return bShowValid;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setShowOnlyValidEntries(bool bShowValid)
{
	CString	zScope;

	// Set string
	if (bShowValid)
	{
		zScope = "1";
	}
	else
	{
		zScope = "0";
	}

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, SHOW_ONLY_VALID_ENTRIES, 
		(LPCTSTR)zScope );
}
//-------------------------------------------------------------------------------------------------
long TesterConfigMgr::getTypeColumnWidth()
{
	string	strWidth;
	long	lColumnWidth = giRTDLG_MIN_WIDTH / 3;

	// Check for existence of column width
	if (!m_pCfgMgr->keyExists( m_strSectionFolderName, TYPE_COLUMN_WIDTH ))
	{
		// Not found, just default to one-third of minimum width
		m_pCfgMgr->createKey( m_strSectionFolderName, TYPE_COLUMN_WIDTH, 
			DEFAULT_TYPE_COLUMN_WIDTH );
	}
	else
	{
		// Retrieve column width
		strWidth = m_pCfgMgr->getKeyValue( m_strSectionFolderName, 
			TYPE_COLUMN_WIDTH, DEFAULT_TYPE_COLUMN_WIDTH );
		lColumnWidth = asLong( strWidth );
	}

	return lColumnWidth;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setTypeColumnWidth(long lColumnWidth)
{
	CString	zWidth;

	// Format string
	zWidth.Format( "%ld", lColumnWidth );

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, TYPE_COLUMN_WIDTH, 
		(LPCTSTR)zWidth );
}
//-------------------------------------------------------------------------------------------------
string TesterConfigMgr::getLastFileSaveDirectory(void)
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_FILE_SAVE_DIR))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, LAST_FILE_SAVE_DIR, DEFAULT_LAST_FILE_SAVE_DIR);
		return DEFAULT_LAST_FILE_SAVE_DIR;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, LAST_FILE_SAVE_DIR, DEFAULT_LAST_FILE_SAVE_DIR);
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setLastFileSaveDirectory(const string& strFileDir)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, LAST_FILE_SAVE_DIR, strFileDir);
}
//-------------------------------------------------------------------------------------------------
bool TesterConfigMgr::getAutoExpandAttributes()
{
	bool bAutoExpand = false;

	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, AUTO_EXPAND_ATTRIBUTES))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, AUTO_EXPAND_ATTRIBUTES, DEFAULT_AUTO_EXPAND_ATTRIBUTES);
	}
	else
	{
		// Retrieve setting
		string strAutoExpand = m_pCfgMgr->getKeyValue( m_strSectionFolderName,
			AUTO_EXPAND_ATTRIBUTES, DEFAULT_AUTO_EXPAND_ATTRIBUTES);
		bAutoExpand = strAutoExpand != "0";
	}

	return bAutoExpand;
}
//-------------------------------------------------------------------------------------------------
void TesterConfigMgr::setAutoExpandAttributes(bool bAutoExpand)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, AUTO_EXPAND_ATTRIBUTES, bAutoExpand ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
