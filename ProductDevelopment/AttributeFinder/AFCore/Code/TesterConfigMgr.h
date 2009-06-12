#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

class TesterConfigMgr
{
public:
	TesterConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	//	Gets the last file open directory
	//	Sets the last file open directory 	
	std::string getLastFileOpenDirectory(void);
	void setLastFileOpenDirectory(const std::string& strFileDir);

	//	Gets the last file name
	//	Sets the last file name
	std::string getLastFileName(void);
	void setLastFileName(const std::string& strFileName);

	// Returns the left and top values of the window position
	// Stores the left and top values of the window position
	void getWindowPos(long &lPosX, long &lPosY);
	void setWindowPos(long lPosX, long lPosY);

	// Returns the width and height values of the window size
	// Stores the width and height values of the window size
	void getWindowSize(long &lSizeX, long &lSizeY);
	void setWindowSize(long lSizeX, long lSizeY);

	// Returns the last test scope (true = All Attribute, false = Current A.)
	// Stores the last test scope
	bool getAllAttributesTestScope();
	void setAllAttributesTestScope(bool bAllAttributes);

	// Returns the last input type (i.e. Image, File, Manual)
	// Stores the last input type
	std::string getLastInputType(void);
	void setLastInputType(const std::string& strInputType);

	// Returns the width of the Name column
	// Stores the width of the Name column
	long getNameColumnWidth();
	void setNameColumnWidth(long lColumnWidth);

	// Returns the last setting for Show Only Valid Entries
	// Stores the last setting for Show Only Valid Entries
	bool getShowOnlyValidEntries();
	void setShowOnlyValidEntries(bool bShowValid);

	// Returns the vertical position of the splitter
	// Stores the vertical position of the splitter
	long getSplitterPosition();
	void setSplitterPosition(long lSplitterPosition);

	// Returns the width of the Type column
	// Stores the width of the Type column
	long getTypeColumnWidth();
	void setTypeColumnWidth(long lColumnWidth);

	//	Gets the last file save directory
	//	Sets the last file save directory 	
	std::string getLastFileSaveDirectory(void);
	void setLastFileSaveDirectory(const std::string& strFileDir);

	////////////////////
	// Defined as public for use by RuleTesterDlg class
	////////////////////
	// Dialog size bounds
	static const int		giRTDLG_MIN_WIDTH;
	static const int		giRTDLG_MIN_HEIGHT;

	// Strings for Input combo box
	static const std::string gstrTEXTFROMIMAGEWINDOW;
	static const std::string gstrTEXTFROMFILE;
	static const std::string gstrMANUALTEXT;

private:
	// Registry keys for information persistence
	static const std::string LAST_FILE_OPEN_DIR;
	static const std::string LAST_FILE_NAME;
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string ALL_ATTRIBUTES_TEST_SCOPE;
	static const std::string LAST_INPUT_TYPE;
	static const std::string NAME_COLUMN_WIDTH;
	static const std::string SHOW_ONLY_VALID_ENTRIES;
	static const std::string SPLITTER_POS_Y;
	static const std::string TYPE_COLUMN_WIDTH;
	static const std::string LAST_FILE_SAVE_DIR;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strSectionFolderName;
};
