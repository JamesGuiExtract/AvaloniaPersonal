// IDShieldProductDBMgr.cpp : Implementation of CIDShieldProductDBMgr

#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "IDShieldProductDBMgr.h"
#include "IDShield_DB_SQL.h"
#include "IDShield_DB_SQL_80.h"
#include "FAMDBHelperFunctions.h"

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
// !!!ATTENTION!!!
// An UpdateToSchemaVersion method must be added when checking in a new schema version.
const long glIDShieldDBSchemaVersion = 4;
const string gstrID_SHIELD_SCHEMA_VERSION_NAME = "IDShieldSchemaVersion";
static const string gstrSTORE_IDSHIELD_PROCESSING_HISTORY = "StoreIDShieldProcessingHistory";
static const string gstrSTORE_HISTORY_DEFAULT_SETTING = "1"; // TRUE

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

		vecQueries.push_back("ALTER TABLE [IDShieldData] ADD [NumPagesAutoAdvanced] INT NULL");
		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '3' WHERE [Name] = '" + 
			gstrID_SHIELD_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31415");
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
			*pnNumSteps += 3;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("EXEC sp_rename 'dbo.IDShieldData.TotalDuration', 'OverheadTime', 'COLUMN'");
		vecQueries.push_back("UPDATE [dbo].[IDShieldData] SET [OverheadTime] = 0");

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '4' WHERE [Name] = '" + 
			gstrID_SHIELD_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33949");
}

//-------------------------------------------------------------------------------------------------
// CIDShieldProductDBMgr
//-------------------------------------------------------------------------------------------------
CIDShieldProductDBMgr::CIDShieldProductDBMgr()
: m_bStoreIDShieldProcessingHistory(true)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
{
}
//-------------------------------------------------------------------------------------------------
CIDShieldProductDBMgr::~CIDShieldProductDBMgr()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20391");
}
//-------------------------------------------------------------------------------------------------
HRESULT CIDShieldProductDBMgr::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldProductDBMgr::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIDShieldProductDBMgr,
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
STDMETHODIMP CIDShieldProductDBMgr::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI18689", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t("ID Shield database manager").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18690");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_IsLicensed(VARIANT_BOOL  * pbValue)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI19817", pbValue != __nullptr);

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18691");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IProductSpecificDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_AddProductSpecificSchema(IFileProcessingDB *pDB,
																 VARIANT_BOOL bAddUserTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18823", ipDB != __nullptr);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI18824", ipDBConnection != __nullptr);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		// Retrieve the queries for creating IDShield DB table(s).
		const vector<string> vecTableCreationQueries = getTableCreationQueries();
		vector<string> vecCreateQueries(vecTableCreationQueries.begin(), vecTableCreationQueries.end());

		// Add queries for creating indexes & constraints
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELD_FAMFILE);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMFILE);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_MACHINE);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_MACHINE);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX);

		// Execute the queries to create the id shield table
		executeVectorOfSQL(ipDBConnection, vecCreateQueries);

		// Set the default values for the DBInfo settings.
		map<string, string> mapDBInfoDefaultValues = getDBInfoDefaultValues();
		for (map<string, string>::iterator iterDBInfoValues = mapDBInfoDefaultValues.begin();
			iterDBInfoValues != mapDBInfoDefaultValues.end();
			iterDBInfoValues++)
		{
			VARIANT_BOOL vbSetIfExists =
				asVariantBool(iterDBInfoValues->first == gstrID_SHIELD_SCHEMA_VERSION_NAME);

			ipDB->SetDBInfoSetting(iterDBInfoValues->first.c_str(),
				iterDBInfoValues->second.c_str(), vbSetIfExists);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18686");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_AddProductSpecificSchema80(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI34254", ipDB != NULL);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI34255", ipDBConnection != NULL);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		// Create the vector of Queries to execute
		vector<string> vecCreateQueries;

		vecCreateQueries.push_back(gstrCREATE_IDSHIELD_DATA_80);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELD_FAMFILE_80);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMFILE_80);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_FAMUSER_80);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMUSER_80);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_MACHINE_80);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_MACHINE_80);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX_80);
		vecCreateQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
			gstrID_SHIELD_SCHEMA_VERSION_NAME + "', '2')");
		vecCreateQueries.push_back("INSERT INTO [DBInfo] ([Name], [Value]) VALUES('" +
			gstrSTORE_IDSHIELD_PROCESSING_HISTORY + "', '" + gstrSTORE_HISTORY_DEFAULT_SETTING +
			"')");

		// Execute the queries to create the id shield table
		executeVectorOfSQL(ipDBConnection, vecCreateQueries);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34256");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_RemoveProductSpecificSchema(IFileProcessingDB *pDB,
																	VARIANT_BOOL bRetainUserTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18956", ipDB != __nullptr);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI18957", ipDBConnection != __nullptr);
		
		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		vector<string> vecTables;
		getIDShieldTables(vecTables);

		dropTablesInVector(ipDBConnection, vecTables);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18687");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_ValidateSchema(IFileProcessingDB* pDB)
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

		validateIDShieldSchemaVersion(false);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31417");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any DBInfo row is removed, this code needs to be modified so that it does not treat
