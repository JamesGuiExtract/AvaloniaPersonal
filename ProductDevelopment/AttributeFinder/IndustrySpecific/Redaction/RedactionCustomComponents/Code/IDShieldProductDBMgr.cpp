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
const long glIDShieldDBSchemaVersion = 6;
const string gstrID_SHIELD_SCHEMA_VERSION_NAME = "IDShieldSchemaVersion";
// https://extract.atlassian.net/browse/ISSUE-13239
// StoreIDShieldProcessingHistory has been removed (const exists only for
// findUnrecognizedSchemaElements)
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
int UpdateToSchemaVersion5(_ConnectionPtr ipConnection, long* pnNumSteps, 
	IProgressStatusPtr ipProgressStatus)
{
	try
	{
		int nNewSchemaVersion = 5;

		if (pnNumSteps != __nullptr)
		{
			long nSteps = 0;
			executeCmdQuery(ipConnection, 
				"SELECT COUNT(*) AS [ID] FROM [IDShieldData]", false, &nSteps);

			nSteps /= 100;
			nSteps += 3;
			*pnNumSteps += nSteps;
			return nNewSchemaVersion;
		}

		vector<string> vecQueries;

		vecQueries.push_back("ALTER TABLE [IDShieldData] ADD [FileTaskSessionID] [int] NULL");
		vecQueries.push_back(gstrADD_IDSHIELDDATA_FILETASKSESSION_FK);
		vecQueries.push_back(gstrPORT_IDSHEIELDDATA_TO_FILETASKSESSION);
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP CONSTRAINT [FK_IDShieldData_FAMFile]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP CONSTRAINT [FK_IDShieldData_FAMUser]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP CONSTRAINT [FK_IDShieldData_Machine]");
		vecQueries.push_back("DROP INDEX [IDShieldData].[IX_FileID_DateTimeStamp]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [FileID]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [Verified]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [UserID]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [MachineID]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [DateTimeStamp]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [Duration]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] DROP COLUMN [OverheadTime]");
		vecQueries.push_back("ALTER TABLE [IDShieldData] ALTER COLUMN [FileTaskSessionID] [int] NOT NULL");
		vecQueries.push_back(gstrCREATE_IDSHIELDDATA_FILETASKSESSION_INDEX);

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '5' WHERE [Name] = '" + 
			gstrID_SHIELD_SCHEMA_VERSION_NAME + "'");

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38601");
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

		vecQueries.push_back("DELETE FROM [DBInfo] WHERE [Name] = '" +
			gstrSTORE_IDSHIELD_PROCESSING_HISTORY + "'");

		vecQueries.push_back("UPDATE [DBInfo] SET [Value] = '6' WHERE [Name] = '" + 
			gstrID_SHIELD_SCHEMA_VERSION_NAME + "'");

		// This corrects a problem where the TaskClass values may not be in the database
		// https://extract.atlassian.net/browse/ISSUE-13341
		string strInsertVerifyTaskClassIfNeeded = 
			"IF NOT EXISTS (SELECT ID FROM TaskClass WHERE GUID = 'AD7F3F3F-20EC-4830-B014-EC118F6D4567') " +
			gstrINSERT_REDACTION_VERIFY_TASK_CLASS;
		string strInsertCreateRedactedTaskClassIfNeeded = 
			"IF NOT EXISTS (SELECT ID FROM TaskClass WHERE GUID = '36D14C41-CE3D-4950-AC47-2664563340B1') " +
			gstrINSERT_CREATE_REDACTED_IMAGE_TASK_CLASS;
		vecQueries.push_back(strInsertVerifyTaskClassIfNeeded);
		vecQueries.push_back(strInsertCreateRedactedTaskClassIfNeeded);

		executeVectorOfSQL(ipConnection, vecQueries);

		return nNewSchemaVersion;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38665");
}

