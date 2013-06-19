#pragma once

// Connection Status Strings
const string gstrCONNECTING = "Connecting...";
const string gstrNOT_CONNECTED = "Not Connected";
const string gstrCONNECTION_ESTABLISHED = "Connection successfully established!";
const string gstrWRONG_SCHEMA = "Database found, but schema version is not compatible with this application!";
const string gstrDB_NOT_INITIALIZED = "Database found, but is not initialized!";
const string gstrUNABLE_TO_CONNECT_TO_SERVER = "Unable to connect to server!";
const string gstrDB_NOT_FOUND = "Connected to server, but database not found!";

// DBInfo Names
static const string gstrFAMDB_SCHEMA_VERSION = "FAMDBSchemaVersion";
static const string gstrUPDATE_QUEUE_EVENT_TABLE = "UpdateQueueEventTable";
static const string gstrUPDATE_FAST_TABLE = "UpdateFileActionStateTransitionTable";
static const string gstrCOMMAND_TIMEOUT = "CommandTimeout";
static const string gstrNUMBER_CONNECTION_RETRIES = "NumberOfConnectionRetries";
static const string gstrCONNECTION_RETRY_TIMEOUT = "ConnectionRetryTimeout";
static const string gstrAUTO_DELETE_FILE_ACTION_COMMENT = "AutoDeleteFileActionCommentOnComplete";
static const string gstrREQUIRE_PASSWORD_TO_PROCESS_SKIPPED = "RequirePasswordToProcessAllSkippedFiles";
static const string gstrALLOW_DYNAMIC_TAG_CREATION = "AllowDynamicTagCreation";
static const string gstrAUTO_REVERT_LOCKED_FILES = "AutoRevertLockedFiles";
static const string gstrAUTO_REVERT_TIME_OUT_IN_MINUTES = "AutoRevertTimeOutInMinutes";
static const string gstrAUTO_REVERT_NOTIFY_EMAIL_LIST = "AutoRevertNotifyEmailList";
static const string gstrSTORE_FAM_SESSION_HISTORY = "StoreFAMSessionHistory";
static const string gstrENABLE_INPUT_EVENT_TRACKING = "EnableInputEventTracking";
static const string gstrINPUT_EVENT_HISTORY_SIZE = "InputEventHistorySize";
static const string gstrREQUIRE_AUTHENTICATION_BEFORE_RUN = "RequireAuthenticationBeforeRun";
static const string gstrAUTO_CREATE_ACTIONS = "AutoCreateActions";
static const string gstrSKIP_AUTHENTICATION_ON_MACHINES = "SkipAuthenticationForServiceOnMachines";
static const string gstrACTION_STATISTICS_UPDATE_FREQ_IN_SECONDS = "ActionStatisticsUpdateFreqInSeconds";
static const string gstrGET_FILES_TO_PROCESS_TRANSACTION_TIMEOUT = "GetFilesToProcessTransactionTimeout";
static const string gstrSTORE_SOURCE_DOC_NAME_CHANGE_HISTORY = "StoreDocNameChangeHistory";
static const string gstrSTORE_DOC_TAG_HISTORY = "StoreDocTagHistory";
static const string gstrSTORE_DB_INFO_HISTORY = "StoreDBInfoChangeHistory";
static const string gstrMIN_SLEEP_BETWEEN_DB_CHECKS = "MinMillisecondsBetweenCheckForFilesToProcess";
static const string gstrMAX_SLEEP_BETWEEN_DB_CHECKS = "MaxMillisecondsBetweenCheckForFilesToProcess";
static const string gstrLAST_DB_INFO_CHANGE = "LastDBInfoChange";
static const string gstrSTORE_FTP_EVENT_HISTORY = "StoreFTPEventHistory";
static const string gstrALTERNATE_COMPONENT_DATA_DIR = "AlternateComponentDataDir";

// Default Settings
static const long glDEFAULT_COMMAND_TIMEOUT = 120;
static const int giDEFAULT_RETRY_COUNT = 10;
static const double gdDEFAULT_RETRY_TIMEOUT = 120.0;  // seconds
static const long gnPING_TIMEOUT = 60000; // 60 seconds
static const long gnSTATS_MAINT_TIMEOUT = 10000; // 10 seconds
static const long gnMINIMUM_AUTO_REVERT_TIME_OUT_IN_MINUTES = 5; // 5 minutes
static const double gdMINIMUM_TRANSACTION_TIMEOUT = 300.0; // 5 minutes
static const long gnDEFAULT_MIN_SLEEP_TIME_BETWEEN_DB_CHECK = 2000; // 2 seconds
static const long gnDEFAULT_MAX_SLEEP_TIME_BETWEEN_DB_CHECK = 2000; // 2 seconds

// Min and max allowed settings for sleep time between db check
static const long gnMIN_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK = 500;
static const long gnMAX_ALLOWED_SLEEP_TIME_BETWEEN_DB_CHECK = 300000;

// Local machine as database host
static const string gstrLOCAL_STRING = "(local)";
static const string gstrDEFAULT_SQL_INSTANCE_NAME = "MSSQLSERVER";

// Query for getting files with a particular tags
static const string gstrTAG_NAME_VALUE = "<TagNameValue>";
static const string gstrTAG_QUERY_SELECT = "<SelectFileValues>";
static const string gstrQUERY_FILES_WITH_TAGS = "SELECT <SelectFileValues> FROM ([FileTag] INNER JOIN "
	"[Tag] ON [FileTag].[TagID] = [Tag].[ID]) INNER JOIN [FAMFile] ON [FileTag].[FileID] = "
	"[FAMFile].[ID] WHERE [Tag].[TagName] = '<TagNameValue>'";

static const string gstrMAIN_DB_LOCK = "Main";
static const string gstrUSER_COUNTER_DB_LOCK = "UserCounter";
