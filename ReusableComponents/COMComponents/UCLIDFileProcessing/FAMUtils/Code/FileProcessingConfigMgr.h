#pragma once

#include "FAMUtils.h"

#include <string>
#include <vector>

#include <IConfigurationSettingsPersistenceMgr.h>

class FAMUTILS_API FileProcessingConfigMgr
{
public:
	// TODO: The "strSectionName" parameter should no longer be required, and is ignored.
	//		 - it is set to gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing" in the constructor
	FileProcessingConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	// Returns the left and top values of the window position
	// Stores the left and top values of the window position
	void getWindowPos(long &lPosX, long &lPosY);
	void setWindowPos(long lPosX, long lPosY);

	// Returns the width and height values of the window size
	// Stores the width and height values of the window size
	void getWindowSize(long &lSizeX, long &lSizeY);
	void setWindowSize(long lSizeX, long lSizeY);

	// Returns the Maximized state of the window position
	// Stores the Maximized state of the window position
	bool getWindowMaximized();
	void setWindowMaximized(bool bMaximized);

	// get and set last opened file name from Scope property page
	std::string getLastOpenedFileNameFromScope();
	void setLastOpenedFileNameFromScope(const std::string& strFileName);

	// get and set last opened folder name from Scope property page
	std::string getLastOpenedFolderNameFromScope();
	void setLastOpenedFolderNameFromScope(const std::string& strFolderName);

	// get and set opened folder history from Scope property page
	void getOpenedFolderHistoryFromScope(std::vector<std::string>& rvecHistory);
	void setOpenedFolderHistoryFromScope(const std::vector<std::string>& vecHistory);

	// get last used file extension
	std::string getLastUsedFileExtension();
	void setLastUsedFileExtension(const std::string& strExt);

	// get and set file extensions
	std::vector<std::string> getFileExtensionList();
	void setFileExtensionList(const std::vector<std::string>& vecFileExtensionList);

	// get and set last opened file list name from Scope property page
	std::string getLastOpenedListNameFromScope();
	void setLastOpenedListNameFromScope(const std::string& strListName);

	long getMaxStoredRecords();
	void setMaxStoredRecords(long nMaxStoredRecords);

	bool getRestrictNumStoredRecords();
	void setRestrictNumStoredRecords(bool bRestrict);

	// Gets the last good settings saved in the registry for the server and database for
	// FAMDBAdmin
	void getLastGoodDBSettings(std::string& strServer, std::string& strDatabase);
	
	// Saves the server and database as the last good settings in the registry
	void setLastGoodDBSettings(const std::string& strServer, const std::string& strDatabase);

	// Return the Max number of files to get from the database for the recordmgr Defaults to 1
	long getMaxFilesFromDB();

	// Return the number of milliseconds to sleep between checking the DB for new files
	long getMillisecondsBetweenDBCheck();

	// Return the number of seconds to keep trying to obtain a DB Lock
	long getDBLockTimeout();

	// Get / Set the registry key for timer tick speed (Defaults to 1000 which is approx 1 second)
	// Uses an unsigned int because that's what a timer requires for SetTimer()
	unsigned int getTimerTickSpeed();
	void setTimerTickSpeed(std::string strTickSpeed);

	// Get / Set the registry key for the history of database servers from the Select Master DB dialog
	void setDBServerHistory(const std::vector<std::string>& vecHistory);
	void getDBServerHistory(std::vector<std::string>& rvecHistory);

	// Reads the Microsoft SQL Server Installed instance key and returns the instances in a vector
	void getLocalSQLServerInstances(std::vector<std::string>& vecLocalInstances);

	// Returns the state of auto scrolling
	// Stores the auto scrolling state
	bool getAutoScrolling();
	void setAutoScrolling(bool bAutoScroll);

private:
	static const std::string WINDOW_POS_X;
	static const std::string WINDOW_POS_Y;
	static const std::string WINDOW_SIZE_X;
	static const std::string WINDOW_SIZE_Y;
	static const std::string WINDOW_MAXIMIZED;
	static const std::string SCOPE_LAST_OPENED_FILE_NAME;
	static const std::string SCOPE_LAST_OPENED_FOLDER_NAME;
	static const std::string SCOPE_OPENED_FOLDER_HISTORY;
	static const std::string SCOPE_FILE_EXTENSION_LIST;
	static const std::string SCOPE_LAST_USED_FILE_EXTENSION;
	static const std::string SCOPE_LAST_OPENED_LIST_NAME;
	static const std::string NUM_THREADS;
	static const std::string MAX_STORED_RECORDS;
	static const std::string RESTRICT_NUM_STORED_RECORDS;
	static const std::string MAX_FILES_FROM_DB;
	static const std::string MILLISECONDS_BETWEEN_DB_CHECK;
	static const std::string TIMER_TICK_SPEED;
	static const std::string DB_SERVER_HISTORY;
	static const std::string DB_LOCK_TIMEOUT;
	static const std::string AUTO_SCROLLING;
	static const std::string FileProcessingConfigMgr::LAST_GOOD_SERVER;
	static const std::string FileProcessingConfigMgr::LAST_GOOD_DATABASE;

	// Dialog size bounds
	static const int DLG_MIN_WIDTH;
	static const int DLG_MIN_HEIGHT;

	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strSectionFolderName;
};
