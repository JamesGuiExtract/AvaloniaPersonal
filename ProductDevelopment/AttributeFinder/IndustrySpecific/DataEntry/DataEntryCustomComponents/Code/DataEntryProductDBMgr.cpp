// DataEntryProductDBMgr.cpp : Implementation of CDataEntryProductDBMgr

#include "stdafx.h"
#include "DataEntryProductDBMgr.h"
#include "DataEntryProductDBMgr.h"
#include "DataEntry_DB_SQL.h"
#include "DataEntry_DB_SQL_80.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <LockGuard.h>
#include <FAMUtilsConstants.h>
#include <TransactionGuard.h>
#include <ADOUtils.h>
#include <FAMDBHelperFunctions.h>
#include <VectorOperations.h>
#include <cpputil.h>

using namespace ADODB;
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// This must be updated when the DB schema changes
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
static const long glDATAENTRY_DB_SCHEMA_VERSION = 6;
static const string gstrDATA_ENTRY_SCHEMA_VERSION_NAME = "DataEntrySchemaVersion";
// https://extract.atlassian.net/browse/ISSUE-13239
// StoreDataEntryProcessingHistory has been removed (const exists only for
// findUnrecognizedSchemaElements)
static const string gstrSTORE_DATAENTRY_PROCESSING_HISTORY = "StoreDataEntryProcessingHistory";
static const string gstrSTORE_HISTORY_DEFAULT_SETTING = "1"; // TRUE
static const string gstrENABLE_DATA_ENTRY_COUNTERS = "EnableDataEntryCounters";
static const string gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING = "0"; // FALSE
static const string gstrDESCRIPTION = "DataEntry database manager";

