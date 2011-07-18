#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>
#include "ETool.h"
class ConfigMgrSRIR 
{
public:

	//----------------------------------------------------------------------------------------------
	//
	ConfigMgrSRIR(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Get "LastFileOpenDirectory" mode 
	//
	// REQUIRE: Notify all observers about the modification
	//
	// PROMISE: 
	//
	// ARGS:	
	//	
	std::string getLastFileOpenDirectory(void);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: set "LastFileOpenDirectory" mode from "ImageRecognition" section in INI file
	//
	// REQUIRE: Notify all observers about the modification
	//
	// PROMISE: 
	//
	// ARGS:	
	//	
	void setLastFileOpenDirectory(const std::string& strFileDir);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the left and top values of the window position
	//
	// PROMISE:	
	//
	void getWindowPos(long &lPosX, long &lPosY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the left and top values of the window position
	//
	void setWindowPos(long lPosX, long lPosY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the width and height values of the window size
	//
	// PROMISE:	
	//
	void getWindowSize(long &lSizeX, long &lSizeY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the last used OCR region type
	int getOCRRegionType();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Sets the last used OCR region type
	void setOCRRegionType(int iValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the width and height values of the window size
	//
	void setWindowSize(long lSizeX, long lSizeY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the zone highlight height
	//
	// PROMISE:	
	//
	void getZoneHeight(long &lHeight);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the zone highlight height
	//
	void setZoneHeight(long lHeight);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the zone highlight color
	//
	// PROMISE:	
	//
	void getZoneColor(COLORREF &color);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the zone highlight color
	//
	void setZoneColor(COLORREF color);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the zone highlight color as used color
	//
	// PROMISE:	
	//
	void getUsedZoneColor(COLORREF &color);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the zone highlight color as used color
	//
	void setUsedZoneColor(COLORREF color);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the number of rotation steps for the auto-rotate functionality
	//			associated with finding the best OCR text for a highlight
	unsigned long getNumAutoRotateSteps();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the size of a rotation step for the auto-rotate functionality
	//			associated with finding the best OCR text for a highlight
	unsigned long getAutoRotateStepSize();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To return the pad factor to increase the zone by a certain percentage. For
	//			instance, if pad factor is 1.2, the zone height will be increased by 20% on
	//			the top as well as on the bottom of the zone.
	double getZonePadFactor();
	//----------------------------------------------------------------------------------------------
	ETool getLastSelectionTool();
	//----------------------------------------------------------------------------------------------
	void setLastSelectionTool(ETool eTool);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns true if it is in fit to page status
	bool getFitToStatus();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Stores the fit to status, true for fit to page and false for fit to width
	void setFitToStatus(bool bStatus);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns true if percentage should be displayed on the statusbar
	bool getDisplayPercentageEnabled();

private:
	static const std::string LAST_FILE_OPEN_DIR;
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string ZONE_HEIGHT;
	static const std::string ZONE_COLOR_RED;
	static const std::string ZONE_COLOR_GREEN;
	static const std::string ZONE_COLOR_BLUE;
	static const std::string USED_ZONE_COLOR_RED;
	static const std::string USED_ZONE_COLOR_GREEN;
	static const std::string USED_ZONE_COLOR_BLUE;
	static const std::string AUTO_ROTATE_STEP_SIZE;
	static const std::string NUM_AUTO_ROTATE_STEPS;
	static const std::string HEIGHT_PAD_FACTOR;
	static const std::string OCR_REGION_TYPE;
	static const std::string LAST_SELECTION_TOOL;
	static const std::string FIT_TO_STATUS;
	static const std::string DISPLAY_PERCENTAGE;

	static const std::string DEFAULT_LAST_FILE_OPEN_DIR;
	static const std::string DEFAULT_WINDOW_POS_X;
	static const std::string DEFAULT_WINDOW_POS_Y;
	static const std::string DEFAULT_WINDOW_SIZE_X;
	static const std::string DEFAULT_WINDOW_SIZE_Y;
	static const std::string DEFAULT_ZONE_HEIGHT;
	static const std::string DEFAULT_ZONE_COLOR_RED;
	static const std::string DEFAULT_ZONE_COLOR_GREEN;
	static const std::string DEFAULT_ZONE_COLOR_BLUE;
	static const std::string DEFAULT_USED_ZONE_COLOR_RED;
	static const std::string DEFAULT_USED_ZONE_COLOR_GREEN;
	static const std::string DEFAULT_USED_ZONE_COLOR_BLUE;
	static const std::string DEFAULT_AUTO_ROTATE_STEP_SIZE;
	static const std::string DEFAULT_NUM_AUTO_ROTATE_STEPS;
	static const std::string DEFAULT_HEIGHT_PAD_FACTOR;
	static const std::string DEFAULT_OCR_REGION_TYPE;
	static const std::string DEFAULT_LAST_SELECTION_TOOL;
	static const std::string DEFAULT_FIT_TO_STATUS;
	static const std::string DEFAULT_DISPLAY_PERCENTAGE;

	IConfigurationSettingsPersistenceMgr *m_pCfgMgr;
	std::string m_strSectionFolderName;
};