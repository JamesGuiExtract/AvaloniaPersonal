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
const string FileProcessingConfigMgr::LAST_GOOD_ADV_CONN_STR_PROPERTIES = "LastGoodAdvConnStrProperties";
const string FileProcessingConfigMgr::USE_PRE_NORMALIZED = "UsePreNormalized";
const string FileProcessingConfigMgr::AUTO_SAVE_FPS_FILE = "AutoSaveFPS";
const string FileProcessingConfigMgr::DB_MANAGER_TYPE = "DBManagerType";
const string FileProcessingConfigMgr::USE_APPLICATION_ROLES = "UseApplicationRoles";
const string FileProcessingConfigMgr::USE_CONNECTION_POOLING = "UseConnectionPooling";

const string FileProcessingConfigMgr::DEFAULT_WINDOW_POS_X = "10";
const string FileProcessingConfigMgr::DEFAULT_WINDOW_POS_Y = "10";
const string FileProcessingConfigMgr::DEFAULT_WINDOW_SIZE_X = "280";
const string FileProcessingConfigMgr::DEFAULT_WINDOW_SIZE_Y = "170";
const string FileProcessingConfigMgr::DEFAULT_WINDOW_MAXIMIZED = "0";
const string FileProcessingConfigMgr::DEFAULT_SCOPE_LAST_OPENED_FILE_NAME = "";
const string FileProcessingConfigMgr::DEFAULT_SCOPE_LAST_OPENED_FOLDER_NAME = "";
const string FileProcessingConfigMgr::DEFAULT_SCOPE_OPENED_FOLDER_HISTORY = "";
const string FileProcessingConfigMgr::DEFAULT_SCOPE_LAST_USED_FILE_EXTENSION = "*.*";
const string FileProcessingConfigMgr::DEFAULT_SCOPE_LAST_OPENED_LIST_NAME = "";
const string FileProcessingConfigMgr::DEFAULT_MAX_STORED_RECORDS = "1000";
const string FileProcessingConfigMgr::DEFAULT_RESTRICT_NUM_STORED_RECORDS = "1";
const string FileProcessingConfigMgr::DEFAULT_MS_BETWEEN_DB_CHECK = "2000";
const string FileProcessingConfigMgr::DEFAULT_TIMER_TICK_SPEED = "2000";
const string FileProcessingConfigMgr::DEFAULT_DB_SERVER_HISTORY = "";
const string FileProcessingConfigMgr::DEFAULT_DB_LOCK_TIMEOUT = "300"; // 5 min
const string FileProcessingConfigMgr::DEFAULT_AUTO_SCROLLING = "1";
const string FileProcessingConfigMgr::DEFAULT_LAST_GOOD_SERVER = "0";
const string FileProcessingConfigMgr::DEFAULT_LAST_GOOD_DATABASE = "0";
const string FileProcessingConfigMgr::DEFAULT_LAST_GOOD_ADV_CONN_STR_PROPERTIES = "";
const string FileProcessingConfigMgr::DEFAULT_USE_PRE_NORMALIZED = "1";
const string FileProcessingConfigMgr::DEFAULT_AUTO_SAVE_FPS_FILE = "0";
const string FileProcessingConfigMgr::DEFAULT_DB_MANAGER_TYPE = "CPP";
const string FileProcessingConfigMgr::DEFAULT_USE_APPLICATION_ROLES = "1";
const string FileProcessingConfigMgr::DEFAULT_USE_CONNECTION_POOLING = "0";

// Minimum width and height for the dialog
const int FileProcessingConfigMgr::DLG_MIN_WIDTH = 380;
const int FileProcessingConfigMgr::DLG_MIN_HEIGHT = 200;

// a numThreads value of 0 means use one thread for each processor
static const int DEFAULT_NUM_THREADS = 0;

static const string DEFAULT_DB_CONNECTION_STRING = 
			"Provider=SQLNCLI11;Server=(local);"
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

