#pragma once

// Connection Status Strings
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

// Default Settings
static const long glDEFAULT_COMMAND_TIMEOUT = 120;
static const int giDEFAULT_RETRY_COUNT = 10;
static const double gdDEFAULT_RETRY_TIMEOUT = 120.0;  // seconds

// Local machine as database host
static const string gstrLOCAL_STRING = "(local)";
static const string gstrDEFAULT_SQL_INSTANCE_NAME = "MSSQLSERVER";