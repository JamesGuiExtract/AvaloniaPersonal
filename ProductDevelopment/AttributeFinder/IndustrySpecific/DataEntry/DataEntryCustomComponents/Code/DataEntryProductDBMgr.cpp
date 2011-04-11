// DataEntryProductDBMgr.cpp : Implementation of CDataEntryProductDBMgr

#include "stdafx.h"
#include "DataEntryProductDBMgr.h"
#include "DataEntryProductDBMgr.h"
#include "DataEntry_DB_SQL.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <LockGuard.h>
#include <FAMUtilsConstants.h>
#include <TransactionGuard.h>
#include <ADOUtils.h>
#include <FAMDBHelperFunctions.h>

using namespace ADODB;
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// This must be updated when the DB schema changes
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
static const long glDATAENTRY_DB_SCHEMA_VERSION = 2;
static const string gstrDATA_ENTRY_SCHEMA_VERSION_NAME = "DataEntrySchemaVersion";
static const string gstrSTORE_DATAENTRY_PROCESSING_HISTORY = "StoreDataEntryProcessingHistory";
static const string gstrSTORE_HISTORY_DEFAULT_SETTING = "1"; // TRUE
static const string gstrENABLE_DATA_ENTRY_COUNTERS = "EnableDataEntryCounters";
static const string gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING = "0"; // FALSE
static const string gstrDESCRIPTION = "DataEntry database manager";