//-------------------------------------------------------------------------------------------------
// Schema update functions
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion3(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 3;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("EXEC sp_rename 'dbo.DataEntryData.TotalDuration', 'OverheadTime', 'COLUMN'");
		vecQueries.push_back("UPDATE [dbo].[DataEntryData] SET [OverheadTime] = 0");

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '3' WHERE [Name] = '" + 
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33952");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion4(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 4;

		if (pnNumSteps != __nullptr)
		{
			long nSteps = 0;
			executeCmdQuery(ipConnection, 
				"SELECT COUNT(*) AS [ID] FROM [DataEntryData]", false, &nSteps);

			nSteps /= 100;
			nSteps += 3;
			*pnNumSteps += nSteps;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [DataEntryCounterValue] DROP CONSTRAINT [FK_DataEntryCounterValue_Instance]");
		vecQueries.push_back(gstrPORT_DATAENTRYDATA_TO_FILETASKSESSION);
		vecQueries.push_back("DROP TABLE [DataEntryData]");
		vecQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE_V4);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '4' WHERE [Name] = '" + 
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38600");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion5(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 5;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("DELETE FROM [DBInfo] WHERE [Name] = '" +
			gstrSTORE_DATAENTRY_PROCESSING_HISTORY + "'");

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '5' WHERE [Name] = '" + 
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME + "'");

		// This is to fix a problem were the TaskClass values may not be in the database
		// https://extract.atlassian.net/browse/ISSUE-13341
		string strInsertTaskClassIfNeeded = 
			"IF NOT EXISTS (SELECT ID FROM TaskClass WHERE GUID = '59496DF7-3951-49b7-B063-8C28F4CD843F') " +
			gstrINSERT_DATA_ENTRY_VERIFY_TASK_CLASS;
		vecQueries.push_back(strInsertTaskClassIfNeeded);
		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40357");
}
//-------------------------------------------------------------------------------------------------
int UpdateToSchemaVersion6(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 6;

		if (pnNumSteps != __nullptr)
		{
			*pnNumSteps += 1;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back(gstrCREATE_DATA_ENTRY_COUNTER_VALUE_INSTANCEID_TYPE_INDEX);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '6' WHERE [Name] = '" + 
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39113");
}

//-------------------------------------------------------------------------------------------------
// CDataEntryProductDBMgr
//-------------------------------------------------------------------------------------------------
CDataEntryProductDBMgr::CDataEntryProductDBMgr()
: m_ipFAMDB(NULL)
, m_ipDBConnection(NULL)
, m_ipAFUtility(NULL)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
, m_currentRole(CppBaseApplicationRoleConnection::kExtractRole)
{
}
//-------------------------------------------------------------------------------------------------
CDataEntryProductDBMgr::~CDataEntryProductDBMgr()
{
	try
	{
		m_ipFAMDB = __nullptr;
		m_ipDBConnection = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28985");
}
//-------------------------------------------------------------------------------------------------
HRESULT CDataEntryProductDBMgr::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::FinalRelease()
{
	try
	{
		// Release COM objects before the object is destructed
		m_ipFAMDB = __nullptr;
		m_ipDBConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29010");
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDataEntryProductDBMgr,
		&__uuidof(IProductSpecificDBMgr),
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI28986", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t(gstrDESCRIPTION.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28987");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_IsLicensed(VARIANT_BOOL  * pbValue)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI28988", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28989");
}

//-------------------------------------------------------------------------------------------------
// IProductSpecificDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_AddProductSpecificSchema(_Connection* pConnection,
                                                                  IFileProcessingDB *pDB,
																  VARIANT_BOOL bOnlyTables,
																  VARIANT_BOOL bAddUserTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI28990", ipDB != __nullptr);

		// Create the connection object
		_ConnectionPtr ipDBConnection(pConnection);
		ASSERT_RESOURCE_ALLOCATION("ELI28991", ipDBConnection != __nullptr);

		// Retrieve the queries for creating DataEntry DB table(s).
		const vector<string> vecTableCreationQueries = getTableCreationQueries(asCppBool(bAddUserTables));
		vector<string> vecCreateQueries(vecTableCreationQueries.begin(), vecTableCreationQueries.end());

		// Add the queries to create keys/constraints.
		vecCreateQueries.push_back(gstrPOPULATE_DATAENTRY_COUNTER_TYPES);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE_V4);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE);
		vecCreateQueries.push_back(gstrINSERT_DATA_ENTRY_VERIFY_TASK_CLASS);
		vecCreateQueries.push_back(gstrCREATE_DATA_ENTRY_COUNTER_VALUE_INSTANCEID_TYPE_INDEX);

		// Execute the queries to create the data entry table
		executeVectorOfSQL(ipDBConnection, vecCreateQueries);

		// Set the default values for the DBInfo settings.
		map<string, string> mapDBInfoDefaultValues = getDBInfoDefaultValues();
		for (map<string, string>::iterator iterDBInfoValues = mapDBInfoDefaultValues.begin();
			iterDBInfoValues != mapDBInfoDefaultValues.end();
			iterDBInfoValues++)
		{
			VARIANT_BOOL vbSetIfExists =
				asVariantBool(iterDBInfoValues->first == gstrDATA_ENTRY_SCHEMA_VERSION_NAME);

			ipDB->SetDBInfoSetting(iterDBInfoValues->first.c_str(),
				iterDBInfoValues->second.c_str(), vbSetIfExists, VARIANT_FALSE);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28992");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_AddProductSpecificSchema80(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI34261", ipDB != NULL);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		NoRoleConnection roleConnection(strDatabaseServer, strDatabaseName);

		// Create the vector of Queries to execute
		vector<string> vecCreateQueries;

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_DATA_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_FAMFILE_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_FAMUSER_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_ACTION_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_MACHINE_80);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX_80);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_DEFINITION_80);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_TYPE_80);
		vecCreateQueries.push_back(gstrPOPULATE_DATAENTRY_COUNTER_TYPES_80);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_VALUE_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID_80);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE_80);

		vecCreateQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME + "', '2')");
		vecCreateQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
			gstrSTORE_DATAENTRY_PROCESSING_HISTORY + "', '" + gstrSTORE_HISTORY_DEFAULT_SETTING +
			"')");
		vecCreateQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
			gstrENABLE_DATA_ENTRY_COUNTERS + "', '" +
			gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING + "')");

		// Execute the queries to create the data entry table
		executeVectorOfSQL(roleConnection.ADOConnection(), vecCreateQueries);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34260");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_RemoveProductSpecificSchema(_Connection* pConnection,
																	 IFileProcessingDB *pDB,
																	 VARIANT_BOOL bOnlyTables,
																	 VARIANT_BOOL bRetainUserTables,
																	 VARIANT_BOOL *pbSchemaExists)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI38280", pbSchemaExists != __nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_RESOURCE_ALLOCATION("ELI53065", ipConnection != __nullptr);

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI28993", ipDB != __nullptr);

		m_ipFAMDB = ipDB;

		string strValue = asString(ipDB->GetDBInfoSetting(
			gstrDATA_ENTRY_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));

		if (strValue.empty())
		{
			*pbSchemaExists = VARIANT_FALSE;
			return S_OK;
		}
		else
		{
			*pbSchemaExists = VARIANT_TRUE;
		}
		
		vector<string> vecTables;
		getDataEntryTables(vecTables);

		if (asCppBool(bRetainUserTables))
		{
			eraseFromVector(vecTables, gstrDATAENTRY_DATA_COUNTER_DEFINITION);
		}

		dropTablesInVector(ipConnection, vecTables);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28995");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_ValidateSchema(IFileProcessingDB* pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Update the FAMDB pointer if it is new.
		if (m_ipFAMDB != pDB)
		{
			m_ipFAMDB = pDB;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = __nullptr;
		}

		validateDataEntrySchemaVersion(true);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31423");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any DBInfo row is removed, this code needs to be modified so that it does not treat
