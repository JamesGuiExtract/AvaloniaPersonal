#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

class ConfigManager
{
public:
	ConfigManager(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);
	//----------------------------------------------------------------------------------------------
	//	Gets the last file open directory
	std::string getLastFileOpenDirectory(void);
	//----------------------------------------------------------------------------------------------
	//	Sets the last file open directory 	
	//
	void setLastFileOpenDirectory(const std::string& strFileDir);
	//----------------------------------------------------------------------------------------------
	// Return the font name
	//
	std::string getTextFont();
	//----------------------------------------------------------------------------------------------
	// Gets the directory of CopyTextToClipboard.exe
	//
	void setTextFont(const std::string& strFontName);
	//----------------------------------------------------------------------------------------------
	// Returns the size in Twips
	//
	std::string getTextSizeInTwips();
	//----------------------------------------------------------------------------------------------
	// Set the size for the text
	//
	void setTextSizeInTwips(const std::string& strTextSize);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Returns the last used text processing scope
	int getTextProcessingScope();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  Sets the last used text processing scope
	void setTextProcessingScope(int iValue);
	//----------------------------------------------------------------------------------------------
	// Returns the left and top values of the window position
	//
	void getWindowPos(long &lPosX, long &lPosY);
	//----------------------------------------------------------------------------------------------
	// Stores the left and top values of the window position
	//
	void setWindowPos(long lPosX, long lPosY);
	//---------------------------------------------------------------------------------------------
	// Returns the width and height values of the window size
	//
	void getWindowSize(long &lSizeX, long &lSizeY);
	//----------------------------------------------------------------------------------------------
	// Stores the width and height values of the window size
	//
	void setWindowSize(long lSizeX, long lSizeY);
	//----------------------------------------------------------------------------------------------
	//	Gets last selected input finder index from the combo
	// PROMISE: To return "" if setLastInputFinderName() was never called, or to return
	//			the string passed to the last call to setLastInputFinderName() if
	//			setLastInputFinderName() has been called.
	std::string getLastInputFinderName();
	//----------------------------------------------------------------------------------------------
	//	Stores last selected input finder name
	void setLastInputFinderName(const std::string& strInputFinderName);
	//----------------------------------------------------------------------------------------------

private:
	static const std::string LAST_FILE_OPEN_DIR;
	static const std::string TEXT_FONT;
	static const std::string TEXT_SIZE;
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string TEXT_PROCESSING_SCOPE;
	// last selected input finder name
	static const std::string LAST_INPUT_FINDER;

	static const string DEFAULT_LAST_FILE_OPEN_DIR;
	static const string DEFAULT_TEXT_FONT;
	static const string DEFAULT_TEXT_SIZE;
	static const string DEFAULT_WINDOW_POS_X;
	static const string DEFAULT_WINDOW_POS_Y;
	static const string DEFAULT_WINDOW_SIZE_X;
	static const string DEFAULT_WINDOW_SIZE_Y;
	static const string DEFAULT_LAST_INPUT_FINDER;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strSectionFolderName;
};