//-------------------------------------------------------------------------------------------------
// CDataEntryProductDBMgr
//-------------------------------------------------------------------------------------------------
CDataEntryProductDBMgr::CDataEntryProductDBMgr()
: m_ipFAMDB(NULL)
, m_ipDBConnection(NULL)
, m_bStoreDataEntryProcessingHistory(true)
, m_ipAFUtility(NULL)
, m_lNextInstanceToken(0)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
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
STDMETHODIMP CDataEntryProductDBMgr::raw_AddProductSpecificSchema(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI28990", ipDB != __nullptr);

		// Create the connection object
		_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI28991", ipDBConnection != __nullptr);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		// Retrieve the queries for creating DataEntry DB table(s).
		const vector<string> vecTableCreationQueries = getTableCreationQueries();
		vector<string> vecCreateQueries(vecTableCreationQueries.begin(), vecTableCreationQueries.end());

		// Add the queries to create keys/constraints.
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_FAMFILE);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_ACTION);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_MACHINE);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX);
		vecCreateQueries.push_back(gstrPOPULATE_DATAENTRY_COUNTER_TYPES);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE);

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
				iterDBInfoValues->second.c_str(), vbSetIfExists);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28992");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::raw_RemoveProductSpecificSchema(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI28993", ipDB != __nullptr);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI28994", ipDBConnection != __nullptr);
		
		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		vector<string> vecTables;
		getDataEntryTables(vecTables);

		dropTablesInVector(ipDBConnection, vecTables);

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

		validateDataEntrySchemaVersion(false);

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

		const vector<string> vecTableCreationQueries = getTableCreationQueries();
		vector<string> vecTablesNames = getTableNamesFromCreationQueries(vecTableCreationQueries);
		for (vector<string>::iterator iter = vecTablesNames.begin();
			 iter != vecTablesNames.end();
			 iter++)
		{
			ipTables->PushBack(iter->c_str());
		}

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
				return S_OK;
			}

			*pnProdSchemaVersion = asLong(strVersion);
		}

		switch (*pnProdSchemaVersion)
		{
			case 2: break;

			default:
				{
					UCLIDException ue("ELI31431",
						"Automatic updates are not supported for the current schema.");
					ue.addDebugInfo("FAM Schema Version", nFAMDBSchemaVersion, false);
					ue.addDebugInfo("ID Shield Schema Version", *pnProdSchemaVersion, false);
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
STDMETHODIMP CDataEntryProductDBMgr::AddDataEntryData(long lFileID, long nActionID,
													  double lDuration, long* plInstanceID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		if (!AddDataEntryData_Internal(false, lFileID, nActionID, lDuration, plInstanceID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);

			AddDataEntryData_Internal(true, lFileID, nActionID, lDuration, plInstanceID);
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29008");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::put_FAMDB(IFileProcessingDB* newVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI28996", newVal != __nullptr);

		// Only update if it is a new value
		if (m_ipFAMDB != newVal)
		{
			m_ipFAMDB = newVal;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = __nullptr;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28997");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::RecordCounterValues(long* plInstanceToken,
									long lDataEntryDataInstanceID, IIUnknownVector* pAttributes)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
		if (!RecordCounterValues_Internal(false, plInstanceToken, lDataEntryDataInstanceID, pAttributes))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(getThisAsCOMPtr(),
				gstrMAIN_DB_LOCK);
			RecordCounterValues_Internal(true, plInstanceToken,	lDataEntryDataInstanceID, pAttributes);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29056");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
ADODB::_ConnectionPtr CDataEntryProductDBMgr::getDBConnection()
{
	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == __nullptr)
	{
		UCLIDException ue("ELI29003",
			"FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
	}

	// Check if connection has been created
	if (m_ipDBConnection == __nullptr)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection));
		ASSERT_RESOURCE_ALLOCATION("ELI29002", m_ipDBConnection != __nullptr);
	}

	// if closed and Database server and database name are defined,  open the database connection
	if ( m_ipDBConnection->State == adStateClosed)
	{
		// Get database server from FAMDB
		string strDatabaseServer = asString(m_ipFAMDB->DatabaseServer);

		// Get DatabaseName from FAMDB
		string strDatabaseName = asString(m_ipFAMDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);
		if (!strDatabaseServer.empty() && !strDatabaseName.empty())
		{
			m_ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified);

			// Get the command timeout from the FAMDB DBInfo table
			string strValue = asString(
				m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str(), VARIANT_TRUE));

			// Set the command timeout
			m_ipDBConnection->CommandTimeout = asLong(strValue);

			// Get the setting for storing Data Entry processing history
			strValue = asString(m_ipFAMDB->GetDBInfoSetting(
				gstrSTORE_DATAENTRY_PROCESSING_HISTORY.c_str(), VARIANT_TRUE));

			// Set the local setting for storing history
			m_bStoreDataEntryProcessingHistory = strValue == asString(TRUE);
		}
	}
	
	return m_ipDBConnection;
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::validateLicense()
{
	// May eventually want to create & use a gnDATA_ENTRY_CORE_OBJECTS license ID.
	VALIDATE_LICENSE( gnDATA_ENTRY_CORE_COMPONENTS, "ELI29004", gstrDESCRIPTION);
}
//-------------------------------------------------------------------------------------------------
void CDataEntryProductDBMgr::getDataEntryTables(vector<string>& rvecTables)
{
	rvecTables.clear();
	rvecTables.push_back(gstrDATA_ENTRY_DATA);
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
UCLID_DATAENTRYCUSTOMCOMPONENTSLib::IDataEntryProductDBMgrPtr CDataEntryProductDBMgr::getThisAsCOMPtr()
{
	UCLID_DATAENTRYCUSTOMCOMPONENTSLib::IDataEntryProductDBMgrPtr ipThis;
	ipThis = this;
	ASSERT_RESOURCE_ALLOCATION("ELI30713", ipThis != __nullptr);
	return this;
}
//-------------------------------------------------------------------------------------------------
// Internal versions of Interface methods
//-------------------------------------------------------------------------------------------------
bool CDataEntryProductDBMgr::AddDataEntryData_Internal(bool bDBLocked, long lFileID, long nActionID,
													  double lDuration, long* plInstanceID)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI29051", plInstanceID != __nullptr);

			// Validate data entry schema
			validateDataEntrySchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			// Get the file ID as a string
			string strFileId = asString(lFileID);

			long nUserID = getKeyID(ipConnection, "FAMUser", "UserName", getCurrentUserName());
			long nMachineID = getKeyID(ipConnection, "Machine", "MachineName", getComputerName());

			// -------------------------------------------
			// Need to get the current TotalDuration value
			// -------------------------------------------
			double dTotalDuration = lDuration;
			string strSql = "SELECT TOP 1 [TotalDuration] FROM [DataEntryData] WHERE [FileID] = "
				+ strFileId + " ORDER BY [ID] DESC";

			// Create a pointer to a recordset
			_RecordsetPtr ipSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI29009", ipSet != __nullptr );

			// Open the recordset
			ipSet->Open( strSql.c_str(), _variant_t((IDispatch *)ipConnection, true),
				adOpenStatic, adLockReadOnly, adCmdText );

			// If there is an entry, then get the total duration from it
			if (ipSet->adoEOF == VARIANT_FALSE)
			{
				// Get the total duration from the datbase
				dTotalDuration += getDoubleField(ipSet->Fields, "TotalDuration");
			}

			// Build insert SQL query 
			string strInsertSQL = gstrINSERT_DATAENTRY_DATA_RCD + "(" + strFileId
				+ ", " + asString(nUserID) + ", " + asString(nActionID) + ", "
				+ asString(nMachineID) + ", GETDATE(), " + asString(lDuration) + ", "
				+ asString(dTotalDuration) + ")";

			// Create a transaction guard
			TransactionGuard tg(ipConnection);

			// If not storing previous history need to delete it
			if (!m_bStoreDataEntryProcessingHistory)
			{
				string strDeleteQuery = gstrDELETE_PREVIOUS_STATUS_FOR_FILEID;
				replaceVariable(strDeleteQuery, "<FileID>", strFileId);

				// Delete previous records with the fileID
				executeCmdQuery(ipConnection, strDeleteQuery);
			}

			// Insert the record
			executeCmdQuery(ipConnection, strInsertSQL);

			*plInstanceID = getLastTableID(ipConnection, "DataEntryData");

			// Commit the transactions
			tg.CommitTrans();

			END_ADO_CONNECTION_RETRY(
				ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29837");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30714");
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
bool CDataEntryProductDBMgr::RecordCounterValues_Internal(bool bDBLocked, long* plInstanceToken,
									long lDataEntryDataInstanceID, IIUnknownVector* pAttributes)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI29052", plInstanceToken != __nullptr);

			IIUnknownVectorPtr ipAttributes(pAttributes);

			// -1 instance token indicates the counts are "OnLoad" counts and that a new instance
			// token needs to be returned.
			bool bOnLoad = (*plInstanceToken == -1);
			if (bOnLoad)
			{
				*plInstanceToken = m_lNextInstanceToken++;
			}

			// Create or find the set of queries to record counts for the specified hierarchy of
			// attributes.
			vector<string>& strQueries = m_mapVecCounterValueInsertionQueries[*plInstanceToken];

			// If there is nothing to record, return now.
			if ((bOnLoad && ipAttributes == __nullptr) ||
				(!bOnLoad && ipAttributes == __nullptr && strQueries.empty()))
			{
				return S_OK;
			}

			// If counts are for "OnSave", update any existing "OnLoad" queries with the specified
			// DataEntryData instance ID.
			if (!bOnLoad)
			{
				string strInstanceID = asString(lDataEntryDataInstanceID);

				for (size_t i = 0; i < strQueries.size(); i++)
				{
					string strQuery = strQueries[i];
					replaceVariable(strQuery, "<InstanceID>", strInstanceID);
					strQueries[i] = strQuery;
				}
			}

			// Validate data entry schema
			validateDataEntrySchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			// Cache the result of areCountersEnabled;
			static bool countersAreEnabled = areCountersEnabled();
			if (!countersAreEnabled)
			{
				throw UCLIDException("ELI29053", "Data entry counters are not currently enabled.");
			}

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
					strQueries.push_back(gstrINSERT_DATAENTRY_COUNTER_VALUE + "(" +
						(bOnLoad ? "<InstanceID>" : asString(lDataEntryDataInstanceID)) + ", " +
						asString(lCounterID) + ", " + (bOnLoad ? "'L'" : "'S'") + ", " +
						asString(matchingAttributes->Size()) + ")");

					ipRecordSet->MoveNext();
				}
			}

			// If this is call is for "OnSave" so that we now have a data entry instance ID, record the
			// counts.
			if (!bOnLoad && !strQueries.empty())
			{
				// Create a transaction guard
				TransactionGuard tg(ipConnection);

				executeVectorOfSQL(ipConnection, strQueries);

				// Commit the transactions
				tg.CommitTrans();

				// Now that the counts have been recorded, clear the queries from the map.
				m_mapVecCounterValueInsertionQueries.erase(
					m_mapVecCounterValueInsertionQueries.find(*plInstanceToken));
			}

			END_ADO_CONNECTION_RETRY(
				ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29838");
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
const vector<string> CDataEntryProductDBMgr::getTableCreationQueries()
{
	vector<string> vecQueries;

	// WARNING: If any table is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	vecQueries.push_back(gstrCREATE_DATAENTRY_DATA);
	vecQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_DEFINITION);
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
	mapDefaultValues[gstrSTORE_DATAENTRY_PROCESSING_HISTORY] = gstrSTORE_HISTORY_DEFAULT_SETTING;
	mapDefaultValues[gstrENABLE_DATA_ENTRY_COUNTERS] = gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING;

	return mapDefaultValues;
}//-------------------------------------------------------------------------------------------------