// the removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CDataEntryProductDBMgr::raw_GetDBInfoRows(IVariantVector** ppDBInfoRows)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipDBInfoRows(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI31424", ipDBInfoRows != __nullptr);

		map<string, string> mapDBInfoValues = getDBInfoDefaultValues();
		for (map<string, string>::iterator iterDBInfoValues = mapDBInfoValues.begin();
			 iterDBInfoValues != mapDBInfoValues.end();
			 iterDBInfoValues++)
		{
			ipDBInfoRows->PushBack(iterDBInfoValues->first.c_str());
		}

		// https://extract.atlassian.net/browse/ISSUE-13239
		// Ability to turn off record session history for data entry has been removed.
		ipDBInfoRows->PushBack(gstrSTORE_DATAENTRY_PROCESSING_HISTORY.c_str());

		*ppDBInfoRows = ipDBInfoRows.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31425");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any table is removed, this code needs to be modified so that it does not treat the
// removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CDataEntryProductDBMgr::raw_GetTables(IVariantVector** ppTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipTables(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI31426", ipTables != __nullptr);

		const vector<string> vecTableCreationQueries = getTableCreationQueries(true);
		vector<string> vecTablesNames = getTableNamesFromCreationQueries(vecTableCreationQueries);
		for (vector<string>::iterator iter = vecTablesNames.begin();
			 iter != vecTablesNames.end();
			 iter++)
		{
			ipTables->PushBack(iter->c_str());
		}

		// Legacy table names:
		// FileTaskSession replaces DataEntryData in schema version 4
		ipTables->PushBack(gstrDATA_ENTRY_DATA.c_str());

		*ppTables = ipTables.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31427");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_UpdateSchemaForFAMDBVersion(IFileProcessingDB* pDB,
	_Connection* pConnection, long nFAMDBSchemaVersion, long* pnProdSchemaVersion, long* pnNumSteps,
	IProgressStatus* pProgressStatus)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_ARGUMENT("ELI31428", ipDB != __nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_ARGUMENT("ELI31429", ipConnection != __nullptr);

		ASSERT_ARGUMENT("ELI31430", pnProdSchemaVersion != __nullptr);

		if (*pnProdSchemaVersion == 0)
		{
			string strVersion = asString(
				ipDB->GetDBInfoSetting(gstrDATA_ENTRY_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));
			
			// If the DataEntry specific components are missing, there is nothing to do.
			if (strVersion.empty())
			{
				// if FAMDBSchemaVersion is 184 all product specific schemas should exist so add the product schema
				if (nFAMDBSchemaVersion == 184)
				{
					if (pnNumSteps != __nullptr)
					{
						*pnNumSteps = 8;
						*pnProdSchemaVersion = 0;
					}
					else
					{
						IProductSpecificDBMgrPtr ipThis(this);
						ipThis->AddProductSpecificSchema(ipConnection, ipDB, VARIANT_FALSE, VARIANT_TRUE);
						*pnProdSchemaVersion = glDATAENTRY_DB_SCHEMA_VERSION;
					}
				}
				return S_OK;
			}
			*pnProdSchemaVersion = asLong(strVersion);
		}

		switch (*pnProdSchemaVersion)
		{
			case 2:	// The schema update from 2 to 3 needs to take place against FAM DB schema version 110
					if (nFAMDBSchemaVersion == 110)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion3(ipConnection, pnNumSteps, NULL);
					}
					break;

			case 3:	if (nFAMDBSchemaVersion == 129)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion4(ipConnection, pnNumSteps, NULL);
					}
					// Break is intentionally missing as schema updates 4 and 5 both correspond with
					// nFAMDBSchemaVersion 129

			case 4:	if (nFAMDBSchemaVersion == 129)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion5(ipConnection, pnNumSteps, NULL);
					}
					break;
					
			case 5: if (nFAMDBSchemaVersion == 133)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion6(ipConnection, pnNumSteps, NULL);
					}
					break;

			case 6: break;

			default:
				{
					UCLIDException ue("ELI31431",
						"Automatic updates are not supported for the current schema.");
					ue.addDebugInfo("FAM Schema Version", nFAMDBSchemaVersion, false);
					ue.addDebugInfo("Data entry Schema Version", *pnProdSchemaVersion, false);
					throw ue;
				}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31432");
}

