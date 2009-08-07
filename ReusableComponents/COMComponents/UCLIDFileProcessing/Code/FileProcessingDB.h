// FileProcessingDB.h : Declaration of the CFileProcessingDB

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "FP_UI_Notifications.h"
#include "TransactionGuard.h"

#include <RegistryPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <LockGuard.h>

#include <string>
#include <map>
#include <vector>

using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// moved to header file to be accessible to multiple files
// as per [p13 #4920]
// User name for FAM DB Admin access
const string gstrADMIN_USER = "admin";

//-------------------------------------------------------------------------------------------------
// CFileProcessingDB
//-------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CFileProcessingDB :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileProcessingDB, &CLSID_FileProcessingDB>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFileProcessingDB, &__uuidof(IFileProcessingDB), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1, /* wMinor = */ 0>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>
{
public:
	CFileProcessingDB();
	~CFileProcessingDB();

	DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGDB)

	BEGIN_COM_MAP(CFileProcessingDB)
		COM_INTERFACE_ENTRY2(IDispatch, IFileProcessingDB)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IFileProcessingDB)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IFileProcessingDB Methods
	STDMETHOD(DefineNewAction)( BSTR strAction,  long * pnID);
	STDMETHOD(DeleteAction)( BSTR strAction);
	STDMETHOD(GetActions)( IStrToStrMap * * pmapActionNameToID);
	STDMETHOD(AddFile)( BSTR strFile,  BSTR strAction, VARIANT_BOOL bForceStatusChange, 
		VARIANT_BOOL bFileModified, EActionStatus eNewStatus, 
		VARIANT_BOOL * pbAlreadyExists, EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord);
	STDMETHOD(RemoveFile)( BSTR strFile, BSTR strAction );
	STDMETHOD(RemoveFolder)( BSTR strFolder, BSTR strAction );
	STDMETHOD(NotifyFileProcessed)( long nFileID,  BSTR strAction);
	STDMETHOD(NotifyFileFailed)( long nFileID,  BSTR strAction,  BSTR strException);
	STDMETHOD(SetFileStatusToPending)( long nFileID,  BSTR strAction);
	STDMETHOD(SetFileStatusToUnattempted)( long nFileID,  BSTR strAction);
	STDMETHOD(SetFileStatusToSkipped)(long nFileID, BSTR strAction, VARIANT_BOOL vbUpdateSkippedFileTable);
	STDMETHOD(GetFileStatus)( long nFileID,  BSTR strAction,  EActionStatus * pStatus);
	STDMETHOD(SearchAndModifyFileStatus)( long nWhereActionID,  EActionStatus eWhereStatus,  
		long nToActionID, EActionStatus eToStatus, BSTR bstrSkippedFromUserName, long nFromActionID,
		long * pnNumRecordsModified);
	STDMETHOD(SetStatusForAllFiles)( BSTR strAction,  EActionStatus eStatus);
	STDMETHOD(SetStatusForFile)( long nID,  BSTR strAction,  EActionStatus eStatus,  EActionStatus * poldStatus);
	STDMETHOD(GetFilesToProcess)( BSTR strAction,  long nMaxFiles, VARIANT_BOOL bGetSkippedFiles,
		BSTR bstrSkippedForUserName, IIUnknownVector * * pvecFileRecords);
	STDMETHOD(GetStats)(/*[in]*/ long nActionID, /*[out, retval]*/ IActionStatistics* *pStats);
	STDMETHOD(Clear)();
	STDMETHOD(CopyActionStatusFromAction)( /*[in]*/ long  nFromAction, /*[in]*/ long nToAction );
	STDMETHOD(RenameAction)(/*[in]*/ long  nActionID, /*[in]*/ BSTR strNewActionName );
	STDMETHOD(ExportFileList)(BSTR strQuery, BSTR strOutputFileName, long *pnNumRecordsOutput);
	STDMETHOD(ResetDBLock)(void);
	STDMETHOD(GetActionID)(/*[in]*/ BSTR bstrActionName, /*[out, retval]*/ long* pnActionID);
	STDMETHOD(ResetDBConnection)(void);
	STDMETHOD(SetNotificationUIWndHandle)(long nHandle);
	STDMETHOD(ShowAdminLogin)(VARIANT_BOOL* pbLoginCancelled, VARIANT_BOOL* pbLoginValid);
	STDMETHOD(get_DBSchemaVersion)(LONG* pVal);
	STDMETHOD(ChangeAdminLogin)(VARIANT_BOOL* pbChangeCancelled, VARIANT_BOOL* pbChangeValid);
	STDMETHOD(GetCurrentConnectionStatus)(BSTR* pVal);
	STDMETHOD(get_DatabaseServer)(BSTR* pVal);
	STDMETHOD(put_DatabaseServer)(BSTR newVal);
	STDMETHOD(get_DatabaseName)(BSTR* pVal);
	STDMETHOD(put_DatabaseName)(BSTR newVal);
	STDMETHOD(CreateNewDB)(BSTR bstrNewDBName);
	STDMETHOD(ConnectLastUsedDBThisProcess)();
	STDMETHOD(SetDBInfoSetting)(BSTR bstrSettingName, BSTR bstrSettingValue );
	STDMETHOD(GetDBInfoSetting)(BSTR bstrSettingName, BSTR* pbstrSettingValue );
	STDMETHOD(LockDB)();
	STDMETHOD(UnlockDB)();
	STDMETHOD(GetResultsForQuery)(BSTR bstrQuery, _Recordset** ppVal);
	STDMETHOD(AsStatusString)(EActionStatus eaStatus, BSTR *pbstrStatusString);
	STDMETHOD(AsEActionStatus)(BSTR bstrStatus, EActionStatus *peaStatus);
	STDMETHOD(GetFileID)(BSTR bstrFileName, long* pnFileID);
	STDMETHOD(GetActionName)(long nActionID, BSTR* pbstrActionName);
	STDMETHOD(NotifyFileSkipped)(long nFileID, long nActionID);
	STDMETHOD(SetFileActionComment)(long nFileID, long nActionID, BSTR bstrComment);
	STDMETHOD(GetFileActionComment)(long nFileID, long nActionID, BSTR* pbstrComment);
	STDMETHOD(ClearFileActionComment)(long nFileID, long nActionID);
	STDMETHOD(ModifyActionStatusForQuery)(BSTR bstrQueryFrom, BSTR bstrToAction,
		EActionStatus eaStatus, BSTR bstrFromAction);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	friend class DBLockGuard;
	// Variables

	// Map that contains the open connection for each thread.
	map<DWORD, ADODB::_ConnectionPtr> m_mapThreadIDtoDBConnections;

	// Mutex is locked only inside of the get connection method
	CMutex m_mutex;
	
	// handle to window that should receive the database status notifications
	HWND m_hUIWindow;

	// method to post notifications to the notification window if the window
	// handle has been set.
	void postStatusUpdateNotification(EDatabaseWrapperObjectStatus eStatus);

	// map to keep the stats for each of the Action
	map<long, UCLID_FILEPROCESSINGLib::IActionStatisticsPtr> m_mapActionIDtoStats;

	// Member variable that contains the last read schema version. 0 indicates no version
	int m_iDBSchemaVersion;

	// This contains the UniqueProcess Identifier (UPI)
	string m_strUPI;

	// Flag indicating that this instance has the lock on the DB
	bool m_bDBLocked;

	// Machine username
	string m_strFAMUserName;

	// Used to hold the FAMUserID of m_strFAMUserName in the database
	// this value is set to 0 if the database connection info changes
	long m_lFAMUserID;
	
	// Machine name
	string m_strMachineName;

	// Used to hold the MachineID of m_strMachineName in the databse
	// this value is set to 0 if the database connection info changes
	long m_lMachineID;

	// The DB Lock time out in seconds
	long m_lDBLockTimeout;

	// Registry configuration managers
	RegistryPersistenceMgr m_regUserCfgMgr;
	FileProcessingConfigMgr m_regFPCfgMgr;

	// This string should always contain the current status string
	string m_strCurrentConnectionStatus;

	// The database server to connect to
	string m_strDatabaseServer;

	// The database to connect to
	string m_strDatabaseName;

	// Saves the last set server name in the current process
	static string ms_strCurrServerName;

	// Saves the last set Database name in the current process
	static string ms_strCurrDBName;

	// Contains the timeout for query execution
	int m_iCommandTimeout;

	// Flag indicating if records should be added to the QueueEvent table
	bool m_bUpdateQueueEventTable;
	
	// Flag indicating if records should be added to the FileActionStatusTransition table
	bool m_bUpdateFASTTable;

	// Contains the number of times an attempt to reconnect. Each
	// time the reconnect attempt times out an exception will be logged.
	int m_iNumberOfRetries;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	//-------------------------------------------------------------------------------------------------
	// Methods
	//-------------------------------------------------------------------------------------------------
	
	// PROMISE: returns a pointer to a new FileRecord object filled from ipFields
	UCLID_FILEPROCESSINGLib::IFileRecordPtr getFileRecordFromFields(ADODB::FieldsPtr ipFields);

	// PROMISE: To transfer the data from the ipFileRecord object to the appropriate Field in ipFields
	// NOTE: This does not set the ID field from the ipFileRecord.
	void setFieldsFromFileRecord(ADODB::FieldsPtr ipFields, UCLID_FILEPROCESSINGLib::IFileRecordPtr ipFileRecord);
	
	// PROMISE:	To return the EActionStatus code for the string 
	//			representation( "U", "P", "R", "F", or "C" ) given
	EActionStatus asEActionStatus ( const string& strStatus );

	// PROMISE:	To return the string representation of the given EActionStatus
	string asStatusString ( EActionStatus eStatus );

	// PROMISE:	To add a single record to the QueueEvent table in the database with the given data
	//			using the connection provided
	void addQueueEventRecord( ADODB::_ConnectionPtr ipConnection, long nFileID, 
		string strFileName, string strQueueEventCode );

	// PROMISE: To add a single record to the FileActionStateTransition table with the given data
	// ARGS:	ipConnection	- Connection object to use to update the tables
	//			nFileID			- ID of the file in the FPMFile table that is changing states
	//			nActionID		- The ID of the action state is changing
	//			strFromState	- The old state from the action
	//			strToState		- The new state for the action
	//			strException	- Contains the exception string if this transitioning to a failed state
	//			strComment		- Comment for the added records
	void addFileActionStateTransition ( ADODB::_ConnectionPtr ipConnection, long nFileID, 
		long nActionID, const string &strFromState, const string &strToState, 
		const string &strException, const string &strComment );

	// PROMISE:	To add multiple ActionStateTransition table records that are represented be the given data.
	// ARGS:	ipConnection	- Connection object to use to update the tables
	//			strAction		- The action whos state is changing
	//			nActionID		- The id of the action that is changing
	//			strToState		- The new state for the action
	//			strException	- Contains the exception string if this transitioning to a failed state
	//			strComment		- Comment for the added records
	//			strWhereClause	- Where clause to select the records from the FPMFile that the state is changing, this should
	//							  be used so that only one current action state is selected
	//			strTopClause	- Top clause to specifiy the number of records that meet the where clause condition that should
	//							  have records added to the ActionStateTransition table
	void addASTransFromSelect ( ADODB::_ConnectionPtr ipConnection, const string &strAction,
		long nActionID, const string &strToState, const string &strException, const string &strComment, 
		const string &strWhereClause, const string &strTopClause );

	// PROMISE:	To return the ID from the Action table from the given Action Name and modify strActionName to match
	//			the action name stored in the database using the connection object provided.
	long getActionID( ADODB::_ConnectionPtr ipConnection, string& rstrActionName );

	// PROMISE: To return the Action name for the given ID using the connection object provided;
	string getActionName(ADODB::_ConnectionPtr ipConnection, long nActionID);

	// PROMISE: To return the ID from the FAMFile table for the given File name and
	// modify strFileName to match the file name stored in the database using the connection provided.
	long getFileID(ADODB::_ConnectionPtr ipConnection, string& rstrFileName);

	// PROMISE: To return the m_ipDBConnection opened database connection for the current thread.
	ADODB::_ConnectionPtr getDBConnection();

	// PROMISE: To close the database connection on the current thread, if it is open currently
	void closeDBConnection();

	// PROMISE:	 To set the given File's action state for the action given by strAction to the 
	//			state in strState and returns the old state using the connection object provided.
	// NOTE:	If bLockDB == false then the outer scope must lock the DB and declare a transaction
	//			guard, if bLockDB == true then this method will lock the DB and declare a
	//			transaction guard (in this case the outer scope MUST NOT lock the DB or begin
	//			a transaction)
	EActionStatus setFileActionState( ADODB::_ConnectionPtr ipConnection, long nFileID,
		string strAction, const string& strState, const string& strException,
		long nActionID = -1, bool bLockDB = true, bool bUpdateSkippedTable = true);

	// PROMISE: Recalculates the statistics for the given Action ID using the connection provided.
	void reCalculateStats( ADODB::_ConnectionPtr ipConnection, long nActionID );

	// PROMISE:	To drop all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void dropTables();
	
	// PROMISE:	To Add all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void addTables();
	
	// PROMISE:	To set the initial values for QueueEventCode, ActionState and DBInfo
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void initializeTableValues();

	// PROMISE: To copy the status from the strFrom action to the strTo action
	//			if bAddTransRecords is true records will be added to the transition table
	//			using the connection provided.
	void copyActionStatus( ADODB::_ConnectionPtr ipConnection, string strFrom, 
		string strTo, bool bAddTransRecords, long nToActionID = -1);

	// PROMISE:	To add action related columns and indexes to the FPMFile table
	void addActionColumn(string strAction);

	// PROMISE: To remove action related columns and indexes from the FPMFile table
	void removeActionColumn(string strAction);

	// PROMISE: To update totals in the ActionStatistics table using the connection provided
	//			The FileSize and Pages from the ipNewRecord are added to the eToStatus's totals and
	//			the FileSize and Pages from the ipOldRecord are subtracted from the eFromStatus stats
	//			Only time a ipNewRecord can be NULL is if the to status is kActionUnattempted
	//			Only time a ipOldRecord can be NULL is if the from status is kActionUnattempted
	void updateStats( ADODB::_ConnectionPtr ipConnection, long nActionID, EActionStatus eFromStatus, 
		EActionStatus eToStatus, UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewRecord, 
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord);
					
	// PROMISE: To load the current stats record from the db from ActionStatistics table using the
	//			connection provided.
	//			if a record does not exist one is created by running recalculate stats
	//			the record will be placed in the m_mapActionIDtoStats
	//			if recalculate is called this returns true
	bool loadStats( ADODB::_ConnectionPtr ipConnection, long nActionID );

	// PROMISE: To save the current stats record to that db ActionStatistics table
	void saveStats( ADODB::_ConnectionPtr ipConnection, long nActionID );

	// Returns the DBSchemaVersion
	int getDBSchemaVersion();

	// Locks the db for use using the connection provided.
	void lockDB( ADODB::_ConnectionPtr ipConnection );
	
	// unlocks db using the connection provided.
	void unlockDB(ADODB::_ConnectionPtr ipConnection );

	// Looks up the username: admin in the Login table
	// if no record return empty string
	// if there is a record returns the stored password
	string getEncryptedAdminPWFromDB();

	// Encrypts the provided user name + password string
	// and stores the result in the Login table
	void encryptAndStoreUserNamePassword(const string strUserNameAndPassword);

	// Returns the result of encrypting the input string
	string getEncryptedString(const string strInput);

	// Throws and exception if the DBSchemaVersion in the DB is different from the current DBSchemaVersion
	void validateDBSchemaVersion();

	// Returns the this pointer as a IFileProcssingDBPtr COM pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getThisAsCOMPtr();

	// Checks the strPassword value agains the password from the database
	bool isAdminPasswordValid(const string& strPassword);

	// Checks for blank database and if it is blank will clear the database to set it up
	void initializeIfBlankDB();

	// Fills the rvecTables vector with the Extract Tables
	void getExpectedTables(vector<string>& rvecTables);

	// Returns true if the given strTable is in the vector of expected tables
	bool isExtractTable(const string& strTable);

	// Drops all of the foreign key constraints for the base Extract tables
	void dropAllFKConstraints();

	// If m_lMachineID == 0 will lookup m_strMachineName in the machine table and add it if it is not there
	// If m_lMachineID != 0 will return the value of m_lMachineID
	long getMachineID(ADODB::_ConnectionPtr ipConnection);

	// If m_lFAMUserID == 0 will lookup m_strMachineUserName in the FAMUser table and add it if it is not there
	// If m_lFAMUserID != 0 will return the value of m_lFAMUserID;
	long getFAMUserID(ADODB::_ConnectionPtr ipConnection);

	// Loads settings from the DBInfo table if any exceptions are thrown while
	// obtaining the settings the exception will be logged. 
	// so that this function will always return	
	void loadDBInfoSettings(ADODB::_ConnectionPtr ipConnection);

	// Returns the running apps main window handle if can't get the main window returns NULL;
	HWND getAppMainWndHandle();

	// Returns an IUnknownVector of ProductSpecificMgrs
	IIUnknownVectorPtr getLicensedProductSpecificMgrs();

	// Removes the schema for each of the licensed product specific managers
	void removeProductSpecificDB();

	// Adds the schema for each of the licensed product specific managers
	void addProductSpecificDB();

	// Try's to read the sql server time using the provided connection and if it fails returns false
	bool isConnectionAlive(ADODB::_ConnectionPtr ipConnection);

	// Recreates the connection for the current thread. If there is no connection object for 
	// the current thread it will be created using the getDBConnection method. If the creation of
	// the connection object fails it will be reattempted for gdRETRY_TIMEOUT (120sec). If after the
	// timeout it was still not possible to create the connection object false will be returned,
	// otherwise true will be returned.
	bool reConnectDatabase();

	// Adds a record to the skipped file table
	void addSkipFileRecord(const ADODB::_ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Removes a record from the skipped file table
	void removeSkipFileRecord(const ADODB::_ConnectionPtr& ipConnection, long nFileID,
		long nActionID);

	// Internal reset DB connection function
	void resetDBConnection();

	// Internal clear DB function
	void clear();

	// Fills a vector with the FileIDs of files skipped by a specific user (or skipped by
	// all users if strUserName is "")
	void getFilesSkippedByUser(vector<long>& rvecSkippedFileIDs, long nActionID,
		string strUserName, const ADODB::_ConnectionPtr& ipConnection);

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingDB), CFileProcessingDB)
