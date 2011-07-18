#include "stdafx.h"
#include "ConfigMgrSRIR.h"

#include <UCLIDException.h>
#include <cpputil.h>

#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//keys
const string ConfigMgrSRIR::LAST_FILE_OPEN_DIR = "LastFileOpenDirectory";
const string ConfigMgrSRIR::WINDOW_POS_X = "WindowPositionX";
const string ConfigMgrSRIR::WINDOW_POS_Y = "WindowPositionY";
const string ConfigMgrSRIR::WINDOW_SIZE_X = "WindowSizeX";
const string ConfigMgrSRIR::WINDOW_SIZE_Y = "WindowSizeY";
const string ConfigMgrSRIR::ZONE_HEIGHT = "ZoneHeight";
const string ConfigMgrSRIR::ZONE_COLOR_RED = "ZoneColorRed";
const string ConfigMgrSRIR::ZONE_COLOR_GREEN = "ZoneColorGreen";
const string ConfigMgrSRIR::ZONE_COLOR_BLUE = "ZoneColorBlue";
const string ConfigMgrSRIR::USED_ZONE_COLOR_RED = "UsedZoneColorRed";
const string ConfigMgrSRIR::USED_ZONE_COLOR_GREEN = "UsedZoneColorGreen";
const string ConfigMgrSRIR::USED_ZONE_COLOR_BLUE = "UsedZoneColorBlue";
const string ConfigMgrSRIR::AUTO_ROTATE_STEP_SIZE = "AutoRotateStepSize";
const string ConfigMgrSRIR::NUM_AUTO_ROTATE_STEPS = "NumAutoRotateSteps";
const string ConfigMgrSRIR::HEIGHT_PAD_FACTOR = "ZoneHeightPadFactor";
const string ConfigMgrSRIR::OCR_REGION_TYPE = "OCRRegionType";
const string ConfigMgrSRIR::LAST_SELECTION_TOOL = "SelectionTool";
const string ConfigMgrSRIR::FIT_TO_STATUS = "FitToStatus";
const string ConfigMgrSRIR::DISPLAY_PERCENTAGE = "DisplayPercentageEnabled";

const string ConfigMgrSRIR::DEFAULT_LAST_FILE_OPEN_DIR = ".";
const string ConfigMgrSRIR::DEFAULT_WINDOW_POS_X = "10";
const string ConfigMgrSRIR::DEFAULT_WINDOW_POS_Y = "10";
const string ConfigMgrSRIR::DEFAULT_WINDOW_SIZE_X = "280";
const string ConfigMgrSRIR::DEFAULT_WINDOW_SIZE_Y = "170";
const string ConfigMgrSRIR::DEFAULT_ZONE_HEIGHT = "45";
const string ConfigMgrSRIR::DEFAULT_ZONE_COLOR_RED = "255";
const string ConfigMgrSRIR::DEFAULT_ZONE_COLOR_GREEN = "255";
const string ConfigMgrSRIR::DEFAULT_ZONE_COLOR_BLUE = "0";
const string ConfigMgrSRIR::DEFAULT_USED_ZONE_COLOR_RED = "255";
const string ConfigMgrSRIR::DEFAULT_USED_ZONE_COLOR_GREEN = "192";
const string ConfigMgrSRIR::DEFAULT_USED_ZONE_COLOR_BLUE = "192";
const string ConfigMgrSRIR::DEFAULT_AUTO_ROTATE_STEP_SIZE = "4";
const string ConfigMgrSRIR::DEFAULT_NUM_AUTO_ROTATE_STEPS = "1";
const string ConfigMgrSRIR::DEFAULT_HEIGHT_PAD_FACTOR = "1.2"; // 20%
const string ConfigMgrSRIR::DEFAULT_OCR_REGION_TYPE = "0";
const string ConfigMgrSRIR::DEFAULT_LAST_SELECTION_TOOL = asString((long)kSelectText);
const string ConfigMgrSRIR::DEFAULT_FIT_TO_STATUS = "1";
const string ConfigMgrSRIR::DEFAULT_DISPLAY_PERCENTAGE = "0";

// Definitions
#define	MIN_WIDTH				425
#define	MIN_HEIGHT				115

