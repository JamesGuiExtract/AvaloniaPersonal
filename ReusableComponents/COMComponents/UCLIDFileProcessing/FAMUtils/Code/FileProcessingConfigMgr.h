#pragma once

#include "FAMUtils.h"

#include <RegistryPersistenceMgr.h>

#include <string>
#include <vector>
#include <memory>

using namespace std;

class FAMUTILS_API FileProcessingConfigMgr
{
public:
	FileProcessingConfigMgr();

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
	string getLastOpenedFileNameFromScope();
	void setLastOpenedFileNameFromScope(const string& strFileName);

	// get and set last opened folder name from Scope property page
	string getLastOpenedFolderNameFromScope();
	void setLastOpenedFolderNameFromScope(const string& strFolderName);

	// get and set opened folder history from Scope property page
	void getOpenedFolderHistoryFromScope(vector<string>& rvecHistory);
	void setOpenedFolderHistoryFromScope(const vector<string>& vecHistory);

	// get last used file extension
	string getLastUsedFileExtension();
	void setLastUsedFileExtension(const string& strExt);

	// get and set file extensions
	vector<string> getFileExtensionList();
	void setFileExtensionList(const vector<string>& vecFileExtensionList);

	// get and set last opened file list name from Scope property page
	string getLastOpenedListNameFromScope();
	void setLastOpenedListNameFromScope(const string& strListName);

	long getMaxStoredRecords();
	void setMaxStoredRecords(long nMaxStoredRecords);

	bool getRestrictNumStoredRecords();
	void setRestrictNumStoredRecords(bool bRestrict);

	// Gets the last good settings saved in the registry for the server and database for
	// FAMDBAdmin
	void getLastGoodDBSettings(string& strServer, string& strDatabase);
	
	// Saves the server and database as the last good settings in the registry
	void setLastGoodDBSettings(const string& strServer, const string& strDatabase);

	// Return the number of milliseconds to sleep between checking the DB for new files
	long getMillisecondsBetweenDBCheck();

	// Return the number of seconds to keep trying to obtain a DB Lock
	long getDBLockTimeout();

	// Get / Set the registry key for timer tick speed (Defaults to 1000 which is approx 1 second)
	// Uses an unsigned int because that's what a timer requires for SetTimer()
	unsigned int getTimerTickSpeed();
	void setTimerTickSpeed(string strTickSpeed);

	// Get / Set the registry key for the history of database servers from the Select Master DB dialog
	void setDBServerHistory(const vector<string>& vecHistory);
	void getDBServerHistory(vector<string>& rvecHistory);

	// Reads the Microsoft SQL Server Installed instance key and returns the instances in a vector
	void getLocalSQLServerInstances(vector<string>& vecLocalInstances);

	// Returns the state of auto scrolling
	// Stores the auto scrolling state
	bool getAutoScrolling();
	void setAutoScrolling(bool bAutoScroll);

	// Gets the UsePreNormalized setting from the registry;
	bool getUsePreNormalized();

private:
	static const string WINDOW_POS_X;
	static const string WINDOW_POS_Y;
	static const string WINDOW_SIZE_X;
	static const string WINDOW_SIZE_Y;
	static const string WINDOW_MAXIMIZED;
	static const string SCOPE_LAST_OPENED_FILE_NAME;
	static const string SCOPE_LAST_OPENED_FOLDER_NAME;
	static const string SCOPE_OPENED_FOLDER_HISTORY;
	static const string SCOPE_FILE_EXTENSION_LIST;
	static const string SCOPE_LAST_USED_FILE_EXTENSION;
	static const string SCOPE_LAST_OPENED_LIST_NAME;
	static const string NUM_THREADS;
	static const string MAX_STORED_RECORDS;
	static const string RESTRICT_NUM_STORED_RECORDS;
	static const string MAX_FILES_FROM_DB;
	static const string MILLISECONDS_BETWEEN_DB_CHECK;
	static const string TIMER_TICK_SPEED;
	static const string DB_SERVER_HISTORY;
	static const string DB_LOCK_TIMEOUT;
	static const string AUTO_SCROLLING;
	static const string FileProcessingConfigMgr::LAST_GOOD_SERVER;
	static const string FileProcessingConfigMgr::LAST_GOOD_DATABASE;
	static const string USE_PRE_NORMALIZED;

	// Dialog size bounds
	static const int DLG_MIN_WIDTH;
	static const int DLG_MIN_HEIGHT;

	auto_ptr<RegistryPersistenceMgr> m_apHKLM;
	auto_ptr<RegistryPersistenceMgr> m_apHKCU;

	// Method for lazy instantiation of the HKLM registry persistence manager
	RegistryPersistenceMgr* getHKLM();

	// Method to initialize the HKCU registry persistance manager
	void initHKCU();
};
