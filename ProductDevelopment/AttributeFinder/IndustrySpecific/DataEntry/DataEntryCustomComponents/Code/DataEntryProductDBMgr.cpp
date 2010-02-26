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

using namespace ADODB;
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// This must be updated when the DB schema changes
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
		m_ipFAMDB = NULL;
		m_ipDBConnection = NULL;
		m_ipAFUtility = NULL;
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
		m_ipFAMDB = NULL;
		m_ipDBConnection = NULL;
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

		ASSERT_ARGUMENT("ELI28986", pstrComponentDescription != NULL);

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

		ASSERT_ARGUMENT("ELI28988", pbValue != NULL);

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
		ASSERT_RESOURCE_ALLOCATION("ELI28990", ipDB != NULL);

		// Create the connection object
		_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI28991", ipDBConnection != NULL);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		// Create the vector of Queries to execute
		vector<string> vecCreateQueries;

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_DATA);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_FAMFILE);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_ACTION);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRYDATA_MACHINE);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_DEFINITION);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_TYPE);
		vecCreateQueries.push_back(gstrPOPULATE_DATAENTRY_COUNTER_TYPES);

		vecCreateQueries.push_back(gstrCREATE_DATAENTRY_COUNTER_VALUE);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_INSTANCE);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_ID);
		vecCreateQueries.push_back(gstrADD_FK_DATAENTRY_COUNTER_VALUE_TYPE);

		// Execute the queries to create the data entry table
		executeVectorOfSQL(ipDBConnection, vecCreateQueries);

		// Set the schema version
		ipDB->SetDBInfoSetting(gstrDATA_ENTRY_SCHEMA_VERSION_NAME.c_str(), 
			asString(glDATAENTRY_DB_SCHEMA_VERSION).c_str(), VARIANT_TRUE);

		// Set the default
		ipDB->SetDBInfoSetting(gstrSTORE_DATAENTRY_PROCESSING_HISTORY.c_str(), 
			gstrSTORE_HISTORY_DEFAULT_SETTING.c_str(), VARIANT_FALSE);

		// Set data entry counters
		ipDB->SetDBInfoSetting(gstrENABLE_DATA_ENTRY_COUNTERS.c_str(), 
			gstrENABLE_DATA_ENTRY_COUNTERS_DEFAULT_SETTING.c_str(), VARIANT_FALSE);
	
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
		ASSERT_RESOURCE_ALLOCATION("ELI28993", ipDB != NULL);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI28994", ipDBConnection != NULL);
		
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
// IDataEntryProductDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDataEntryProductDBMgr::AddDataEntryData(long lFileID, long nActionID,
													  double lDuration, long* plInstanceID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI29051", plInstanceID != NULL);

		// Validate data entry schema
		validateDataEntrySchemaVersion();

		// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
		_ConnectionPtr ipConnection = NULL;

		BEGIN_ADO_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<IFileProcessingDBPtr> lg(m_ipFAMDB);

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
		ASSERT_RESOURCE_ALLOCATION("ELI29009", ipSet != NULL );

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

		ASSERT_ARGUMENT("ELI28996", newVal != NULL);

		// Only update if it is a new value
		if (m_ipFAMDB != newVal)
		{
			m_ipFAMDB = newVal;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = NULL;
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

		ASSERT_ARGUMENT("ELI29052", plInstanceToken != NULL);

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
		if ((bOnLoad && ipAttributes == NULL) ||
			(!bOnLoad && ipAttributes == NULL && strQueries.empty()))
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
		validateDataEntrySchemaVersion();

		// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
		_ConnectionPtr ipConnection = NULL;

		BEGIN_ADO_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Lock the database
		LockGuard<IFileProcessingDBPtr> lg(m_ipFAMDB);

		// Cache the result of areCountersEnabled;
		static bool countersAreEnabled = areCountersEnabled();
		if (!countersAreEnabled)
		{
			throw UCLIDException("ELI29053", "Data entry counters are not currently enabled.");
		}

		if (ipAttributes != NULL)
		{
			// Query to find all counters a value needs to be recorded for.
			string strSql = "SELECT [ID], [AttributeQuery] FROM [DataEntryCounterDefinition] WHERE " +
				string(bOnLoad ? "[RecordOnLoad]" : "RecordOnSave") + " = 1";

			// Create a pointer to a recordset
			_RecordsetPtr ipRecordSet(__uuidof(Recordset));
			ASSERT_RESOURCE_ALLOCATION("ELI29054", ipRecordSet != NULL);

			// Open the recordset
			ipRecordSet->Open( strSql.c_str(), _variant_t((IDispatch *)ipConnection, true),
				adOpenForwardOnly, adLockReadOnly, adCmdText);

			while (ipRecordSet->adoEOF == VARIANT_FALSE)
			{
				FieldsPtr ipFields(ipRecordSet->Fields);
				ASSERT_RESOURCE_ALLOCATION("ELI29066", ipFields != NULL);

				// Get the counter ID and query used to count qualifying attributes
				long lCounterID = getLongField(ipFields, "ID");
				string strQuery = getStringField(ipFields, "AttributeQuery");
				
				// Query for all matching attributes
				IIUnknownVectorPtr matchingAttributes = 
					 getAFUtility()->QueryAttributes(ipAttributes, strQuery.c_str(), false);
				ASSERT_RESOURCE_ALLOCATION("ELI29055", matchingAttributes != NULL);

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
	if (m_ipFAMDB == NULL)
	{
		UCLIDException ue("ELI29003",
			"FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
	}

	// Check if connection has been created
	if (m_ipDBConnection == NULL)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection));
		ASSERT_RESOURCE_ALLOCATION("ELI29002", m_ipDBConnection != NULL);
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
			string strValue = asString(m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str()));

			// Set the command timeout
			m_ipDBConnection->CommandTimeout = asLong(strValue);

			// Get the setting for storing Data Entry processing history
			strValue = asString(m_ipFAMDB->GetDBInfoSetting(
				gstrSTORE_DATAENTRY_PROCESSING_HISTORY.c_str()));

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
void CDataEntryProductDBMgr::validateDataEntrySchemaVersion()
{
	ASSERT_RESOURCE_ALLOCATION("ELI29005", m_ipFAMDB != NULL);

	// Get the Version from the FAMDB DBInfo table
	string strValue = asString(m_ipFAMDB->GetDBInfoSetting(gstrDATA_ENTRY_SCHEMA_VERSION_NAME.c_str()));

	// Check against expected version
	if (asLong(strValue) != glDATAENTRY_DB_SCHEMA_VERSION)
	{
		UCLIDException ue("ELI29006", "Data Entry database schema is not current version!");
		ue.addDebugInfo("Expected", glDATAENTRY_DB_SCHEMA_VERSION);
		ue.addDebugInfo("Database Version", strValue);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
bool CDataEntryProductDBMgr::areCountersEnabled()
{
	try
	{
		string strValue = asString(m_ipFAMDB->GetDBInfoSetting(gstrENABLE_DATA_ENTRY_COUNTERS.c_str()));
		return strValue == "1";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29059");
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CDataEntryProductDBMgr::getAFUtility()
{
	if (m_ipAFUtility == NULL)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI29060", m_ipAFUtility != NULL);
	}
	
	return m_ipAFUtility;
}