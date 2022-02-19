
#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "FAMDBHelperFunctions.h"
#include "FilePriorityHelper.h"
#include "FP_UI_Notifications.h"
#include "TransactionGuard.h"
#include "DatabaseIDValues.h"
#include "DBCounter.h"
#include "DBCounterUpdate.h"
#include "DBCounterChangeValue.h"

#include <RegistryPersistenceMgr.h>
#include <FileProcessingConfigMgr.h>
#include <LockGuard.h>
#include <Win32Event.h>
#include <StringCSIS.h>
#include <CsisUtils.h>
#include <ApplicationRoleUtility.h>
#include <CppApplicationRoleConnection.h>

#include <string>
#include <map>
#include <vector>
#include <set>
#include <tuple>

using namespace std;
using namespace ADODB;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// moved to header file to be accessible to multiple files
// as per [p13 #4920]
// User name for FAM DB Admin access
const string gstrADMIN_USER = "admin";
const string gstrONE_TIME_ADMIN_USER = "<Admin>";

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
static const string gstrPROCESSING_FAM = "ProcessingFAM"; // No long exists, but keep for schema updates
static const string gstrACTIVE_FAM = "ActiveFAM";
static const string gstrLOCKED_FILE = "LockedFile";
static const string gstrUSER_CREATED_COUNTER = "UserCreatedCounter";
static const string gstrFPS_FILE = "FPSFile";
static const string gstrFAM_SESSION = "FAMSession";
static const string gstrINPUT_EVENT = "InputEvent";
static const string gstrFILE_ACTION_STATUS = "FileActionStatus";
static const string gstrSOURCE_DOC_CHANGE_HISTORY = "SourceDocChangeHistory";
static const string gstrDOC_TAG_HISTORY = "DocTagHistory";
static const string gstrDB_INFO_HISTORY = "DBInfoChangeHistory";
static const string gstrDB_FTP_ACCOUNT = "FTPAccount";
static const string gstrDB_FTP_EVENT_HISTORY = "FTPEventHistory";
static const string gstrDB_QUEUED_ACTION_STATUS_CHANGE = "QueuedActionStatusChange";
static const string gstrDB_FIELD_SEARCH = "FieldSearch";
static const string gstrDB_LAUNCH_APP= "LaunchApp"; // No long exists, but keep for schema updates
static const string gstrDB_FILE_HANDLER= "FileHandler";
static const string gstrDB_FEATURE= "Feature";	
static const string gstrWORK_ITEM="WorkItem";
static const string gstrWORK_ITEM_GROUP="WorkItemGroup";
static const string gstrMETADATA_FIELD="MetadataField";
static const string gstrFILE_METADATA_FIELD_VALUE="FileMetadataFieldValue";
static const string gstrTASK_CLASS="TaskClass";
static const string gstrFILE_TASK_SESSION="FileTaskSession";
static const string gstrFILE_TASK_SESSION_CACHE = "FileTaskSessionCache";
static const string gstrSECURE_COUNTER="SecureCounter";
static const string gstrSECURE_COUNTER_VALUE_CHANGE="SecureCounterValueChange";
static const string gstrPAGINATION="Pagination";
static const string gstrWORKFLOW_TYPE = "WorkflowType";
static const string gstrWORKFLOW = "Workflow";
static const string gstrWORKFLOW_FILE = "WorkflowFile";
static const string gstrWORKFLOWCHANGE = "WorkflowChange";
static const string gstrWORKFLOWCHANGE_FILE = "WorkflowChangeFile";
static const string gstrMLMODEL = "MLModel";
static const string gstrMLDATA = "MLData";
static const string gstrWEB_APP_CONFIG = "WebAppConfig";
static const string gstrDATABASE_SERVICE = "DatabaseService";
static const string gstrREPORTING_VERIFICATION_RATES = "ReportingVerificationRates";
static const string gstrDASHBOARD = "Dashboard";
static const string gstrREPORTING_DATABASE_MIGRATION_WIZARD = "ReportingDatabaseMigrationWizard";
static const string gstrROLE = "Role";
static const string gstrSECURITY_GROUP = "Group";
static const string gstrLOGIN_GROUP_MEMBERSHIP = "LoginGroupMembership";
static const string gstrSECURITY_GROUP_ACTION = "GroupAction";
static const string gstrSECURITY_GROUP_DASHBOARD = "GroupDashboard";
static const string gstrSECURITY_GROUP_REPORT = "GroupReport";
static const string gstrSECURITY_GROUP_WORKFLOW = "GroupWorkflow";
static const string gstrSECURITY_GROUP_Role = "GroupRole";
static const string gstrEMAIL_SOURCE = "EmailSource";

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
	STDMETHOD(AddFile)(BSTR strFile, BSTR strAction, long nWorkflowID, EFilePriority ePriority,
		VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified, EActionStatus eNewStatus, 
		VARIANT_BOOL bSkipPageCount, VARIANT_BOOL* pbAlreadyExists, EActionStatus* pPrevStatus,
		IFileRecord** ppFileRecord);
	STDMETHOD(RemoveFile)(BSTR strFile, BSTR strAction);
	STDMETHOD(RemoveFolder)(BSTR strFolder, BSTR strAction);
	STDMETHOD(NotifyFileProcessed)(long nFileID, BSTR strAction, LONG nWorkflowID,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(NotifyFileFailed)(long nFileID, BSTR strAction, LONG nWorkflowID, BSTR strException,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(SetFileStatusToPending)(long nFileID, BSTR strAction,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(SetFileStatusToUnattempted)(long nFileID, BSTR strAction,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(SetFileStatusToSkipped)(long nFileID, BSTR strAction, 
		VARIANT_BOOL bRemovePreviousSkipped, VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(GetFileStatus)(long nFileID, BSTR strAction, VARIANT_BOOL vbAttemptRevertIfLocked,
		EActionStatus* pStatus);
	STDMETHOD(SetStatusForAllFiles)(BSTR strAction, EActionStatus eStatus);
	STDMETHOD(SetStatusForFile)(long nID, BSTR strAction, long nWorkflowID, EActionStatus eStatus, 
		VARIANT_BOOL vbQueueChangeIfProcessing, VARIANT_BOOL vbAllowQueuedStatusOverride,
		EActionStatus* poldStatus);
	STDMETHOD(GetFilesToProcess)(BSTR strAction, long nMaxFiles, VARIANT_BOOL bGetSkippedFiles,
		BSTR bstrSkippedForUserName, IIUnknownVector** pvecFileRecords);
	STDMETHOD(GetFilesToProcessAdvanced)(BSTR strAction, long nMaxFiles, VARIANT_BOOL bGetSkippedFiles,
		BSTR bstrSkippedForUserName, VARIANT_BOOL bUseRandomIDForQueueOrder, VARIANT_BOOL bLimitToUserQueue, 
		IIUnknownVector** pvecFileRecords);
	STDMETHOD(GetStats)(long nActionID, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(GetVisibleFileStats)(long nActionID, VARIANT_BOOL vbForceUpdate, VARIANT_BOOL vbRevertTimedOutFAMs, IActionStatistics** pStats);
	STDMETHOD(GetInvisibleFileStats)(long nActionID, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(Clear)(VARIANT_BOOL vbRetainUserValues);
	STDMETHOD(CopyActionStatusFromAction)(long  nFromAction, long nToAction);
	STDMETHOD(RenameAction)(BSTR bstrOldActionName, BSTR bstrNewActionNam);
	STDMETHOD(ExportFileList)(BSTR strQuery, BSTR strOutputFileName,
		IRandomMathCondition* pRandomCondition,long* pnNumRecordsOutput);
	STDMETHOD(ResetDBLock)(void);
	STDMETHOD(GetActionID)(BSTR bstrActionName, long* pnActionID);
	STDMETHOD(ResetDBConnection)(VARIANT_BOOL bResetCredentials, VARIANT_BOOL vbCheckForUnnaffiliatedFiles);
	STDMETHOD(SetNotificationUIWndHandle)(long nHandle);
	STDMETHOD(ChangePassword)(BSTR userName, BSTR oldPassword, BSTR newPassword);
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
	STDMETHOD(CreateNewDB)(BSTR bstrNewDBName, BSTR bstrInitWithPassword);
	STDMETHOD(CreateNew80DB)(BSTR bstrNewDBName);
	STDMETHOD(ConnectLastUsedDBThisProcess)();
	STDMETHOD(SetDBInfoSetting)(BSTR bstrSettingName, BSTR bstrSettingValue,
		VARIANT_BOOL vbSetIfExists, VARIANT_BOOL vbRecordHistory);
	STDMETHOD(GetDBInfoSetting)(BSTR bstrSettingName, VARIANT_BOOL vbThrowIfMissing,
		BSTR* pbstrSettingValue);
	STDMETHOD(LockDB_InternalOnly)(BSTR bstrLockName);
	STDMETHOD(UnlockDB_InternalOnly)(BSTR bstrLockName);
	STDMETHOD(GetResultsForQuery)(BSTR bstrQuery, _Recordset** ppVal);
	STDMETHOD(AsStatusString)(EActionStatus eaStatus, BSTR* pbstrStatusString);
	STDMETHOD(AsEActionStatus)(BSTR bstrStatus, EActionStatus* peaStatus);
	STDMETHOD(AsStatusName)(EActionStatus eaStatus, BSTR *pbstrStatusName);
	STDMETHOD(GetFileID)(BSTR bstrFileName, long* pnFileID);
	STDMETHOD(GetActionName)(long nActionID, BSTR* pbstrActionName);
	STDMETHOD(NotifyFileSkipped)(long nFileID, BSTR bstrAction, long nWorkflowID,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	STDMETHOD(SetFileActionComment)(long nFileID, long nActionID, BSTR bstrComment);
	STDMETHOD(GetFileActionComment)(long nFileID, long nActionID, BSTR* pbstrComment);
	STDMETHOD(ClearFileActionComment)(long nFileID, long nActionID);
	STDMETHOD(ModifyActionStatusForSelection)(IFAMFileSelector* pFileSelector, BSTR bstrToAction,
		EActionStatus eaStatus, BSTR bstrFromAction,
		VARIANT_BOOL vbModifyWhenTargetActionMissingForSomeFiles, long* pnNumRecordsModified);
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
	STDMETHOD(GetPriorities)(IVariantVector** ppvecPriorities);
	STDMETHOD(AsPriorityString)(EFilePriority ePriority, BSTR* pbstrPriority);
	STDMETHOD(AsEFilePriority)(BSTR bstrPriority, EFilePriority* pePriority);
	STDMETHOD(ExecuteCommandQuery)(BSTR bstrQuery, long* pnRecordsAffected);
	STDMETHOD(ExecuteCommandReturnLongLongResult)( BSTR bstrQuery, 
		BSTR bstrResultColumnName, long long* pResult, long* pnRecordsAffected );
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
	STDMETHOD(RegisterActiveFAM)();
	STDMETHOD(UnregisterActiveFAM)();
	STDMETHOD(RecordFAMSessionStart)(BSTR bstrFPSFileName, BSTR bstrActionName, VARIANT_BOOL vbQueuing,
		VARIANT_BOOL vbProcessing);
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
	STDMETHOD(RecordFTPEvent)(long nFileId, long nActionID, VARIANT_BOOL vbQueueing,
		EFTPAction eFTPAction, BSTR bstrServerAddress, BSTR bstrUserName, BSTR bstrArg1,
		BSTR bstrArg2, long nRetries, BSTR bstrException);
	STDMETHOD(RecalculateStatistics)();
	STDMETHOD(IsAnyFAMActive)(VARIANT_BOOL* pvbFAMIsActive);
	STDMETHOD(get_RetryOnTimeout)(VARIANT_BOOL* pVal);
	STDMETHOD(put_RetryOnTimeout)(VARIANT_BOOL newVal);
	STDMETHOD(get_AdvancedConnectionStringProperties)(BSTR *pVal);
	STDMETHOD(put_AdvancedConnectionStringProperties)(BSTR newVal);
	STDMETHOD(get_IsConnected)(VARIANT_BOOL* pbIsConnected);
	STDMETHOD(ShowSelectDB)(BSTR bstrPrompt, VARIANT_BOOL bAllowCreation,
		VARIANT_BOOL bRequireAdminLogin, VARIANT_BOOL* pbConnected);
	STDMETHOD(GetFileCount)(VARIANT_BOOL bUseOracleSyntax, LONGLONG* pnFileCount);
	STDMETHOD(get_ConnectionString)(BSTR* pbstrConnectionString);
	STDMETHOD(get_LoggedInAsAdmin)(VARIANT_BOOL* pbLoggedInAsAdmin);
	STDMETHOD(IsFeatureEnabled)(BSTR bstrFeatureName, VARIANT_BOOL* pbFeatureIsEnabled);
	STDMETHOD(DuplicateConnection)(IFileProcessingDB *pConnectionSource);
	STDMETHOD(CreateWorkItemGroup)(long nFileID, long nActionID, BSTR stringizedTask, long nNumberOfWorkItems,
			BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID);
	STDMETHOD(AddWorkItems)(long nWorkItemGroupID, IIUnknownVector *pWorkItems);
	STDMETHOD(GetWorkItemsForGroup)(long nWorkItemGroupID, long nStartPos, long nCount, IIUnknownVector **pWorkItems);
	STDMETHOD(GetWorkItemGroupStatus)(long nWorkItemGroupID, WorkItemGroupStatus *pWorkGroupStatus,
		EWorkItemStatus *pStatus);
	STDMETHOD(GetWorkItemToProcess)(BSTR bstrActionName, VARIANT_BOOL vbRestrictToFAMSession, IWorkItemRecord **ppWorkItem);
	STDMETHOD(NotifyWorkItemFailed)(long nWorkItemID, BSTR stringizedException);
	STDMETHOD(NotifyWorkItemCompleted)(long nWorkItemID);
	STDMETHOD(GetWorkGroupData)(long WorkItemGroupID, long *pnNumberOfWorkItems, BSTR *pstringizedTask);
	STDMETHOD(SaveWorkItemOutput)(long WorkItemID, BSTR strWorkItemOutput);
	STDMETHOD(FindWorkItemGroup)(long nFileID, long nActionID, BSTR stringizedTask, long nNumberOfWorkItems,
			BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID);
	STDMETHOD(SaveWorkItemBinaryOutput)(long WorkItemID, IUnknown *pBinaryOutput);
	STDMETHOD(GetFileSets)(IVariantVector **pvecIDs);
	STDMETHOD(AddFileSet)(BSTR bstrFileSetName, IVariantVector *pvecIDs);
	STDMETHOD(GetFileSetFileIDs)(BSTR bstrFileSetName, IVariantVector **ppvecFileIDs);
	STDMETHOD(GetFileSetFileNames)(BSTR bstrFileSetName, IVariantVector **ppvecFileNames);
	STDMETHOD(GetFileToProcess)(long nFileID, BSTR strAction, BSTR bstrFromState, IFileRecord** ppFileRecord);
	STDMETHOD(SetFallbackStatus)(IFileRecord* pFileRecord, EActionStatus eaFallbackStatus);
	STDMETHOD(GetWorkItemsToProcess)(BSTR bstrActionName, VARIANT_BOOL vbRestrictToFAMSessionID, 
			long nMaxWorkItemsToReturn, EFilePriority eMinPriority, IIUnknownVector **ppWorkItems);
	STDMETHOD(SetWorkItemToPending)(long nWorkItemID);
	STDMETHOD(GetFailedWorkItemsForGroup)(long nWorkItemGroupID, IIUnknownVector **ppWorkItems);
	STDMETHOD(SetMetadataFieldValue)(long nFileID, BSTR bstrMetadataFieldName, BSTR bstrMetadataFieldValue);
	STDMETHOD(GetMetadataFieldValue)(long nFileID, BSTR bstrMetadataFieldName, BSTR *pbstrMetadataFieldValue);
	STDMETHOD(AddMetadataField)(BSTR bstrMetadataFieldName);
	STDMETHOD(DeleteMetadataField)(BSTR bstrMetadataFieldName);
	STDMETHOD(RenameMetadataField)(BSTR bstrOldMetadataFieldName, BSTR bstrNewMetadataFieldName);
	STDMETHOD(GetMetadataFieldNames)(IVariantVector** ppvecMetadataFieldNames);
	STDMETHOD(GetLastConnectionStringConfiguredThisProcess)(BSTR *pbstrConnectionString);
	STDMETHOD(get_ActiveFAMID)(long *pnActiveFAMID);
	STDMETHOD(get_FAMSessionID)(long *pnFAMSessionID);
	STDMETHOD(StartFileTaskSession)(BSTR bstrTaskClassGuid, long nFileID, long nActionID, long *pnFileTaskSessionID);
	STDMETHOD(EndFileTaskSession)(long nFileTaskSessionID, double dOverheadTime,
		double dActivityTime, VARIANT_BOOL vbSessionTimeOut);
	STDMETHOD(GetFileNameFromFileID)( /*[in]*/ long fileID, /*[out, retval]*/ BSTR* pbstrFileName );
	STDMETHOD(GetSecureCounters)(VARIANT_BOOL vbRefresh, IIUnknownVector** ppSecureCounters);
	STDMETHOD(GetSecureCounterName)(long nCounterID, BSTR *pstrCounterName);
	STDMETHOD(ApplySecureCounterUpdateCode)(BSTR strUpdateCode, BSTR *pbstrResult);
	STDMETHOD(GetSecureCounterValue)(long nCounterID, long* pnCounterValue);
	STDMETHOD(DecrementSecureCounter)(long nCounterID, long decrementAmount, long* pnCounterValue);
	STDMETHOD(SecureCounterConsistencyCheck)(VARIANT_BOOL* pvbValid);
	STDMETHOD(GetCounterUpdateRequestCode)(BSTR* pstrUpdateRequestCode);
	STDMETHOD(get_DatabaseID)(BSTR* pbstrDatabaseID);
	STDMETHOD(get_ConnectedDatabaseServer)(BSTR* pbstrDatabaseServer);
	STDMETHOD(get_ConnectedDatabaseName)(BSTR* pbstrDatabaseName);
	STDMETHOD(SetSecureCounterAlertLevel)(long nCounterID, long nAlertLevel, long nAlertMultiple);
	STDMETHOD(AddFileNoQueue)(BSTR bstrFile, long long llFileSize, long lPageCount,
		EFilePriority ePriority, long nWorkflowID, long* pnID);
	STDMETHOD(AddPaginationHistory)(long nOutputFileID, IIUnknownVector* pSourcePageInfo,
		IIUnknownVector* pDeletedSourcePageInfo, long nFileTaskSessionID);
	STDMETHOD(AddWorkflow)(BSTR bstrName, EWorkflowType eType, long* pnID);
	STDMETHOD(DeleteWorkflow)(long nID);
	STDMETHOD(GetWorkflowDefinition)(long nID, IWorkflowDefinition** ppWorkflowDefinition);
	STDMETHOD(SetWorkflowDefinition)(IWorkflowDefinition* pWorkflowDefinition);
	STDMETHOD(GetWorkflows)(IStrToStrMap ** pmapWorkFlowNameToID);
	STDMETHOD(GetWorkflowActions)(long nID, IIUnknownVector** pvecActions);
	STDMETHOD(SetWorkflowActions)(long nID, IIUnknownVector* pActionList);
	STDMETHOD(get_ActiveWorkflow)(BSTR* pbstrWorkflowName);
	STDMETHOD(put_ActiveWorkflow)(BSTR bstrWorkflowName);
	STDMETHOD(get_ActiveActionID)(long* pnActionID);
	STDMETHOD(GetStatsAllWorkflows)(BSTR bstrActionName, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(GetVisibleFileStatsAllWorkflows)(BSTR bstrActionName, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(GetInvisibleFileStatsAllWorkflows)(BSTR bstrActionName, VARIANT_BOOL vbForceUpdate, IActionStatistics** pStats);
	STDMETHOD(GetAllActions)(IStrToStrMap** pmapActionNameToID);
	STDMETHOD(GetWorkflowStatus)(long nFileID, EActionStatus* peaStatus);
	STDMETHOD(GetAggregateWorkflowStatus)(long *pnUnattempted, long *pnProcessing, long *pnCompleted, long *pnFailed);
	STDMETHOD(GetWorkflowStatusAllFiles)(BSTR *pbstrStatusListing);
	STDMETHOD(LoginUser)(BSTR bstrUserName, BSTR bstrPassword);
	STDMETHOD(get_RunningAllWorkflows)(VARIANT_BOOL *pRunningAllWorkflows);
	STDMETHOD(GetWorkflowID)(BSTR bstrWorkflowName, long *pnID);
	STDMETHOD(IsFileInWorkflow)(long nFileID, long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow);
	STDMETHOD(get_UsingWorkflows)(VARIANT_BOOL *pbUsingWorkflows);
	STDMETHOD(GetWorkflowNameFromActionID)(long nActionID, BSTR* pbstrWorkflowName);
	STDMETHOD(GetActionIDForWorkflow)(BSTR bstrActionName, long nWorkflowID, long* pnActionID);
	STDMETHOD(put_NumberOfConnectionRetries)(long nNewVal);
	STDMETHOD(get_NumberOfConnectionRetries)(long *pnVal);
	STDMETHOD(put_ConnectionRetryTimeout)(long nNewVal);
	STDMETHOD(get_ConnectionRetryTimeout)(long *pnVal);
	STDMETHOD(SetNewPassword)(BSTR bstrUserName, VARIANT_BOOL* pbSuccess);
	STDMETHOD(MoveFilesToWorkflowFromQuery)(BSTR bstrQuery, long nSourceWorkflowID, long nDestWorkflowID, long *pnCount);
	STDMETHOD(GetAttributeValue)(BSTR bstrSourceDocName, BSTR bstrAttributeSetName, BSTR bstrAttributePath,
		BSTR* pbstrValue);
	STDMETHOD(IsFileNameInWorkflow)(BSTR bstrFileName, long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow);
	STDMETHOD(SaveWebAppSettings)(long nWorkflowID, BSTR bstrType, BSTR bstrSettings);
	STDMETHOD(LoadWebAppSettings)(long nWorkflowID, BSTR bstrType, BSTR *pbstrSettings);
	STDMETHOD(DefineNewMLModel)(BSTR strModelName, long* pnID);
	STDMETHOD(DeleteMLModel)(BSTR strModelName);
	STDMETHOD(GetMLModels)(IStrToStrMap** pmapModelNameToID);
	STDMETHOD(RecordWebSessionStart)(BSTR bstrType, VARIANT_BOOL vbForQueuing, BSTR bstrLoginId, BSTR bstrIpAddress, BSTR bstrUser);
	STDMETHOD(GetActiveUsers)(BSTR bstrAction, IVariantVector** ppvecUserNames);
	STDMETHOD(AbortFAMSession)(long nFAMSessionID);
	STDMETHOD(MarkFileDeleted)(long nFileID, long nWorkflowID);
	STDMETHOD(ResumeWebSession)(long nFAMSessionID, long* pnFileTaskSessionID, long* pnOpenFileID, VARIANT_BOOL* pbIsFileOpen);
	STDMETHOD(SuspendWebSession)();
	STDMETHOD(IsFAMSessionOpen)(long nFAMSessionID, VARIANT_BOOL* pbIsFAMSessionOpen);
	STDMETHOD(GetNumberSkippedForUser)(BSTR bstrUserName, long nActionID, VARIANT_BOOL bRevertTimedOutFAMs, long* pnFilesSkipped);
	STDMETHOD(CacheFileTaskSessionData)(long nFileTaskSessionID, long nPage,
		SAFEARRAY *parrayImageData, BSTR bstrUssData, BSTR bstrWordZoneData, BSTR bstrAttributeData, BSTR bstrException,
		VARIANT_BOOL vbCrucialUpdate, VARIANT_BOOL* pbWroteData);
	STDMETHOD(GetCachedFileTaskSessionData)(long nFileTaskSessionID, long nPage,
		ECacheDataType eDataType, VARIANT_BOOL vbCrucialData,
		SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* bstrAttributeData, BSTR* pbstrException,
		VARIANT_BOOL* pbFoundCacheData);
	STDMETHOD(GetCachedPageNumbers)(long nFileTaskSessionID, ECacheDataType eDataType, SAFEARRAY** parrayCachedPages);
	STDMETHOD(CacheAttributeData)(long nFileTaskSessionID, IStrToStrMap* pmapAttributeData, VARIANT_BOOL bOverwriteModifiedData);
	STDMETHOD(MarkAttributeDataUnmodified)(long nFileTaskSessionID);
	STDMETHOD(GetUncommittedAttributeData)(long nFileID, long nActionID,
		BSTR bstrExceptIfMoreRecentAttributeSetName, IIUnknownVector** ppUncommittedPagesOfData);
	STDMETHOD(DiscardOldCacheData)(long nFileID, long nActionID, long nExceptFileTaskSessionID);
	STDMETHOD(GetOneTimePassword)(BSTR* pVal);
	STDMETHOD(get_CurrentDBSchemaVersion)(LONG* pVal);
	STDMETHOD(SetFileInformationForFile)(int fileID, long long fileSize, int pageCount);
	STDMETHOD(get_HasCounterCorruption)(VARIANT_BOOL* pVal);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

private:
	// Current schema version for the non normalized DB
	static const long ms_lFAMDBSchemaVersion;

	class SetFileActionData
	{
	public:
		SetFileActionData(long fileId, UCLID_FILEPROCESSINGLib::IFileRecordPtr ipRecord,
			EActionStatus eaFromStatus, bool isFileDeleted)
			: FileID(fileId),
			FileRecord(ipRecord),
			FromStatus(eaFromStatus),
			IsFileDeleted(isFileDeleted)
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
		bool IsFileDeleted; // Indicates whether the file has been marked invisible in the active workflow
		EActionStatus FromStatus;
	};

	// Common parameters for spGetFilesToProcessForActionID, setFilesToProcessing, and GetFilesToProcess_Internal
	struct FilesToProcessRequest {
		const string actionName;
		const bool getSkippedFiles;
		const string skippedUser;
		long maxFiles;
		const bool useRandomIDForQueueOrder;
		const bool limitToUserQueue;

		string statusToSelect() const
		{
			return getSkippedFiles ? "S" : "P";
		}
	};

	friend class DBLockGuard;
	// Variables

	// Map that contains the open connection for each thread.
	map<DWORD, shared_ptr<CppBaseApplicationRoleConnection>> m_mapThreadIDtoDBConnections;

	FAMUtils::AppRole m_currentRole;

	// Cache the actionID associated with each action name for the current workflow, or all action IDs
	// for all workflows (represented with a key of "").
	map<string, string> m_mapActionIdsForActiveWorkflow;

	// Cache the workflow definiation for each workflow ID.
	map<long, UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr> m_mapWorkflowDefinitions;

	// For synchronization of connections, active workflow, and other resources that are otherwise
	// not thread-safe
	CCriticalSection m_criticalSection;

	// Ensure only one thread per process is trying to ping the database at once to avoid the chance
	// of pingDB failing because multiple threads in the same process are trying to add or update
	// the same an ActiveFAM entry.
	static CMutex ms_mutexPingDBLock;

	// Used to coordinate revertTimedOutProcessingFAMs calls so that it is not over-executed within
	// a given process.
	static CMutex ms_mutexAutoRevertLock;

	// Used to synchronize logging that should occur only once per process.
	static CMutex ms_mutexSpecialLoggingLock;

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

	// This contains the ID for the registered row in the ActiveFAM table in the DB
	// If 0 there is not a registered ActiveFAM row.
	int m_nActiveFAMID;

	// This contains the ID for the registered entry in the ActiveFAM table in the DB
	// If 0 there is not a registered FAM session.
	int m_nFAMSessionID;

	// The name of the FPS file for any currently active FAM session.
	string m_strFPSFileName;

	// Machine username
	string m_strFAMUserName;

	// Used to hold the FAMUserID of m_strFAMUserName in the database
	// this value is set to 0 if the database connection info changes
	long m_lFAMUserID;
	
	// Machine name
	string m_strMachineName;

	// Used to hold the MachineID of m_strMachineName in the database
	// this value is set to 0 if the database connection info changes
	long m_lMachineID;

	// The DB Lock time out in seconds
	long m_lDBLockTimeout;

	// The action ID this instance is currently registered against.
	volatile int m_nActiveActionID;

	// The currently active workflow. Empty when workflows are not being used or processing an action
	// across all workflows.
	// Use getActiveWorkflow rather than checking this variable directly in order to synchronize access
	// except in cases where m_criticalSection is locked.
	string m_strActiveWorkflow;

	// Zero indicates the ID needs to be looked up next time the ID is requested.
	// -1 indicates there is no active workflow.
	long m_nActiveWorkflowID;

	// Indicates whether workflows are being used for processing within the context of the current action.
	volatile bool m_bUsingWorkflowsForCurrentAction;

	// Indicates whether an action is being processed across all workflows. This will be true
	// when m_bUsingWorkflowsForCurrentAction is true and m_strActiveWorkflow empty.
	volatile bool m_bRunningAllWorkflows;

	// Keeps track of the last time this instance checked stats.
	CTime m_timeLastStatsCheck;

	// Registry configuration manager
	FileProcessingConfigMgr m_regFPCfgMgr;

	// This string should always contain the current status string
	string m_strCurrentConnectionStatus;

	// The connection string used on the last successful login. Used to ensure credentials
	// are not carried over from one DB to another.
	string m_strLastConnectionString;

	// The database server to connect to
	string m_strDatabaseServer;

	// The database to connect to
	string m_strDatabaseName;

	// Database connection string properties that should override or be used in additional to the
	// default connection string properties.
	string m_strAdvConnStrProperties;

	// Saves the last set server name in the current process
	static string ms_strCurrServerName;

	// Saves the last set Database name in the current process
	static string ms_strCurrDBName;

	// Saves the last set advanced connection string properties in the current process.
	static string ms_strCurrAdvConnProperties;

	// The last connection string that was used by this process.
	static string ms_strLastUsedAdvConnStr;

	// The last workflow that was used by this process.
	static string ms_strLastWorkflow;

	// Contains the timeout for query execution
	int m_iCommandTimeout;

	// Flag indicating if records should be added to the QueueEvent table
	bool m_bUpdateQueueEventTable;
	
	// Flag indicating if records should be added to the FileActionStatusTransition table
	bool m_bUpdateFASTTable;

	// Flag indicating whether file action comments should be deleted when files are completed
	bool m_bAutoDeleteFileActionComment;

	// Whether to load balance between workflows when processing on <All workflows>
	bool m_bLoadBalance;

	// Timeout value for automatically reverting files
	int m_nAutoRevertTimeOutInMinutes;

	// List of email addresses to send email when files are reverted.
	string m_strAutoRevertNotifyEmailList;

	// Contains the number of times an attempt to reconnect. Each
	// time the reconnect attempt times out an exception will be logged.
	int m_iNumberOfRetries;

	// This flag indicates that m_iNumberOfRetires was set as a property instead
	// of from DBInfo - if this is true m_iNumberOfRetries will not be loaded
	// from DBInfo
	bool m_bNumberOfRetriesOverridden;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	// This flag indicates that m_dRetryTimeout was set as a property instead
	// of from DBInfo - if this is true m_dRetryTimeout will not be loaded from DBInfo
	bool m_bRetryTimeoutOverridden;

	// Contains the timeout in seconds to keep retrying the GetFilesToProcess Transaction
	double m_dGetFilesToProcessTransactionTimeout;

	// Period of inactivity required before verification sessions time out and close, in seconds
	double m_dVerificationSessionTimeout;

	// Number of Seconds between refreshing the ActionStatistics
	long m_nActionStatisticsUpdateFreqInSeconds;

	// Flag indicating whether to store source doc change history
	bool m_bStoreSourceDocChangeHistory;

	// Flag indicating whether tags can be dynamically created.
	bool m_bAllowDynamicTagCreation;

	// Flag indicating whether to store doc tag history
	bool m_bStoreDocTagHistory;

	// Flag indicating whether to store FTP event history
	bool m_bStoreFTPEventHistory;

	// If this is true work item groups and work items will not be deleted
	// when file processing is changed to Pending - also orphaned processing work items will
	// be reset to pending when files are reverted.
	bool m_bAllowRestartableProcessing;

	bool m_bStoreDBInfoChangeHistory;
	
	IMiscUtilsPtr m_ipMiscUtils;

	// Events used for the ping and statistics maintenance threads.
	Win32Event m_eventStopMaintenanceThreads;
	Win32Event m_eventPingThreadExited;
	Win32Event m_eventStatsThreadExited;

	// Flag to indicate that the FAM has been registered for auto revert
	// if this is false and then pingDB just returns without doing anything
	// if this is true pingDB updates the LastPingTime in ActiveFAM record 
	// and will log changes of the m_nActiveFAMID
	volatile bool m_bFAMRegistered;

	// The tick count from the last time the ping time was updated.
	volatile DWORD m_dwLastPingTime;
	
	// Time since last revertTimedOutProcessingFAMs call was executed this process.
	static DWORD ms_dwLastRevertTime;

	// Indicates whether the DB schema is currently being validated or upgraded.
	volatile bool m_bValidatingOrUpdatingSchema;

	// Indicates that a revert has been started on another thread so it is not
	// necessary to start it again.
	volatile bool m_bRevertInProgress;

	// Indicates that a work item revert is in progress
	volatile bool m_bWorkItemRevertInProgress;

	// Indicates whether retries will be attempted per the CommandTimeout DBInfo setting if a query
	// times out.
	bool m_bRetryOnTimeout;

	// Indicates whether the user was denied permission to run the fast file count query.
	bool m_bDeniedFastCountPermission;

	// Indicates whether the user has entered valid admin credentials.
	bool m_bLoggedInAsAdmin;

	// A map of all enabled features to a boolean that indicates whether the feature should be
	// available for admin users only.
	map<string, bool> m_mapEnabledFeatures;

	// The file IDs for each defined file set (file set name not case-sensitive)
	csis_map<vector<int>>::type m_mapFileSets;

	// This is only used when procesing all workflows
	// It is loaded when processing with the ActionIDs for all the workflows for the ActionName being processes
	// if Load balancing is being used ActionsIDs will can be in the vector multiple times
	// the order of the vector is randomized when originally loaded - the files will be processed in the same order
	// until processing is restarted. Files will be gotten 1 at a time (ignoring the value in the FAM to get more files.
	vector<int> m_vecActionsProcessOrder;

	// The position in the m_vecActionsProcessOrder vector to start getting files
	int m_nProcessStart = 0;

	// The encrypted DatabaseID loaded from the DBInfo table
	string m_strEncryptedDatabaseID;

	DatabaseIDValues m_DatabaseIDValues;

	// This is used to only create the counters vector once
	IIUnknownVectorPtr m_ipSecureCounters;

	// Track whether we've validated or m_DatabaseIDValues or logged as invalid since the last
	// time it was loaded.
	bool m_bDatabaseIDValuesValidated;
	bool m_bLoggedInvalidDatabaseID;

	// This is set when the SecureCounters is called using IDENT_CURRENT on the FAMFile table
	// which returns the last ID that was used.
	long m_nLastFAMFileID;

	// Used to expand path tags in a workflow's OutputFilePathInitializationFunction
	// Used in initOutputFileMetadataFieldValue()
	UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr m_ipFAMTagManager;

	// Whether current session is a web session. If true, no maintenance threads will be started
	bool m_bCurrentSessionIsWebSession;

	// After establishing connection, cache DBInfo settings to avoid unnecessary hits on the database.
	IStrToStrMapPtr m_ipDBInfoSettings;

	ApplicationRoleUtility m_roleUtility;

	//-------------------------------------------------------------------------------------------------
	// Methods
	//-------------------------------------------------------------------------------------------------

	_RecordsetPtr spGetFilesToProcessForActionID(const _ConnectionPtr& ipConnection,
		const FilesToProcessRequest& request, const int actionID);

	// Extracts the IFileRecordPtrs from the Recordset
	vector<UCLID_FILEPROCESSINGLib::IFileRecordPtr> getFilesFromRecordset(_RecordsetPtr ipFileSet);

	// Loads the m_vecActionsProcessOrder vector
	void loadActionsProcessOrder(_ConnectionPtr ipConnection, const string& strActionName);
	
	// Returns true if there is any active FAM; false otherwise.
	bool isFAMActiveForAnyAction(bool bDBLocked);

	// PROMISE: Throws an exception if processing is active on the action.
	// NOTE: If Auto revert is enabled the files will be reverted in a transaction, so this
	//		 must be called outside of an active transaction.
	//		bDBLocked - indicates if the database is locked, this is needed because the auto revert
	//		requires the database to be locked.
	//		If workflows are defined, this will check the action for all workflows, not just the active
	//		workflow.
	void assertProcessingNotActiveForAction(bool bDBLocked, _ConnectionPtr ipConnection,
		const string &strActionName);

	// PROMISE: Throws an exception if processing is active on any action.
	// NOTE: If Auto revert is enabled the files will be reverted in a transaction, so this
	//		 must be called outside of an active transaction.
	//		bDBLocked - indicates if the database is locked, this is needed because the auto revert
	//		requires the database to be locked.
	void assertProcessingNotActiveForAnyAction(bool bDBLocked);

	// Ensures that there are no entries in the ProcessingFAM (for schema versions < 110) or the
	// ActiveFAM table (for schema versions >= 110) before allowing a schema update.
	void assertNotActiveBeforeSchemaUpdate();

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

	// PROMISE: To return a user-readable name for the specified EActionStatus
	string asStatusName(EActionStatus eStatus);

	// PROMISE:	To add a single record to the QueueEvent table in the database with the given data
	//			using the connection provided. For calls where a file that does not yet exist at the
	//			specified location is being programmatically added, llFileSize can be used to
	//			specify the files size recorded in the QueueEvent table.
	void addQueueEventRecord(_ConnectionPtr ipConnection, long nFileID, long nActionID,
		string strFileName, string strQueueEventCode, long long llFileSize = -1);

	// PROMISE: To add a single record to the FileActionStateTransition table with the given data
	// ARGS:	ipConnection	- Connection object to use to update the tables
	//			nFileID			- ID of the file in the FPMFile table that is changing states
	//			nActionID		- The ID of the action state is changing
	//			strFromState	- The old state from the action
	//			strToState		- The new state for the action
	//			strException	- Contains the exception string if this transitioning to a failed state
	//			strComment		- Comment for the added records
	//			nQueuedActionStatusChangeID - The QueuedActionStatusChange record to link the added
	//							  FAST table record to (-1 to not link).
	void addFileActionStateTransition (_ConnectionPtr ipConnection, long nFileID, 
		long nActionID, const string &strFromState, const string &strToState, 
		const string &strException, const string &strComment, long nQueuedActionStatusChangeID = -1);

	// PROMISE:	To add multiple ActionStateTransition table records that are represented be the given data.
	// ARGS:	ipConnection	- Connection object to use to update the tables
	//			strAction		- The action whose state is changing
	//			nActionID		- The id of the action that is changing
	//			strToState		- The new state for the action
	//			strException	- Contains the exception string if this transitioning to a failed state
	//			strComment		- Comment for the added records
	//			strWhereClause	- Where clause to select the records from the FPMFile that the state is changing, this should
	//							  be used so that only one current action state is selected
	//			strTopClause	- Top clause to specify the number of records that meet the where clause condition that should
	//							  have records added to the ActionStateTransition table
	//			params			- map of parameters that will be passed to the query
	void addASTransFromSelect (_ConnectionPtr ipConnection, map<string, variant_t> &params, const string &strAction,
		long nActionID, const string &strToState, const string &strException, const string &strComment, 
		const string &strWhereClause, const string &strTopClause);

	// Gets the ID of the currently active workflow or -1 if no workflow is active.
	long getActiveWorkflowID(_ConnectionPtr ipConnection);

	// PROMISE: To return the ID for the given provided workflow name.
	long getWorkflowID(_ConnectionPtr ipConnection, string strWorkflowName);

	// PROMISE: To get the workflowID associated with the specified ActionID
	long getWorkflowID(_ConnectionPtr ipConnection, long nActionID);

	// Indicates whether the specified file is in the specified workflow.
	// -1 No record of file in workflow
	// 0 File is marked deleted in workflow
	// 1 File is in workflow
	// If the specified workflow ID is -1, the current workflow will be tested.
	// If there are no workflows defined, the result will indicate whether the file ID is present in the DB.
	int isFileInWorkflow(_ConnectionPtr ipConnection, long nFileID, long nWorkflowID);
	int isFileInWorkflow(_ConnectionPtr ipConnection, string strFileName, long nWorkflowID);

	// Gets the currently active workflow. Should be checked instead of m_strActiveWorkflow in order
	// to synchronize access.
	string getActiveWorkflow();

	// Sets the currently active workflow. Should be used instead of m_strActiveWorkflow in order
	// to synchronize access and managed cached workflow info.
	void setActiveWorkflow(string strWorkflowName);

	// PROMISE:	To return the ID from the Action table from the given Action Name and modify strActionName to match
	//			the action name stored in the database using the connection object provided.
	//			NOTE: This will be the action ID unassociated with workflows if workflows are present.
	long getActionID(_ConnectionPtr ipConnection, const string& strActionName);

	// PROMISE:	To return the ID from the Action table from the given strActionName for the specified workflow ID
	//			NOTE: This will be the action ID associated with the specified workflow. If nWorkflow is -1, 
	//			it will return the ID of the currently active workflow. If -1 and no workflow is active, the
	//			ID of the action unassociated with workflows will be returned.
	long getActionID(_ConnectionPtr ipConnection, const string& strActionName, long nWorkflow);
	// PROMISE:	To return the ID from the Action table from the given strActionName for the specified workflow name
	//			NOTE: This will be the action ID associated with the specified workflow. If strWorkflow is empty, 
	//			it will return the ID of the action unassociated with workflows.
	long getActionID(_ConnectionPtr ipConnection, const string& strActionName, const string& strWorkflow);

	// The same as getActionID, except if the specified action does not exist in the current workflow,
	// rather than throwing an error, 0 will be returned.
	long getActionIDNoThrow(_ConnectionPtr ipConnection, const string& strActionName, const string& strWorkflow);
	long getActionIDNoThrow(_ConnectionPtr ipConnection, const string& strActionName, long nWorkflowID);

	// Returns a comma-delimited list of IDs to process in the currently active workflow (if any).
	// In most cases this will be a single action ID, the exception being cases where
	// m_bRunningAllWorkflows is true.
	string getActionIDsForActiveWorkflow(_ConnectionPtr ipConnection, const string& strActionName);

	// PROMISE: To return the Action name for the given ID using the connection object provided;
	string getActionName(_ConnectionPtr ipConnection, long nActionID);

	// PROMISE: Adds an action with the specified name. Returns the action ID of the newly created action.
	// NOTE: If strWorkflow is not empty, a second Action row will be added where the action is
	// associated with the specified workflow.
	long addAction(_ConnectionPtr ipConnection, const string &strAction, const string &strWorkflow);

	// PROMISE: To return the ID from the FAMFile table for the given File name and
	// modify strFileName to match the file name stored in the database using the connection provided.
	long getFileID(_ConnectionPtr ipConnection, string& rstrFileName);

	// PROMISE: To return a CppBaseApplicationRoleConnection containing the m_ipDBConnection 
	// opened database connection for the current thread.
	shared_ptr<CppBaseApplicationRoleConnection> getAppRoleConnection();

	void resetOpenConnectionData();

	// PROMISE: To return a connection that does not have application role enabled
	// NOTE: This is intended to be used for temporary connections only
	_ConnectionPtr getDBConnectionWithoutAppRole();

	// Returns a connection regardless of what application role may be applied to it.
	// This is intended to use for reading from the DBInfo table where select access if available
	// for the public role. By not caring what role is applied, we can leverage whatever
	// connection is currently cached for the current thread instead of opening a new one.
	_ConnectionPtr getDBConnectionRegardlessOfRole();

	shared_ptr<NoRoleConnection> confirmNoRoleConnection(const string& eliCode, const shared_ptr<CppBaseApplicationRoleConnection>& appRoleConnection);

	void validateServerAndDatabase();

	// PROMISE: To close the database connection on the current thread, if it is open currently
	void closeDBConnection();

	// PROMISE:	 To set the given File's action state for the action given by strAction to the 
	//			state in strState and returns the old state using the connection object provided.
	// NOTE:	The outer scope should always lock the DB if required and create transaction if required
	//			nWorkflowID should be used to change the file in a particular workflow; If -1,
	//			the action status will be changed in the active workflow (if one is set).
	//			If bRemovePreviousSkipped is true and strState == "S" then the skipped file table
	//			will be updated for the file with the information for the current user and process.
	//			If bRemovePreviousSkipped is false and strState == "S" the FAMSessionID will be updated,
	//			but all other skipped file fields will be unmodified.
	//			If bQueueChangeIfProcessing is true and the document is already processing, queue
	//			the new strState via the QueuedActionStatusChange table such that when it is done
	//			processing it will be moved into that state.
	//			If bAllowQueuedStatusOverride is true and the QueuedActionStatusChange table has a
	//			pending change for the file, that change will be applied. If
	//			bAllowQueuedStatusOverride is false, the QueuedActionStatusChange will be ignored
	//			and the status will be set to strState.
	EActionStatus setFileActionState(_ConnectionPtr ipConnection, long nFileID,
		string strAction, long nWorkflowID, const string& strState, const string& strException,
		bool bQueueChangeIfProcessing, bool bAllowQueuedStatusOverride, long nActionID = -1,
		bool bRemovePreviousSkipped = false, const string& strFASTComment = "", bool bThisIsRevertingStuckFile = false);

	// PROMISE: To set the specified group of files' action state for the specified action.
	// NOTE:	This will clear the skipped file state for any file ID in the list if
	//			the file is currently in a skipped state.
	//			The outer scope that calls this function must lock the DB and
	//			create a transaction guard.
	void setFileActionState(_ConnectionPtr ipConnection,
		const vector<SetFileActionData>& vecFileData, string strAction, const string& strState);

	// A helper function for SetFileActionState that sets the status for the specified file ID to
	// the specified state on the specified action.
	// nWorkflowID should be used to change the file in a particular workflow; If - 1,
	// the action status will be changed in the active workflow (if one is set).
	// If bQueueChangeIfProcessing is true and the file is currently in the processing state on the
	// specified action, the new state will be queued via the QueuedActionStatusChange table such
	// that when it is done processing it will be moved into that state.
	// If bAllowQueuedStatusOverride is true and the QueuedActionStatusChange table has a pending
	// change for the file, that change will be applied. If bAllowQueuedStatusOverride is false,
	// the QueuedActionStatusChange will be ignored and the status will be set to strState.
	// poldStatus will return the previous action status of the document if not null.
	void setStatusForFile(_ConnectionPtr ipConnection, long nFileID,  string strAction,
		long nWorkflowID, EActionStatus eStatus, bool bQueueChangeIfProcessing,
		bool bAllowQueuedStatusOverride, EActionStatus *poldStatus = __nullptr);

	// PROMISE: Recalculates the statistics for the given Action ID using the connection provided.
	void reCalculateStats(_ConnectionPtr ipConnection, long nActionID);

	// PROMISE:	To drop all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void dropTables(bool bRetainUserTables);
	
	// PROMISE:	To Add all tables in the database
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void addTables(bool bAddUserTables);

	// PROMISE:	To Add all tables in the database with the schema that existed as of the release of
	//			Flex/IDS 8.0.
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void addTables80();

	// PROMISE: Retrieves a vector of SQL queries that creates all the tables for the current DB schema.
	vector<string> getTableCreationQueries(bool bIncludeUserTables);
	
	// PROMISE:	To set the initial values for QueueEventCode, ActionState and DBInfo
	// NOTE:	If the operation is to be transactional the BeginTransaction should be done before calling
	void initializeTableValues(bool bInitializeUserTables);

	// PROMISE:	To set the initial values for QueueEventCode, ActionState and DBInfo according to
	// the schema that existed as of the release of Flex/IDS 8.0.
	void initializeTableValues80();

	// PROMISE: Gets the default values for each of the DBInfo values managed by the FAM DB.
	map<string, string> getDBInfoDefaultValues();

	// Creates a vector of all features that can be managed from the FAM DB.
	vector<string> getFeatureNames();

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
		UCLID_FILEPROCESSINGLib::IFileRecordPtr ipOldRecord, bool bFileIsDeleted);

	// Type of stats to load
	enum class EWorkflowVisibility { All, Visible, Invisible };

	// PROMISE: To load the stats record from the db from ActionStatistics table using the
	//			connection provided.
	//			if a record does not exist and bDBLocked is true, stats will be calculate stats for the action
	//			if the statistics loaded are out of date and bDBLocked is true, stats will be updated
	//			from the ActionStatisticsDelta table
	//			if the bDBLocked is false and no record exists or stats are out of date an exception
	//			will be thrown.
	UCLID_FILEPROCESSINGLib::IActionStatisticsPtr loadStats(_ConnectionPtr ipConnection, 
		long nActionID, EWorkflowVisibility eWorkflowVisibility, bool bForceUpdate, bool bDBLocked);

	// Returns the DBSchemaVersion
	int getDBSchemaVersion();

	// Locks the db for use using the connection provided.
	// The caller must unlock the database on the same thread on which LockDB was called.
	void lockDB(_ConnectionPtr ipConnection, const string& strLockName);
	
	// Unlocks db using the connection provided.
	// The caller must unlock the database on the same thread on which LockDB was called.
	void unlockDB(_ConnectionPtr ipConnection, const string& strLockName);

	// Locks the specified table to prevent all read/write access to other sessions for the duration
	// of the active transaction
	void lockDBTableForTransaction(_ConnectionPtr ipConnection, const string& strTableName);

	// Looks up the current username in the Login table if bUseAdmin is false
	// and looks up the admin username if bUseAdmin is true
	// if no record rstrEncrypted is an empty string and the return value is false
	// if there is a record returns the stored password and the return value is true;
	bool getEncryptedPWFromDB(string &rstrEncryptedPW, bool bUseAdmin);

	// Encrypts the provided user name + password string and stores the result in the Login table
	void encryptAndStoreUserNamePassword(const string& strUser, const string& strPassword,
										 bool bFailIfUserDoesNotExist = false);

	// Stores the value contained in the encrypted string into the database.
	// NOTE: This method assumes that the provided string has already been encrypted,
	// DO NOT call this method with an unencrypted string unless you intend to have
	// the combined user and password value stored in plain text.
	void storeEncryptedPasswordAndUserName(const string& strUser, const string& strEncryptedPW,
		bool bFailIfUserDoesNotExist = false, bool bCreateTransactionGuard = true);

	// Returns the result of encrypting the input
	template <typename T>
	string getEncryptedString(size_t nCount, const T input, ...);

	// Throws and exception if the DBSchemaVersion in the DB is different from the current DBSchemaVersion
	void validateDBSchemaVersion(bool bCheckForUnaffiliatedFiles = false);

	// Returns whether, if workflows exist, files exist that haven't been assigned to a workflow.
	bool unaffiliatedWorkflowFilesExist();

	// Assigns m_nActiveActionID, m_bUsingWorkflowsForCurrentAction, and m_bRunningAllWorkflows based on the
	// current workflow and strActionName.
	void setActiveAction(_ConnectionPtr ipConnection, const string& strActionName);

	// Returns the this pointer as a IFileProcssingDBPtr COM pointer
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr getThisAsCOMPtr();

	// Checks the strPassword value against the password from the database
	bool isPasswordValid(const string& strPassword, bool bUseAdmin);

	// Authenticates an admin session using the specified one-time strPassword
	void authenticateOneTimePassword(const string& strPassword);

	// Gets a one-time admin password based on the current m_nFAMSessionID (which must be non-zero).
	// m_bLoggedInAsAdmin must also be true for call to succeed.
	string getOneTimePassword(_ConnectionPtr ipConnection);

	// Returns true if the configured DB exists and false if it does not
	bool isExistingDB();

	// Returns true if the configured database exists but has no tables
	// Returns false if the configured database exists and has tables
	bool isBlankDB();

	// Clears the database to set it up.
	// Returns true if the database has been initialized.
	// Returns false if the user was prompted to initialize the database, but declined.
	// initWithoutPrompt-	If true, the database will be initialized with the specified strAdminPassword.
	//						If false, a prompt will be displayed for the admin password following initialization.
	bool initializeDB(bool initWithoutPrompt, string strAdminPassword);

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

	// if the m_strFAMUserName is the same as the current user and the FullUserName is null in the FAMUserTable
	// or there is no record for the username the FAMUser will be created or updated to the proper values
	// return value will be the FAMUserID
	long addOrUpdateFAMUser(_ConnectionPtr ipConnection);

	// Loads settings from the DBInfo table if any exceptions are thrown while
	// obtaining the settings the exception will be logged. 
	// so that this function will always return	
	void loadDBInfoSettings(_ConnectionPtr ipConnection = __nullptr);

	// Indicates whether the feature data has been retrieved from the database.
	bool m_bCheckedFeatures;

	// Retrieves data for all features enabled in the database.
	void checkFeatures(_ConnectionPtr ipConnection);

	// Returns the running apps main window handle if can't get the main window returns NULL;
	HWND getAppMainWndHandle();

	// Returns an IUnknownVector of ProductSpecificMgrs
	IIUnknownVectorPtr getLicensedProductSpecificMgrs();

	// Removes the schema for each of the licensed product specific managers that currently
	// exist in the database. The schemas that were installed and present are returned.
	IIUnknownVectorPtr removeProductSpecificDB(_ConnectionPtr ipConnection, bool bOnlyTables, bool bRetainUserTables);

	// Adds the schema for the specified product specific managers
	void addProductSpecificDB(_ConnectionPtr ipConnection,
		IIUnknownVectorPtr ipProdSpecMgrs, bool bOnlyTables, bool bAddUserTables);

	// Adds the schema for each of the licensed product specific managers with the schema that
	// existed as of the release of Flex/IDS 8.0.
	void addProductSpecificDB80();

	// Try's to read the sql server time using the provided connection and if it fails returns false
	bool isConnectionAlive(_ConnectionPtr ipConnection);

	// Recreates the connection for the current thread. If there is no connection object for 
	// the current thread it will be created using the getAppRoleConnection method. If the creation of
	// the connection object fails it will be reattempted for gdRETRY_TIMEOUT (120sec) If after the
	// timeout it was still not possible to create the connection object false will be returned,
	// otherwise true will be returned.
	bool reConnectDatabase(string ELICodeOfCaller );

	// Adds a record to the skipped file table
	void addSkipFileRecord(const _ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Removes a record from the skipped file table
	void removeSkipFileRecord(const _ConnectionPtr& ipConnection, long nFileID,
		long nActionID);

	// Internal reset DB connection function
	void resetDBConnection(bool bCheckForUnaffiliatedFiles = false);

	// Internal close all DB connections. Credentials will be maintained if bTemporaryClose is used
	// as long as the re-connection is to the same database.
	void closeAllDBConnections(bool bTemporaryClose);

	// Internal clear DB function
	// Use bInitializing only if initializing a database for the first time.
	void clear(bool bLocked, bool bInitializing, bool retainUserValues);

	// Internal DB initialization function for the 8.0 schema.
	void init80DB();

	// Internal getActions
	IStrToStrMapPtr getActions(_ConnectionPtr ipConnection, const string& strWorkflow = "");

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
	void revertLockedFilesToPreviousState(const _ConnectionPtr& ipConnection, long nActiveFAMID,
		const string& strFASTComment = "", UCLIDException* pUE = NULL);

	// Method checks for timed out FAM's and reverts file status for ones that are found.
	// If there are files to revert and bDBLocked is false an exception will be thrown
	void revertTimedOutProcessingFAMs(bool bDBLocked, const _ConnectionPtr& ipConnection);

	// Method that resets workitems that are marked as processing for FAM's that have timed out
	// if there is an exception it will be thrown if bDBLocked is false otherwise an exception
	// will be logged.
	void revertTimedOutWorkItems(bool bDBLocked, const _ConnectionPtr &ipConnection);

	// Verifies that the current instance is registered via RegisterActiveFAM and registers it if it
	// is not.
	void ensureFAMRegistration();

	// Thread function that maintains the LastPingtime in the ActiveFAM table in
	// the database pData should be a pointer to the database object
	static UINT maintainLastPingTimeForRevert(void* pData);

	// Thread function that ensures the ActionStatisticsDelta table is folded into ActionStatistics
	// on a regular basis even if this instance is not querying stats for the active action.
	static UINT maintainActionStatistics(void* pData);

	// Method updates the ActiveFAM LastPingTime for the currently registered FAM
	void pingDB();

	// Method that creates a thread to send the mail message
	void emailMessage(const string& strMessage);

	// Checks if the current machine is in the list of machines to skip user authentication
	// when running as a service
	bool isMachineInListOfMachinesToSkipUserAuthentication(const _ConnectionPtr& ipConnection);

	// Checks whether the specified action name is valid
	void validateNewActionName(const string& strActionName);
	
	// Checks whether the specified metadata field name is valid
	void validateMetadataFieldName(const string& strMetadataFieldName);

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
	// the FAST table and adding an appropriate entry to the locked file table. If
	// strAllowedCurrentStatus is not empty, the function will throw an exception if the current
	// action status code is not part of the specified string.
	// REQUIRE:	The query must return the following columns from the FAMFile table:
	//			SELECT ID, FileName, Pages, FileSize, Priority, the action status and the action ID
	//			for the current action from the FileActionStatus table.
	//			if bDBLocked is false and there are files that need to be reverted an exception
	//			will be thrown.
	// RETURNS: A vector of IFileRecords for the files that were set to processing.
	IIUnknownVectorPtr setFilesToProcessing(bool bDBLocked, const _ConnectionPtr &ipConnection,
		const string& strSelectSQL, const string& strActionName, long nMaxFiles,
		const string& strAllowedCurrentStatus);

	// Gets a set containing the File ID's for all files that are skipped for the specified action
	set<long> getSkippedFilesForAction(const _ConnectionPtr& ipConnection, long nActionId);

	// Marks all files of the specified strStatusToSelect to processing except 'U'.  The processing in this
	// function includes attempting to auto-revert locked files, recording appropriate entries in
	// the FAST table and adding an appropriate entry to the locked file table. This method will process using the old
	// GetFilesToProcess functionality if DBInfo setting UseGetFilesLegacy is 1 otherwise calls
	// the GetFilesToProcessForActionID to get the files to process
	// RETURNS: A vector of IFileRecords for the files that were set to processing.
	IIUnknownVectorPtr setFilesToProcessing(bool bDBLocked, const _ConnectionPtr& ipConnection,
		const FilesToProcessRequest& request);

	// Returns recordset opened as static containing the status record the file with nFileID and 
	// action nActionID. If the status is unattempted the recordset will be empty
	_RecordsetPtr getFileActionStatusSet(_ConnectionPtr& ipConnection, long nFileID, long nActionID);

	// Determines if ActionStatistics is due to be update from the ActionStatisticsDelta table.
	bool isStatisticsUpdateFromDeltaNeeded(const _ConnectionPtr& ipConnection, const long nActionID);

	// Method to add the current records from the ActionStatisticsDelta table to the 
	// ActionStatistics table
	void updateActionStatisticsFromDelta(const _ConnectionPtr& ipConnection, const long nActionID);

	// Returns a vector of the names of all DBInfo rows and tables in the database that are not
	// managed by the FAM DB itself or by one of the installed product-specific DBs
	vector<string> findUnrecognizedSchemaElements(const _ConnectionPtr& ipConnection);

	// Adds any old DB info values into the list of current db info values so that
	// findUnrecognizedSchemaElements does not fail
	void addOldDBInfoValues(map<string, string>& mapValues);

	// Adds any old table names into vecTables so that findUnrecognizedSchemaElements does not fail
	void addOldTables(vector<string>& vecTables);

	// Runs the UpdateSchemaForFAMDBVersion function for each installed product-specific database.
	void executeProdSpecificSchemaUpdateFuncs(_ConnectionPtr ipConnection, 
		IIUnknownVectorPtr ipProdSpecificMgrs, int nFAMSchemaVersion, long *pnStepCount,
		IProgressStatusPtr ipProgressStatus, map<string, long> &rmapProductSpecificVersions);

	UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr getWorkItemFromFields(const FieldsPtr& ipFields);
	IIUnknownVectorPtr setWorkItemsToProcessing(bool bDBLocked, string strActionName, long nNumberToGet,
		bool bRestrictToFAMSessionID, EFilePriority eMinPriority, const _ConnectionPtr &ipConnection);
	UCLID_FILEPROCESSINGLib::IWorkItemRecordPtr setWorkItemToProcessing(bool bDBLocked, string strActionName, 
		bool bRestrictToFAMSessionID, EFilePriority eMinPriority, const _ConnectionPtr &ipConnection);

	// Checks for new Product Specific DB managers
	void checkForNewDBManagers();

	// If m_bDatabaseIDValuesValidated is true this returns true - if false 
	// the m_strEncryptedDatabaseID will be checked for validity - if it is empty it will be loaded 
	// from DBInfo table and then checked for validity
	// m_bDatabaseIDValuesValidated will be set to result of this function an if an exception is thrown
	// m_bDatabaseIDValuesValidated will be set to false.
	// bRefreshData will force the database ID to be refeshed from the DBInfo table; otherwise
	// cached database ID data may be used.
	// if bThowIfInvalid is true, an exception will be throw instead of returning a value
	// bIsRetry indicates if the call is being retried to account for a DatabaseID that may have
	// been updated since DatabaseID was retrieved.
	bool checkDatabaseIDValid(_ConnectionPtr ipConnection, bool bRefreshData, bool bThowIfInvalid, bool bIsRetry = false);

	// NOTE: Assumes the database ID has separately been validated; the counter validation here will trust
	// the current database ID is valid.
	// If no counters are present, true will be returned.
	// pvecDBCounters can be provided to receive the counters found in the database to perform this check.
	bool checkCountersValid(_ConnectionPtr& ipConnection, vector<DBCounter>* pvecDBCounters = __nullptr);

	// Method applies the changes that are in the counterUpdates argument as well as update the 
	// existing counters that are not being modified to use the new databaseID caused by the update
	// Should be executed within a transaction
	// returns a string that has one line for each update performed
	string updateCounters(_ConnectionPtr ipConnection, DBCounterUpdate &counterUpdates, UCLIDException &ueLog);

	// Method unlocks the counters if they are in a bad state
	void unlockCounters(_ConnectionPtr ipConnection, DBCounterUpdate &counterUpdates, UCLIDException &ueLog);

	// Method generates the queries for the given counter to fix corruption
	string getQueryToResetCounterCorruption(CounterOperation counter, DatabaseIDValues databaseID, 
		UCLIDException &ueLog, string strComment = "Unlock");

	// Returns a map with all the existing counters as CounterOperation records. All of the records
	// returned will have the m_eOperation set to kNone. As the changes are processed they will be 
	// changes to reflect the operations specified with the upgrade code.
	// if bCheckCounterHash is true the Hash portion of the encrypted value SecureCounterValue will
	// be checked that the DatabaseID hash portion is what is expected if bCheckCounterHash is false
	// only the CounterID portion will be checked against the record's ID
	void getCounterInfo(map<long, CounterOperation> &mapOfCounterOps, bool bCheckCounterHash = true);
	
	// Creates counter update queries for all existing counters. The queries make the counters valid for the 
	// given databaseIDValues
	void createCounterUpdateQueries(const DatabaseIDValues &databaseIDValues, vector<string> &vecCounterUpdates, 
		map<long, CounterOperation> &mapCounters );

	// Creates a new databaseID and stores in the database. 
	// Role passwords will be updated to reflect the change.
	void createAndStoreNewDatabaseID(const NoRoleConnection& noAppRoleConnection);

	// Stores the specified databaseID in the database as the new database ID
	// Role passwords will be updated to reflect the change.
	void storeNewDatabaseID(const NoRoleConnection& noAppRoleConnection, DatabaseIDValues databaseID);

	// Checks if the file was created in a currently active FAMSession thru pagination.
	bool isFileInPagination(_ConnectionPtr ipConnection, long nFileID);

	// Method to update DatabaseID and Secure Counter tables after schema updated to 183
	void updateDatabaseIDAndSecureCounterTablesSchema183(const NoRoleConnection& noAppRoleConnection);

	// Gets the specified workflow definition
	UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr getWorkflowDefinition(_ConnectionPtr ipConnection, long nID);
	
	// Gets the specified workflow in cases where performance is important and it's not expected
	// that the workflow will have changed.
	UCLID_FILEPROCESSINGLib::IWorkflowDefinitionPtr getCachedWorkflowDefinition(_ConnectionPtr ipConnection, long nID = -1);

	// For every action in a workflow, gets the action id, name and whether main sequence.
	vector<tuple<long, string, bool>> getWorkflowActions(_ConnectionPtr ipConnection, long nWorkflowID);

	vector<pair<string, string>> getWorkflowNamesAndIDs(_ConnectionPtr ipConnection);

	// Gets the status of a file in a workflow or all files in a workflow if nFileID = -1.
	// When bReturnFileStatuses = false, return vector lists the file count for each file status.
	// When bReturnFileStatuses = true, return vector lists a file ID and associated status
	vector<tuple<long, string>> getWorkflowStatus(long nFileID, bool bReturnFileStatuses = false);

	// Indicates whether any workflows are currently defined in the database.
	bool databaseUsingWorkflows(_ConnectionPtr ipConnection);

	// Helper function for SetStatusForAllFiles COM method that may be called once per workflow when
	// called for <All workflows>
	void setStatusForAllFiles(_ConnectionPtr ipConnection, const string& strAction, EActionStatus eStatus);

	// Helper function for ModifyActionStatusForSelection COM method that may be called once per workflow when
	// called for <All workflows>
	void modifyActionStatusForSelection(UCLID_FILEPROCESSINGLib::IFAMFileSelectorPtr ipFileSelector, string strToAction,
		string strNewStatus, string strFromAction, long* pnNumRecordsModified);

	// Sets the value of the output file name metadata field based on the workflow configuration
	void initOutputFileMetadataFieldValue(_ConnectionPtr ipConnection, long nFileID, string strFileName, long nWorkflowID);
	
	// Sets the value of a metadata field
	void setMetadataFieldValue(_ConnectionPtr connection, long fileID, string metadataFieldName, string metadataFieldValue);

	// Verifies that the destination actions exits when moving from one workflow to another - 
	// strSelectionFrom is expected to have 2 action tables
	//		SA - source action with a field of ASCName and should never be NULL
	//		DA - with ID column - This should be joined so that it will be NULL if action not in 
	void verifyDestinationActions(ADODB::_ConnectionPtr &ipConnection, std::string &strSelectionFrom);

	// Creates the temp table #SelectedFilesToMove that will be used for moving workflows
	// strQueryFrom is the query that selects the file ids to move
	void createTempTableOfSelectedFiles(ADODB::_ConnectionPtr &ipConnection, std::string &strQueryFrom);

	// Gets the web application settings for the specified workflow and type as a JSON string.
	string getWebAppSettings(_ConnectionPtr ipConnection, long nWorkflowId, string strType);

	// Gets a specific JSON string setting from a specified JSON string.
	string getWebAppSetting(const string& strSettings, const string& strSettingName);

	void validateLicense();

	// Internal implementation methods
	bool DefineNewAction_Internal(bool bDBLocked, BSTR strAction, long* pnID);
	bool DeleteAction_Internal(bool bDBLocked, BSTR strAction);
	bool GetActions_Internal(bool bDBLocked, IStrToStrMap * * pmapActionNameToID);
	bool AddFile_Internal(bool bDBLocked, BSTR strFile,  BSTR strAction, long nWorkflowID,
		EFilePriority ePriority, VARIANT_BOOL bForceStatusChange, VARIANT_BOOL bFileModified,
		EActionStatus eNewStatus, VARIANT_BOOL bSkipPageCount, VARIANT_BOOL * pbAlreadyExists,
		EActionStatus *pPrevStatus, IFileRecord* * ppFileRecord);
	bool RemoveFile_Internal(bool bDBLocked, BSTR strFile, BSTR strAction);
	bool NotifyFileProcessed_Internal(bool bDBLocked, long nFileID,  BSTR strAction, long nWorkflowID,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool NotifyFileFailed_Internal(bool bDBLocked, long nFileID,  BSTR strAction, long nWorkflowID,
		BSTR strException, VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool SetFileStatusToPending_Internal(bool bDBLocked, long nFileID,  BSTR strAction,
		/*long nWorkflowID,*/ VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool SetFileStatusToUnattempted_Internal(bool bDBLocked, long nFileID,  BSTR strAction, 
		/*long nWorkflowID,*/ VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool SetFileStatusToSkipped_Internal(bool bDBLocked, long nFileID, BSTR strAction,
		VARIANT_BOOL bRemovePreviousSkipped, /*long nWorkflowID,*/ VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool GetFileStatus_Internal(bool bDBLocked, long nFileID,  BSTR strAction,
		VARIANT_BOOL vbAttemptRevertIfLocked, EActionStatus * pStatus);
	bool SetStatusForAllFiles_Internal(bool bDBLocked, BSTR strAction,  EActionStatus eStatus);
	bool SetStatusForFile_Internal(bool bDBLocked, long nID, BSTR strAction, long nWorkflowID,
		EActionStatus eStatus, VARIANT_BOOL vbQueueChangeIfProcessing, VARIANT_BOOL vbAllowQueuedStatusOverride,
		EActionStatus * poldStatus);
	bool GetFilesToProcess_Internal(bool bDBLocked, const FilesToProcessRequest& request, IIUnknownVector** pvecFileRecords);
	bool GetFileToProcess_Internal(bool bDBLocked, long nFileID, BSTR strAction, BSTR bstrFromState, IFileRecord** ppFileRecord);
	bool RemoveFolder_Internal(bool bDBLocked, BSTR strFolder, BSTR strAction);
	bool GetStats_Internal(bool bDBLocked, long nActionID, VARIANT_BOOL vbForceUpdate, VARIANT_BOOL vbRevertTimedOutFAMs,
		EWorkflowVisibility eWorkflowVisibility, IActionStatistics* *pStats);
	bool CopyActionStatusFromAction_Internal(bool bDBLocked, long  nFromAction, long nToAction);
	bool RenameAction_Internal(bool bDBLocked, BSTR bstrOldActionName, BSTR bstrNewActionNam);
	bool Clear_Internal(bool bDBLocked, VARIANT_BOOL vbRetainUserValues);
	bool ExportFileList_Internal(bool bDBLocked, BSTR strQuery, BSTR strOutputFileName,
		IRandomMathCondition* pRandomCondition, long *pnNumRecordsOutput);
	bool GetActionID_Internal(bool bDBLocked, BSTR bstrActionName, long* pnActionID); 
	bool SetDBInfoSetting_Internal(bool bDBLocked, BSTR bstrSettingName, BSTR bstrSettingValue, 
		VARIANT_BOOL vbSetIfExists, VARIANT_BOOL vbRecordHistory);
	bool GetDBInfoSetting_Internal(bool bDBLocked, const string& strSettingName, bool bThrowIfMissing,
		string& rstrSettingValue);
	bool GetResultsForQuery_Internal(bool bDBLocked, BSTR bstrQuery, _Recordset** ppVal);
	bool GetFileID_Internal(bool bDBLocked, BSTR bstrFileName, long *pnFileID);
	bool GetActionName_Internal(bool bDBLocked, long nActionID, BSTR *pbstrActionName);
	bool NotifyFileSkipped_Internal(bool bDBLocked, long nFileID, BSTR bstrAction, long nWorkflowID,
		VARIANT_BOOL vbAllowQueuedStatusOverride);
	bool SetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID, BSTR bstrComment);
	bool GetFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID, 
		BSTR* pbstrComment);
	bool ClearFileActionComment_Internal(bool bDBLocked, long nFileID, long nActionID);
	bool ModifyActionStatusForSelection_Internal(bool bDBLocked, IFAMFileSelector* pFileSelector,
		BSTR bstrToAction, EActionStatus eaStatus, BSTR bstrFromAction, 
		VARIANT_BOOL vbModifyWhenTargetActionMissingForSomeFiles, long* pnNumRecordsModified);
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
	bool ExecuteCommandQuery_Internal(bool bDBLocked, BSTR bstrQuery, long* pnRecordsAffected);
	bool ExecuteCommandReturnLongLongResult_Internal( bool bDBLocked, BSTR bstrQuery, 
		long* pnRecordsAffected, BSTR bstrResultColumnName, long long* pResult );
	bool UnregisterActiveFAM_Internal(bool bDBLocked);
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
	bool RecordFAMSessionStart_Internal(bool bDBLocked, BSTR bstrFPSFileName, BSTR bstrActionName,
		VARIANT_BOOL vbQueuing, VARIANT_BOOL vbProcessing);
	bool RecordWebSessionStart_Internal(bool bDBLocked, VARIANT_BOOL vbForQueuing);
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
	bool SetDBInfoSettings_Internal(bool bDBLocked, vector<_CommandPtr> vecCommands, long& nNumRowsUpdated);
	bool RecordFTPEvent_Internal(bool bDBLocked, long nFileId, long nActionID,
		VARIANT_BOOL vbQueueing, EFTPAction eFTPAction, BSTR bstrServerAddress,
		BSTR bstrUserName, BSTR bstrArg1, BSTR bstrArg2, long nRetries, BSTR bstrException);
	bool IsAnyFAMActive_Internal(bool bDBLocked, VARIANT_BOOL* pvbFAMIsActive);
	bool GetFileCount_Internal(bool bDBLocked, VARIANT_BOOL bUseOracleSyntax, LONGLONG* pnFileCount);
	bool IsFeatureEnabled_Internal(bool bDBLocked, BSTR bstrFeatureName, VARIANT_BOOL* pbFeatureIsEnabled);
	bool CreateWorkItemGroup_Internal(bool bDBLocked, long nFileID, long nActionID, BSTR stringizedTask,
		long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID);
	bool AddWorkItems_Internal(bool bDBLocked, long nWorkItemGroupID, IIUnknownVector *pWorkItems);
	bool GetWorkItemsForGroup_Internal(bool bDBLocked, long nWorkItemGroupID, long nStartPos, long nCount, IIUnknownVector **ppWorkItems);
	bool GetWorkItemGroupStatus_Internal(bool bDBLocked, long nWorkItemGroupID,
		WorkItemGroupStatus *pWorkGroupStatus, EWorkItemStatus *pStatus);
	bool GetWorkItemToProcess_Internal(bool bDBLocked, string strActionName, VARIANT_BOOL vbRestrictToFAMSession, IWorkItemRecord **ppWorkItem);
	bool NotifyWorkItemFailed_Internal(bool bDBLocked, long nWorkItemID, BSTR strizedException);
	bool NotifyWorkItemCompleted_Internal(bool bDBLocked, long nWorkItemID);
	bool GetWorkGroupData_Internal(bool bDBLocked, long WorkItemGroupID, long *pnNumberOfWorkItems, BSTR *pstringizedTask);
	bool SaveWorkItemOutput_Internal(bool bDBLocked, long WorkItemID, BSTR strWorkItemOutput);
	bool FindWorkItemGroup_Internal(bool bDBLocked, long nFileID, long nActionID, BSTR stringizedTask, 
		long nNumberOfWorkItems, BSTR bstrRunningTaskDescription, long *pnWorkItemGroupID);
	bool SaveWorkItemBinaryOutput_Internal(bool bDBLocked, long WorkItemID, IUnknown *pBinaryOutput);
	bool GetFileSetFileNames_Internal(bool bDBLocked, BSTR bstrFileSetName, IVariantVector **ppvecFileNames);
	bool SetFallbackStatus_Internal(bool bDBLocked, IFileRecord* pFileRecord, EActionStatus eaFallbackStatus);
	bool GetWorkItemsToProcess_Internal(bool bDBLocked, string strActionName, VARIANT_BOOL vbRestrictToFAMSessionID, 
			long nMaxWorkItemsToReturn, EFilePriority eMinPriority, IIUnknownVector **ppWorkItems);
	bool SetWorkItemToPending_Internal(bool bDBLocked, long nWorkItemID);
	bool GetFailedWorkItemsForGroup_Internal(bool bDBLocked, long nWorkItemGroupID, IIUnknownVector **ppWorkItems);
	bool SetMetadataFieldValue_Internal(bool bDBLocked, long nFileID, BSTR bstrMetadataFieldName, BSTR bstrMetadataFieldValue);
	bool GetMetadataFieldValue_Internal(bool bDBLocked, long nFileID, BSTR bstrMetadataFieldName, BSTR *pbstrMetadataFieldValue);
	bool GetMetadataFieldNames_Internal(bool bDBLocked, IVariantVector **ppMetadataFieldNames);
	bool AddMetadataField_Internal(bool bDBLocked, const string& strMetadataFieldName);
	bool DeleteMetadataField_Internal(bool bDBLocked, BSTR bstrMetadataFieldName);
	bool RenameMetadataField_Internal(bool bDBLocked, BSTR bstrOldMetadataFieldName, BSTR bstrNewMetadataFieldName);
	bool StartFileTaskSession_Internal(bool bDBLocked, BSTR bstrTaskClassGuid, long nFileID, long nActionID, long *pnFileTaskSessionID);
	bool EndFileTaskSession_Internal(bool bDBLocked, long nFileTaskSessionID, double dOverheadTime,
		double dActivityTime, bool bSessionTimeOut);
	bool GetSecureCounters_Internal(bool bDBLocked, VARIANT_BOOL vbRefresh, IIUnknownVector** ppSecureCounters);
	bool GetSecureCounterName_Internal(bool bDBLocked, long nCounterID, BSTR *pstrCounterName);
	bool ApplySecureCounterUpdateCode_Internal(bool bDBLocked, BSTR strUpdateCode, BSTR *pbstrResult);
	bool GetSecureCounterValue_Internal(bool bDBLocked, long nCounterID, long* pnCounterValue);
	bool DecrementSecureCounter_Internal(bool bDBLocked, long nCounterID, long decrementAmount, long* pnCounterValue);
	bool SecureCounterConsistencyCheck_Internal(bool bDBLocked, VARIANT_BOOL* pvbValid);
	bool GetCounterUpdateRequestCode_Internal(bool bDBLocked, BSTR* pstrUpdateRequestCode);
	bool SetSecureCounterAlertLevel_Internal(bool bDBLocked, long nCounterID, long nAlertLevel, long nAlertMultiple);
	bool AddFileNoQueue_Internal(bool bDBLocked, BSTR bstrFile, long long llFileSize, long lPageCount,
		EFilePriority ePriority, long nWorkflowID, long* pnID);
	bool AddPaginationHistory_Internal(bool bDBLocked, long nOutputFileID, IIUnknownVector* pSourcePageInfo,
		IIUnknownVector* pDeletedSourcePageInfo, long nFileTaskSessionID);
	bool AddWorkflow_Internal(bool bDBLocked, BSTR bstrName, EWorkflowType eType, long* pnID);
	bool DeleteWorkflow_Internal(bool bDBLocked, long nID);
	bool GetWorkflowDefinition_Internal(bool bDBLocked, long nID, IWorkflowDefinition** ppWorkflowDefinition);
	bool SetWorkflowDefinition_Internal(bool bDBLocked, IWorkflowDefinition* pWorkflowDefinition);
	bool GetWorkflows_Internal(bool bDBLocked, IStrToStrMap ** pmapWorkFlowNameToID);
	bool GetWorkflowActions_Internal(bool bDBLocked, long nID, IIUnknownVector ** pvecActions);
	bool SetWorkflowActions_Internal(bool bDBLocked, long nID, IIUnknownVector* pActionList);
	bool GetStatsAllWorkflows_Internal(bool bDBLocked, BSTR bstrActionName, VARIANT_BOOL vbForceUpdate, EWorkflowVisibility eWorkflowVisibility,
		IActionStatistics* *pStats);
	bool GetAllActions_Internal(bool bDBLocked, IStrToStrMap** pmapActionNameToID);
	bool GetWorkflowStatus_Internal(bool bDBLocked, long nFileID, EActionStatus* peaStatus);
	bool GetAggregateWorkflowStatus_Internal(bool bDBLocked, long *pnUnattempted, long *pnProcessing,
		long *pnCompleted, long *pnFailed);
	bool GetWorkflowStatusAllFiles_Internal(bool bDBLocked, BSTR *pbstrStatusListing);
	bool GetWorkflowID_Internal(bool bDBLocked, BSTR bstrWorkflowName, long *pnID);
	bool IsFileInWorkflow_Internal(bool bDBLocked, long nFileID, long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow);
	bool GetUsingWorkflows_Internal(bool bDBLocked, VARIANT_BOOL *pbUsingWorkflows);
	bool GetWorkflowNameFromActionID_Internal(bool bDBLocked, long nActionID, BSTR* pbstrWorkflowName);
	bool GetActionIDForWorkflow_Internal(bool bDBLocked, BSTR bstrActionName, long nWorkflowID, long* pnActionID);
	bool MoveFilesToWorkflowFromQuery_Internal(bool bDBLocked, BSTR bstrQuery, long nSourceWorkflowID,  long nDestWorkflowID, long *pnCount);
	bool GetAttributeValue_Internal(bool bDBLocked, BSTR bstrSourceDocName, BSTR bstrAttributeSetName, BSTR bstrAttributePath,
		BSTR* pbstrValue);
	bool IsFileNameInWorkflow_Internal(bool bDBLocked, BSTR bstrFileName, long nWorkflowID, VARIANT_BOOL *pbIsInWorkflow);
	bool SaveWebAppSettings_Internal(bool bDBLocked, long nWorkflowID, BSTR bstrType, BSTR bstrSettings);
	bool LoadWebAppSettings_Internal(bool bDBLocked, long nWorkflowID, BSTR bstrType, BSTR *pbstrSettings);
	bool DefineNewMLModel_Internal(bool bDBLocked, BSTR strModelName, long* pnID);
	bool DeleteMLModel_Internal(bool bDBLocked, BSTR strModelName);
	bool GetMLModels_Internal(bool bDBLocked, IStrToStrMap * * pmapModelNameToID);
	bool GetActiveUsers_Internal(bool bDBLocked, BSTR bstrAction, IVariantVector** ppvecUserNames);
	bool AbortFAMSession_Internal(bool bDBLocked, long nFAMSessionID);
	bool MarkFileDeleted_Internal(bool bDBLocked, long nFileID, long nWorkflowID);
	bool CacheFileTaskSessionData_Internal(bool bDBLocked, long nFileTaskSessionID, long nPage,
		SAFEARRAY* parrayImageData, BSTR bstrUssData, BSTR bstrWordZoneData, BSTR bstrAttributeData,
		BSTR bstrException, VARIANT_BOOL vbCrucialUpdate, VARIANT_BOOL* pbWroteData);
	void CacheFileTaskSessionData_InternalHelper(ADODB::_ConnectionPtr ipConnection, long nFileTaskSessionID,
		long nPage, SAFEARRAY* parrayImageData, BSTR bstrUssData, BSTR bstrWordZoneData, BSTR bstrAttributeData,
		BSTR bstrException, bool bCrucialUpdate, VARIANT_BOOL* pbWroteData);
	bool GetCachedFileTaskSessionData_Internal(bool bDBLocked, long nFileTaskSessionID, long nPage,
		ECacheDataType eDataType, VARIANT_BOOL vbCrucialData, SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, 
		BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData, BSTR* pbstrException, VARIANT_BOOL* pbFoundCacheData);
	bool GetCachedFileTaskSessionData_InternalHelper(ADODB::_ConnectionPtr ipConnection,
		long nFileTaskSessionID, long nPage, ECacheDataType eDataType,
		SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData, BSTR* pbstrException);
	bool GetCachedFileTaskSessionData_QueryCachedData(_ConnectionPtr ipConnection, long nFileTaskSessionID, long nPage,
		ECacheDataType eDataType, SAFEARRAY** pparrayImageData, BSTR* pbstrUssData, BSTR* pbstrWordZoneData, BSTR* pbstrAttributeData,
		BSTR* pbstrException);
	bool CacheAttributeData_Internal(bool bDBLocked, long nFileTaskSessionID, IStrToStrMap* pmapAttributeData,
		VARIANT_BOOL bOverwriteModifiedData);
	bool MarkAttributeDataUnmodified_Internal(bool bDBLocked, long nFileTaskSessionID);
	bool GetUncommittedAttributeData_Internal(bool bDBLocked, long nFileID, long nActionID,
		BSTR bstrExceptIfMoreRecentAttributeSetName, IIUnknownVector** ppUncommitedPagesOfData);
	bool DiscardOldCacheData_Internal(bool bDBLocked, long nFileID, long nActionID, long nExceptFileTaskSessionID);

	void setDefaultSessionMemberValues();
	void promptForNewPassword(VARIANT_BOOL bShowAdmin, const std::string& strPasswordComplexityRequirements,
		VARIANT_BOOL* pbLoginCancelled, VARIANT_BOOL* pbLoginValid);
};

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingDB), CFileProcessingDB)

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 The purpose of this macro is to declare and initialize local variables and define the
//			 beginning of a do...while loop that contains a try...catch block to be used to retry
//			 the block of code between the BEGIN_CONNECTION_RETRY macro and the END_CONNECTION_RETRY
//			 macro.  If an exception is thrown within the block of code between the connection retry
//			 macros the connection passed to END_CONNECTION_RETRY macro will be tested to see if it 
//			 is a good connection if it is the caught exception is rethrown, if it is no longer a 
//			 good connection a check is made to see the retry count is equal to maximum retries, if
//			 not, the exception will be logged if this is the first retry and the connection will be
//			 reinitialized.  If the number of retires is exceeded the exception will be rethrown.
// REQUIRES: An ADODB::ConnectionPtr variable to be declared before the BEGIN_CONNECTION_RETRY macro
//			 is used so it can be passed to the END_CONNECTION_RETRY macro.
//-------------------------------------------------------------------------------------------------
#define BEGIN_CONNECTION_RETRY() \
		int nRetryCount = 0; \
		bool bRetryExceptionLogged = false; \
		bool bRetrySuccess = false; \
		do \
		{ \
			CSingleLock retryLock(&m_criticalSection, TRUE); \
			try \
			{\
				try\
				{\

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 To define the end of the block of code to be retried. (see above)
#define END_CONNECTION_RETRY(ipRetryConnection, strELICode) \
					bRetrySuccess = true; \
				}\
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELICode)\
			} \
			catch(UCLIDException &ue) \
			{ \
				bool bConnectionAlive = isConnectionAlive(ipRetryConnection); \
				bool bTimeout = ue.getTopText().find("timeout") != string::npos; \
				bool bDBSettingsValid = !m_strDatabaseServer.empty() && !m_strDatabaseName.empty(); \
				if (!bDBSettingsValid \
					|| !(bTimeout && m_bRetryOnTimeout) && bConnectionAlive \
					|| nRetryCount >= m_iNumberOfRetries) \
				{ \
					throw ue; \
				}\
				if (!bRetryExceptionLogged) \
				{ \
					UCLIDException uex("ELI32030", bTimeout \
						? "Application trace: Database query timed out. Re-attemping..." \
						: "Application trace: Database connection failed. Attempting to reconnect.", \
							ue); \
					uex.log(); \
					bRetryExceptionLogged = true; \
				} \
				if (!bConnectionAlive) \
				{ \
					reConnectDatabase(strELICode); \
				} \
				nRetryCount++; \
			} \
		} \
		while (!bRetrySuccess);

