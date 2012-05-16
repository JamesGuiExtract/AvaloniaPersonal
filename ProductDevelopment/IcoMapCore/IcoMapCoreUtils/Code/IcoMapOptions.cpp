//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapOptions.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "IcoMapOptions.h"

#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <RegConstants.h>
#include <LicenseMgmt.h>
#include <SafeNetLicenseCfg.h>
#include <SafeNetLicenseMgr.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
#include <io.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE IcoMapCoreUtilsDLL;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
//Keys
const string ICOMAP_ROOT = "\\IcoMap for ArcGIS";
const string OPTIONS_ROOT = "\\Options";
const string OPTIONS_GENERAL = ICOMAP_ROOT + OPTIONS_ROOT + "\\General";
const string DIRECTIONSHORTCUTS = ICOMAP_ROOT + OPTIONS_ROOT + "\\DirectionShortcuts";		
const string COMMANDSHORTCUTS = ICOMAP_ROOT + OPTIONS_ROOT + "\\CommandShortcuts";	
const string DIRECTION = ICOMAP_ROOT + OPTIONS_ROOT + "\\Direction";
const string LICENSEMANAGEMENT = ICOMAP_ROOT + "\\LicenseManagement";
const string PATH = ICOMAP_ROOT + "\\Path";
const string DIALOGE = ICOMAP_ROOT + "\\Dialogs";
const string CURVEDJINNI = ICOMAP_ROOT + "\\CurveDjinni";
const string ICOMAPDLG = DIALOGE + "\\IcoMapMainDialog";
const string ATTRIBUTEDLG = DIALOGE + "\\AttributeViewer";
const string IF_APP_SPECIFIC = "\\InputFunnel\\ApplicationSpecificSettings";
const string IMAGE_EDIT = "\\ReusableComponents\\OcxAndDlls\\ImageEdit";

//Value names
const string AUTO_SRC_LINKING = "AutoSourceDocLinking";
const string ATTR_FIELD_CREATION = "CreateAttrField";
const string DEFAULT_DISTANCE_UNIT = "DefaultDistanceUnit";
const string ACTIVE_PAGE_NUM = "ActiveOptionPageNum";
const string LM_SETTINGS_INITIALIZED = "LMSettingsInitialized";
const string INPUT_DIRECTION = "InputDirection";
const string LINE_ANGLE_DEFINITION = "LineAngleDefinition";
const string DIG_VISIBLE = "DIGVisible";
const string STATUSBAR_VISIBLE = "StatusBarVisible";
const string LICENSE_MANAGEMENT_MODE = "Mode";
const string LICENSE_MANAGEMENT_IORFILE = "NamingServiceIORFile";
const string LICENSE_MANAGEMENT_EVALCODE = "EvaluationCode";
const string PATH_BIN = "Bin";
const string PATH_HELP = "Help";
const string PRECISION_DIGITS = "Digits";
const string ALWAYS_ALLOW_EDITING_OF_CURRENT_ATTRIBUTES = "AlwaysAllowEditingOfCurrentAttributes";
const string LICENSE_SERVER = "LicenseServer";
const string PDF_RESOLUTION = "PDFResolution";

// Constraints
const string gstrMIN_PDF_RESOLUTION = "50";
const string gstrMAX_PDF_RESOLUTION = "300";	// also used as default
const int gnMIN_PDF_RESOLUTION      = 50;
const int gnMAX_PDF_RESOLUTION      = 300;

// Patch
const string gstrPATCH_LETTER = ""; // "", "A", "B", "C", etc

using namespace std;

// The only instance of the SafeNet license manager used for concurrent license
auto_ptr<SafeNetLicenseMgr> ga_pSnLM;

