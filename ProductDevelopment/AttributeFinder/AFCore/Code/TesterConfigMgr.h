#pragma once

#include <string>

#include <IConfigurationSettingsPersistenceMgr.h>

using namespace std;

class TesterConfigMgr
{
public:
	TesterConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const string& strSectionName);

	//	Gets the last file open directory
	//	Sets the last file open directory 	
	string getLastFileOpenDirectory(void);
	void setLastFileOpenDirectory(const string& strFileDir);

	//	Gets the last file name
	//	Sets the last file name
	string getLastFileName(void);
	void setLastFileName(const string& strFileName);

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
	string getLastInputType(void);
	void setLastInputType(const string& strInputType);

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
	string getLastFileSaveDirectory(void);
	void setLastFileSaveDirectory(const string& strFileDir);

	// Gets/sets whether the attribute tree in the rule tester should auto expand
	bool getAutoExpandAttributes();
	void setAutoExpandAttributes(bool bAutoExpand);

	////////////////////
	// Defined as public for use by RuleTesterDlg class
	////////////////////
	// Dialog size bounds
	static const int		giRTDLG_MIN_WIDTH;
	static const int		giRTDLG_MIN_HEIGHT;

	// Strings for Input combo box
	static const string gstrTEXTFROMIMAGEWINDOW;
	static const string gstrTEXTFROMFILE;
	static const string gstrMANUALTEXT;

private:
	// Registry keys for information persistence
	static const string LAST_FILE_OPEN_DIR;
	static const string LAST_FILE_NAME;
	static const string WINDOW_POS_X;
	static const string WINDOW_POS_Y;
	static const string WINDOW_SIZE_X;
	static const string WINDOW_SIZE_Y;
	static const string ALL_ATTRIBUTES_TEST_SCOPE;
	static const string LAST_INPUT_TYPE;
	static const string NAME_COLUMN_WIDTH;
	static const string SHOW_ONLY_VALID_ENTRIES;
	static const string SPLITTER_POS_Y;
	static const string TYPE_COLUMN_WIDTH;
	static const string LAST_FILE_SAVE_DIR;
	static const string AUTO_EXPAND_ATTRIBUTES;

	static const string DEFAULT_LAST_FILE_OPEN_DIR;
	static const string DEFAULT_LAST_FILE_NAME;
	static const string DEFAULT_WINDOW_POS_X;
	static const string DEFAULT_WINDOW_POS_Y;
	static const string DEFAULT_WINDOW_SIZE_X;
	static const string DEFAULT_WINDOW_SIZE_Y;
	static const string DEFAULT_ALL_ATTRIBUTES_TEST_SCOPE;
	static const string DEFAULT_NAME_COLUMN_WIDTH;
	static const string DEFAULT_SPLITTER_POS_Y;
	static const string DEFAULT_SHOW_ONLY_VALID_ENTRIES;
	static const string DEFAULT_TYPE_COLUMN_WIDTH;
	static const string DEFAULT_LAST_FILE_SAVE_DIR;
	static const string DEFAULT_AUTO_EXPAND_ATTRIBUTES;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	string m_strSectionFolderName;
};