// Define four UCLID passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the FileProcessingDB.cpp
const unsigned long	gulPasswordKey1 = 0x99474F69;
const unsigned long	gulPasswordKey2 = 0x4DF44632;
const unsigned long	gulPasswordKey3 = 0x9A4B51F2;
const unsigned long	gulPasswordKey4 = 0x1EFC7784;

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulPasswordKey1;
	bsm << gulPasswordKey2;
	bsm << gulPasswordKey3;
	bsm << gulPasswordKey4;
	bsm.flushToByteStream(8);
}

//-------------------------------------------------------------------------------------------------
// FileProcessingConfigMgr
//-------------------------------------------------------------------------------------------------
FileProcessingConfigMgr::FileProcessingConfigMgr() 
{
	try
	{
		initHKCU();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30016");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getWindowPos(long &lPosX, long &lPosY)
{
	string	strX;
	string	strY;

	// Check for existence of X position
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X))
	{
		// Not found, just default to 10
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
		lPosX = asLong(DEFAULT_WINDOW_POS_X);
	}
	else
	{
		// Retrieve X position
		strX = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X, DEFAULT_WINDOW_POS_X);
		lPosX = asLong(strX);
	}

	// Check for existence of Y position
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y))
	{
		// Not found, just default to 10
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
		lPosY = asLong(DEFAULT_WINDOW_POS_Y);
	}
	else
	{
		// Retrieve Y position
		strY = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y, DEFAULT_WINDOW_POS_Y);
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
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_X, (LPCTSTR)pszX);
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_POS_Y, (LPCTSTR)pszY);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getWindowSize(long &lSizeX, long &lSizeY)
{
	string	strX;
	string	strY;

	// Check for existence of width
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X))
	{
		// Not found, just default to 280
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X, DEFAULT_WINDOW_SIZE_X);
		lSizeX = asLong(DEFAULT_WINDOW_SIZE_X);
	}
	else
	{
		// Retrieve width
		strX = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X, DEFAULT_WINDOW_SIZE_X);
		lSizeX = asLong(strX);
	}

	// Check for existence of height
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y))
	{
		// Not found, just default to 170
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y, DEFAULT_WINDOW_SIZE_Y);
		lSizeY = asLong(DEFAULT_WINDOW_SIZE_Y);
	}
	else
	{
		// Retrieve height
		strY = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y, DEFAULT_WINDOW_SIZE_Y);
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
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_X, (LPCTSTR)pszX);
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_SIZE_Y, (LPCTSTR)pszY);
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getWindowMaximized()
{
	bool bMaximized = false;
	string strValue;

	// Check for existence of maximized key
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED))
	{
		// Not found, just default to false
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED, DEFAULT_WINDOW_MAXIMIZED);
	}
	else
	{
		// Retrieve setting
		strValue = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED,
			DEFAULT_WINDOW_MAXIMIZED);
		bMaximized = asCppBool(strValue);
	}

	return bMaximized;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setWindowMaximized(bool bMaximized)
{
	// Store setting
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, WINDOW_MAXIMIZED, bMaximized ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastOpenedFileNameFromScope()
{
	return m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME,
		DEFAULT_SCOPE_LAST_OPENED_FILE_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedFileNameFromScope(const string& strFileName)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME, strFileName);
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastOpenedFolderNameFromScope()
{
	return m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FOLDER_NAME,
		DEFAULT_SCOPE_LAST_OPENED_FOLDER_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedFolderNameFromScope(const string& strFolderName)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FOLDER_NAME, strFolderName);
}
//-------------------------------------------------------------------------------------------------
vector<string> FileProcessingConfigMgr::getFileExtensionList()
{
	vector<string> vecExtList;
	// default set of images support by our current SSOCR
	string strList =  "*.tif"
					  "|*.bmp"
					  "|*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;"
					  "*.flc;*.fli;*.gif;*.jpg;*.pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf"
					  "|*.*";
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST))
	{
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST, strList);
	}
	else
	{
		m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST, strList);
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

	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_FILE_EXTENSION_LIST, strList);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getOpenedFolderHistoryFromScope(vector<string>& rvecHistory)
{
	string strList("");
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY))
	{
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY,
			DEFAULT_SCOPE_OPENED_FOLDER_HISTORY);
	}
	strList = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY,
		DEFAULT_SCOPE_OPENED_FOLDER_HISTORY);

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setOpenedFolderHistoryFromScope(const vector<string>& vecHistory)
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

	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_OPENED_FOLDER_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastUsedFileExtension()
{
	return m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_USED_FILE_EXTENSION,
		DEFAULT_SCOPE_LAST_USED_FILE_EXTENSION);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastUsedFileExtension(const string& strExt)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_USED_FILE_EXTENSION, strExt);
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getLastOpenedListNameFromScope()
{
	return m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_LIST_NAME,
		DEFAULT_SCOPE_LAST_OPENED_LIST_NAME);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastOpenedListNameFromScope(const string& strListName)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, SCOPE_LAST_OPENED_FILE_NAME, strListName);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getMaxStoredRecords()
{
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS))
	{
		m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS,
			DEFAULT_MAX_STORED_RECORDS);
	}

	return asLong(m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS,
		DEFAULT_MAX_STORED_RECORDS));
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setMaxStoredRecords(long nMaxStoredRecords)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, MAX_STORED_RECORDS, asString(nMaxStoredRecords));
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getRestrictNumStoredRecords()
{
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS))
	{
		m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS,
			DEFAULT_RESTRICT_NUM_STORED_RECORDS);
	}

	return asCppBool(m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS,
		DEFAULT_RESTRICT_NUM_STORED_RECORDS));
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setRestrictNumStoredRecords(bool bRestrict)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, RESTRICT_NUM_STORED_RECORDS, bRestrict ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getMillisecondsBetweenDBCheck()
{
	// Set return value to defaultg
	long rtnValue = asLong(DEFAULT_MS_BETWEEN_DB_CHECK);
	
	// if key exists get that value other wise set the default value
	if ( m_apHKCU->keyExists(gstrFP_RECORD_MGR_PATH, MILLISECONDS_BETWEEN_DB_CHECK))
	{
		rtnValue = asLong(m_apHKCU->getKeyValue(gstrFP_RECORD_MGR_PATH,
			MILLISECONDS_BETWEEN_DB_CHECK, DEFAULT_MS_BETWEEN_DB_CHECK));
	}
	else
	{
		// Set the default value
		m_apHKCU->setKeyValue(gstrFP_RECORD_MGR_PATH, MILLISECONDS_BETWEEN_DB_CHECK,
			DEFAULT_MS_BETWEEN_DB_CHECK);
	}
	return rtnValue;
}
//-------------------------------------------------------------------------------------------------
unsigned int FileProcessingConfigMgr::getTimerTickSpeed()
{
	if(m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED) )
	{
		// If the registry key exists, return it
		string strTemp = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED,
			DEFAULT_TIMER_TICK_SPEED);
		if (strTemp != "")
		{
			unsigned long ulTemp = asUnsignedLong( strTemp );
			unsigned int uiTemp = static_cast<unsigned int>(ulTemp);
			return uiTemp;
		}
		else
		{
			// Empty string, just return the default
			return asLong(DEFAULT_TIMER_TICK_SPEED);
		}
	}
	else
	{
		// If the registry key does not exist, set the default value and return it
		m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED, 
							DEFAULT_TIMER_TICK_SPEED);
		return asLong(DEFAULT_TIMER_TICK_SPEED);
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setTimerTickSpeed(string strTickSpeed)
{
	// Set the new tick speed
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, TIMER_TICK_SPEED, strTickSpeed );
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getDBServerHistory(vector<string>& rvecHistory)
{
	string strList(DEFAULT_DB_SERVER_HISTORY);
	if (!m_apHKCU->keyExists(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY))
	{
		m_apHKCU->createKey(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY, strList);
	}
	strList = m_apHKCU->getKeyValue(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY,
		DEFAULT_DB_SERVER_HISTORY);
	

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setDBServerHistory(const vector<string>& vecHistory)
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
	m_apHKCU->setKeyValue(gstrFP_DB_REGISTRY_PATH, DB_SERVER_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
long FileProcessingConfigMgr::getDBLockTimeout()
{
	// Check if key exists
	if (!m_apHKCU->keyExists(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT))
	{
		// if not create key and give it a default value of DEFAULT_DB_LOCK_TIMEOUT
		m_apHKCU->createKey(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT, DEFAULT_DB_LOCK_TIMEOUT);
	}
	// Get the DB Lock time out from the registry
	long lDBLockTimeout = asLong(m_apHKCU->getKeyValue(gstrFP_DB_REGISTRY_PATH, DB_LOCK_TIMEOUT,
		DEFAULT_DB_LOCK_TIMEOUT));

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
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING))
	{
		// Not found, just default to true
		m_apHKCU->createKey(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING, DEFAULT_AUTO_SCROLLING);
	}
	else
	{
		// Retrieve setting
		strValue = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING,
			DEFAULT_AUTO_SCROLLING);
		bAutoScroll = asCppBool(strValue);
	}

	return bAutoScroll;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setAutoScrolling(bool bAutoScroll)
{
	// Store setting
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SCROLLING, bAutoScroll ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getLastGoodDBSettings(string& strServer, string& strDatabase,
	string& strAdvConnStrProperties)
{
	// Set server and datbase to empty values
	strServer = "";
	strDatabase = "";
	strAdvConnStrProperties = "";
	
	// Get the server
	if(m_apHKCU->keyExists(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER)) 
	{
		strServer = m_apHKCU->getKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER,
			DEFAULT_LAST_GOOD_SERVER);
	}

	// Get the databse
	if(m_apHKCU->keyExists(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE)) 
	{
		strDatabase = m_apHKCU->getKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE,
			DEFAULT_LAST_GOOD_DATABASE);
	}

	// Get the additional connection string attributes
	if(m_apHKCU->keyExists(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_ADV_CONN_STR_PROPERTIES)) 
	{
		strAdvConnStrProperties = getDecryptedString(m_apHKCU->getKeyValue(gstrFAM_DBADMIN_REG_PATH,
			LAST_GOOD_ADV_CONN_STR_PROPERTIES, DEFAULT_LAST_GOOD_ADV_CONN_STR_PROPERTIES));
	}
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setLastGoodDBSettings(const string& strServer,
	const string& strDatabase, const string& strAdvConnStrProperties)
{
	// Store Server
	m_apHKCU->setKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_SERVER, strServer);

	// Store Database
	m_apHKCU->setKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_DATABASE, strDatabase);

	// Store connection string attributes
	m_apHKCU->setKeyValue(gstrFAM_DBADMIN_REG_PATH, LAST_GOOD_ADV_CONN_STR_PROPERTIES,
		getEncryptedString(strAdvConnStrProperties));
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::getLocalSQLServerInstances(vector<string>& vecLocalInstances)
{
	// If there is an exception trying to get the instances names just return an
	// empty vector but log the exception to track possible problems
	try
	{
		// Clear the vector
		vecLocalInstances.clear();

		// check if the SQL Server InstalledInstances key exists
		if (getHKLM()->keyExists(gstrSQL_SERVER_REG_PATH, gstrSQL_SERVER_INSTALLED_INSTANCES_KEY))
		{
			// Get the instances from the registry
			vecLocalInstances = getHKLM()->getKeyMultiStringValue(gstrSQL_SERVER_REG_PATH, gstrSQL_SERVER_INSTALLED_INSTANCES_KEY);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20376");
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getUsePreNormalized()
{
	// Default to true
	bool bUsePreNormalized = true;
	string strValue;

	// Check for existence of UsePreNormalized key
	if (!m_apHKCU->keyExists(gstrFP_DB_REGISTRY_PATH, USE_PRE_NORMALIZED))
	{
		// Not found, just default to true
		m_apHKCU->createKey(gstrFP_DB_REGISTRY_PATH, USE_PRE_NORMALIZED, DEFAULT_USE_PRE_NORMALIZED);
	}
	else
	{
		// Retrieve setting
		strValue = m_apHKCU->getKeyValue(gstrFP_DB_REGISTRY_PATH, USE_PRE_NORMALIZED,
			DEFAULT_USE_PRE_NORMALIZED);
		bUsePreNormalized = asCppBool(strValue);
	}

	return bUsePreNormalized;
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getAutoSaveFPSOnRun()
{
	bool bAutoSave = false;
	if (!m_apHKCU->keyExists(gstrFP_DLG_REGISTRY_PATH, AUTO_SAVE_FPS_FILE))
	{
		m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SAVE_FPS_FILE,
			DEFAULT_AUTO_SAVE_FPS_FILE);
	}
	else
	{
		string strVal = m_apHKCU->getKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SAVE_FPS_FILE,
			DEFAULT_AUTO_SAVE_FPS_FILE);
		bAutoSave = asCppBool(strVal);
	}

	return bAutoSave;
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setAutoSaveFPSOnRun(bool bAutoSave)
{
	m_apHKCU->setKeyValue(gstrFP_DLG_REGISTRY_PATH, AUTO_SAVE_FPS_FILE, bAutoSave ? "1" : "0");
}
//-------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getDBManagerType()
{
	if (!m_apHKCU->keyExists(gstrFP_DB_REGISTRY_PATH, DB_MANAGER_TYPE))
	{
		m_apHKCU->setKeyValue(gstrFP_DB_REGISTRY_PATH, DB_MANAGER_TYPE, DEFAULT_DB_MANAGER_TYPE);
		return DEFAULT_DB_MANAGER_TYPE;
	}
	return m_apHKCU->getKeyValue(gstrFP_DB_REGISTRY_PATH, DB_MANAGER_TYPE,
		DEFAULT_DB_MANAGER_TYPE);
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::setDBManagerType(const string& strDBManagerType)
{
	m_apHKCU->setKeyValue(gstrFP_DB_REGISTRY_PATH, DEFAULT_DB_MANAGER_TYPE, strDBManagerType);
}
//-------------------------------------------------------------------------------------------------
bool FileProcessingConfigMgr::getUseApplicationRoles()
{
	if (!getHKLM()->keyExists(gstrFP_DB_REGISTRY_PATH, USE_APPLICATION_ROLES))
	{
		//getHKLM()->setKeyValue(gstrFP_DB_REGISTRY_PATH, USE_APPLICATION_ROLES, DEFAULT_USE_APPLICATION_ROLES);
		return asCppBool(DEFAULT_USE_APPLICATION_ROLES);
	}
	string strVal = getHKLM()->getKeyValue(gstrFP_DB_REGISTRY_PATH, USE_APPLICATION_ROLES,
		DEFAULT_USE_APPLICATION_ROLES);
	return asCppBool(strVal);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
RegistryPersistenceMgr* FileProcessingConfigMgr::getHKLM()
{
	if (m_apHKLM.get() == NULL)
	{
		m_apHKLM.reset(new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI30014", m_apHKLM.get() != __nullptr);
	}

	return m_apHKLM.get();
}
//-------------------------------------------------------------------------------------------------
void FileProcessingConfigMgr::initHKCU()
{
	if (m_apHKCU.get() == NULL)
	{
		m_apHKCU.reset(new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI30015", m_apHKCU.get() != __nullptr);
	}
}
//--------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getEncryptedString(const string strInput)
{
	if (strInput.empty())
	{
		return "";
	}

	// Put the input string into the byte manipulator
	ByteStream bytes;
	ByteStreamManipulator bytesManipulator(ByteStreamManipulator::kWrite, bytes);

	bytesManipulator << strInput;

	// Convert information to a stream of bytes
	// with length divisible by 8 (in variable called 'bytes')
	bytesManipulator.flushToByteStream(8);

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getPassword(pwBS);

	// Do the encryption
	ByteStream encryptedBS;
	MapLabel encryptionEngine;
	encryptionEngine.setMapLabel(encryptedBS, bytes, pwBS);

	// Return the encrypted value
	return encryptedBS.asString();
}
//--------------------------------------------------------------------------------------------------
string FileProcessingConfigMgr::getDecryptedString(const string strInput)
{
	if (strInput.empty())
	{
		return "";
	}

	// Get the password 'key' based on the 4 hex global variables
	ByteStream passwordBS;
	getPassword(passwordBS);

	// Stream to hold the decrypted valud
	ByteStream decryptedBS;

	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decryptedBS, strInput, passwordBS);
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, decryptedBS);

	string strDecrypted = "";
	bsm >> strDecrypted;

	// Return the decrypted value
	return strDecrypted;
}