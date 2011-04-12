// FileProcessingDB.h : Declaration of the CFileProcessingDB

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "FAMDBHelperFunctions.h"
#include "FilePriorityHelper.h"
#include "FP_UI_Notifications.h"
#include "TransactionGuard.h"

#include <RegistryPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <LockGuard.h>
#include <Win32Event.h>
#include <StringCSIS.h>

#include <string>
#include <map>
#include <vector>
#include <set>

using namespace std;
using namespace ADODB;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// moved to header file to be accessible to multiple files
// as per [p13 #4920]
// User name for FAM DB Admin access
const string gstrADMIN_USER = "admin";

// Table names
static const string gstrACTION = "Action";
static const string gstrACTION_STATE = "ActionState";
static const string gstrACTION_STATISTICS = "ActionStatistics";
static const string gstrACTION_STATISTICS_DELTA = "ActionStatisticsDelta";
static const string gstrDB_INFO = "DBInfo";
static const string gstrFAM_FILE = "FAMFile";
static const string gstrFILE_ACTION_STATE_TRANSITION = "FileActionStateTransition";
static const string gstrLOCK_TABLE = "LockTable";
static const string gstrLOGIN = "Login";
static const string gstrQUEUE_EVENT = "QueueEvent";
static const string gstrQUEUE_EVENT_CODE = "QueueEventCode";
static const string gstrMACHINE = "Machine";
static const string gstrFAM_USER = "FAMUser";
static const string gstrFAM_FILE_ACTION_COMMENT = "FileActionComment";
static const string gstrFAM_SKIPPED_FILE = "SkippedFile";
static const string gstrFAM_TAG = "Tag";
static const string gstrFAM_FILE_TAG = "FileTag";
static const string gstrPROCESSING_FAM = "ProcessingFAM";
static const string gstrLOCKED_FILE = "LockedFile";
static const string gstrUSER_CREATED_COUNTER = "UserCreatedCounter";
static const string gstrFPS_FILE = "FPSFile";
static const string gstrFAM_SESSION = "FAMSession";
static const string gstrINPUT_EVENT = "InputEvent";
static const string gstrFILE_ACTION_STATUS = "FileActionStatus";
static const string gstrSOURCE_DOC_CHANGE_HISTORY = "SourceDocChangeHistory";
static const string gstrDOC_TAG_HISTORY = "DocTagHistory";
static const string gstrDB_INFO_HISTORY = "DBInfoChangeHistory";

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

	void FinalRelease();

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IFileProcessingDB Methods
	STDMETHOD(DefineNewAction)(BSTR strAction, long* pnID);
	STDMETHOD(DeleteAction)(BSTR strAction);
	STDMETHOD(GetActions)(IStrToStrMap** pmapActionNameToID);
	STDMETHOD(AddFile)(BSTR strFile, BSTR strAction, EFilePriority ePriority,
		VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified, EActionStatus eNewStatus, 
		VARIANT_BOOL* pbAlreadyExists, EActionStatus* pPrevStatus, IFileRecord** ppFileRecord);
	STDMETHOD(RemoveFile)(BSTR strFile, BSTR strAction);
	STDMETHOD(RemoveFolder)(BSTR strFolder, BSTR strAction);
	STDMETHOD(NotifyFileProcessed)(long nFileID, BSTR strAction);
	STDMETHOD(NotifyFileFailed)(long nFileID, BSTR strAction, BSTR strException);
	STDMETHOD(SetFileStatusToPending)(long nFileID, BSTR strAction);
	STDMETHOD(SetFileStatusToUnattempted)(long nFileID, BSTR strAction);
	STDMETHOD(SetFileStatusToSkipped)(long nFileID, BSTR strAction, 
		VARIANT_BOOL bRemovePreviousSkipped);
	STDMETHOD(GetFileStatus)(long nFileID, BSTR strAction, VARIANT_BOOL vbAttemptRevertIfLocked,
		EActionStatus* pStatus);
	STDMETHOD(SearchAndModifyFileStatus)(long nWhereActionID, EActionStatus eWhereStatus, 
		long nToActionID, EActionStatus eToStatus, BSTR bstrSkippedFromUserName, long nFromActionID,
		long* pnNumRecordsModified);
	STDMETHOD(SetStatusForAllFiles)(BSTR strAction, EActionStatus eStatus);
	STDMETHOD(SetStatusForFile)(long nID, BSTR strAction, EActionStatus eStatus, 
		EActionStatus* poldStatus);
	STDMETHOD(GetFilesToProcess)(BSTR strAction, long nMaxFiles, VARIANT_BOOL bGetSkippedFiles,
		BSTR bstrSkippedForUserName, IIUnknownVector** pvecFileRecords);
	STDMETHOD(GetStats)(long nActionID, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(Clear)(VARIANT_BOOL vbRetainUserValues);
	STDMETHOD(CopyActionStatusFromAction)(long  nFromAction, long nToAction);
	STDMETHOD(RenameAction)(long  nActionID, BSTR strNewActionName);
	STDMETHOD(ExportFileList)(BSTR strQuery, BSTR strOutputFileName,
		IRandomMathCondition* pRandomCondition,long* pnNumRecordsOutput);
	STDMETHOD(ResetDBLock)(void);
	STDMETHOD(GetActionID)(BSTR bstrActionName, long* pnActionID);
	STDMETHOD(ResetDBConnection)(void);
	STDMETHOD(SetNotificationUIWndHandle)(long nHandle);
	STDMETHOD(ShowLogin)(VARIANT_BOOL bShowAdmin, VARIANT_BOOL* pbLoginCancelled, 
		VARIANT_BOOL* pbLoginValid);
	STDMETHOD(get_DBSchemaVersion)(LONG* pVal);
	STDMETHOD(ChangeLogin)(VARIANT_BOOL bChangeAdmin, VARIANT_BOOL* pbChangeCancelled, 
		VARIANT_BOOL* pbChangeValid);
	STDMETHOD(GetCurrentConnectionStatus)(BSTR* pVal);
	STDMETHOD(get_DatabaseServer)(BSTR* pVal);
	STDMETHOD(put_DatabaseServer)(BSTR newVal);
	STDMETHOD(get_DatabaseName)(BSTR* pVal);
	STDMETHOD(put_DatabaseName)(BSTR newVal);
	STDMETHOD(CreateNewDB)(BSTR bstrNewDBName);
	STDMETHOD(ConnectLastUsedDBThisProcess)();
	STDMETHOD(SetDBInfoSetting)(BSTR bstrSettingName, BSTR bstrSettingValue, VARIANT_BOOL vbSetIfExists);
	STDMETHOD(GetDBInfoSetting)(BSTR bstrSettingName, VARIANT_BOOL vbThrowIfMissing,
		BSTR* pbstrSettingValue);
	STDMETHOD(LockDB)(BSTR bstrLockName);
	STDMETHOD(UnlockDB)(BSTR bstrLockName);
	STDMETHOD(GetResultsForQuery)(BSTR bstrQuery, _Recordset** ppVal);
	STDMETHOD(AsStatusString)(EActionStatus eaStatus, BSTR* pbstrStatusString);
	STDMETHOD(AsEActionStatus)(BSTR bstrStatus, EActionStatus* peaStatus);
	STDMETHOD(GetFileID)(BSTR bstrFileName, long* pnFileID);
	STDMETHOD(GetActionName)(long nActionID, BSTR* pbstrActionName);
	STDMETHOD(NotifyFileSkipped)(long nFileID, long nActionID);
	STDMETHOD(SetFileActionComment)(long nFileID, long nActionID, BSTR bstrComment);
	STDMETHOD(GetFileActionComment)(long nFileID, long nActionID, BSTR* pbstrComment);
	STDMETHOD(ClearFileActionComment)(long nFileID, long nActionID);
	STDMETHOD(ModifyActionStatusForQuery)(BSTR bstrQueryFrom, BSTR bstrToAction,
		EActionStatus eaStatus, BSTR bstrFromAction, IRandomMathCondition* pRandomCondition,
		long* pnNumRecordsModified);
	STDMETHOD(GetTags)(IStrToStrMap** ppTags);
	STDMETHOD(GetTagNames)(IVariantVector** ppTagNames);
	STDMETHOD(HasTags)(VARIANT_BOOL* pvbVal);
	STDMETHOD(TagFile)(long nFileID, BSTR bstrTagName);
	STDMETHOD(UntagFile)(long nFileID, BSTR bstrTagName);
	STDMETHOD(ToggleTagOnFile)(long nFileID, BSTR bstrTagName);
	STDMETHOD(AddTag)(BSTR bstrTagName, BSTR bstrTagDescription, VARIANT_BOOL vbFailIfExists);
	STDMETHOD(DeleteTag)(BSTR bstrTagName);
	STDMETHOD(ModifyTag)(BSTR bstrOldTagName, BSTR bstrNewTagName, BSTR bstrNewTagDescription);
	STDMETHOD(GetFilesWithTags)(IVariantVector* pvecTagNames, VARIANT_BOOL vbAndOperation,
		IVariantVector** ppvecFileIDs);
	STDMETHOD(GetTagsOnFile)(long nFileID, IVariantVector** ppvecTagNames);
	STDMETHOD(AllowDynamicTagCreation)(VARIANT_BOOL* pvbVal);
	STDMETHOD(SetStatusForFilesWithTags)(IVariantVector* pvecTagNames, VARIANT_BOOL vbAndOperation,
		long nToActionID, EActionStatus eaNewStatus, long nFromActionID);
	STDMETHOD(GetPriorities)(IVariantVector** ppvecPriorities);
	STDMETHOD(AsPriorityString)(EFilePriority ePriority, BSTR* pbstrPriority);
	STDMETHOD(AsEFilePriority)(BSTR bstrPriority, EFilePriority* pePriority);
	STDMETHOD(ExecuteCommandQuery)(BSTR bstrQuery, long* pnRecordsAffected);
	STDMETHOD(SetPriorityForFiles)(BSTR bstrSelectQuery, EFilePriority eNewPriority,
		IRandomMathCondition* pRandomCondition, long* pnNumRecordsModified);
	STDMETHOD(AddUserCounter)(BSTR bstrCounterName, LONGLONG llInitialValue);
	STDMETHOD(RemoveUserCounter)(BSTR bstrCounterName);
	STDMETHOD(RenameUserCounter)(BSTR bstrCounterName, BSTR bstrNewCounterName);
	STDMETHOD(SetUserCounterValue)(BSTR bstrCounterName, LONGLONG llNewValue);
	STDMETHOD(GetUserCounterValue)(BSTR bstrCounterName, LONGLONG* pllValue);
	STDMETHOD(GetUserCounterNames)(IVariantVector** ppvecNames);
	STDMETHOD(GetUserCounterNamesAndValues)(IStrToStrMap** ppmapUserCounters);
	STDMETHOD(IsUserCounterValid)(BSTR bstrCounterName, VARIANT_BOOL* pbCounterValid);
	STDMETHOD(OffsetUserCounter)(BSTR bstrCounterName, LONGLONG llOffsetValue, LONGLONG* pllNewValue);
	STDMETHOD(RegisterProcessingFAM)(long lActionID);
	STDMETHOD(UnregisterProcessingFAM)();
	STDMETHOD(RecordFAMSessionStart)(BSTR bstrFPSFileName);
	STDMETHOD(RecordFAMSessionStop)();
	STDMETHOD(RecordInputEvent)(BSTR bstrTimeStamp, long nActionID, long nEventCount,
		long nProcessID); 
	STDMETHOD(GetLoginUsers)(IStrToStrMap** ppUsers);
	STDMETHOD(AddLoginUser)(BSTR bstrUserName);
	STDMETHOD(RemoveLoginUser)(BSTR bstrUserName);
	STDMETHOD(RenameLoginUser)(BSTR bstrUserNameToRename, BSTR bstrNewUserName);	
	STDMETHOD(ClearLoginUserPassword)(BSTR bstrUserName);
	STDMETHOD(GetAutoCreateActions)(VARIANT_BOOL* pvbValue);
	STDMETHOD(AutoCreateAction)(BSTR bstrActionName, long* plId);
	STDMETHOD(CanSkipAuthenticationOnThisMachine)(VARIANT_BOOL* pvbSkipAuthentication);
	STDMETHOD(GetFileRecord)(BSTR bstrFile, BSTR bstrActionName, IFileRecord** ppFileRecord);
	STDMETHOD(SetFileStatusToProcessing)(long nFileId, long nActionID);
	STDMETHOD(GetConnectionRetrySettings)(long* pnNumberOfRetries, double* pdRetryTimeout);
	STDMETHOD(CloseAllDBConnections)();
	STDMETHOD(UpgradeToCurrentSchema)(IProgressStatus* pProgressStatus);
	STDMETHOD(RenameFile)(IFileRecord* pFileRecord, BSTR bstrNewName);
	STDMETHOD(get_DBInfoSettings)(IStrToStrMap** ppSettings);
	STDMETHOD(SetDBInfoSettings)(IStrToStrMap* pSettings, long* plNumUpdatedRows);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

private:
	// Current schema version for the non normailized DB
	static const long ms_lFAMDBSchemaVersion;

	class SetFileActionData
	{
	public:
		SetFileActionData(long fileId, UCLID_FILEPROCESSINGLib::IFileRecordPtr ipRecord,
			EActionStatus eaFromStatus)
			: FileID(fileId),
			FileRecord(ipRecord),
			FromStatus(eaFromStatus)
		{
		}
		~SetFileActionData()
		{
			try
			{
				FileRecord = NULL;
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI30296");
		}

		UCLID_FILEPROCESSINGLib::IFileRecordPtr FileRecord;
		long FileID;
		EActionStatus FromStatus;
	};

	friend class DBLockGuard;
	// Variables

	// Map that contains the open connection for each thread.
	map<DWORD, _ConnectionPtr> m_mapThreadIDtoDBConnections;

	// Mutex is locked only inside of the get connection method
	CMutex m_mutex;
	
	// handle to window that should receive the database status notifications
	HWND m_hUIWindow;

	// method to post notifications to the notification window if the window
	// handle has been set.
	void postStatusUpdateNotification(EDatabaseWrapperObjectStatus eStatus);

	// Member variable that contains the last read schema version. 0 indicates no version
	int m_iDBSchemaVersion;

	// For the name of each product specific database component, its associated schema version.
	bool m_bProductSpecificDBSchemasAreValid;

	// This contains the UniqueProcess Identifier (UPI)
	string m_strUPI;

	// This contains the ID for the registered UPI in the ProcessFAM table in the DB
	// If 0 there is not a registered UPI
	int m_nUPIID;

	// Flags indicating that this instance has the specified lock on the DB
	// NOTE: If other locks are added, be sure to add the map entry in the
	// constructor for the new lock.
	bool m_bMainLock;
	bool m_bUserCounterLock;
	map<string, bool*> m_mapDbLocks;

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

	// Registry configuration manager
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

	// Flag indicating whether file action comments should be deleted when files are completed
	bool m_bAutoDeleteFileActionComment;

	// Flag indicating whether to automatically revert files whose FAM is no longer processing
	bool m_bAutoRevertLockedFiles;

	// Timeout value for automatically reverting files
	int m_nAutoRevertTimeOutInMinutes;

	// List of email addresses to send email when files are reverted.
	string m_strAutoRevertNotifyEmailList;

	// Contains the number of times an attempt to reconnect. Each
	// time the reconnect attempt times out an exception will be logged.
	int m_iNumberOfRetries;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	// Contains the timeout in seconds to keep retrying the GetFilesToProcess Transaction
	double m_dGetFilesToProcessTransactionTimeout;

	// Number of Seconds between refreshing the ActionStatistics
	long m_nActionStatisticsUpdateFreqInSeconds;

	// Flag indicating whether to store source doc change history
	bool m_bStoreSourceDocChangeHistory;

	// Flag indicating whether tags can be dynamically created.
	bool m_bAllowDynamicTagCreation;

	// Flag indicating whether to store doc tag history
	bool m_bStoreDocTagHistory;

	IMiscUtilsPtr m_ipMiscUtils;

	// Events used for the LastPingThread
	Win32Event m_eventStopPingThread;
	Win32Event m_eventPingThreadExited;

	// Flag to indicate that the FAM has been registered for auto revert
	// if this is false and then pingDB just returns without doing anything
	// if this is true pingDB updates the LastPingTime in ProcessingFAM record 
	// and will log changes of the m_nUPIID
	bool m_bFAMRegistered;

	// Indicates whether the DB schema is currently being validated or upgraded.
	volatile bool m_bValidatingOrUpdatingSchema;

	// Indicates that a revert has been started on another thread so it is not
	// necessary to start it again.
	volatile bool m_bRevertInProgress;

	//-------------------------------------------------------------------------------------------------
	// Methods
	//-------------------------------------------------------------------------------------------------
	
	// PROMISE: Throws an exception if processing is active on the action.
	// NOTE: If Auto revert is enabled the files will be reverted in a transaction, so this
	//		 must be called outside of an active transaction.
	//		bDBLocked - indicates if the database is locked, this is needed because the auto revert
	//		requires the database to be locked.
	void assertProcessingNotActiveForAction(bool bDBLocked, _ConnectionPtr ipConnection, const long &lActionID);

	// PROMISE: Throws an exception if processing is active on any action.
	// NOTE: If Auto revert is enabled the files will be reverted in a transaction, so this
	//		 must be called outside of an active transaction.
	//		bDBLocked - indicates if the database is locked, this is needed because the auto revert
	//		requires the database to be locked.
	void assertProcessingNotActiveForAnyAction(bool bDBLocked);

	// PROMISE: returns a pointer to a new FileRecord object filled from ipFields
	UCLID_FILEPROCESSINGLib::IFileRecordPtr getFileRecordFromFields(const FieldsPtr& ipFields,
		bool bGetPriority = true);

	// PROMISE: To transfer the data from the ipFileRecord object to the appropriate Field in ipFields
	// NOTE: This does not set the ID field from the ipFileRecord.
	void setFieldsFromFileRecord(const FieldsPtr& ipFields,
		const UCLID_FILEPROCESSINGLib::IFileRecordPtr& ipFileRecord, bool bSetPriority = true);
	
	// PROMISE:	To return the EActionStatus code for the string 
	//			representation("U", "P", "R", "F", or "C") given
	EActionStatus asEActionStatus (const string& strStatus);

	// PROMISE:	To return the string representation of the given EActionStatus
	string asStatusString (EActionStatus eStatus);

	// PROMISE: To return a user-readable name for the specified status string
	//			("U", "P", "R", "F", "S" or "C")
	string asStatusName(const string& strStatus);

	// PROMISE:	To add a single record to the QueueEvent table in the database with the given data
	//			using the connection provided
	void addQueueEventRecord(_ConnectionPtr ipConnection, long nFileID, long nActionID,
		string strFileName, string strQueueEventCode);

	// PROMISE: To add a single record to the FileActionStateTransition table with the given data
	// ARGS:	ipConnection	- Connection object to use to update the tables
	//			nFileID			- ID of the file in the FPMFile table that is changing states
	//			nActionID		- The ID of the action state is changing
	//			strFromState	- The old state from the action
	//			strToState		- The new state for the action
	//			strException	- Contains the exception string if this transitioning to a failed state
	//			strComment		- Comment for the added records
	void addFileActionStateTransition (_ConnectionPtr ipConnection, long nFileID, 
		long nActionID, const string &strFromState, const string &strToState, 
		const string &strException, const string &strComment);

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
	void addASTransFromSelect (_ConnectionPtr ipConnection, const string &strAction,
		long nActionID, const string &strToState, const string &strException, const string &strComment, 
		const string &strWhereClause, const string &strTopClause);

	// PROMISE:	To return the ID from the Action table from the given Action Name and modify strActionName to match
	//			the action name stored in the database using the connection object provided.
	long getActionID(_ConnectionPtr ipConnection, string& rstrActionName);

	// PROMISE: To return the Action name for the given ID using the connection object provided;
	string getActionName(_ConnectionPtr ipConnection, long nActionID);

	// PROMISE: To return a record set containing the action with the specified name. The record 
	// set will be empty if no such action exists.
	_RecordsetPtr getActionSet(_ConnectionPtr ipConnection, const string &strAction);

	// PROMISE: Adds an action with the specified name to the specified record set. Returns 
	// the action ID of the newly created action.
	long addActionToRecordset(_ConnectionPtr ipConnection, _RecordsetPtr ipRecordset, 
		const string &strAction);

	// PROMISE: To return the ID from the FAMFile table for the given File name and
	// modify strFileName to match the file name stored in the database using the connection provided.
	long getFileID(_ConnectionPtr ipConnection, string& rstrFileName);

	// PROMISE: To return the m_ipDBConnection opened database connection for the current thread.
	_ConnectionPtr getDBConnection();

	// PROMISE: To close the database connection on the current thread, if it is open currently
	void closeDBConnection();

	// PROMISE:	 To set the given File's action state for the action given by strAction to the 
	//			state in strState and returns the old state using the connection object provided.
	// NOTE:	The outer scope should always lock the DB if required and create transaction if required
	//			If bRemovePreviousSkipped is true and strState == "S" then the skipped file table
	//			will be updated for the file with the information for the current user and process.
	//			If bRemovePreviousSkipped is false and strState == "S" the UPIID will be updated,
	//			but all other skipped file fields will be unmodified.
	EActionStatus setFileActionState(_ConnectionPtr ipConnection, long nFileID,
		string strAction, const string& strState, const string& strException,
		long nActionID = -1, bool bRemovePreviousSkipped = false, 
		const string& strFASTComment = "");

	// PROMISE: To set the specified group of files' action state for the specified action.
	// NOTE:	This will clear the skipped file state for any file ID in the list if
	//			the file is currently in a skipped state.
	//			The outer scope that calls this function must lock the DB and
	//			create a transaction guard.
	void setFileActionState(_ConnectionPtr ipConnection,
		const vector<SetFileActionData>& vecFileData, string strAction, const string& strState);

	// PROMISE: Recalculates the statistics for the given Action ID using the connection provided.
	void reCalculateStats(_ConnectionPtr ipConnection, long nActionID);

	// PROMISE:	To drop all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void dropTables(bool bRetainUserTables);
	
	// PROMISE:	To Add all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void addTables(bool bAddUserTables);

	// PROMISE: Retrieves a vector of SQL queries that creates all the tables for the current DB schema.
	vector<string> getTableCreationQueries(bool bIncludeUserTables);
	
	// PROMISE:	To set the initial values for QueueEventCode, ActionState and DBInfo
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void initializeTableValues(bool bInitializeUserTables);

	// PROMISE: Gets the default values for each of the DBInfo values managed by the FAM DB.
	map<string, string> getDBInfoDefaultValues();

	// PROMISE: To copy the status from the strFrom action to the strTo action
	//			if bAddTransRecords is true records will be added to the transition table
	//			using the connection provided.
	void copyActionStatus(const _ConnectionPtr& ipConnection, const string& strFrom, 
		string strTo, bool bAddTransRecords, long nToActionID = -1);

	// PROMISE:	To add action related columns and indexes to the FPMFile table
	void addActionColumn(const _ConnectionPtr& ipConnection, const string& strAction);

	// PROMISE: To remove action related columns and indexes from the FPMFile table
	void removeActionColumn(const _ConnectionPtr& ipConnection, const string& strAction);

	// PROMISE: To update totals in the ActionStatistics table using the connection provided
	//			The FileSize and Pages from the ipNewRecord are added to the eToStatus's totals and
	//			the FileSize and Pages from the ipOldRecord are subtracted from the eFromStatus stats
	//			Only time a ipNewRecord can be NULL is if the to status is kActionUnattempted
	//			Only time a ipOldRecord can be NULL is if the from status is kActionUnattempted
	void updateStats(_ConnectionPtr ipConnection, long nActionID, EActionStatus eFromStatus, 
		EActionStatus eToStatus, UCLID_FILEPROCESSINGLib::IFileRecordPtr ipNewRecord, 
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord, bool bUpdateAndSaveStats = true);
					
	// PROMISE: To load the stats record from the db from ActionStatistics table using the
	//			connection provided.
	//			if a record does not exist and bDBLocked is true, stats will be calculate stats for the action
	//			if the statistics loaded are out of date and bDBLocked is true, stats will be updated
	//			from the ActionStatisticsDelta table
	//			if the bDBLocked is false and no record exists or stats are out of date an exception
	//			will be thrown.
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr loadStats(_ConnectionPtr ipConnection, 
		long nActionID, bool bForceUpdate, bool bDBLocked);

	// Returns the DBSchemaVersion
	int getDBSchemaVersion();

	// Locks the db for use using the connection provided.
	void lockDB(_ConnectionPtr ipConnection, const string& strLockName);
	
	// unlocks db using the connection provided.
	void unlockDB(_ConnectionPtr ipConnection, const string& strLockName);

	// Looks up the current username in the Login table if bUseAdmin is false
	// and looks up the admin username if bUseAdmin is true
	// if no record rstrEncrypted is an empty string and the return value is false
	// if there is a record returns the stored password and the return value is true;
	bool getEncryptedPWFromDB(string &rstrEncryptedPW, bool bUseAdmin);

	// Encrypts the provided user name + password string
	// and stores the result in the Login table
	void encryptAndStoreUserNamePassword(const string strUserNameAndPassword, bool bUseAdmin,
										bool bFailIfUserDoesNotExist = false);

	// Stores the value contained in the encrypted string into the database.
	// NOTE: This method assumes that the provided string has already been encrypted,
	// DO NOT call this method with an unencrypted string unless you intend to have
	// the combined user and password value stored in plain text.
	void storeEncryptedPasswordAndUserName(const string& strEncryptedPW, bool bUseAdmin,
		bool bFailIfUserDoesNotExist = false, bool bCreateTransactionGuard = true);

	// Returns the result of encrypting the input string
	string getEncryptedString(const string strInput);

	// Throws and exception if the DBSchemaVersion in the DB is different from the current DBSchemaVersion
	void validateDBSchemaVersion();

	// Returns the this pointer as a IFileProcssingDBPtr COM pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getThisAsCOMPtr();

	// Checks the strPassword value agains the password from the database
	bool isPasswordValid(const string& strPassword, bool bUseAdmin);

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
	long getMachineID(_ConnectionPtr ipConnection);

	// If m_lFAMUserID == 0 will lookup m_strMachineUserName in the FAMUser table and add it if it is not there
	// If m_lFAMUserID != 0 will return the value of m_lFAMUserID;
	long getFAMUserID(_ConnectionPtr ipConnection);

	// Loads settings from the DBInfo table if any exceptions are thrown while
	// obtaining the settings the exception will be logged. 
	// so that this function will always return	
	void loadDBInfoSettings(_ConnectionPtr ipConnection);

	// Returns the running apps main window handle if can't get the main window returns NULL;
	HWND getAppMainWndHandle();

	// Returns an IUnknownVector of ProductSpecificMgrs
	IIUnknownVectorPtr getLicensedProductSpecificMgrs();

	// Removes the schema for each of the licensed product specific managers
	void removeProductSpecificDB();

	// Adds the schema for each of the licensed product specific managers
	void addProductSpecificDB();

	// Try's to read the sql server time using the provided connection and if it fails returns false
	bool isConnectionAlive(_ConnectionPtr ipConnection);

	// Recreates the connection for the current thread. If there is no connection object for 
	// the current thread it will be created using the getDBConnection method. If the creation of
	// the connection object fails it will be reattempted for gdRETRY_TIMEOUT (120sec). If after the
	// timeout it was still not possible to create the connection object false will be returned,
	// otherwise true will be returned.
	bool reConnectDatabase();

	// Adds a record to the skipped file table
	void addSkipFileRecord(const _ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Removes a record from the skipped file table
	void removeSkipFileRecord(const _ConnectionPtr& ipConnection, long nFileID,
		long nActionID);

	// Internal reset DB connection function
	void resetDBConnection();

	// Internal close all DB connections
	void closeAllDBConnections();


	// Internal clear DB function
	void clear(bool retainUserValues = false);

	// Internal getActions
	IStrToStrMapPtr getActions(_ConnectionPtr ipConnection);

	// Internal define new action function
	long defineNewAction(_ConnectionPtr ipConnection, const string& strActionName);

	// Fills a vector with the FileIDs of files skipped by a specific user (or skipped by
	// all users if strUserName is "")
	void getFilesSkippedByUser(vector<long>& rvecSkippedFileIDs, long nActionID,
		string strUserName, const _ConnectionPtr& ipConnection);

	// Clears the file action comment for the specified fileID and actionID pair.  If
	// nActionID == -1 will clear comments for the specified file for all actions.
	// If nFileID == -1 will clear comments for all files for the specified action.  If
	// both nActionID == -1 and nFileID == -1 then all comments from the table will be cleared.
	void clearFileActionComment(const _ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Validates the tag name, tag is valid if:
	// 1. It is not NULL or empty string
	// 2. It matches the regular express: ^[a-zA-Z0-9_][a-zA-Z0-9\s_]*$
	void validateTagName(const string& strTagName);

	// Validates the provided file ID
	void validateFileID(const _ConnectionPtr& ipConnection, long nFileID);

	// Gets the tag ID for the specified tag name
	long getTagID(const _ConnectionPtr& ipConnection, string& rstrTagName);

	// Internal function for getting DB info settings
	string getDBInfoSetting(const _ConnectionPtr& ipConnection, const string& strSettingName,
		bool bThrowIfMissing);

	// Reverts file in the LockedFile table to the previous status if the current
	// status is still processing.
	void revertLockedFilesToPreviousState(const _ConnectionPtr& ipConnection, long nUPIID,
		const string& strFASTComment = "", UCLIDException* pUE = NULL);

	// Method checks for timed out FAM's and reverts file status for ones that are found.
	// If there are files to revert and bDBLocked is false an exception will be thrown
	void revertTimedOutProcessingFAMs(bool bDBLocked, const _ConnectionPtr& ipConnection);

	// Thread function that maintains the LastPingtime in the ProcessingFAM table in
	// the database pData should be a pointer to the database object
	static UINT maintainLastPingTimeForRevert(void* pData);

	// Method updates the ProcessingFAM LastPingTime for the currently registered FAM
	void pingDB();

	// Method that creates a thread to send the mail message
	void emailMessage(const string& strMessage);

	// Method to check whether input event tracking is on in the database
	bool isInputEventTrackingEnabled(const _ConnectionPtr& ipConnection);

	// Method to remove old Input events from the InputEvents table
	void deleteOldInputEvents(const _ConnectionPtr& ipConnection);

	// Checks if the current machine is in the list of machines to skip user authentication
	// when running as a service
	bool isMachineInListOfMachinesToSkipUserAuthentication(const _ConnectionPtr& ipConnection);

	// Checks whether the specified action name is valid
	void validateNewActionName(const string& strActionName);

	// Gets a regular expression parser
	IRegularExprParserPtr getParser();

	// Checks the login table for the given UserName
	bool doesLoginUserNameExist(const _ConnectionPtr& ipConnection, const string &strUserName);

	// Class to contain the thread data for the emailMessageThread
	class EmailThreadData
	{
	public:
		string m_strRecipients;
		string m_strMessage;

		EmailThreadData(const string& strRecipients, const string& strMessage):
		  m_strRecipients(strRecipients), m_strMessage(strMessage){};
	};

	// Thread function to email message, pData should be allocated with new and be a pointer to
	// emailThreadData
	static UINT emailMessageThread(void* pData);

	// Marks all records indicated by the specified query to processing. The processing in this
	// function includes attempting to auto-revert locked files, recording appropriate entries in
	// the FAST table and adding an appropriate entry to the locked file table.
	// REQUIRE:	The query must return the following columns from the FAMFile table:
	//			SELECT ID, FileName, Pages, FileSize, Priority and the action status for the
	//			current action from the FileActionStatus table.
	//			if bDBLocked is false and there are files that need to be reverted an exception
	//			will be thrown.
	// RETURNS: A vector of IFileRecords for the files that were set to processing.
	IIUnknownVectorPtr setFilesToProcessing(bool bDBLocked, const _ConnectionPtr &ipConnection,
		const string& strSelectSQL, long nActionID);

	// Gets a set containing the File ID's for all files that are skipped for the specified action
	set<long> getSkippedFilesForAction(const _ConnectionPtr& ipConnection, long nActionId);

	// Returns recordset opened as static containing the status record the file with nFileID and 
	// action nActionID. If the status is unattempted the recordset will be empty
	_RecordsetPtr getFileActionStatusSet(_ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Method to add the current records from the ActionStatisticsDelta table to the 
	// ActionStatistics table
	void updateActionStatisticsFromDelta(const _ConnectionPtr& ipConnection, const long nActionID);

	// Returns a vector of the names of all DBInfo rows and tables in the database that are not
	// managed by the FAM DB itself or by one of the installed product-specific DBs
	vector<string> findUnrecognizedSchemaElements(const _ConnectionPtr& ipConnection);

	// Adds any old DB info values into the list of current db info values so that
	// findUnrecognizedSchemaElements does not fail
	void addOldDBInfoValues(map<string, string>& mapValues);

	// Runs the UpdateSchemaForFAMDBVersion function for each installed product-specific database.
	void executeProdSpecificSchemaUpdateFuncs(_ConnectionPtr ipConnection, int nFAMSchemaVersion,
		long *pnStepCount, IProgressStatusPtr ipProgressStatus,
		map<string, long> &rmapProductSpecificVersions);

	void validateLicense();

	// Internal implementation methods
	bool DefineNewAction_Internal(bool bDBLocked, BSTR strAction, long* pnID);
	bool DeleteAction_Internal(bool bDBLocked, BSTR strAction);
	bool GetActions_Internal(bool bDBLocked, IStrToStrMap * * pmapActionNameToID);
	bool AddFile_Internal(bool bDBLocked, BSTR strFile,  BSTR strAction, EFilePriority ePriority,
		VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
		EActionStatus eNewStatus, VARIANT_BOOL * pbAlreadyExists,
		EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord);
	bool RemoveFile_Internal(bool bDBLocked, BSTR strFile, BSTR strAction);
	bool NotifyFileProcessed_Internal(bool bDBLocked, long nFileID,  BSTR strAction);
	bool NotifyFileFailed_Internal(bool bDBLocked, long nFileID,  BSTR strAction,  BSTR strException);
	bool SetFileStatusToPending_Internal(bool bDBLocked, long nFileID,  BSTR strAction);
	bool SetFileStatusToUnattempted_Internal(bool bDBLocked, long nFileID,  BSTR strAction);
	bool SetFileStatusToSkipped_Internal(bool bDBLocked, long nFileID, BSTR strAction,
		VARIANT_BOOL bRemovePreviousSkipped);
	bool GetFileStatus_Internal(bool bDBLocked, long nFileID,  BSTR strAction,
		VARIANT_BOOL vbAttemptRevertIfLocked, EActionStatus * pStatus);
	bool SearchAndModifyFileStatus_Internal(bool bDBLocked,  
		long nWhereActionID,  EActionStatus eWhereStatus,  long nToActionID, EActionStatus eToStatus,
		BSTR bstrSkippedFromUserName, long nFromActionID, long * pnNumRecordsModified);
	bool SetStatusForAllFiles_Internal(bool bDBLocked, BSTR strAction,  EActionStatus eStatus);
	bool SetStatusForFile_Internal(bool bDBLocked, long nID,  BSTR strAction,  EActionStatus eStatus,  
		EActionStatus * poldStatus);
	bool GetFilesToProcess_Internal(bool bDBLocked, BSTR strAction,  long nMaxFiles, VARIANT_BOOL bGetSkippedFiles,
		BSTR bstrSkippedForUserName, IIUnknownVector * * pvecFileRecords);
	bool RemoveFolder_Internal(bool bDBLocked, BSTR strFolder, BSTR strAction);
	bool GetStats_Internal(bool bDBLocked, long nActionID, VARIANT_BOOL vbForceUpdate, IActionStatistics* *pStats);
	bool CopyActionStatusFromAction_Internal(bool bDBLocked, long  nFromAction, long nToAction);
	bool RenameAction_Internal(bool bDBLocked, long nActionID, BSTR strNewActionName);
	bool Clear_Internal(bool bDBLocked, VARIANT_BOOL vbRetainUserValues);
	bool ExportFileList_Internal(bool bDBLocked, BSTR strQuery, BSTR strOutputFileName,
		IRandomMathCondition* pRandomCondition, long *pnNumRecordsOutput);
	bool GetActionID_Internal(bool bDBLocked, BSTR bstrActionName, long* pnActionID); 
	bool SetDBInfoSetting_Internal(bool bDBLocked, BSTR bstrSettingName, BSTR bstrSettingValue, 
		VARIANT_BOOL vbSetIfExists);
	bool GetDBInfoSetting_Internal(bool bDBLocked, const string& strSettingName, bool bThrowIfMissing,
		string& rstrSettingValue);
	bool GetResultsForQuery_Internal(bool bDBLocked, BSTR bstrQuery, _Recordset** ppVal);
	bool GetFileID_Internal(bool bDBLocked, BSTR bstrFileName, long *pnFileID);
	bool GetActionName_Internal(bool bDBLocked, long nActionID, BSTR *pbstrActionName);
	bool NotifyFileSkipped_Internal(bool bDBLocked, long nFileID, long nActionID);
	bool SetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID, BSTR bstrComment);
	bool GetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID, 
		BSTR* pbstrComment);
	bool ClearFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID);
	bool ModifyActionStatusForQuery_Internal(bool bDBLocked, BSTR bstrQueryFrom, 
		BSTR bstrToAction, EActionStatus eaStatus, BSTR bstrFromAction, 
		IRandomMathCondition* pRandomCondition, long* pnNumRecordsModified);
	bool GetTags_Internal(bool bDBLocked, IStrToStrMap **ppTags);
	bool GetTagNames_Internal(bool bDBLocked, IVariantVector **ppTagNames);
	bool HasTags_Internal(bool bDBLocked, VARIANT_BOOL* pvbVal);
	bool TagFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName);
	bool UntagFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName);
	bool ToggleTagOnFile_Internal(bool bDBLocked, long nFileID, BSTR bstrTagName);
	bool AddTag_Internal(bool bDBLocked, const string& strTagName, const string& strTagDescription,
		bool bFailIfExists);
	bool DeleteTag_Internal(bool bDBLocked, BSTR bstrTagName);
	bool ModifyTag_Internal(bool bDBLocked, BSTR bstrOldTagName, BSTR bstrNewTagName,
		BSTR bstrNewTagDescription);
	bool GetFilesWithTags_Internal(bool bDBLocked, IVariantVector* pvecTagNames,
		VARIANT_BOOL vbAndOperation, IVariantVector** ppvecFileIDs);
	bool GetTagsOnFile_Internal(bool bDBLocked, long nFileID, IVariantVector** ppvecTagNames);
	bool AllowDynamicTagCreation_Internal(bool bDBLocked, VARIANT_BOOL* pvbVal);
	bool SetStatusForFilesWithTags_Internal(bool bDBLocked, IVariantVector *pvecTagNames,
		VARIANT_BOOL vbAndOperation, long nToActionID, EActionStatus eaNewStatus, long nFromActionID);
	bool ExecuteCommandQuery_Internal(bool bDBLocked, BSTR bstrQuery, long* pnRecordsAffected);
	bool UnregisterProcessingFAM_Internal(bool bDBLocked);
	bool SetPriorityForFiles_Internal(bool bDBLocked, BSTR bstrSelectQuery, EFilePriority eNewPriority,
		IRandomMathCondition *pRandomCondition, long *pnNumRecordsModified);
	bool AddUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llInitialValue);
	bool RemoveUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName);
	bool RenameUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, BSTR bstrNewCounterName);
	bool SetUserCounterValue_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llNewValue);
	bool GetUserCounterValue_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG *pllValue);
	bool GetUserCounterNames_Internal(bool bDBLocked, IVariantVector** ppvecNames);
	bool GetUserCounterNamesAndValues_Internal(bool bDBLocked, IStrToStrMap** ppmapUserCounters);
	bool IsUserCounterValid_Internal(bool bDBLocked, BSTR bstrCounterName, VARIANT_BOOL* pbCounterValid);
	bool OffsetUserCounter_Internal(bool bDBLocked, BSTR bstrCounterName, LONGLONG llOffsetValue,
		LONGLONG* pllNewValue);
	bool RecordFAMSessionStart_Internal(bool bDBLocked, BSTR bstrFPSFileName);
	bool RecordFAMSessionStop_Internal(bool bDBLocked);
	bool RecordInputEvent_Internal(bool bDBLocked, BSTR bstrTimeStamp, long nActionID,
		long nEventCount, long nProcessID);
	bool GetLoginUsers_Internal(bool bDBLocked, IStrToStrMap**  ppUsers);
	bool AddLoginUser_Internal(bool bDBLocked, BSTR bstrUserName);
	bool RemoveLoginUser_Internal(bool bDBLocked, BSTR bstrUserName);
	bool RenameLoginUser_Internal(bool bDBLocked, BSTR bstrUserNameToRename, BSTR bstrNewUserName);
	bool ClearLoginUserPassword_Internal(bool bDBLocked, BSTR bstrUserName);
	bool GetAutoCreateActions_Internal(bool bDBLocked, VARIANT_BOOL* pvbValue);
	bool AutoCreateAction_Internal(bool bDBLocked, BSTR bstrActionName, long* plId);
	bool GetFileRecord_Internal(bool bDBLocked, BSTR bstrFile, BSTR bstrActionName,
		IFileRecord** ppFileRecord);
	bool SetFileStatusToProcessing_Internal(bool bDBLocked, long nFileId, long nActionID);
	bool UpgradeToCurrentSchema_Internal(bool bDBLocked, IProgressStatusPtr ipProgressStatus);
	bool RenameFile_Internal(bool bDBLocked, IFileRecord* pFileRecord, BSTR bstrNewName);
	bool get_DBInfoSettings_Internal(bool bDBLocked, IStrToStrMap** ppSettings);
	bool SetDBInfoSettings_Internal(bool bDBLocked, bool bUpdateHistory,
		vector<string> vecQueries, long& nNumRowsUpdated);
};

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingDB), CFileProcessingDB)