//---------------------------------------------------------------------------
ConfigMgrSRIR::ConfigMgrSRIR(IConfigurationSettingsPersistenceMgr* pConfigMgr,
							 const string& strSectionName)
: m_pCfgMgr(pConfigMgr),
  m_strSectionFolderName(strSectionName)
{
}
//---------------------------------------------------------------------------
string ConfigMgrSRIR::getLastFileOpenDirectory(void)
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_FILE_OPEN_DIR))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, LAST_FILE_OPEN_DIR,
			DEFAULT_LAST_FILE_OPEN_DIR);
		return DEFAULT_LAST_FILE_OPEN_DIR;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR,
		DEFAULT_LAST_FILE_OPEN_DIR);
}

//---------------------------------------------------------------------------
void ConfigMgrSRIR::setLastFileOpenDirectory(const string& strFileDir)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, LAST_FILE_OPEN_DIR, strFileDir);
}
//---------------------------------------------------------------------------
void ConfigMgrSRIR::getWindowPos(long &lPosX, long &lPosY)
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
		strX = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
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
		strY = m_pCfgMgr->getKeyValue(m_strSectionFolderName, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
		lPosY = asLong( strY );
	}
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setWindowPos(long lPosX, long lPosY)
{
	char	pszX[100];
	char	pszY[100];

	// Format strings
	sprintf_s( pszX, sizeof(pszX), "%ld", lPosX );
	sprintf_s( pszY, sizeof(pszY), "%ld", lPosY );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_X, pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_POS_Y, pszY );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::getWindowSize(long &lSizeX, long &lSizeY)
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
		strX = m_pCfgMgr->getKeyValue( m_strSectionFolderName, WINDOW_SIZE_X,
			DEFAULT_WINDOW_SIZE_X );
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
		strY = m_pCfgMgr->getKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y,
			DEFAULT_WINDOW_SIZE_Y );
		lSizeY = asLong( strY );
	}

	// Check for minimum width
	if (lSizeX < MIN_WIDTH)
	{
		lSizeX = MIN_WIDTH;
	}

	// Check for minimum height
	if (lSizeY < MIN_HEIGHT)
	{
		lSizeY = MIN_HEIGHT;
	}
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setWindowSize(long lSizeX, long lSizeY)
{
	char	pszX[100];
	char	pszY[100];
	long	lActualX = lSizeX;
	long	lActualY = lSizeY;

	// Check for minimum width
	if (lActualX < MIN_WIDTH)
	{
		lActualX = MIN_WIDTH;
	}

	// Check for minimum height
	if (lActualY < MIN_HEIGHT)
	{
		lActualY = MIN_HEIGHT;
	}

	// Format strings
	sprintf_s( pszX, sizeof(pszX), "%ld", lActualX );
	sprintf_s( pszY, sizeof(pszY), "%ld", lActualY );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_X, pszX );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, WINDOW_SIZE_Y, pszY );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::getZoneHeight(long &lHeight)
{
	string	strHeight;

	// Check for existence of height
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, ZONE_HEIGHT))
	{
		// Not found, just default to 45
		m_pCfgMgr->createKey( m_strSectionFolderName, ZONE_HEIGHT, DEFAULT_ZONE_HEIGHT );
		lHeight = asLong(DEFAULT_ZONE_HEIGHT);
	}
	else
	{
		// Retrieve height
		strHeight = m_pCfgMgr->getKeyValue( m_strSectionFolderName, ZONE_HEIGHT, DEFAULT_ZONE_HEIGHT );
		lHeight = asLong( strHeight );
	}
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setZoneHeight(long lHeight)
{
	char	pszHeight[100];

	// Format string
	sprintf_s( pszHeight, sizeof(pszHeight), "%ld", lHeight );

	// Store string
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ZONE_HEIGHT, pszHeight );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::getZoneColor(COLORREF &color)
{
	string	strRed;
	string	strGreen;
	string	strBlue;
	int		iRed = 0;
	int		iGreen = 0;
	int		iBlue = 0;

	// Check for existence of red
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, ZONE_COLOR_RED))
	{
		// Not found, just default to 0
		m_pCfgMgr->createKey( m_strSectionFolderName, ZONE_COLOR_RED, DEFAULT_ZONE_COLOR_RED);
		iRed = asLong(DEFAULT_ZONE_COLOR_RED);
	}
	else
	{
		// Retrieve red
		strRed = m_pCfgMgr->getKeyValue( m_strSectionFolderName, ZONE_COLOR_RED,
			DEFAULT_ZONE_COLOR_RED );
		iRed = ::asLong(strRed);

		// Limit color between 0 and 255
		if (iRed > 255)
		{
			iRed = 255;
		}

		if (iRed < 0)
		{
			iRed = 0;
		}
	}

	// Check for existence of green
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, ZONE_COLOR_GREEN))
	{
		// Not found, just default to 255
		m_pCfgMgr->createKey( m_strSectionFolderName, ZONE_COLOR_GREEN, DEFAULT_ZONE_COLOR_GREEN );
		iGreen = asLong(DEFAULT_ZONE_COLOR_GREEN);
	}
	else
	{
		// Retrieve green
		strGreen = m_pCfgMgr->getKeyValue( m_strSectionFolderName, ZONE_COLOR_GREEN,
			DEFAULT_ZONE_COLOR_GREEN);
		iGreen = ::asLong(strGreen);

		// Limit color between 0 and 255
		if (iGreen > 255)
		{
			iGreen = 255;
		}

		if (iGreen < 0)
		{
			iGreen = 0;
		}
	}

	// Check for existence of blue
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, ZONE_COLOR_BLUE))
	{
		// Not found, just default to 255
		m_pCfgMgr->createKey( m_strSectionFolderName, ZONE_COLOR_BLUE, DEFAULT_ZONE_COLOR_BLUE );
		iBlue = asLong(DEFAULT_ZONE_COLOR_BLUE);
	}
	else
	{
		// Retrieve blue
		strBlue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, ZONE_COLOR_BLUE,
			DEFAULT_ZONE_COLOR_BLUE );
		iBlue = ::asLong(strBlue);

		// Limit color between 0 and 255
		if (iBlue > 255)
		{
			iBlue = 255;
		}

		if (iBlue < 0)
		{
			iBlue = 0;
		}
	}

	// Combine colors into COLORREF
	color = RGB( iRed, iGreen, iBlue );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setZoneColor(COLORREF color)
{
	char	pszRed[100];
	char	pszGreen[100];
	char	pszBlue[100];
	int		iRed = 0;
	int		iGreen = 0;
	int		iBlue = 0;

	// Retrieve individual colors
	iRed = GetRValue( color );
	iGreen = GetBValue( color );
	iBlue = GetBValue( color );

	// Format strings
	sprintf_s( pszRed, sizeof(pszRed), "%d", iRed );
	sprintf_s( pszGreen, sizeof(pszGreen), "%d", iGreen );
	sprintf_s( pszBlue, sizeof(pszBlue), "%d", iBlue );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ZONE_COLOR_RED, 
		pszRed );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ZONE_COLOR_GREEN, 
		pszGreen );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, ZONE_COLOR_BLUE, 
		pszBlue );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::getUsedZoneColor(COLORREF &color)
{
	string	strRed;
	string	strGreen;
	string	strBlue;
	int		iRed = 0;
	int		iGreen = 0;
	int		iBlue = 0;

	// Check for existence of red
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, USED_ZONE_COLOR_RED))
	{
		// Not found, just default to 255
		m_pCfgMgr->createKey( m_strSectionFolderName, USED_ZONE_COLOR_RED,
			DEFAULT_USED_ZONE_COLOR_RED);
		iRed = asLong(DEFAULT_USED_ZONE_COLOR_RED);
	}
	else
	{
		// Retrieve red
		strRed = m_pCfgMgr->getKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_RED,
			DEFAULT_USED_ZONE_COLOR_RED);
		iRed = ::asLong(strRed);

		// Limit color between 0 and 255
		if (iRed > 255)
		{
			iRed = 255;
		}

		if (iRed < 0)
		{
			iRed = 0;
		}
	}

	// Check for existence of green
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, USED_ZONE_COLOR_GREEN))
	{
		// Not found, just default to 192
		m_pCfgMgr->createKey( m_strSectionFolderName, USED_ZONE_COLOR_GREEN,
			DEFAULT_USED_ZONE_COLOR_GREEN );
		iGreen = asLong(DEFAULT_USED_ZONE_COLOR_GREEN);
	}
	else
	{
		// Retrieve green
		strGreen = m_pCfgMgr->getKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_GREEN,
			DEFAULT_USED_ZONE_COLOR_GREEN);
		iGreen = ::asLong(strGreen);

		// Limit color between 0 and 255
		if (iGreen > 255)
		{
			iGreen = 255;
		}

		if (iGreen < 0)
		{
			iGreen = 0;
		}
	}

	// Check for existence of blue
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, USED_ZONE_COLOR_BLUE))
	{
		// Not found, just default to 192
		m_pCfgMgr->createKey( m_strSectionFolderName, USED_ZONE_COLOR_BLUE,
			DEFAULT_USED_ZONE_COLOR_BLUE );
		iBlue = asLong(DEFAULT_USED_ZONE_COLOR_BLUE);
	}
	else
	{
		// Retrieve blue
		strBlue = m_pCfgMgr->getKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_BLUE,
			DEFAULT_USED_ZONE_COLOR_BLUE);
		iBlue = ::asLong(strBlue);

		// Limit color between 0 and 255
		if (iBlue > 255)
		{
			iBlue = 255;
		}

		if (iBlue < 0)
		{
			iBlue = 0;
		}
	}

	// Combine colors into COLORREF
	color = RGB( iRed, iGreen, iBlue );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setUsedZoneColor(COLORREF color)
{
	char	pszRed[100];
	char	pszGreen[100];
	char	pszBlue[100];
	int		iRed = 0;
	int		iGreen = 0;
	int		iBlue = 0;

	// Retrieve individual colors
	iRed = GetRValue( color );
	iGreen = GetBValue( color );
	iBlue = GetBValue( color );

	// Format strings
	sprintf_s( pszRed, sizeof(pszRed), "%d", iRed );
	sprintf_s( pszGreen, sizeof(pszGreen), "%d", iGreen );
	sprintf_s( pszBlue, sizeof(pszBlue), "%d", iBlue );

	// Store strings
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_RED, 
		pszRed );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_GREEN, 
		pszGreen );
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, USED_ZONE_COLOR_BLUE, 
		pszBlue );
}
//--------------------------------------------------------------------------------------------------
unsigned long ConfigMgrSRIR::getNumAutoRotateSteps()
{
	unsigned long lResult = asLong(DEFAULT_NUM_AUTO_ROTATE_STEPS); // default number of steps

	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, NUM_AUTO_ROTATE_STEPS))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, NUM_AUTO_ROTATE_STEPS,
			DEFAULT_NUM_AUTO_ROTATE_STEPS);
	}
	else
	{
		// key found - return its value
		string strResult = m_pCfgMgr->getKeyValue(m_strSectionFolderName, NUM_AUTO_ROTATE_STEPS,
			DEFAULT_NUM_AUTO_ROTATE_STEPS);
		lResult = ::asLong(strResult);
	}

	return lResult;
}
//--------------------------------------------------------------------------------------------------
unsigned long ConfigMgrSRIR::getAutoRotateStepSize()
{
	unsigned long lResult = asLong(DEFAULT_AUTO_ROTATE_STEP_SIZE); // default step size

	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, AUTO_ROTATE_STEP_SIZE))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, AUTO_ROTATE_STEP_SIZE, DEFAULT_AUTO_ROTATE_STEP_SIZE);
	}
	else
	{
		// key found - return its value
		string strResult = m_pCfgMgr->getKeyValue(m_strSectionFolderName, AUTO_ROTATE_STEP_SIZE,
			DEFAULT_AUTO_ROTATE_STEP_SIZE);
		lResult = ::asLong(strResult);
	}

	return lResult;
}
//--------------------------------------------------------------------------------------------------
int ConfigMgrSRIR::getOCRRegionType()
{
	unsigned long iResult = asLong(DEFAULT_OCR_REGION_TYPE);

	// Check for existence of key
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, OCR_REGION_TYPE))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, OCR_REGION_TYPE, DEFAULT_OCR_REGION_TYPE);
	}
	else
	{
		// key found - return its value
		string strResult = m_pCfgMgr->getKeyValue(m_strSectionFolderName, OCR_REGION_TYPE,
			DEFAULT_OCR_REGION_TYPE);
		iResult = ::asLong(strResult);
	}

	// if the value is within valid range, then reset the value to the default
	// value.
	if (iResult < 0 || iResult > 2)
	{
		iResult = asLong(DEFAULT_OCR_REGION_TYPE);
		setOCRRegionType(iResult);
	}

	return iResult;
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setOCRRegionType(int iValue)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, OCR_REGION_TYPE, asString(iValue) );
}
//--------------------------------------------------------------------------------------------------
double ConfigMgrSRIR::getZonePadFactor()
{
	// set default to 1.2 (20%)
	double dPadFactor = asDouble(DEFAULT_HEIGHT_PAD_FACTOR);
	
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, HEIGHT_PAD_FACTOR))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, HEIGHT_PAD_FACTOR, DEFAULT_HEIGHT_PAD_FACTOR);
	}
	else
	{
		// key found - return its value
		string strResult = m_pCfgMgr->getKeyValue(m_strSectionFolderName, HEIGHT_PAD_FACTOR,
			DEFAULT_HEIGHT_PAD_FACTOR);
		dPadFactor = ::asDouble(strResult);
	}

	return dPadFactor;
}
//--------------------------------------------------------------------------------------------------
ETool ConfigMgrSRIR::getLastSelectionTool()
{

	// Check for existence of tool
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, LAST_SELECTION_TOOL))
	{
		// Not found, just default to 45
		m_pCfgMgr->createKey( m_strSectionFolderName, LAST_SELECTION_TOOL, DEFAULT_LAST_SELECTION_TOOL );
	}

	// Retrieve tool
	string	strTool;
	strTool = m_pCfgMgr->getKeyValue( m_strSectionFolderName, LAST_SELECTION_TOOL,
		DEFAULT_LAST_SELECTION_TOOL);
	return (ETool)asLong( strTool );
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setLastSelectionTool(ETool eTool)
{
	m_pCfgMgr->setKeyValue( m_strSectionFolderName, LAST_SELECTION_TOOL, asString((long)eTool) );
}
//--------------------------------------------------------------------------------------------------
bool ConfigMgrSRIR::getFitToStatus()
{
	// Check for existence of the status
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, FIT_TO_STATUS))
	{
		// Set to fit to page if does not exist
		m_pCfgMgr->createKey(m_strSectionFolderName, FIT_TO_STATUS, DEFAULT_FIT_TO_STATUS);
		return DEFAULT_FIT_TO_STATUS == "1";
	}

	// Retrieve the stataus
	bool bRet = m_pCfgMgr->getKeyValue(m_strSectionFolderName, FIT_TO_STATUS,
		DEFAULT_FIT_TO_STATUS) == "1";

	return bRet;
}
//----------------------------------------------------------------------------------------------
void ConfigMgrSRIR::setFitToStatus(bool bStatus)
{
	// Strore the status
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, FIT_TO_STATUS, bStatus ? "1" : "0");
}
//----------------------------------------------------------------------------------------------
bool ConfigMgrSRIR::getDisplayPercentageEnabled()
{
	// Check for existence of the status
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, DISPLAY_PERCENTAGE))
	{
		// Set to fit to page if does not exist
		m_pCfgMgr->createKey(m_strSectionFolderName, DISPLAY_PERCENTAGE,
			DEFAULT_DISPLAY_PERCENTAGE);
		return asCppBool(DEFAULT_DISPLAY_PERCENTAGE);
	}

	// Retrieve the stataus
	bool bRet = asCppBool(m_pCfgMgr->getKeyValue(m_strSectionFolderName, DISPLAY_PERCENTAGE,
		DEFAULT_DISPLAY_PERCENTAGE));

	return bRet;
}
//--------------------------------------------------------------------------------------------------
