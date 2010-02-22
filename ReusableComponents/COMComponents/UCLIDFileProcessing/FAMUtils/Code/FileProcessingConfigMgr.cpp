#include "stdafx.h"
#include "FileProcessingConfigMgr.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <cpputil.h>
#include <StringTokenizer.h>
#include <RegConstants.h>
#include <EncryptionEngine.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <UCLIDException.h>

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
const string FileProcessingConfigMgr::WINDOW_POS_X = "WindowPositionX";
const string FileProcessingConfigMgr::WINDOW_POS_Y = "WindowPositionY";
const string FileProcessingConfigMgr::WINDOW_SIZE_X = "WindowSizeX";
const string FileProcessingConfigMgr::WINDOW_SIZE_Y = "WindowSizeY";
const string FileProcessingConfigMgr::WINDOW_MAXIMIZED = "WindowMaximized";
const string FileProcessingConfigMgr::SCOPE_LAST_OPENED_FILE_NAME = "ScopeLastOpenedFile";
const string FileProcessingConfigMgr::SCOPE_LAST_OPENED_FOLDER_NAME = "ScopeLastOpenedFolder";
const string FileProcessingConfigMgr::SCOPE_OPENED_FOLDER_HISTORY = "ScopeOpenedFolderHistory";
const string FileProcessingConfigMgr::SCOPE_FILE_EXTENSION_LIST = "ScopeFileExtensionList";
const string FileProcessingConfigMgr::SCOPE_LAST_USED_FILE_EXTENSION = "ScopeLastUsedFileExtension";
const string FileProcessingConfigMgr::SCOPE_LAST_OPENED_LIST_NAME = "ScopeLastOpenedList";
const string FileProcessingConfigMgr::MAX_STORED_RECORDS = "MaxStoredRecords";
const string FileProcessingConfigMgr::RESTRICT_NUM_STORED_RECORDS = "RestrictNumStoredRecords";
const string FileProcessingConfigMgr::MAX_FILES_FROM_DB = "MaxFilesFromDB";
const string FileProcessingConfigMgr::MILLISECONDS_BETWEEN_DB_CHECK = "MillisecondsBetweenDBCheck";
const string FileProcessingConfigMgr::TIMER_TICK_SPEED = "TimerTickSpeed";
const string FileProcessingConfigMgr::DB_SERVER_HISTORY = "DBServerHistory";
const string FileProcessingConfigMgr::DB_LOCK_TIMEOUT = "DBLockTimeout";
const string FileProcessingConfigMgr::AUTO_SCROLLING = "AutoScrolling";
const string FileProcessingConfigMgr::LAST_GOOD_SERVER = "LastGoodServer";
const string FileProcessingConfigMgr::LAST_GOOD_DATABASE = "LastGoodDatabase";

// Minimum width and height for the dialog
const int FileProcessingConfigMgr::DLG_MIN_WIDTH = 380;
const int FileProcessingConfigMgr::DLG_MIN_HEIGHT = 200;

// a numThreads value of 0 means use one thread for each processor
static const int DEFAULT_NUM_THREADS = 0;

static const int DEFAULT_MAX_STORED_RECORDS = 1000;
static const string DEFAULT_RESTRICT_NUM_STORED_RECORDS = "1";

static const string DEFAULT_DB_CONNECTION_STRING = 
			"Provider=SQLNCLI;Server=(local);"
			"Database=FPDB;Integrated Security=SSPI;"
			"DataTypeCompatibility=80;"
			"MARS Connection=True;";

//Registry Path for FileProcessingDlg
static const string gstrFP_DLG_REGISTRY_PATH = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FileProcessingDlg";

//Registry Path for FileProcessingDB
static const string gstrFP_DB_REGISTRY_PATH = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FileProcessingDB";

// Registry Path for File Processing Record Manager (FPRecordManager)
static const string gstrFP_RECORD_MGR_PATH = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FPRecordManager";

// Registry path for FAMDBAdmin
static const string gstrFAM_DBADMIN_REG_PATH = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FAMDBAdmin";

// Registry path for the Micrsoft SQL Server keys
static const string gstrSQL_SERVER_REG_PATH = "SOFTWARE\\Microsoft\\Microsoft SQL Server";

