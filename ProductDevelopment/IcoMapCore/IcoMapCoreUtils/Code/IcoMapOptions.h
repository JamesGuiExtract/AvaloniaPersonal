//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapOptions.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//-------------------------------------------------------------------------------------------------
#pragma once

#include "IcoMapCoreUtils.h"
#include "EDirectionType.h"
#include "ECurveToolID.h"
#include "EShortcutType.h"

#include <ECurveParameter.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <Singleton.h>

#include <string>
#include <vector>
#include <map>
#include <memory>

enum ELicenseManagementMode
{
	kNodeLocked = 0,
	kConcurrent,
	kLicenseCount			// Placeholder for bounds checking
};

// Definitions
const int	giMIN_PRECISION_DIGITS		= 1;
const int	giDEFAULT_PRECISION_DIGITS	= 2;
const int	giMAX_PRECISION_DIGITS		= 10;

class EXPORT_IcoMapCoreUtils IcoMapOptions: public Singleton<IcoMapOptions>
{
	ALLOW_SINGLETON_ACCESS(IcoMapOptions);
public:
	~IcoMapOptions();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Get a pointer to the object of a Configuration manager
	//
	IConfigurationSettingsPersistenceMgr *getUserPersistenceMgr();
	//---------------------------------------------------------------------------------------------
	void enableAutoSourceDocLinking(bool bEnable);
	//---------------------------------------------------------------------------------------------
	bool autoSourceDocLinkingIsEnabled();
	//---------------------------------------------------------------------------------------------
	void enableIcoMapAttrFieldCreation(bool bEnable);
	//---------------------------------------------------------------------------------------------
	bool isIcoMapAttrFieldCreationEnabled();
	//---------------------------------------------------------------------------------------------
//	void showHistory(bool bShow);
	//---------------------------------------------------------------------------------------------
//	bool isHistoryVisible();
	//---------------------------------------------------------------------------------------------
	// show/hide dynamic input grid
	void showDIG(bool bShow);
	//---------------------------------------------------------------------------------------------
	bool isDIGVisible();
	//---------------------------------------------------------------------------------------------
	// show/hide status bar on the bottom of the icomap dialog
	void showStatusBar(bool bShow);
	//---------------------------------------------------------------------------------------------
	bool isStatusBarVisible();
	//---------------------------------------------------------------------------------------------
	void setKeyboardInputCode(EDirectionType eDirection, char pszKeyCode);
	//---------------------------------------------------------------------------------------------
	char getKeyboardInputCode(EDirectionType eDirection);
	//---------------------------------------------------------------------------------------------
	// Retrieves direction string according to the input keycode
	std::string getDirection(char cKeycode);
	//---------------------------------------------------------------------------------------------
	void setActiveOptionPageNum(int iPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: get last active page number from options dlg
	//
	int getActiveOptionPageNum();
	//---------------------------------------------------------------------------------------------
	//REQUIRE: strParameterNum must be one of the following: P1, P2, P3
	void setCurveToolParameter(ECurveToolID eCurveToolID, int nParameterNum, ECurveParameterType eCurveParameter);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get curve parameter based upon a certain curve tool id and parameter id number
	ECurveParameterType getCurveToolParameter(ECurveToolID eCurveToolID, int nParameterNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get all curve parameters for a certain curve tool
	std::vector<ECurveParameterType> getCurveToolParameters(ECurveToolID eCurveToolID);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get default curve parameters for a certain curve tool
	std::vector<ECurveParameterType> getCurveToolDefaultParameters(ECurveToolID eCurveToolID);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the mode of licensing that this product should use.
	ELicenseManagementMode getLicenseManagementMode();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the mode of licensing that this product should use.
	void setLicenseManagementMode(ELicenseManagementMode eMode);
	//---------------------------------------------------------------------------------------------
	// Retrieves evaluation license code from user
	// REQUIRE: getLicenseManagementMode() == kEvalution.
	//std::string getEvalLicenseCode();
	//---------------------------------------------------------------------------------------------
	// Stores evaluation license code from user
	// REQUIRE: getLicenseManagementMode() == kEvaluation.
	//void setEvalLicenseCode(const std::string& strEvalCode);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the name of the file containing the IOR for the naming service
	// REQUIRE: getLicenseManagementMode() == kFloating.
	//std::string getNamingServiceIORFile();
	//---------------------------------------------------------------------------------------------
	// REQUIRE: getLicenseManagementMode() == kFloating.
	//void setNamingServiceIORFile(const std::string& strIORFile);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the location of the bin directory.
	std::string getBinDirectory();
	//---------------------------------------------------------------------------------------------
	std::string getProductVersion();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the location of the Help directory.
	std::string getHelpDirectory();
	//---------------------------------------------------------------------------------------------
	void setShortcutCode(EShortcutType eShortcut, std::string strShortcut);
	//---------------------------------------------------------------------------------------------
	std::string getShortcut(EShortcutType eShortcut);
	//---------------------------------------------------------------------------------------------
	// Retrieves direction string according to the input keycode
	EShortcutType getShortcutType(std::string strShortcut);
	//---------------------------------------------------------------------------------------------
	void setPrecisionDigits(int iDigits);
	//---------------------------------------------------------------------------------------------
	int getPrecisionDigits();
	//---------------------------------------------------------------------------------------------
	bool alwaysAllowEditingOfCurrentAttributes();
	//---------------------------------------------------------------------------------------------
	// nDirection: 1, 2, or 3
	// 1 -- Bearing Direction
	// 2 -- Polar Angle Direction
	// 3 -- Azimuth Direction
	void setInputDirection(int nDirection);
	//---------------------------------------------------------------------------------------------
	int getInputDirection();
	//---------------------------------------------------------------------------------------------
	void setDefaultDistanceUnitType(EDistanceUnitType eDefaultUnit);
	//---------------------------------------------------------------------------------------------
	EDistanceUnitType getDefaultDistanceUnitType();
	//---------------------------------------------------------------------------------------------
	//	Sets the angle definition to be deflection or internal angle
	//	bIsDeflectionAngle: true -- deflection angle
	//						false -- internal angle
	void setLineAngleDefinition(bool bIsDeflectionAngle);
	//---------------------------------------------------------------------------------------------
	//	Whether the line angle is definied as deflection angle or internal angle
	bool isDefinedAsDeflectionAngle();

	//---------------------------------------------------------------------------------------------
	//	Throws an exception if IcoMap is not licensed either by node or concurrent
	void validateIcoMapLicensed();

	//---------------------------------------------------------------------------------------------
	// Releases any IcoMap concurrent licnese that might have been obtained
	void releaseConcurrentIcoMapLicense();

	//---------------------------------------------------------------------------------------------
	// Sets DPI to be used to load all PDF images.  Stores min or max resolution if requested 
	// setting is outside the bounds.  50 <= nDotsPerInch <= 300
	void setPDFResolution(int nDotsPerInch);
	//---------------------------------------------------------------------------------------------
	// Retrieves DPI used for loading PDF images.  Returned value is limited: 50 <= nDPI <= 300
	int getPDFResolution();
	//---------------------------------------------------------------------------------------------

public:
	//this special public function checks if the input character is alphanumeric character which
	//excludes "N", "E", "S", "W"
	//REUQIRE: input cstrChar must have one and only one character
	bool isSpecialAlphaNumeric(CString cstrChar);
	//retrieve default keycode for each direction
	std::string getDefaultKeycode(EDirectionType eDirection);

	// Retrieve default shortcut
	std::string getDefaultShortcut(EShortcutType eShortcut);

protected:
	//==============================================================================================
	// Required by Singleton template
	IcoMapOptions();
	
	// following required for all singletons in general
	IcoMapOptions(const IcoMapOptions& toCopy);
	IcoMapOptions& operator = (const IcoMapOptions& toAssign);


private:

	std::map<EDirectionType, std::string> m_mapDirectionsToKeycodes;
	//contains ECurveParameter types in string format. ex. "k"
	std::vector<std::string> m_vecCurveParametersInStrings;
	//contains ECurveToolID types in string format
	std::vector<std::string> m_vecCurveToolIDsInStrings;
	std::vector<std::string> m_vecParameterNumsInStrings;
	
	// persistence managers for user and machine settings
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pMachineCfgMgr;

	// Mapping between shortcuts and character strings
	std::map<EShortcutType, std::string> m_mapShortcutsToStrings;


	// get spplication specific key in Registry
	std::string getApplicationSpecificRoot();
	//initialize some member variables
	void Init();
};