//-------------------------------------------------------------------------------------------------
// IDataEntryProductDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::RecordCounterValues(VARIANT_BOOL vbOnLoad,
														 long lFileTaskSessionID, 
														 IIUnknownVector* pAttributes)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
		if (!RecordCounterValues_Internal(false, vbOnLoad, lFileTaskSessionID, pAttributes))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(m_ipFAMDB, gstrMAIN_DB_LOCK);
			RecordCounterValues_Internal(true, vbOnLoad, lFileTaskSessionID, pAttributes);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29056");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::Initialize(IFileProcessingDB* pFAMDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI40354", pFAMDB != nullptr);

		m_ipFAMDB = pFAMDB;
		ASSERT_RESOURCE_ALLOCATION("ELI40355", m_ipFAMDB != nullptr);

		m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38609");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
shared_ptr<CppBaseApplicationRoleConnection> CDataEntryProductDBMgr::getAppRoleConnection(bool bReset)
{
	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == __nullptr)
	{
		UCLIDException ue("ELI29003",
			"FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
	}

	if (bReset)
	{
		m_ipDBConnection = __nullptr;
	}

	bool connectionExists = m_ipDBConnection != __nullptr && m_ipDBConnection->ADOConnection()->State != adStateClosed;

	if (connectionExists)
	{
		return m_ipDBConnection;
	}

	_ConnectionPtr adoConnection(_uuidof(Connection));
	ASSERT_RESOURCE_ALLOCATION("ELI51855", adoConnection != __nullptr);

	// Get database server from FAMDB
	string strDatabaseServer = asString(m_ipFAMDB->DatabaseServer);

	// Get DatabaseName from FAMDB
	string strDatabaseName = asString(m_ipFAMDB->DatabaseName);

	// create the connection string
	string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);
	if (!strDatabaseServer.empty() && !strDatabaseName.empty())
	{
		adoConnection->Open(strConnectionString.c_str(), "", "", adConnectUnspecified);

		// Get the command timeout from the FAMDB DBInfo table
		adoConnection->CommandTimeout =
			asLong(m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str(), VARIANT_TRUE));
	}

	long nDBHash = asLong(m_ipFAMDB->GetDBInfoSetting("DatabaseHash", VARIANT_FALSE));
	m_ipDBConnection = m_roleUtility.CreateAppRole(adoConnection, m_currentRole, nDBHash);

	return m_ipDBConnection;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::validateLicense()
{
	// May eventually want to create & use a gnDATA_ENTRY_CORE_OBJECTS license ID.
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI29004", gstrDESCRIPTION);
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::getDataEntryTables(vector<string>& rvecTables)
{
	rvecTables.clear();
	rvecTables.push_back(gstrDATAENTRY_DATA_COUNTER_DEFINITION);
	rvecTables.push_back(gstrDATAENTRY_DATA_COUNTER_TYPE);
	rvecTables.push_back(gstrDATAENTRY_DATA_COUNTER_VALUE);
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::validateDataEntrySchemaVersion(bool bThrowIfMissing)
{
	ASSERT_RESOURCE_ALLOCATION("ELI29005", m_ipFAMDB != __nullptr);

	// Get the Version from the FAMDB DBInfo table
	string strValue = asString(m_ipFAMDB->GetDBInfoSetting(
		gstrDATA_ENTRY_SCHEMA_VERSION_NAME.c_str(), asVariantBool(bThrowIfMissing)));

	if (bThrowIfMissing || !strValue.empty())
	{
		// Check against expected version
		if (asLong(strValue) != glDATAENTRY_DB_SCHEMA_VERSION)
		{
			UCLIDException ue("ELI29006", "Data Entry database schema is not current version!");
			ue.addDebugInfo("Expected", glDATAENTRY_DB_SCHEMA_VERSION);
			ue.addDebugInfo("Database Version", strValue);
			throw ue;
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool CDataEntryProductDBMgr::areCountersEnabled()
{
	try
	{
		string strValue = asString(
			m_ipFAMDB->GetDBInfoSetting(gstrENABLE_DATA_ENTRY_COUNTERS.c_str(), VARIANT_TRUE));
		return strValue == "1";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29059");
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CDataEntryProductDBMgr::getAFUtility()
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI29060", m_ipAFUtility != __nullptr);
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
// Internal versions of Interface methods
//-------------------------------------------------------------------------------------------------
bool CDataEntryProductDBMgr::RecordCounterValues_Internal(bool bDBLocked, VARIANT_BOOL vbOnLoad,
									long lFileTaskSessionID, IIUnknownVector* pAttributes)
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipAttributes(pAttributes);

			bool bOnLoad = asCppBool(vbOnLoad);

			// If there is nothing to record, return now.
			if (ipAttributes == __nullptr)
			{
				return S_OK;
			}

			// Validate data entry schema
			validateDataEntrySchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Cache the result of areCountersEnabled;
			static bool countersAreEnabled = areCountersEnabled();
			if (!countersAreEnabled)
			{
				throw UCLIDException("ELI29053", "Data entry counters are not currently enabled.");
			}

			vector<string> vecQueries;

			if (ipAttributes != __nullptr)
			{
				// Query to find all counters a value needs to be recorded for.
				string strSql = "SELECT [ID], [AttributeQuery] FROM [DataEntryCounterDefinition] WHERE " +
					string(bOnLoad ? "[RecordOnLoad]" : "RecordOnSave") + " = 1";

				// Create a pointer to a recordset
				_RecordsetPtr ipRecordSet(__uuidof(Recordset));
				ASSERT_RESOURCE_ALLOCATION("ELI29054", ipRecordSet != __nullptr);

				// Open the recordset
				ipRecordSet->Open( strSql.c_str(), _variant_t((IDispatch *)ipConnection, true),
					adOpenForwardOnly, adLockReadOnly, adCmdText);

				while (ipRecordSet->adoEOF == VARIANT_FALSE)
				{
					FieldsPtr ipFields(ipRecordSet->Fields);
					ASSERT_RESOURCE_ALLOCATION("ELI29066", ipFields != __nullptr);

					// Get the counter ID and query used to count qualifying attributes
					long lCounterID = getLongField(ipFields, "ID");
					string strQuery = getStringField(ipFields, "AttributeQuery");

					// Query for all matching attributes
					IIUnknownVectorPtr matchingAttributes = 
						getAFUtility()->QueryAttributes(ipAttributes, strQuery.c_str(), false);
					ASSERT_RESOURCE_ALLOCATION("ELI29055", matchingAttributes != __nullptr);

					// Insert a query that to record the counts into the vector of queries for the
					// current data entry instance.
					vecQueries.push_back(gstrINSERT_DATAENTRY_COUNTER_VALUE + "(" +
						asString(lFileTaskSessionID) + ", " +
						asString(lCounterID) + ", " + (bOnLoad ? "'L'" : "'S'") + ", " +
						asString(matchingAttributes->Size()) + ")");

					ipRecordSet->MoveNext();
				}
			}

			// If this is call is for "OnSave" so that we now have a data entry instance ID, record the
			// counts.
			if (!vecQueries.empty())
			{
				// Create a transaction guard
				TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

				executeVectorOfSQL(ipConnection, vecQueries);

				// Commit the transactions
				tg.CommitTrans();
			}

			END_ADO_CONNECTION_RETRY(
				ipConnection, getAppRoleConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29838");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30715");
	}
	catch(UCLIDException ue)
	{
		if (!bDBLocked)
		{
			return false;
		}
		throw ue;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
const vector<string> CDataEntryProductDBMgr::getTableCreationQueries(bool bAddUserTables)
{
	vector<string> vecQueries;

	// WARNING: If any table is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	if (bAddUserTables)
	{
		vecQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_DEFINITION);
	}
	vecQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_TYPE);
	vecQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_VALUE);

	return vecQueries;
}
//-------------------------------------------------------------------------------------------------
map<string, string> CDataEntryProductDBMgr::getDBInfoDefaultValues()
{
	map<string, string> mapDefaultValues;

	// WARNING: If any DBInfo row is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	mapDefaultValues[gstrDATA_ENTRY_SCHEMA_VERSION_NAME] = asString(glDATAENTRY_DB_SCHEMA_VERSION);
	mapDefaultValues[gstrENABLE_DATA_ENTRY_COUNTERS] = gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING;

	return mapDefaultValues;
}