// the removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CIDShieldProductDBMgr::raw_GetDBInfoRows(IVariantVector** ppDBInfoRows)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipDBInfoRows(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI31418", ipDBInfoRows != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31414");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any table is removed, this code needs to be modified so that it does not treat the
// removed element(s) on and old schema versions as unrecognized.
STDMETHODIMP CIDShieldProductDBMgr::raw_GetTables(IVariantVector** ppTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipTables(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI31419", ipTables != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31420");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_UpdateSchemaForFAMDBVersion(IFileProcessingDB* pDB,
	_Connection* pConnection, long nFAMDBSchemaVersion, long* pnProdSchemaVersion, long* pnNumSteps,
	IProgressStatus* pProgressStatus)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_ARGUMENT("ELI31409", ipDB != __nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_ARGUMENT("ELI31410", ipConnection != __nullptr);

		ASSERT_ARGUMENT("ELI31411", pnProdSchemaVersion != __nullptr);

		// If the schema version is not specified, use the current schema version as the starting
		// point.
		if (*pnProdSchemaVersion == 0)
		{
			string strVersion = asString(
				ipDB->GetDBInfoSetting(gstrID_SHIELD_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));

			// If the IDShield specific components are missing, there is nothing to do.
			if (strVersion.empty())
			{
				return S_OK;
			}

			*pnProdSchemaVersion = asLong(strVersion);
		}

		switch (*pnProdSchemaVersion)
		{
			case 2:	// The schema update from 2 to 3 needs to take place against FAM DB schema version 102
					if (nFAMDBSchemaVersion == 102)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion3(ipConnection, pnNumSteps, NULL);
					}
			case 3:	// The schema update from 3 to 4 needs to take place against FAM DB schema version 110
					if (nFAMDBSchemaVersion == 110)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion4(ipConnection, pnNumSteps, NULL);
					}
			case 4: break;

			default:
				{
					UCLIDException ue("ELI31412",
						"Automatic updates are not supported for the current schema.");
					ue.addDebugInfo("FAM Schema Version", nFAMDBSchemaVersion, false);
					ue.addDebugInfo("ID Shield Schema Version", *pnProdSchemaVersion, false);
					throw ue;
				}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31413");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::AddIDShieldData(long lFileID, VARIANT_BOOL vbVerified, 
		double dDuration, double dOverheadTime, long lNumHCDataFound, long lNumMCDataFound,
		long lNumLCDataFound, long lNumCluesDataFound, long lTotalRedactions,
		long lTotalManualRedactions, long lNumPagesAutoAdvanced)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		if (!AddIDShieldData_Internal(false, lFileID, vbVerified, dDuration, dOverheadTime,
			lNumHCDataFound, lNumMCDataFound, lNumLCDataFound, lNumCluesDataFound, lTotalRedactions, 
			lTotalManualRedactions, lNumPagesAutoAdvanced))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(m_ipFAMDB, gstrMAIN_DB_LOCK);
			
			AddIDShieldData_Internal(true, lFileID, vbVerified, dDuration, dOverheadTime,
				lNumHCDataFound, lNumMCDataFound, lNumLCDataFound, lNumCluesDataFound,
				lTotalRedactions, lTotalManualRedactions, lNumPagesAutoAdvanced);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19037");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::put_FAMDB(IFileProcessingDB* newVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI19810", newVal != __nullptr);

		// Only update if it is a new value
		if (m_ipFAMDB != newVal)
		{
			m_ipFAMDB = newVal;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = __nullptr;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19039");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::GetResultsForQuery(BSTR bstrQuery, _Recordset** ppVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI19882", ppVal != __nullptr);

		validateLicense();

		if (!GetResultsForQuery_Internal(false, bstrQuery, ppVal))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(m_ipFAMDB, gstrMAIN_DB_LOCK);

			GetResultsForQuery_Internal(true, bstrQuery, ppVal);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19530");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::GetFileID(BSTR bstrFileName, long* plFileID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// ensure plFileID is non-NULL
		ASSERT_ARGUMENT("ELI20178", plFileID != __nullptr);

		// validate the license
		validateLicense();

		if (!GetFileID_Internal(false, bstrFileName, plFileID))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(m_ipFAMDB, gstrMAIN_DB_LOCK);

			GetFileID_Internal(true, bstrFileName, plFileID);
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20179");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
ADODB::_ConnectionPtr CIDShieldProductDBMgr::getDBConnection()
{
	// Check if connection has been created
	if (m_ipDBConnection == __nullptr)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI19795", m_ipDBConnection != __nullptr);
	}

	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == __nullptr)
	{
		UCLIDException ue("ELI18935", "FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
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
			m_ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

			// Get the command timeout from the FAMDB DBInfo table
			string strValue = asString(
				m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str(), VARIANT_TRUE));

			// Set the command timeout
			m_ipDBConnection->CommandTimeout = asLong(strValue);

			// Get the setting for storeing IDShield processing history
			strValue = asString(m_ipFAMDB->GetDBInfoSetting(
				gstrSTORE_IDSHIELD_PROCESSING_HISTORY.c_str(), VARIANT_TRUE));

			// Set the local setting for storing history
			m_bStoreIDShieldProcessingHistory = strValue == asString(TRUE);
		}
	}
	
	return m_ipDBConnection;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldProductDBMgr::validateLicense()
{
	VALIDATE_LICENSE( gnIDSHIELD_CORE_OBJECTS, "ELI18688", "ID Shield DB Manager" );
}
//-------------------------------------------------------------------------------------------------
void CIDShieldProductDBMgr::getIDShieldTables(vector<string>& rvecTables)
{
	rvecTables.clear();
	rvecTables.push_back(gstrIDSHIELD_DATA);
}
//-------------------------------------------------------------------------------------------------
void CIDShieldProductDBMgr::validateIDShieldSchemaVersion(bool bThrowIfMissing)
{
	ASSERT_RESOURCE_ALLOCATION("ELI19818", m_ipFAMDB != __nullptr);

	// Get the Version from the FAMDB DBInfo table
	string strValue = asString(m_ipFAMDB->GetDBInfoSetting(
		gstrID_SHIELD_SCHEMA_VERSION_NAME.c_str(), asVariantBool(bThrowIfMissing)));

	if (bThrowIfMissing || !strValue.empty())
	{
		// Check against expected version
		if (asLong(strValue) != glIDShieldDBSchemaVersion)
		{
			UCLIDException ue("ELI19798", "ID Shield database schema is not current version!");
			ue.addDebugInfo("Expected", glIDShieldDBSchemaVersion);
			ue.addDebugInfo("Database Version", strValue);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldProductDBMgr::AddIDShieldData_Internal(bool bDBLocked, long lFileID,
		VARIANT_BOOL vbVerified, double dDuration, double dOverheadTime, long lNumHCDataFound,
		long lNumMCDataFound, long lNumLCDataFound, long lNumCluesDataFound, long lTotalRedactions,
		long lTotalManualRedactions, long lNumPagesAutoAdvanced)
{
	try
	{
		try
		{
			ASSERT_RESOURCE_ALLOCATION("ELI19096", m_ipFAMDB != __nullptr); 

			// Validate IDShield Schema
			validateIDShieldSchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			long nUserID = getKeyID(ipConnection, "FAMUser", "UserName", getCurrentUserName());
			long nMachineID = getKeyID(ipConnection, "Machine", "MachineName", getComputerName());

			// Get the file ID as a string
			string strFileId = asString(lFileID);
			string strVerified = vbVerified == VARIANT_TRUE ? "1" : "0";

			// Create a pointer to a recordset
			_RecordsetPtr ipSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI28069", ipSet != __nullptr );

			// Build insert SQL query 
			string strInsertSQL = gstrINSERT_IDSHIELD_DATA_RCD + "(" + strFileId
				+ ", " + strVerified + ", " + asString(nUserID) + ", "
				+ asString(nMachineID) + ", GETDATE(), " + asString(dDuration)
				+ ", " + asString(dOverheadTime) + ", " + asString(lNumHCDataFound) + ", "
				+ asString(lNumMCDataFound) + ", " + asString(lNumLCDataFound) + ", "
				+ asString(lNumCluesDataFound) + ", " + asString(lTotalRedactions) + ", "
				+ asString(lTotalManualRedactions) + ", "
				+ asString(lNumPagesAutoAdvanced) + ")";

			// Create a transaction guard
			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			// If not storing previous history need to delete it
			if (!m_bStoreIDShieldProcessingHistory)
			{
				string strDeleteQuery = gstrDELETE_PREVIOUS_STATUS_FOR_FILEID;
				replaceVariable(strDeleteQuery, "<FileID>", strFileId);
				replaceVariable(strDeleteQuery, "<Verified>", strVerified);

				// Delete previous records with the fileID
				executeCmdQuery(ipConnection, strDeleteQuery);
			}

			// Insert the record
			executeCmdQuery(ipConnection, strInsertSQL);

			// Commit the transactions
			tg.CommitTrans();

			END_ADO_CONNECTION_RETRY(
				ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29858");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30710");
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
bool CIDShieldProductDBMgr::GetResultsForQuery_Internal(bool bDBLocked, BSTR bstrQuery,
	_Recordset** ppVal)
{
	try
	{
		try
		{
			// validate IDShield schema
			validateIDShieldSchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			// Create a pointer to a recordset
			_RecordsetPtr ipResultSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI19531", ipResultSet != __nullptr );

			// Open the Action table
			ipResultSet->Open( bstrQuery, _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
				adLockReadOnly, adCmdText );

			*ppVal = ipResultSet.Detach();

			END_ADO_CONNECTION_RETRY(
				ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29859");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34112");
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
bool CIDShieldProductDBMgr::GetFileID_Internal(bool bDBLocked, BSTR bstrFileName, long* plFileID)
{
	try
	{
		try
		{
			// validate IDShield schema
			validateIDShieldSchemaVersion(true);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			// query the database for the file ID
			*plFileID = getKeyID(ipConnection, "FAMFile", "FileName", asString(bstrFileName), 
				false);

			END_ADO_CONNECTION_RETRY(
				ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29860");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34113");
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
const vector<string> CIDShieldProductDBMgr::getTableCreationQueries()
{
	vector<string> vecQueries;

	// WARNING: If any table is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	vecQueries.push_back(gstrCREATE_IDSHIELD_DATA);

	return vecQueries;
}
//-------------------------------------------------------------------------------------------------
map<string, string> CIDShieldProductDBMgr::getDBInfoDefaultValues()
{
	map<string, string> mapDefaultValues;

	// WARNING: If any DBInfo row is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	mapDefaultValues[gstrID_SHIELD_SCHEMA_VERSION_NAME] = asString(glIDShieldDBSchemaVersion);
	mapDefaultValues[gstrSTORE_IDSHIELD_PROCESSING_HISTORY] = gstrSTORE_HISTORY_DEFAULT_SETTING;

	return mapDefaultValues;
}
//-------------------------------------------------------------------------------------------------