//-------------------------------------------------------------------------------------------------
// CIDShieldProductDBMgr
//-------------------------------------------------------------------------------------------------
CIDShieldProductDBMgr::CIDShieldProductDBMgr()
: m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
, m_currentRole(CppBaseApplicationRoleConnection::kExtractRole)
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
STDMETHODIMP CIDShieldProductDBMgr::raw_AddProductSpecificSchema(_Connection* pConnection, 
																 IFileProcessingDB *pDB,
																 VARIANT_BOOL bOnlyTables,
																 VARIANT_BOOL bAddUserTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18823", ipDB != __nullptr);

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection(pConnection);
		ASSERT_RESOURCE_ALLOCATION("ELI18824", ipDBConnection != __nullptr);

		// Retrieve the queries for creating IDShield DB table(s).
		const vector<string> vecTableCreationQueries = getTableCreationQueries();
		vector<string> vecCreateQueries(vecTableCreationQueries.begin(), vecTableCreationQueries.end());

		// Add queries for creating indexes & constraints
		vecCreateQueries.push_back(gstrADD_IDSHIELDDATA_FILETASKSESSION_FK);
		vecCreateQueries.push_back(gstrCREATE_IDSHIELDDATA_FILETASKSESSION_INDEX);
		vecCreateQueries.push_back(gstrINSERT_REDACTION_VERIFY_TASK_CLASS);
		vecCreateQueries.push_back(gstrINSERT_CREATE_REDACTED_IMAGE_TASK_CLASS);

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
				iterDBInfoValues->second.c_str(), vbSetIfExists, VARIANT_FALSE);
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
STDMETHODIMP CIDShieldProductDBMgr::raw_RemoveProductSpecificSchema(_Connection* pConnection,
																	IFileProcessingDB *pDB,
																	VARIANT_BOOL bOnlyTables,
																	VARIANT_BOOL bRetainUserTables,
																	VARIANT_BOOL *pbSchemaExists)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI38281", pbSchemaExists != __nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_RESOURCE_ALLOCATION("ELI53067", ipConnection != __nullptr);

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI18956", ipDB != __nullptr);

		m_ipFAMDB = ipDB;

		string strValue = asString(ipDB->GetDBInfoSetting(
			gstrID_SHIELD_SCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE));

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
		getIDShieldTables(vecTables);

		dropTablesInVector(ipConnection, vecTables);
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

		validateIDShieldSchemaVersion(true);

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

		// https://extract.atlassian.net/browse/ISSUE-13239
		// Ability to turn off record session history for ID Shield has been removed.
		ipDBInfoRows->PushBack(gstrSTORE_IDSHIELD_PROCESSING_HISTORY.c_str());

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
				// if FAMDBSchemaVersion is 184 all product specific schemas should exist so add the product schema
				if (nFAMDBSchemaVersion == 184)
				{
					if (pnNumSteps != __nullptr)
					{
						*pnNumSteps = 2;
						*pnProdSchemaVersion = 0;
						strVersion = "0";
					}
					else
					{
						IProductSpecificDBMgrPtr ipThis(this);
						ipThis->AddProductSpecificSchema(ipConnection, ipDB, VARIANT_FALSE, VARIANT_TRUE);

						*pnProdSchemaVersion= glIDShieldDBSchemaVersion;
					}
				}
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
					break;

			case 3:	// The schema update from 3 to 4 needs to take place against FAM DB schema version 110
					if (nFAMDBSchemaVersion == 110)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion4(ipConnection, pnNumSteps, NULL);
					}
					break;

			case 4: if (nFAMDBSchemaVersion == 129)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion5(ipConnection, pnNumSteps, NULL);
					}
					// Break is intentionally missing as schema updates 5 and 6 both correspond with
					// nFAMDBSchemaVersion 129

			case 5: if (nFAMDBSchemaVersion == 129)
					{
						*pnProdSchemaVersion = UpdateToSchemaVersion6(ipConnection, pnNumSteps, NULL);
					}

			case 6: break;

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
STDMETHODIMP CIDShieldProductDBMgr::AddIDShieldData(long nFileTaskSessionID,
		double dDuration, double dOverheadTime, double dActivityTime, long lNumHCDataFound, 
		long lNumMCDataFound, long lNumLCDataFound, long lNumCluesDataFound, long lTotalRedactions,
		long lTotalManualRedactions, long lNumPagesAutoAdvanced, VARIANT_BOOL sessionTimedOut)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// EndFileTaskSession has it's own optimistic locking, no need to do so here.
		m_ipFAMDB->EndFileTaskSession(nFileTaskSessionID, dOverheadTime, dActivityTime, sessionTimedOut);

		if (!AddIDShieldData_Internal(false, nFileTaskSessionID, lNumHCDataFound, lNumMCDataFound,
			lNumLCDataFound, lNumCluesDataFound, lTotalRedactions, lTotalManualRedactions,
			lNumPagesAutoAdvanced))
		{
			// Lock the database
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dblg(m_ipFAMDB, gstrMAIN_DB_LOCK);
			
			AddIDShieldData_Internal(true, nFileTaskSessionID, lNumHCDataFound, lNumMCDataFound,
				lNumLCDataFound, lNumCluesDataFound, lTotalRedactions, lTotalManualRedactions,
				lNumPagesAutoAdvanced);
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19037");
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
STDMETHODIMP CIDShieldProductDBMgr::Initialize(IFileProcessingDB* pFAMDB)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// validate the license
		validateLicense();
		
		ASSERT_ARGUMENT("ELI38615", pFAMDB != nullptr);

		m_ipFAMDB = pFAMDB;
		ASSERT_RESOURCE_ALLOCATION("ELI38616", m_ipFAMDB != nullptr);

		m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38603");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
shared_ptr<CppBaseApplicationRoleConnection> CIDShieldProductDBMgr::getAppRoleConnection(bool bReset)
{
	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == __nullptr)
	{
		UCLIDException ue("ELI18935", "FAMDB pointer has not been initialized! Unable to open connection.");
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
	ASSERT_RESOURCE_ALLOCATION("ELI51857", adoConnection != __nullptr);	

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
void CIDShieldProductDBMgr::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18688", "ID Shield DB Manager" );
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
bool CIDShieldProductDBMgr::AddIDShieldData_Internal(bool bDBLocked, long nFileTaskSessionID,
		long lNumHCDataFound, long lNumMCDataFound, long lNumLCDataFound, long lNumCluesDataFound,
		long lTotalRedactions, long lTotalManualRedactions, long lNumPagesAutoAdvanced)
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

			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Create a pointer to a recordset
			_RecordsetPtr ipSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI28069", ipSet != __nullptr );

			// Query to add the corresponding IDShieldData record.
			string strInsertIDSD_SQL = gstrINSERT_IDSHIELD_DATA_RCD;
			replaceVariable(strInsertIDSD_SQL, "<FileTaskSessionID>", asString(nFileTaskSessionID));
			replaceVariable(strInsertIDSD_SQL, "<NumHCDataFound>", asString(lNumHCDataFound));
			replaceVariable(strInsertIDSD_SQL, "<NumMCDataFound>", asString(lNumMCDataFound));
			replaceVariable(strInsertIDSD_SQL, "<NumLCDataFound>", asString(lNumLCDataFound));
			replaceVariable(strInsertIDSD_SQL, "<NumCluesFound>", asString(lNumCluesDataFound));
			replaceVariable(strInsertIDSD_SQL, "<TotalRedactions>", asString(lTotalRedactions));
			replaceVariable(strInsertIDSD_SQL, "<TotalManualRedactions>", asString(lTotalManualRedactions));
			replaceVariable(strInsertIDSD_SQL, "<NumPagesAutoAdvanced>", asString(lNumPagesAutoAdvanced));

			// Create a transaction guard
			TransactionGuard tg(ipConnection, adXactChaos, __nullptr);

			// Create the corresponding IDShieldData record.
			executeCmdQuery(ipConnection, strInsertIDSD_SQL);

			// Commit the transactions
			tg.CommitTrans();

			END_ADO_CONNECTION_RETRY(
				ipConnection, getAppRoleConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29858");
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
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// Create a pointer to a recordset
			_RecordsetPtr ipResultSet( __uuidof( Recordset ));
			ASSERT_RESOURCE_ALLOCATION("ELI19531", ipResultSet != __nullptr );

			// Open the Action table
			ipResultSet->Open( bstrQuery, _variant_t((IDispatch *)ipConnection, true), adOpenStatic,
				adLockReadOnly, adCmdText );

			*ppVal = ipResultSet.Detach();

			END_ADO_CONNECTION_RETRY(
				ipConnection, getAppRoleConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29859");
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
			auto role = getAppRoleConnection();
			ipConnection = role->ADOConnection();

			// query the database for the file ID
			*plFileID = getKeyID(ipConnection, "FAMFile", "FileName", asString(bstrFileName), 
				false);

			END_ADO_CONNECTION_RETRY(
				ipConnection, getAppRoleConnection, m_nNumberOfRetries, m_dRetryTimeout, "ELI29860");
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
	vecQueries.push_back(gstrCREATE_IDSHIELD_DATA_V5);

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

	return mapDefaultValues;
}
//-------------------------------------------------------------------------------------------------