// Key name that contains the instance names of installed SQL servers
static const string gstrSQL_SERVER_INSTALLED_INSTANCES_KEY = "InstalledInstances";

// Default for Max files from a database
static const long DEFAULT_MAX_FILES_FROM_DB = 1;

// Default for milliseconds between db checks
static const long DEFAULT_MS_BETWEEN_DB_CHECK = 2000;

// Default timer tick speed (2000 ms)
static const unsigned int DEFAULT_TIMER_TICK_SPEED = 2000;

// Default Database lock timeout in sec
static const long DEFAULT_DB_LOCK_TIMEOUT = 300; // 5 min

//-------------------------------------------------------------------------------------------------
// FileProcessingConfigMgr
//-------------------------------------------------------------------------------------------------
FileProcessingConfigMgr::FileProcessingConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, 
												 const string& strSectionName)
:m_pCfgMgr(pConfigMgr), m_strSectionFolderName(strSectionName)
{
	// TODO: The "strSectionName" parameter should no longer be required.
	// This is a hack since most of the files are checked out. 
	m_strSectionFolderName = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing";
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getWindowPos(long &lPosX, long &lPosY)
{
	string	strX;
	string	strY;

	// Check for existence of X position
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X, "10");
		lPosX = 10;
	}
	else
	{
		// Retrieve X position
		strX = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X);
		lPosX = asLong(strX);
	}

	// Check for existence of Y position
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y, "10");
		lPosY = 10;
	}
	else
	{
		// Retrieve Y position
		strY = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y);
		lPosY = asLong(strY);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setWindowPos(long lPosX, long lPosY)
{
	CString	pszX, pszY;

	// Format strings
	pszX.Format("%ld", lPosX);
	pszY.Format("%ld", lPosY);

	// Store strings
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X, (LPCTSTR)pszX);
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y, (LPCTSTR)pszY);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getWindowSize(long &lSizeX, long &lSizeY)
{
	string	strX;
	string	strY;

	// Check for existence of width
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X))
	{
		// Not found, just default to 280
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X, "280");
		lSizeX = 280;
	}
	else
	{
		// Retrieve width
		strX = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X);
		lSizeX = asLong(strX);
	}

	// Check for existence of height
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y))
	{
		// Not found, just default to 170
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y, "170");
		lSizeY = 170;
	}
	else
	{
		// Retrieve height
		strY = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y);
		lSizeY = asLong(strY);
	}

	// Check for minimum width
	if (lSizeX < DLG_MIN_WIDTH)
	{
		lSizeX = DLG_MIN_WIDTH;
	}

	// Check for minimum height
	if (lSizeY < DLG_MIN_HEIGHT)
	{
		lSizeY = DLG_MIN_HEIGHT;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setWindowSize(long lSizeX, long lSizeY)
{
	long	lActualX = lSizeX;
	long	lActualY = lSizeY;

	// Check for minimum width
	if (lActualX < DLG_MIN_WIDTH)
	{
		lActualX = DLG_MIN_WIDTH;
	}

	// Check for minimum height
	if (lActualY < DLG_MIN_HEIGHT)
	{
		lActualY = DLG_MIN_HEIGHT;
	}

	CString pszX, pszY;
	pszX.Format("%ld", lActualX);
	pszY.Format("%ld", lActualY);

	// Store strings
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X, (LPCTSTR)pszX);
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y, (LPCTSTR)pszY);
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getWindowMaximized()
{
	bool bMaximized = false;
	string strValue;

	// Check for existence of maximized key
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED))
	{
		// Not found, just default to false
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED, "0");
	}
	else
	{
		// Retrieve setting
		strValue = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED);
		if (strValue.length() > 0)
		{
			bMaximized = (strValue == "1");
		}
	}

	return bMaximized;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setWindowMaximized(bool bMaximized)
{
	// Store setting
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED, bMaximized ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastOpenedFileNameFromScope()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME))
	{
		return "";
	}

	return m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedFileNameFromScope(const string& strFileName)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME, strFileName);
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastOpenedFolderNameFromScope()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FOLDER_NAME))
	{
		return "";
	}

	return m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FOLDER_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedFolderNameFromScope(const string& strFolderName)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FOLDER_NAME, strFolderName);
}
//-------------------------------------------------------------------------------------------------
vector<string> FileProcessingConfigMgr::getFileExtensionList()
{
	vector<string> vecExtList;
	// default set of images support by our current SSOCR
	string strList("");
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST))
	{
		strList = "*.tif"
				  "|*.bmp"
				  "|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;"
				  "*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*"
				  "|*.*";
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST, strList);
	}

	if (strList.empty())
	{
		strList = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST);
	}

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", vecExtList);

	return vecExtList;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setFileExtensionList(const vector<string>& vecFileExtensionList)
{
	string strList("");
	for (unsigned int n = 0; n < vecFileExtensionList.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecFileExtensionList[n];
	}

	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST, strList);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getOpenedFolderHistoryFromScope(std::vector<std::string>& rvecHistory)
{
	string strList("");
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY))
	{
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY, strList);
	}
	strList = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY);
	

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setOpenedFolderHistoryFromScope(const std::vector<std::string>& vecHistory)
{
	string strList("");
	for (unsigned int n = 0; n<vecHistory.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecHistory[n];
	}

	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
std::string FileProcessingConfigMgr::getLastUsedFileExtension()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_USED_FILE_EXTENSION))
	{
		return "*.*";
	}

	return m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_USED_FILE_EXTENSION);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastUsedFileExtension(const std::string& strExt)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_USED_FILE_EXTENSION, strExt);
}
//-------------------------------------------------------------------------------------------------
std::string FileProcessingConfigMgr::getLastOpenedListNameFromScope()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_LIST_NAME))
	{
		return "";
	}

	return m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_LIST_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedListNameFromScope(const std::string& strListName)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME, strListName);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getMaxStoredRecords()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS))
	{
		m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS, asString(DEFAULT_MAX_STORED_RECORDS));
	}

	return asLong(m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS));
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setMaxStoredRecords(long nMaxStoredRecords)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS, asString(nMaxStoredRecords));
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getRestrictNumStoredRecords()
{
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS))
	{
		m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS, DEFAULT_RESTRICT_NUM_STORED_RECORDS);
	}

	return m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS) != "0";
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setRestrictNumStoredRecords(bool bRestrict)
{
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS, bRestrict ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getMaxFilesFromDB()
{
	// Set return value to defaultg
	long rtnValue = DEFAULT_MAX_FILES_FROM_DB;
	
	// if key exists get that value other wise set the default value
	if ( m_pCfgMgr->keyExists(gstrFP_RECORD_MGR_PATH, MAX_FILES_FROM_DB))
	{
		rtnValue = asLong(m_pCfgMgr->getKeyValue(gstrFP_RECORD_MGR_PATH, MAX_FILES_FROM_DB));
	}
	else
	{
		// Set the default value
		m_pCfgMgr->setKeyValue(gstrFP_RECORD_MGR_PATH, MAX_FILES_FROM_DB, asString(DEFAULT_MAX_FILES_FROM_DB));
	}
	return rtnValue;
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getMillisecondsBetweenDBCheck()
{
	// Set return value to defaultg
	long rtnValue = DEFAULT_MS_BETWEEN_DB_CHECK;
	
	// if key exists get that value other wise set the default value
	if ( m_pCfgMgr->keyExists(gstrFP_RECORD_MGR_PATH, MILLISECONDS_BETWEEN_DB_CHECK))
	{
		rtnValue = asLong(m_pCfgMgr->getKeyValue(gstrFP_RECORD_MGR_PATH,
			MILLISECONDS_BETWEEN_DB_CHECK));
	}
	else
	{
		// Set the default value
		m_pCfgMgr->setKeyValue(gstrFP_RECORD_MGR_PATH, MILLISECONDS_BETWEEN_DB_CHECK,
			asString(DEFAULT_MS_BETWEEN_DB_CHECK));
	}
	return rtnValue;
}
//-------------------------------------------------------------------------------------------------
unsigned int FileProcessingConfigMgr::getTimerTickSpeed()
{
	if(m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED) )
	{
		// If the registry key exists, return it
		string strTemp = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED);
		if (strTemp != "")
		{
			unsigned long ulTemp = asUnsignedLong( strTemp );
			unsigned int uiTemp = static_cast<unsigned int>(ulTemp);
			return uiTemp;
		}
		else
		{
			// Empty string, just return the default
			return DEFAULT_TIMER_TICK_SPEED ;
		}
	}
	else
	{
		// If the registry key does not exist, set the default value and return it
		m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED, 
							asString( DEFAULT_TIMER_TICK_SPEED ) );
		return DEFAULT_TIMER_TICK_SPEED ;
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setTimerTickSpeed(std::string strTickSpeed)
{
	// Set the new tick speed
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED, strTickSpeed );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getDBServerHistory(std::vector<std::string>& rvecHistory)
{
	string strList("");
	if (!m_pCfgMgr->keyExists(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY))
	{
		m_pCfgMgr->createKey(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY, strList);
	}
	strList = m_pCfgMgr->getKeyValue(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY);
	

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setDBServerHistory(const std::vector<std::string>& vecHistory)
{
	// Set the registry key for the database server history
	string strList("");

	// Loop through the vector and add each item to the string with a | delimiter
	for (unsigned int n = 0; n<vecHistory.size(); n++)
	{
		// Make sure that the item doesnt already exist in the history list
		if(string::npos == strList.find(vecHistory[n]) )
		{
			if (n > 0)
			{
				strList += "|";
			}
			strList += vecHistory[n];
		}
	}

	// Set the registry key
	m_pCfgMgr->setKeyValue(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getDBLockTimeout()
{
	// Check if key exists
	if (!m_pCfgMgr->keyExists(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT))
	{
		// if not create key and give it a default value of DEFAULT_DB_LOCK_TIMEOUT
		m_pCfgMgr->createKey(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT, asString(DEFAULT_DB_LOCK_TIMEOUT));
	}
	// Get the DB Lock time out from the registry
	long lDBLockTimeout = asLong(m_pCfgMgr->getKeyValue(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT));

	// Return the time out
	return lDBLockTimeout;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getAutoScrolling()
{
	// Default to true
	bool bAutoScroll = true;
	string strValue;

	// Check for existence of auto scrolling key
	if (!m_pCfgMgr->keyExists(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING))
	{
		// Not found, just default to true
		m_pCfgMgr->createKey(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING, "1");
	}
	else
	{
		// Retrieve setting
		strValue = m_pCfgMgr->getKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING);
		if (strValue.length() > 0)
		{
			bAutoScroll = (strValue == "1");
		}
	}

	return bAutoScroll;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setAutoScrolling(bool bAutoScroll)
{
	// Store setting
	m_pCfgMgr->setKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING, bAutoScroll ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getLastGoodDBSettings(std::string& strServer, std::string& strDatabase)
{
	// Set server and datbase to empty values
	strServer = "";
	strDatabase = "";
	
	// Get the server
	if(m_pCfgMgr->keyExists(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER)) 
	{
		strServer = m_pCfgMgr->getKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER);
	}

	// Get the databse
	if(m_pCfgMgr->keyExists(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE)) 
	{
		strDatabase = m_pCfgMgr->getKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastGoodDBSettings(const std::string& strServer, const std::string& strDatabase)
{
	// Store Server
	m_pCfgMgr->setKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER, strServer);

	// Store Database
	m_pCfgMgr->setKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE, strDatabase);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getLocalSQLServerInstances(std::vector<std::string>& vecLocalInstances)
{
	// If there is an exception trying to get the instances names just return an
	// empty vector but log the exception to track possible problems
	try
	{
		// Clear the vector
		vecLocalInstances.clear();

		// check if the SQL Server InstalledInstances key exists
		if (m_pCfgMgr->keyExists(gstrSQL_SERVER_REG_PATH, gstrSQL_SERVER_INSTALLED_INSTANCES_KEY))
		{
			// Get the instances from the registry
			vecLocalInstances = m_pCfgMgr->getKeyMultiStringValue(gstrSQL_SERVER_REG_PATH, gstrSQL_SERVER_INSTALLED_INSTANCES_KEY);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20376");
}
//-------------------------------------------------------------------------------------------------