//-------------------------------------------------------------------------------------------------
IcoMapOptions::IcoMapOptions()
{
	//This is a singleton object, so all following member variables will only be initialized once
	Init();

	//Now we switch to use Registry for data persistence
	ma_pMachineCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, gstrREG_ROOT_KEY));
	ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrREG_ROOT_KEY));
}
//-------------------------------------------------------------------------------------------------
IcoMapOptions::~IcoMapOptions()
{
	releaseConcurrentIcoMapLicense();
	m_mapDirectionsToKeycodes.clear();
	m_vecCurveParametersInStrings.clear();
	m_vecCurveToolIDsInStrings.clear();
	m_mapShortcutsToStrings.clear();
}
//-------------------------------------------------------------------------------------------------
IcoMapOptions::IcoMapOptions(const IcoMapOptions& toCopy)
{
	throw UCLIDException("ELI02199", "Internal error: copy constructor of singleton class called!");
}
//-------------------------------------------------------------------------------------------------
IcoMapOptions& IcoMapOptions::operator = (const IcoMapOptions& toAssign)
{
	throw UCLIDException("ELI02200", "Internal error: assignment operator of singleton class called!");
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::enableAutoSourceDocLinking(bool bEnable)
{
	string strEnable(bEnable ? "1" : "0");
	ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, AUTO_SRC_LINKING, strEnable);
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::autoSourceDocLinkingIsEnabled()
{
	if (!ma_pUserCfgMgr->keyExists(OPTIONS_GENERAL, AUTO_SRC_LINKING))
	{
		//if key doesn't exist, set it and assign a default value to it
		ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, AUTO_SRC_LINKING, "1");
		return true;
	}

	string strEnable = ma_pUserCfgMgr->getKeyValue(OPTIONS_GENERAL, AUTO_SRC_LINKING);
	bool bEnable = (strEnable == "1");
	
	return bEnable;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::enableIcoMapAttrFieldCreation(bool bEnable)
{
	string strEnable(bEnable ? "1" : "0");
	ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, ATTR_FIELD_CREATION, strEnable);
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::isIcoMapAttrFieldCreationEnabled()
{
	if (!ma_pUserCfgMgr->keyExists(OPTIONS_GENERAL, ATTR_FIELD_CREATION))
	{
		// default to true
		ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, ATTR_FIELD_CREATION, "1");
		return true;
	}

	string strEnable = ma_pUserCfgMgr->getKeyValue(OPTIONS_GENERAL, ATTR_FIELD_CREATION);
	
	return strEnable == "1";
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::showDIG(bool bShow)
{
	string strVisible(bShow ? "1" : "0");
	ma_pUserCfgMgr->setKeyValue(ICOMAPDLG, DIG_VISIBLE, strVisible);
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::isDIGVisible()
{
	if (!ma_pUserCfgMgr->keyExists(ICOMAPDLG, DIG_VISIBLE))
	{
		//if key doesn't exist, set it and assign a default value to it
		ma_pUserCfgMgr->setKeyValue(ICOMAPDLG, DIG_VISIBLE, "1");
		return true;
	}

	string strVisible = ma_pUserCfgMgr->getKeyValue(ICOMAPDLG, DIG_VISIBLE);
	bool bShow = (strVisible == "1");
	
	return bShow;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::showStatusBar(bool bShow)
{
	string strVisible(bShow ? "1" : "0");
	ma_pUserCfgMgr->setKeyValue(ICOMAPDLG, STATUSBAR_VISIBLE, strVisible);
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::isStatusBarVisible()
{
	if (!ma_pUserCfgMgr->keyExists(ICOMAPDLG, STATUSBAR_VISIBLE))
	{
		//if key doesn't exist, set it and assign a default value to it
		ma_pUserCfgMgr->setKeyValue(ICOMAPDLG, STATUSBAR_VISIBLE, "1");
		return true;
	}

	string strVisible = ma_pUserCfgMgr->getKeyValue(ICOMAPDLG, STATUSBAR_VISIBLE);
	bool bShow = (strVisible == "1");
	
	return bShow;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setKeyboardInputCode(EDirectionType eDirection, char pszKeyCode)
{
	string strDirection("");
	map<EDirectionType, string>::iterator mapDirectionIter = m_mapDirectionsToKeycodes.find(eDirection);
	if (mapDirectionIter != m_mapDirectionsToKeycodes.end())
	{
		strDirection = mapDirectionIter->second;
	}
	else
	{
		throw UCLIDException("ELI01103", "Failed to find appropriate direction key");
	}

	string strCode("");
	if (pszKeyCode != 0)
	{
		strCode = pszKeyCode;
	}

	ma_pUserCfgMgr->setKeyValue(DIRECTIONSHORTCUTS, strDirection, strCode);
}
//-------------------------------------------------------------------------------------------------
char IcoMapOptions::getKeyboardInputCode(EDirectionType eDirection)
{
	string strDirection("");
	map<EDirectionType, string>::iterator mapDirectionIter = m_mapDirectionsToKeycodes.find(eDirection);
	if (mapDirectionIter != m_mapDirectionsToKeycodes.end())
	{
		strDirection = mapDirectionIter->second;
	}
	else
	{
		throw UCLIDException("ELI01104", 
		"Internal Error: Failed to find appropriate direction key in m_mapDirectionsToKeycodes");
	}

	string strDefaultKeycode = getDefaultKeycode(eDirection);
	char cKeyCode = strDefaultKeycode[0];
	//if this key doesn't exist
	if (!ma_pUserCfgMgr->keyExists(DIRECTIONSHORTCUTS, strDirection))
	{
		ma_pUserCfgMgr->setKeyValue(DIRECTIONSHORTCUTS, strDirection, strDefaultKeycode);
		return cKeyCode;
	}

	CString cstrKey((ma_pUserCfgMgr->getKeyValue(DIRECTIONSHORTCUTS, strDirection)).c_str());
	//if key has some value..
	if (!cstrKey.IsEmpty())
	{
		//if key value has more than one character, re-set it to default value, i.e. empty
		if (cstrKey.GetLength() != 1)
		{
			ma_pUserCfgMgr->setKeyValue(DIRECTIONSHORTCUTS, strDirection, strDefaultKeycode);
			return cKeyCode;
		}
		
		cKeyCode = cstrKey.GetAt(0);
		//if the key value is invalid, i.e. non-numeric character, set this key to default empty value
		if (!isSpecialAlphaNumeric(cstrKey))
		{
			ma_pUserCfgMgr->setKeyValue(DIRECTIONSHORTCUTS, strDirection, strDefaultKeycode);
			return cKeyCode;
		}
	}

	return cKeyCode;
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getDirection(char cKeycode)
{
	string strDirection("");
	map<EDirectionType, std::string>::iterator iter;
	for (iter = m_mapDirectionsToKeycodes.begin(); iter != m_mapDirectionsToKeycodes.end(); iter++)
	{
		// compare the keycode retrieved from persistent data with the input keycode
		if (toupper(getKeyboardInputCode(iter->first)) == toupper(cKeycode))
		{
			strDirection = iter->second;
			break;
		}
	}

	return strDirection;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setShortcutCode(EShortcutType eShortcut, std::string strShortcut)
{
	string strLabel("");

	// Search map for the label
	map<EShortcutType, string>::iterator mapShortcutIter = 
		m_mapShortcutsToStrings.find( eShortcut );

	// Make sure label was found
	if (mapShortcutIter != m_mapShortcutsToStrings.end())
	{
		// Retrieve label
		strLabel = mapShortcutIter->second;
	}
	else
	{
		throw UCLIDException( "ELI01824", 
			"Failed to find appropriate shortcut label" );
	}

	// Store the shortcut
	ma_pUserCfgMgr->setKeyValue( COMMANDSHORTCUTS, strLabel, strShortcut );
}
//-------------------------------------------------------------------------------------------------
std::string IcoMapOptions::getShortcut(EShortcutType eShortcut)
{
	string strShortcut("");
	string strLabel("");

	// Search map for the label
	map<EShortcutType, string>::iterator mapShortcutIter = 
		m_mapShortcutsToStrings.find( eShortcut );

	// Make sure label was found
	if (mapShortcutIter != m_mapShortcutsToStrings.end())
	{
		// Retrieve label
		strLabel = mapShortcutIter->second;
	}
	else
	{
		throw UCLIDException( "ELI01828", 
			"Failed to find appropriate shortcut label" );
	}

	// Get default
	string strDefault = getDefaultShortcut( eShortcut );

	// Check if this key exists in INI file
	if (!ma_pUserCfgMgr->keyExists( COMMANDSHORTCUTS, strLabel ))
	{
		// Ket does not exist, provide default value
		ma_pUserCfgMgr->setKeyValue( COMMANDSHORTCUTS, strLabel, strDefault );
		return strDefault;
	}
	else
	{
		// Key exists and has value
		strShortcut = ma_pUserCfgMgr->getKeyValue( COMMANDSHORTCUTS, strLabel );
		return strShortcut;
	}
}
//-------------------------------------------------------------------------------------------------
EShortcutType IcoMapOptions::getShortcutType(std::string strShortcut)
{
	string strTest("");

	EShortcutType eShortcutType = kShortcutNull;
	// Iterate through labels
	map<EShortcutType, std::string>::iterator iter;
	for (iter = m_mapShortcutsToStrings.begin(); iter != m_mapShortcutsToStrings.end(); iter++)
	{
		// Get the string associated with this label from map
		strTest = getShortcut( iter->first );

		// Case-insensitive comparison of strings
		if (_strcmpi( strTest.c_str(), strShortcut.c_str() ) == 0)
		{
			eShortcutType = iter->first;
			// Found a match, stop searching
			break;
		}
	}

	return eShortcutType;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setActiveOptionPageNum(int iPageNum)
{
	CString cstrPageNum("0");
	// now total number of option pages is 5, if there are pages added/removed, modify this number
	if (iPageNum < 0 || iPageNum > 4)
	{
		UCLIDException uclidException("ELI01195", "Invalid option dialog page number.");
		uclidException.addDebugInfo("iPageNum", iPageNum);
		throw uclidException;
	}
	
	cstrPageNum.Format("%d", iPageNum);

	ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, ACTIVE_PAGE_NUM, static_cast<LPCTSTR>(cstrPageNum));
}
//-------------------------------------------------------------------------------------------------
int IcoMapOptions::getActiveOptionPageNum()
{
	if (!ma_pUserCfgMgr->keyExists(OPTIONS_GENERAL, ACTIVE_PAGE_NUM))
	{
		ma_pUserCfgMgr->setKeyValue(OPTIONS_GENERAL, ACTIVE_PAGE_NUM, "0");
		return 0;
	}

	string strPageNum(ma_pUserCfgMgr->getKeyValue(OPTIONS_GENERAL, ACTIVE_PAGE_NUM));

	int iPageNum = atoi(strPageNum.c_str());
	if (iPageNum < 0 || iPageNum > 4)
	{
		iPageNum = 0;
	}

	return iPageNum;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setCurveToolParameter(ECurveToolID eCurveToolID, 
										  int nParameterNum, 
										  ECurveParameterType eCurveParameter)
{
	string strCurveToolID (m_vecCurveToolIDsInStrings[eCurveToolID]);
	string strCurveParameter (m_vecCurveParametersInStrings[eCurveParameter]);
	string strParameterNum (m_vecParameterNumsInStrings[nParameterNum - 1]);
	ma_pUserCfgMgr->setKeyValue(CURVEDJINNI+ "\\"+ strCurveToolID, strParameterNum, strCurveParameter);
}
//-------------------------------------------------------------------------------------------------
ECurveParameterType IcoMapOptions::getCurveToolParameter(ECurveToolID eCurveToolID, int nParameterNum)
{
	string strCurveToolID(m_vecCurveToolIDsInStrings[eCurveToolID]);

	//get default parameters for this curve tool type
	vector<ECurveParameterType> vecDefaultCurveParameters = getCurveToolDefaultParameters(eCurveToolID);
	//retrieve a string out from m_vecParameterNumsInStrings(which is 0-based)
	string strParameterNum(m_vecParameterNumsInStrings[nParameterNum - 1]);

	//if this key doesn't exist or it has no value, set it to default value 
	//and at same time the other keys in the same section also need to be set to default value
	string strFolderFullPath = CURVEDJINNI + "\\" + strCurveToolID;
	if (!ma_pUserCfgMgr->keyExists(strFolderFullPath, strParameterNum)
		|| ma_pUserCfgMgr->getKeyValue(strFolderFullPath, strParameterNum).empty())
	{
		for (int i=0; i<3; i++)
		{
			ma_pUserCfgMgr->setKeyValue(strFolderFullPath, 
				m_vecParameterNumsInStrings[i], 
				m_vecCurveParametersInStrings[vecDefaultCurveParameters[i]]);
		}
		return vecDefaultCurveParameters[nParameterNum-1];
	}

	//else, check if the key is valid
	string strCurveParameter(ma_pUserCfgMgr->getKeyValue(strFolderFullPath, strParameterNum));
	vector<string>::iterator iter = find(m_vecCurveParametersInStrings.begin(), 
										 m_vecCurveParametersInStrings.end(), 
										 strCurveParameter);
	if (iter == m_vecCurveParametersInStrings.end())
	{
		//Invalid parameter value found!
		//Then set it to default value and at same time the other
		//keys in the same section also need to be set to default value
		for (int i=0; i<3; i++)
		{
			ma_pUserCfgMgr->setKeyValue(strFolderFullPath, 
				m_vecParameterNumsInStrings[i], 
				m_vecCurveParametersInStrings[vecDefaultCurveParameters[i]]);
		}

		return vecDefaultCurveParameters[nParameterNum-1];
	}

	//else, get the index for this string inside m_vecCurveParametersInStrings
	int nCurveParameter = distance(m_vecCurveParametersInStrings.begin(), iter);
	ECurveParameterType eCurveParameter = static_cast<ECurveParameterType>(nCurveParameter);
	return eCurveParameter;
}
//-------------------------------------------------------------------------------------------------
vector<ECurveParameterType> IcoMapOptions::getCurveToolParameters(ECurveToolID eCurveToolID)
{
	vector<ECurveParameterType> parameters;
	parameters.clear();
	for (unsigned int ui = 1; ui <= m_vecParameterNumsInStrings.size(); ui++)
	{
		parameters.push_back(getCurveToolParameter(eCurveToolID, (int)ui));
	}

	return parameters;
}
//-------------------------------------------------------------------------------------------------
vector<ECurveParameterType> IcoMapOptions::getCurveToolDefaultParameters(ECurveToolID eCurveToolID)
{
	vector<ECurveParameterType> parameters;
	parameters.clear();
	switch(eCurveToolID)
	{
	case kCurve1:
		parameters.push_back(kArcTangentInBearing);
		parameters.push_back(kArcRadius);
		parameters.push_back(kArcDelta);
		break;
	case kCurve2:
		parameters.push_back(kArcTangentInBearing);
		parameters.push_back(kArcRadius);
		parameters.push_back(kArcChordLength);
		break;
	case kCurve3:
		parameters.push_back(kArcRadius);
		parameters.push_back(kArcChordBearing);
		parameters.push_back(kArcChordLength);
		break;
	case kCurve4:
		parameters.push_back(kArcTangentInBearing);
		parameters.push_back(kArcDegreeOfCurveArcDef);
		parameters.push_back(kArcChordLength);
		break;
	case kCurve5:
		parameters.push_back(kArcRadialInBearing);
		parameters.push_back(kArcRadius);
		parameters.push_back(kArcLength);
		break;
	case kCurve6:
		parameters.push_back(kArcTangentInBearing);
		parameters.push_back(kArcTangentOutBearing);
		parameters.push_back(kArcRadius);
		break;
	case kCurve7:
		parameters.push_back(kArcRadialInBearing);
		parameters.push_back(kArcDelta);
		parameters.push_back(kArcRadius);
		break;
	case kCurve8:
		parameters.push_back(kArcRadialInBearing);
		parameters.push_back(kArcRadialOutBearing);
		parameters.push_back(kArcRadius);
		break;
	default:
		throw UCLIDException("ELI01136", "Invalid curve tool ID.");
		break;
	}

	return parameters;
}
//-------------------------------------------------------------------------------------------------
IConfigurationSettingsPersistenceMgr* IcoMapOptions::getUserPersistenceMgr()
{
	return ma_pUserCfgMgr.get();
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::isSpecialAlphaNumeric(CString cstrChar)
{
	if (cstrChar.GetLength() != 1)
	{
		return false;
	}

	if( (cstrChar >= "A" && cstrChar <= "Z" && cstrChar != "N" && cstrChar != "E" 
			&& cstrChar != "S"  && cstrChar != "W")
		|| (cstrChar >= "a" && cstrChar <= "z" && cstrChar != "n" && cstrChar != "e" 
			&& cstrChar != "s"  && cstrChar != "w") 
		|| (cstrChar >= "0" && cstrChar <= "9") ) 
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getDefaultKeycode(EDirectionType eDirection)
{
	string strKeycode("");
	switch (eDirection)
	{
	case kN:
		strKeycode = "8";
		break;
	case kE:
		strKeycode = "6";
		break;
	case kS:
		strKeycode = "2";
		break;
	case kW:
		strKeycode = "4";
		break;
	case kNE:
		strKeycode = "9";
		break;
	case kSE:
		strKeycode = "3";
		break;
	case kSW:
		strKeycode = "1";
		break;
	case kNW:
		strKeycode = "7";
		break;
	default:
		{
			UCLIDException uclidException("ELI01212", "Invalid direction");
			uclidException.addDebugInfo("eDirection", (int)eDirection);
			throw uclidException;
		}
		break;
	}

	return strKeycode;
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getDefaultShortcut(EShortcutType eShortcut)
{
	string strShortcut("");

	switch (eShortcut)
	{
	case kShortcutCurve1:
		strShortcut = "1";
		break;

	case kShortcutCurve2:
		strShortcut = "2";
		break;

	case kShortcutCurve3:
		strShortcut = "3";
		break;

	case kShortcutCurve4:
		strShortcut = "4";
		break;

	case kShortcutCurve5:
		strShortcut = "5";
		break;

	case kShortcutCurve6:
		strShortcut = "6";
		break;

	case kShortcutCurve7:
		strShortcut = "7";
		break;

	case kShortcutCurve8:
		strShortcut = "8";
		break;

	case kShortcutLine:
		strShortcut = "0";
		break;

	case kShortcutLineAngle:
		strShortcut = "A";
		break;

	case kShortcutGenie:
		strShortcut = "9";
		break;

	case kShortcutRight:
		strShortcut = "*";
		break;

	case kShortcutLeft:
		strShortcut = "/";
		break;

	case kShortcutGreater:
		strShortcut = "+";
		break;

	case kShortcutLess:
		strShortcut = "-";
		break;

	case kShortcutForward:
		strShortcut = "F";
		break;

	case kShortcutReverse:
		strShortcut = "R";
		break;

	case kShortcutFinishSketch:
		strShortcut = ".";
		break;

	case kShortcutFinishPart:
		strShortcut = "P";
		break;

	case kShortcutDeleteSketch:
		strShortcut = "X";
		break;

	default:
		{
			UCLIDException uclidException( "ELI01829", "Invalid shortcut key" );
			uclidException.addDebugInfo( "eShortcut", (int)eShortcut );
			throw uclidException;
		}
		break;
	}

	return strShortcut;
}
//-------------------------------------------------------------------------------------------------
ELicenseManagementMode IcoMapOptions::getLicenseManagementMode()
{
	const ELicenseManagementMode eDefaultLicensingMode = kNodeLocked;

	if (ma_pMachineCfgMgr->keyExists(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_MODE))
	{
		// Retrieve license mode string
		string strValue = ma_pMachineCfgMgr->getKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_MODE);

		// Convert string to integer
		// Default mode is kNodeLocked
		int		iMode = kNodeLocked;
		iMode = (int)asLong( strValue );

		// Check bounds
		if ((iMode >= kNodeLocked) && (iMode < kLicenseCount))
		{
			return static_cast<ELicenseManagementMode>(iMode);
		}
		else
		{
			// TODO: record error & use default licensing mode.
			return eDefaultLicensingMode;
		}
	}
	else
	{
		// return the default licensing mode.
		return eDefaultLicensingMode;
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setLicenseManagementMode(ELicenseManagementMode eMode)
{
	ma_pMachineCfgMgr->setKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_MODE, asString((int) eMode));
}
//-------------------------------------------------------------------------------------------------
/*
string IcoMapOptions::getEvalLicenseCode()
{
	// proceed only if the current licensing mode is kEvaluation
	ASSERT_ARGUMENT("ELI02132", getLicenseManagementMode() == kEvaluation);

	if (!ma_pMachineCfgMgr->keyExists(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_EVALCODE))
	{
		throw UCLIDException("ELI02133", "Evalution code not specified!");
	}
	
	return ma_pMachineCfgMgr->getKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_EVALCODE);
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setEvalLicenseCode(const string& strEvalCode)
{
	// proceed only if the current licensing mode is kEvaluation
	ASSERT_ARGUMENT("ELI02134", getLicenseManagementMode() == kEvaluation);

	ma_pMachineCfgMgr->setKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_EVALCODE, strEvalCode);
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getNamingServiceIORFile()
{
	// proceed only if the current licensing mode is kFloating
	ASSERT_ARGUMENT("ELI01378", getLicenseManagementMode() == kFloating);

	if (!ma_pMachineCfgMgr->keyExists(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_IORFILE))
	{
		throw UCLIDException("ELI01379", "Naming Service IOR file not specified!");
	}
	
	return ma_pMachineCfgMgr->getKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_IORFILE);
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setNamingServiceIORFile(const string& strIORFile)
{
	// proceed only if the current licensing mode is kFloating
	ASSERT_ARGUMENT("ELI01494", getLicenseManagementMode() == kFloating);
	
	ma_pMachineCfgMgr->setKeyValue(LICENSEMANAGEMENT, LICENSE_MANAGEMENT_IORFILE, strIORFile);
}*/
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getBinDirectory()
{
	// read version information from the IcoMapCoreUtils.dll which is expected
	// to be in the bin directory.
	return getModuleDirectory(IcoMapCoreUtilsDLL.hModule);
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getHelpDirectory()
{
	return ma_pMachineCfgMgr->getKeyValue(PATH, PATH_HELP);
}
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getProductVersion()
{
	// get the full path to the IcoMapApp.dll file
	string strIcoMapAppDLLPath = getBinDirectory() + "\\";
	strIcoMapAppDLLPath += "IcoMapApp.dll";

	string strVersionLabel = "IcoMap for ArcGIS Ver. ";

	strVersionLabel += ::getFileVersion(strIcoMapAppDLLPath);

	// Add patch character, if defined
	strVersionLabel += gstrPATCH_LETTER.c_str();

	return strVersionLabel;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setPrecisionDigits(int iDigits)
{
	int iActual = iDigits;

	// Sanity check
	if ((iDigits < giMIN_PRECISION_DIGITS) || 
		(iDigits > giMAX_PRECISION_DIGITS))
	{
		// Just use default if out of bounds
		iActual = giDEFAULT_PRECISION_DIGITS;
	}

	// Create string for storage
	char	pszDigits[10];
	sprintf_s( pszDigits, sizeof(pszDigits), "%d", iActual );

	// Store string
	ma_pUserCfgMgr->setKeyValue( ATTRIBUTEDLG, PRECISION_DIGITS, pszDigits );
}
//-------------------------------------------------------------------------------------------------
int IcoMapOptions::getPrecisionDigits()
{
	int		iActual = 0;
	string	strDigits;

	// Check for existence of key
	if (!ma_pUserCfgMgr->keyExists( ATTRIBUTEDLG, PRECISION_DIGITS ))
	{
		// Not found, just use default
		char	pszDigits[10];
		sprintf_s( pszDigits, sizeof(pszDigits), "%d", giDEFAULT_PRECISION_DIGITS );
		ma_pUserCfgMgr->createKey( ATTRIBUTEDLG, PRECISION_DIGITS, pszDigits );
		iActual = giDEFAULT_PRECISION_DIGITS;
	}
	else
	{
		// Retrieve value
		strDigits = ma_pUserCfgMgr->getKeyValue( ATTRIBUTEDLG, PRECISION_DIGITS );
		
		// Convert to integer
		iActual = atoi( strDigits.c_str() );

		// Sanity check
		if ((iActual < giMIN_PRECISION_DIGITS) || 
			(iActual > giMAX_PRECISION_DIGITS))
		{
			// Just use default if out of bounds
			iActual = giDEFAULT_PRECISION_DIGITS;
		}
	}

	return iActual;
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::alwaysAllowEditingOfCurrentAttributes()
{
	// Duan's comment on 8/8/03
	// By default, this value is set to true to allow editing of current
	// feature. (It was default to false before 8/8/03 because of the 
	// whole coverage "back door" thing. Since ArcGIS 8.3 no longer support
	// editing of coverage, this "back door" is now meaningless to coverage.)
	bool bDefaultValue = true;

	// Check for existence of key
	if (!ma_pMachineCfgMgr->keyExists(ATTRIBUTEDLG, ALWAYS_ALLOW_EDITING_OF_CURRENT_ATTRIBUTES))
	{
		return bDefaultValue;
	}
	else
	{
		// Retrieve value
		string strValue = ma_pMachineCfgMgr->getKeyValue(ATTRIBUTEDLG, 
			ALWAYS_ALLOW_EDITING_OF_CURRENT_ATTRIBUTES);
		
		if (strValue == "0")
		{
			return false;
		}
		else 
		{
			return true; // anything non-zero is true.
		}
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setInputDirection(int nDirection)
{
	CString cstrDirection("");
	cstrDirection.Format("%d", nDirection);
	ma_pUserCfgMgr->setKeyValue(DIRECTION, INPUT_DIRECTION, (LPCTSTR)cstrDirection);
}
//-------------------------------------------------------------------------------------------------
int IcoMapOptions::getInputDirection()
{
	string strDirection;
	int nDirection;

	// Check for existence of key
	if (!ma_pUserCfgMgr->keyExists(DIRECTION, INPUT_DIRECTION))
	{
		strDirection = "1";
		// Not found, create a default to Bearing
		ma_pUserCfgMgr->createKey(DIRECTION, INPUT_DIRECTION, strDirection);
	}
	else
	{
		// Retrieve value
		strDirection = ma_pUserCfgMgr->getKeyValue(DIRECTION, INPUT_DIRECTION);
	}

	try
	{
		// convert to int
		nDirection = asUnsignedLong(strDirection);
	}
	catch (UCLIDException& uclidException)
	{
		uclidException.addDebugInfo("ELI02867", "Invalid input Direction");
		throw uclidException;
	}

	return nDirection;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setDefaultDistanceUnitType(EDistanceUnitType eDefaultUnit)
{
	// get application specific key
	string strAppKey(getApplicationSpecificRoot());
	
	CString cstrDefaultUnit("");
	cstrDefaultUnit.Format("%d", eDefaultUnit);
	ma_pUserCfgMgr->setKeyValue(strAppKey, DEFAULT_DISTANCE_UNIT, (LPCTSTR)cstrDefaultUnit);
}
//-------------------------------------------------------------------------------------------------
EDistanceUnitType IcoMapOptions::getDefaultDistanceUnitType()
{
	string strDefaultUnit("1");

	// get application specific key
	string strAppKey(getApplicationSpecificRoot());

	// Check for existence of key
	if (!ma_pUserCfgMgr->keyExists(strAppKey, DEFAULT_DISTANCE_UNIT))
	{
		// default to kFeet if the key is not there
		ma_pUserCfgMgr->createKey(strAppKey, DEFAULT_DISTANCE_UNIT, strDefaultUnit);
	}
	else
	{
		strDefaultUnit = ma_pUserCfgMgr->getKeyValue(strAppKey, DEFAULT_DISTANCE_UNIT);
	}

	unsigned long nUnit = 0;
	try
	{
		nUnit = ::asUnsignedLong(strDefaultUnit);
		if (nUnit == 0 || nUnit > kKilometers)
		{
			// go to the catch block
			throw 0;
		}
	}
	catch (...)
	{
		strDefaultUnit = "1";
		ma_pUserCfgMgr->setKeyValue(strAppKey, DEFAULT_DISTANCE_UNIT, strDefaultUnit);
		nUnit = 1;
	}

	// convert to int
	EDistanceUnitType eDefaultType = static_cast<EDistanceUnitType>(nUnit);

	return eDefaultType;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setLineAngleDefinition(bool bIsDeflectionAngle)
{
	string strAngleDef("");
	if (bIsDeflectionAngle)
	{
		// deflection angle is 1 (true)
		strAngleDef = "1";
	}
	else
	{
		// internal angle is 0 (false)
		strAngleDef = "0";
	}

	ma_pUserCfgMgr->setKeyValue(DIRECTION, LINE_ANGLE_DEFINITION, strAngleDef);
}
//-------------------------------------------------------------------------------------------------
bool IcoMapOptions::isDefinedAsDeflectionAngle()
{
	string strAngleDef;
	int nAngleDef;

	// Check for existence of key
	if (!ma_pUserCfgMgr->keyExists(DIRECTION, LINE_ANGLE_DEFINITION))
	{
		strAngleDef = "1";
		// Not found, create a default as deflection angle
		ma_pUserCfgMgr->createKey(DIRECTION, LINE_ANGLE_DEFINITION, strAngleDef);
	}
	else
	{
		// Retrieve value
		strAngleDef = ma_pUserCfgMgr->getKeyValue(DIRECTION, LINE_ANGLE_DEFINITION);
	}

	try
	{
		// convert to int
		nAngleDef = asUnsignedLong(strAngleDef);
	}
	catch (UCLIDException& uclidException)
	{
		uclidException.addDebugInfo("ELI03032", "Invalid line angle definition");
		throw uclidException;
	}

	return (nAngleDef == 1);
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::validateIcoMapLicensed()
{
	// IcoMapOptions must be licensed at the ARCGIS_OBJECTS level because each 
	// component licensed at this level will call this method to validate its 
	// license state through EITHER a license file for the node-locked case OR 
	// a USB key for the concurrent case.

	// Define license ID used for node-locked license testing
	static const unsigned long ICOMAP_COMPONENT_ID = gnICOMAP_ARCGIS_OBJECTS;
	SafeNetLicenseCfg snlCfg;
	
	// Check registry for node-locked or concurrent mode
	ELicenseManagementMode eMode = IcoMapOptions::sGetInstance().getLicenseManagementMode();
	
	if ( eMode == kConcurrent )
	{
		if ( ga_pSnLM.get() == NULL )
		{
			// Create License manager object and get a license
			ga_pSnLM = auto_ptr<SafeNetLicenseMgr>(new SafeNetLicenseMgr(gusblIcoMap, true));
			ASSERT_RESOURCE_ALLOCATION("ELI13475", ga_pSnLM.get() != NULL );
		}
		// make sure the heartbeat thread is still running
		try
		{
			ga_pSnLM->validateUSBLicense();
			ga_pSnLM->validateHeartbeatActive();
		}
		catch (UCLIDException ue )
		{
			// Set the pointer to null, that way if the this is a concurrent license it
			// will get it the next time get a license if it is available
			ga_pSnLM.reset(NULL);
			throw ue;
		}
	}
	else
	{
		VALIDATE_LICENSE( ICOMAP_COMPONENT_ID, "ELI13476", "IcoMap" );
	}
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::releaseConcurrentIcoMapLicense()
{
	ga_pSnLM.reset(NULL);
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::setPDFResolution(int nDotsPerInch)
{
	// Create string for storage
	string strResolution = asString( nDotsPerInch );

	// Store string
	ma_pUserCfgMgr->setKeyValue( IMAGE_EDIT, PDF_RESOLUTION, strResolution );
}
//-------------------------------------------------------------------------------------------------
int IcoMapOptions::getPDFResolution()
{
	// Check for registry key existence
	if (!ma_pUserCfgMgr->keyExists( IMAGE_EDIT, PDF_RESOLUTION ))
	{
		// If key doesn't exist, set it and assign a default value to it
		ma_pUserCfgMgr->setKeyValue( IMAGE_EDIT, PDF_RESOLUTION, gstrMAX_PDF_RESOLUTION );
		return gnMAX_PDF_RESOLUTION;
	}

	// Retrieve setting
	string strResolution = ma_pUserCfgMgr->getKeyValue( IMAGE_EDIT, PDF_RESOLUTION );
	
	// Convert string to integer
	int nRes = asLong( strResolution );

	// Constrain resolution to be MIN <= nRes <= MAX
	if (nRes < gnMIN_PDF_RESOLUTION)
	{
		nRes = gnMIN_PDF_RESOLUTION;
	}
	else if (nRes > gnMAX_PDF_RESOLUTION)
	{
		nRes = gnMAX_PDF_RESOLUTION;
	}

	// Return constrained result
	return nRes;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
string IcoMapOptions::getApplicationSpecificRoot()
{
	// get current application name
	string strAppName = ::getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
	string strRes = IF_APP_SPECIFIC + "\\" + strAppName;

	return strRes;
}
//-------------------------------------------------------------------------------------------------
void IcoMapOptions::Init()
{
	// Clear the maps and vectors
	m_mapDirectionsToKeycodes.clear();
	m_vecCurveParametersInStrings.clear();
	m_vecCurveToolIDsInStrings.clear();
	m_mapShortcutsToStrings.clear();

	// Populate the Directions map
	m_mapDirectionsToKeycodes[kN] = "N";
	m_mapDirectionsToKeycodes[kE] = "E";
	m_mapDirectionsToKeycodes[kS] = "S";
	m_mapDirectionsToKeycodes[kW] = "W";
	m_mapDirectionsToKeycodes[kNE] = "NE";
	m_mapDirectionsToKeycodes[kSE] = "SE";
	m_mapDirectionsToKeycodes[kSW] = "SW";
	m_mapDirectionsToKeycodes[kNW] = "NW";

	// Populate the Shortcuts map
	m_mapShortcutsToStrings[kShortcutCurve1]		= "Curve1";
	m_mapShortcutsToStrings[kShortcutCurve2]		= "Curve2";
	m_mapShortcutsToStrings[kShortcutCurve3]		= "Curve3";
	m_mapShortcutsToStrings[kShortcutCurve4]		= "Curve4";
	m_mapShortcutsToStrings[kShortcutCurve5]		= "Curve5";
	m_mapShortcutsToStrings[kShortcutCurve6]		= "Curve6";
	m_mapShortcutsToStrings[kShortcutCurve7]		= "Curve7";
	m_mapShortcutsToStrings[kShortcutCurve8]		= "Curve8";
	m_mapShortcutsToStrings[kShortcutGenie]			= "Genie";
	m_mapShortcutsToStrings[kShortcutLine]			= "Line";
	m_mapShortcutsToStrings[kShortcutLineAngle]		= "DefAngLine";
	m_mapShortcutsToStrings[kShortcutRight]			= "Right";
	m_mapShortcutsToStrings[kShortcutLeft]			= "Left";
	m_mapShortcutsToStrings[kShortcutGreater]		= "Greater";
	m_mapShortcutsToStrings[kShortcutLess]			= "Less";
	m_mapShortcutsToStrings[kShortcutForward]		= "Forward";
	m_mapShortcutsToStrings[kShortcutReverse]		= "Reverse";
	m_mapShortcutsToStrings[kShortcutFinishSketch]	= "FinishSketch";
	m_mapShortcutsToStrings[kShortcutFinishPart]	= "FinishPart";
	m_mapShortcutsToStrings[kShortcutDeleteSketch]	= "DeleteSketch";
	m_mapShortcutsToStrings[kShortcutUndo]			= "Undo";
	m_mapShortcutsToStrings[kShortcutRedo]			= "Redo";
	m_mapShortcutsToStrings[kShortcutEnter]			= "Enter";


	//populate the m_vecCurveParametersInStrings with relative string names
	// since now the valid arc parameters starts from 1, let's just put kInvalidParameterType
	// as a dummy parameter in the vector
	m_vecCurveParametersInStrings.push_back("kInvalidParameterType");
	m_vecCurveParametersInStrings.push_back("kArcConcaveLeft");
	m_vecCurveParametersInStrings.push_back("kArcDeltaGreaterThan180Degrees");
	m_vecCurveParametersInStrings.push_back("kArcDelta");
	m_vecCurveParametersInStrings.push_back("kArcStartAngle");
	m_vecCurveParametersInStrings.push_back("kArcEndAngle");
	m_vecCurveParametersInStrings.push_back("kArcDegreeOfCurveChordDef");
	m_vecCurveParametersInStrings.push_back("kArcDegreeOfCurveArcDef");
	m_vecCurveParametersInStrings.push_back("kArcTangentInBearing");
	m_vecCurveParametersInStrings.push_back("kArcTangentOutBearing");
	m_vecCurveParametersInStrings.push_back("kArcChordBearing");
	m_vecCurveParametersInStrings.push_back("kArcRadialInBearing");
	m_vecCurveParametersInStrings.push_back("kArcRadialOutBearing");
	m_vecCurveParametersInStrings.push_back("kArcRadius");
	m_vecCurveParametersInStrings.push_back("kArcLength");
	m_vecCurveParametersInStrings.push_back("kArcChordLength");
	m_vecCurveParametersInStrings.push_back("kArcExternalDistance");
	m_vecCurveParametersInStrings.push_back("kArcMiddleOrdinate");
	m_vecCurveParametersInStrings.push_back("kArcTangentDistance");
	m_vecCurveParametersInStrings.push_back("kArcStartingPoint");
	m_vecCurveParametersInStrings.push_back("kArcMidPoint");
	m_vecCurveParametersInStrings.push_back("kArcEndingPoint");
	m_vecCurveParametersInStrings.push_back("kArcCenter");
	m_vecCurveParametersInStrings.push_back("kArcExternalPoint");
	m_vecCurveParametersInStrings.push_back("kArcChordMidPoint");

	//populate the m_vecCurveToolIDsInStrings with relative string names
	m_vecCurveToolIDsInStrings.push_back("kCurve1");
	m_vecCurveToolIDsInStrings.push_back("kCurve2");
	m_vecCurveToolIDsInStrings.push_back("kCurve3");
	m_vecCurveToolIDsInStrings.push_back("kCurve4");
	m_vecCurveToolIDsInStrings.push_back("kCurve5");
	m_vecCurveToolIDsInStrings.push_back("kCurve6");
	m_vecCurveToolIDsInStrings.push_back("kCurve7");
	m_vecCurveToolIDsInStrings.push_back("kCurve8");

	//vector to contain key names for parameter1, 2, and 3
	m_vecParameterNumsInStrings.push_back("Parameter1");
	m_vecParameterNumsInStrings.push_back("Parameter2");
	m_vecParameterNumsInStrings.push_back("Parameter3");
}
//-------------------------------------------------------------------------------------------------
