// IDShieldProductDBMgr.cpp : Implementation of CIDShieldProductDBMgr

#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "IDShieldProductDBMgr.h"
#include "IDShield_DB_SQL.h"

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
const long glIDShieldDBSchemaVersion = 3;
const string gstrID_SHIELD_SCHEMA_VERSION_NAME = "IDShieldSchemaVersion";
static const string gstrSTORE_IDSHIELD_PROCESSING_HISTORY = "StoreIDShieldProcessingHistory";
static const string gstrSTORE_HISTORY_DEFAULT_SETTING = "1"; // TRUE

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

		ASSERT_ARGUMENT("ELI18689", pstrComponentDescription != NULL);

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

		ASSERT_ARGUMENT("ELI19817", pbValue != NULL);

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
STDMETHODIMP CIDShieldProductDBMgr::raw_AddProductSpecificSchema(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18823", ipDB != NULL);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI18824", ipDBConnection != NULL);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);

		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		// Create the vector of Queries to execute
		vector<string> vecCreateQueries;

		vecCreateQueries.push_back(gstrCREATE_IDSHIELD_DATA);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELD_FAMFILE);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMFILE);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_FAMUSER);
		vecCreateQueries.push_back(gstrADD_FK_IDSHIELDDATA_MACHINE);
		vecCreateQueries.push_back(gstrADD_CHECK_FK_IDSHIELDDATA_MACHINE);
		vecCreateQueries.push_back(gstrCREATE_FILEID_DATETIMESTAMP_INDEX);

		// Execute the queries to create the id shield table
		executeVectorOfSQL(ipDBConnection, vecCreateQueries);

		// Set the schema version
		ipDB->SetDBInfoSetting(gstrID_SHIELD_SCHEMA_VERSION_NAME.c_str(), 
			asString(glIDShieldDBSchemaVersion).c_str(), VARIANT_TRUE);

		// Set the default
		ipDB->SetDBInfoSetting(gstrSTORE_IDSHIELD_PROCESSING_HISTORY.c_str(), 
			gstrSTORE_HISTORY_DEFAULT_SETTING.c_str(), VARIANT_FALSE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18686");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::raw_RemoveProductSpecificSchema(IFileProcessingDB *pDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18956", ipDB != NULL);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI18957", ipDBConnection != NULL);
		
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
STDMETHODIMP CIDShieldProductDBMgr::AddIDShieldData(long lFileID, VARIANT_BOOL vbVerified, 
		double lDuration, long lNumHCDataFound, long lNumMCDataFound, long lNumLCDataFound, 
		long lNumCluesDataFound, long lTotalRedactions, long lTotalManualRedactions,
		long lNumPagesAutoAdvanced)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		if (!AddIDShieldData_Internal(false, lFileID, vbVerified, lDuration, lNumHCDataFound, 
			lNumMCDataFound,	lNumLCDataFound, lNumCluesDataFound, lTotalRedactions, 
			lTotalManualRedactions, lNumPagesAutoAdvanced))
		{
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr ipThis;
			ipThis = this;
			ASSERT_RESOURCE_ALLOCATION("ELI30712", ipThis != NULL);
			
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(ipThis);
			
			AddIDShieldData_Internal(true, lFileID, vbVerified, lDuration, lNumHCDataFound, 
				lNumMCDataFound, lNumLCDataFound, lNumCluesDataFound, lTotalRedactions, 
				lTotalManualRedactions, lNumPagesAutoAdvanced);
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

		ASSERT_ARGUMENT("ELI19810", newVal != NULL);

		// Only update if it is a new value
		if (m_ipFAMDB != newVal)
		{
			m_ipFAMDB = newVal;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = NULL;
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

		ASSERT_ARGUMENT("ELI19882", ppVal != NULL);

		validateLicense();

		// validate IDShield schema
		validateIDShieldSchemaVersion();

		// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
		_ConnectionPtr ipConnection = NULL;

		BEGIN_ADO_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// Create a pointer to a recordset
		_RecordsetPtr ipResultSet( __uuidof( Recordset ));
		ASSERT_RESOURCE_ALLOCATION("ELI19531", ipResultSet != NULL );

		// Open the Action table
		ipResultSet->Open( bstrQuery, _variant_t((IDispatch *)ipConnection, true), adOpenStatic, 
			adLockReadOnly, adCmdText );

		*ppVal = ipResultSet.Detach();

		END_ADO_CONNECTION_RETRY(
			ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29859");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19530");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldProductDBMgr::GetFileID(BSTR bstrFileName, long* plFileID)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// ensure plFileID is non-NULL
		ASSERT_ARGUMENT("ELI20178", plFileID != NULL);

		// validate the license
		validateLicense();

		// validate IDShield schema
		validateIDShieldSchemaVersion();

		// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
		_ConnectionPtr ipConnection = NULL;

		BEGIN_ADO_CONNECTION_RETRY();

		// Get the connection for the thread and save it locally.
		ipConnection = getDBConnection();

		// query the database for the file ID
		*plFileID = getKeyID(ipConnection, "FAMFile", "FileName", asString(bstrFileName), 
			false);

		END_ADO_CONNECTION_RETRY(
			ipConnection, getDBConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29860");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20179");

	return S_OK;

}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
ADODB::_ConnectionPtr CIDShieldProductDBMgr::getDBConnection()
{
	// Check if connection has been created
	if (m_ipDBConnection == NULL)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI19795", m_ipDBConnection != NULL);
	}

	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == NULL)
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
			string strValue = asString(m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str()));

			// Set the command timeout
			m_ipDBConnection->CommandTimeout = asLong(strValue);

			// Get the setting for storeing IDShield processing history
			strValue = asString(m_ipFAMDB->GetDBInfoSetting(
				gstrSTORE_IDSHIELD_PROCESSING_HISTORY.c_str()));

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
void CIDShieldProductDBMgr::validateIDShieldSchemaVersion()
{
	ASSERT_RESOURCE_ALLOCATION("ELI19818", m_ipFAMDB != NULL);

	// Get the Version from the FAMDB DBInfo table
	string strValue = asString(m_ipFAMDB->GetDBInfoSetting(gstrID_SHIELD_SCHEMA_VERSION_NAME.c_str()));

	// Check against expected version
	if (asLong(strValue) != glIDShieldDBSchemaVersion)
	{
		UCLIDException ue("ELI19798", "ID Shield database schema is not current version!");
		ue.addDebugInfo("Expected", glIDShieldDBSchemaVersion);
		ue.addDebugInfo("Database Version", strValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldProductDBMgr::AddIDShieldData_Internal(bool bDBLocked, long lFileID, VARIANT_BOOL vbVerified, 
		double lDuration, long lNumHCDataFound, long lNumMCDataFound, long lNumLCDataFound, 
		long lNumCluesDataFound, long lTotalRedactions, long lTotalManualRedactions,
		long lNumPagesAutoAdvanced)
{
	try
	{
		try
		{
			ASSERT_RESOURCE_ALLOCATION("ELI19096", m_ipFAMDB != NULL); 

			// Validate IDShield Schema
			validateIDShieldSchemaVersion();

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = NULL;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			long nUserID = getKeyID(ipConnection, "FAMUser", "UserName", getCurrentUserName());
			long nMachineID = getKeyID(ipConnection, "Machine", "MachineName", getComputerName());

			// Get the file ID as a string
			string strFileId = asString(lFileID);
			string strVerified = vbVerified == VARIANT_TRUE ? "1" : "0";

			// -------------------------------------------
			// Need to get the current TotalDuration value
			// -------------------------------------------
			double dTotalDuration = lDuration;
			string strSql = "SELECT TOP 1 [TotalDuration] FROM IDShieldData WHERE [FileID] = "
				+ strFileId + " AND [Verified] = " + strVerified + " ORDER BY [ID] DESC";

			// Create a pointer to a recordset
			_RecordsetPtr ipSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI28069", ipSet != NULL );

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
			string strInsertSQL = gstrINSERT_IDSHIELD_DATA_RCD + "(" + strFileId
				+ ", " + strVerified + ", " + asString(nUserID) + ", "
				+ asString(nMachineID) + ", GETDATE(), " + asString(lDuration)
				+ ", " + asString(dTotalDuration) + ", " + asString(lNumHCDataFound) + ", "
				+ asString(lNumMCDataFound) + ", " + asString(lNumLCDataFound) + ", "
				+ asString(lNumCluesDataFound) + ", " + asString(lTotalRedactions) + ", "
				+ asString(lTotalManualRedactions) + ", "
				+ asString(lNumPagesAutoAdvanced) + ")";

			// Create a transaction guard
			TransactionGuard tg(ipConnection);

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
