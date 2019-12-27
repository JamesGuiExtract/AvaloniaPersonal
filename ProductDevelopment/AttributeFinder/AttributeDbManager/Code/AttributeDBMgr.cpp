#include "stdafx.h"
#include "AttributeDBMgr.h"
#include "Attribute_DB_SQL.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <LockGuard.h>
#include <FAMUtilsConstants.h>
#include <TransactionGuard.h>
#include <ADOUtils.h>
#include <FAMDBHelperFunctions.h>
#include <cpputil.h>
#include <zlib.h>

#include <atlsafe.h>
#include "DefinedTypes.h"

using namespace ADODB;

namespace ZipUtil
{
	SAFEARRAY* DecompressAttributes( SAFEARRAY* pSA );
	SAFEARRAY* CompressAttributes( const IPersistStreamPtr& ipStream );
}

namespace
{
	//-------------------------------------------------------------------------------------------------
	// Constants
	//-------------------------------------------------------------------------------------------------

	// This must be updated when the DB schema changes
	// !!!ATTENTION!!!
	// An UpdateToSchemaVersion method must be added when checking in a new schema version.
	// WARNING -- When the version is changed, the corresponding switch handler needs to be updated, see WARNING!!!
	const string gstrSCHEMA_VERSION_NAME = "AttributeCollectionSchemaVersion";
	const string gstrDESCRIPTION = "Attribute database manager";
	const long glSCHEMA_VERSION = 10;
	const long dbSchemaVersionWhenAttributeCollectionWasIntroduced = 129;


	VectorOfString GetCurrentTableNames(bool excludeUserTables = false)
	{
		VectorOfString names;

		if (!excludeUserTables)
		{
			names.push_back(gstrATTRIBUTE_SET_NAME);
		}

		names.push_back(gstrATTRIBUTE_SET_FOR_FILE);
		names.push_back(gstrATTRIBUTE_NAME);
		names.push_back(gstrATTRIBUTE_TYPE);
		names.push_back(gstrATTRIBUTE_INSTANCE_TYPE);
		names.push_back(gstrATTRIBUTE);
		names.push_back(gstrRASTER_ZONE);
		names.push_back(gstrREPORTING_REDACTION_ACCURACY_TABLE);
		names.push_back(gstrREPORTING_DATA_CAPTURE_ACCURACY_TABLE);
		names.push_back(gstrDASHBOARD_ATTRIBUTE_FIELDS_TABLE);
		names.push_back(gstrREPORTING_HIM_STATS_TABLE);

		return names;
	}

	VectorOfString GetTables_v1(bool bAddUserTables)
	{
		VectorOfString tables;

		if (bAddUserTables)
		{
			tables.push_back(gstrCREATE_ATTRIBUTE_SET_NAME_TABLE_v1);
		}

		tables.push_back(gstrCREATE_ATTRIBUTE_SET_FOR_FILE_TABLE_v1);
		tables.push_back(gstrCREATE_ATTRIBUTE_NAME_TABLE_v1);
		tables.push_back(gstrCREATE_ATTRIBUTE_TYPE_TABLE_v1);
		tables.push_back(gstrCREATE_ATTRIBUTE_INSTANCE_TYPE_v1);
		tables.push_back(gstrCREATE_ATTRIBUTE_TABLE_v1);
		tables.push_back(gstrCREATE_RASTER_ZONE_TABLE_v1);

		return tables;
	}

	VectorOfString GetIndexes_v1()
	{
		VectorOfString queries;

		queries.push_back(gstrCREATE_FILEID_ATTRIBUTE_SET_NAME_ID_INDEX);
		return queries;
	}



	VectorOfString GetForeignKeys_v1()
	{
		VectorOfString fKeys;

		fKeys.push_back(gstrADD_ATTRIBUTE_SET_FOR_FILE_FILETASKSESSIONID_FK);
		fKeys.push_back(gstrADD_ATTRIBUTE_SET_FOR_FILE_ATTRIBUTESETNAMEID_FK);
		fKeys.push_back(gstrADD_ATTRIBUTE_INSTANCE_TYPE_ATTRIBUTEID);
		fKeys.push_back(gstrADD_ATTRIBUTE_INSTANCE_TYPE_ATTRIBUTETYPEID);
		fKeys.push_back(gstrADD_ATTRIBUTE_ATTRIBUTE_SET_FILE_FILEID_FK);
		fKeys.push_back(gstrADD_ATTRIBUTE_ATTRIBUTE_NAMEID_FK);
		fKeys.push_back(gstrADD_ATTRIBUTE_PARENT_ATTRIBUTEID_FK);
		fKeys.push_back(gstrADD_RASTER_ZONE_ATTRIBUTEID_FK);

		return fKeys;
	}


	std::string GetVersionInsertStatement(long schemaVersion)
	{
		return "INSERT INTO [DBInfo] ([Name], [Value]) VALUES ('" +
			gstrSCHEMA_VERSION_NAME + "', '" + asString(schemaVersion) + "' )";
	}

	std::string GetVersionUpdateStatement(long schemaVersion)
	{
		char buffer[255];
		_snprintf_s(buffer,
			sizeof(buffer),
			sizeof(buffer) - 1,
			"UPDATE [DBInfo] SET Value='%d' where Name='AttributeCollectionSchemaVersion';",
			schemaVersion);

		return buffer;
	}

	template <typename T>
	void AppendToVector(T& dest, const T& source)
	{
		dest.insert(dest.end(), source.begin(), source.end());
	}


	VectorOfString GetSchema_v1(bool bAddUserTables)
	{
		VectorOfString queries = GetTables_v1(bAddUserTables);
		AppendToVector(queries, GetIndexes_v1());
		AppendToVector(queries, GetForeignKeys_v1());

		return queries;
	}

	VectorOfString GetSchema_v2(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v1(bAddUserTables);
		queries.push_back(gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN);

		return queries;
	}

	VectorOfString GetSchema_v3(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v2(bAddUserTables);

		// Add FK_Workflow_OutputAttributeSet constraint that can't be added from IFileProcessingDB.
		if (bAddUserTables)
		{
			queries.push_back(gstrADD_WORKFLOW_OUTPUTATTRIBUTESET_FK);
		}

		return queries;
	}

	VectorOfString GetSchema_v4(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v3(bAddUserTables);
		queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_TABLE);
		queries.push_back(gstrADD_REPORTING_REDACTION_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK);
		queries.push_back(gstrADD_REPORTING_REDACTION_ATTRIBUTE_SET_FOR_FILE_FOUND_FK);
		queries.push_back(gstrADD_REPORTING_REDACTION_FAMFILE_FK);
		queries.push_back(gstrADD_REPORTING_REDACTION_DATABASE_SERVICE_FK);
		queries.push_back(gstrCREATE_REPORTING_REDACTION_FILEID_DATABASE_SERVICE_IX);

		queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_ACCURACY_TABLE);
		queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK);
		queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_ATTRIBUTE_SET_FOR_FILE_FOUND_FK);
		queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_FAMFILE_FK);
		queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_DATABASE_SERVICE_FK);
		queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_FILEID_DATABASE_SERVICE_IX);

		return queries;
	}

	VectorOfString GetSchema_v5(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v4(bAddUserTables);
		queries.push_back(gstrCREATE_DASHBOARD_ATTRIBUTE_FIELDS);
		queries.push_back(gstrADD_DASHBOARD_ATTRIBUTE_FIELDS_ATTRIBUTESETFORFILE_FK);
		queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_EXPECTED_FAMUSERID_WITH_INCLUDES);
		queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_FOUND_FAMUSERID_WITH_INCLUDES);
		queries.push_back(gstrCREATE_ATTRIBUTESETFORFILE_ATTRIBUTESETID_WITH_NAMEID_VALUE_IX);

		return queries;
	}


	VectorOfString GetSchema_v6(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v5(bAddUserTables);
		queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_EXPECTED_FAMUSERID_WITH_INCLUDES);
		queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_FOUND_FAMUSERID_WITH_INCLUDES);

		return queries;
	}

	VectorOfString GetSchema_v7(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v6(bAddUserTables);
		queries.push_back(gstrCREATE_REPORTING_HIM_STATS);
		queries.push_back(gstrCREATE_FAMUSERID_WITH_INCLUDES_INDEX);

		return queries;
	}

	VectorOfString GetSchema_v8(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v7(bAddUserTables);
		queries.push_back(gstrCREATE_DESTFILE_WITH_INCLUDES_INDEX);

		return queries;
	}

	// Change is to FK that is part of v3 so nothing to do here
	VectorOfString GetSchema_v9(bool bAddUserTables)
	{
		return GetSchema_v8(bAddUserTables);
	}

	VectorOfString GetSchema_v10(bool bAddUserTables)
	{
		VectorOfString queries = GetSchema_v9(bAddUserTables);
		queries.push_back(gstrCreate_ReportingDataCaptureAccuracy_FKS);
		queries.push_back(gstrAdd_FKS_REPORTINGHIMSTATS);
		queries.push_back(gstrAdd_FKS_REPORTINGREDACTIONACCURACY);

		return queries;
	}

	VectorOfString GetCurrentSchema(bool bAddUserTables = true)
	{
		return GetSchema_v10(bAddUserTables);
	}


	//-------------------------------------------------------------------------------------------------
	// Schema update functions
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion1(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 1;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 3;
				return nNewSchemaVersion;
			}

			vector<string> queries = GetSchema_v1(true);
			queries.emplace_back(GetVersionInsertStatement(nNewSchemaVersion));
			executeVectorOfSQL(ipConnection, queries);

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38511");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion2(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 2;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 3;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back(gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN);
			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			executeVectorOfSQL(ipConnection, queries);

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38888");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion3(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 3;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back(gstrADD_WORKFLOW_OUTPUTATTRIBUTESET_FK_V3);
			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			executeVectorOfSQL(ipConnection, queries);

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41922");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion4(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 4;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_TABLE_V4);
			queries.push_back(gstrADD_REPORTING_REDACTION_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK);
			queries.push_back(gstrADD_REPORTING_REDACTION_ATTRIBUTE_SET_FOR_FILE_FOUND_FK);
			queries.push_back(gstrADD_REPORTING_REDACTION_FAMFILE_FK);
			queries.push_back(gstrADD_REPORTING_REDACTION_DATABASE_SERVICE_FK);
			queries.push_back(gstrCREATE_REPORTING_REDACTION_FILEID_DATABASE_SERVICE_IX);

			queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_ACCURACY_TABLE_V4);
			queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_ACCURACY_ATTRIBUTE_SET_FOR_FILE_EXPECTED_FK);
			queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_ATTRIBUTE_SET_FOR_FILE_FOUND_FK);
			queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_FAMFILE_FK);
			queries.push_back(gstrADD_REPORTING_DATA_CAPTURE_DATABASE_SERVICE_FK);
			queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_FILEID_DATABASE_SERVICE_IX);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));

			executeVectorOfSQL(ipConnection, queries);

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45336");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion5(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 5;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 10;
				return nNewSchemaVersion;
			}

			vector<string> queries;

			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [FoundDateTimeStamp] [DATETIME] NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [FoundFAMUserID] INT  NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [FoundActionID] INT NULL");

			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [ExpectedDateTimeStamp] [DATETIME] NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [ExpectedFAMUserID] INT  NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingDataCaptureAccuracy] ADD [ExpectedActionID] INT NULL");

			queries.push_back(
				"UPDATE[dbo].[ReportingDataCaptureAccuracy] "
				"SET[FoundDateTimeStamp] = FoundFileSession.DateTimeStamp "
				", [FoundFAMUserID] = FoundFAMSession.FAMUserID "
				", [FoundActionID] = FoundFileSession.ActionID "
				", [ExpectedDateTimeStamp] = ExpectedFileSession.DateTimeStamp "
				", [ExpectedFAMUserID] = ExpectedFAMSession.FAMUserID "
				", [ExpectedActionID] = ExpectedFileSession.ActionID "
				"FROM[dbo].[ReportingDataCaptureAccuracy] "
				"INNER JOIN AttributeSetForFile FoundSet ON ReportingDataCaptureAccuracy.FoundAttributeSetForFileID = FoundSet.ID "
				"INNER JOIN FileTaskSession FoundFileSession ON FoundSet.FileTaskSessionID = FoundFileSession.ID "
				"INNER JOIN FAMSession FoundFAMSession ON FoundFileSession.FAMSessionID = FoundFAMSession.ID "
				"INNER JOIN AttributeSetForFile ExpectedSet ON ReportingDataCaptureAccuracy.ExpectedAttributeSetForFileID = ExpectedSet.ID "
				"INNER JOIN FileTaskSession ExpectedFileSession ON ExpectedSet.FileTaskSessionID = ExpectedFileSession.ID "
				"INNER JOIN FAMSession ExpectedFAMSession ON ExpectedFileSession.FAMSessionID = ExpectedFAMSession.ID "
			);

			queries.push_back("ALTER TABLE[dbo].[ReportingDataCaptureAccuracy] ALTER COLUMN [FoundDateTimeStamp] [DATETIME] NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingDataCaptureAccuracy] ALTER COLUMN [FoundFAMUserID] INT NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingDataCaptureAccuracy] ALTER COLUMN [ExpectedDateTimeStamp] [DATETIME] NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingDataCaptureAccuracy] ALTER COLUMN [ExpectedFAMUserID] INT NOT NULL");
			queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_EXPECTED_FAMUSERID_WITH_INCLUDES);
			queries.push_back(gstrCREATE_REPORTING_DATA_CAPTURE_FOUND_FAMUSERID_WITH_INCLUDES);
			queries.push_back(gstrCREATE_DASHBOARD_ATTRIBUTE_FIELDS);
			queries.push_back(gstrADD_DASHBOARD_ATTRIBUTE_FIELDS_ATTRIBUTESETFORFILE_FK);
			queries.push_back(gstrCREATE_ATTRIBUTESETFORFILE_ATTRIBUTESETID_WITH_NAMEID_VALUE_IX);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45972");
	}

	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion6(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 6;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 10;
				return nNewSchemaVersion;
			}

			vector<string> queries;

			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [FoundDateTimeStamp] [DATETIME] NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [FoundFAMUserID] INT  NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [FoundActionID] INT NULL");

			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [ExpectedDateTimeStamp] [DATETIME] NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [ExpectedFAMUserID] INT  NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingRedactionAccuracy] ADD [ExpectedActionID] INT NULL");

			queries.push_back(
				"UPDATE[dbo].[ReportingRedactionAccuracy] "
				"SET[FoundDateTimeStamp] = FoundFileSession.DateTimeStamp "
				", [FoundFAMUserID] = FoundFAMSession.FAMUserID "
				", [FoundActionID] = FoundFileSession.ActionID "
				", [ExpectedDateTimeStamp] = ExpectedFileSession.DateTimeStamp "
				", [ExpectedFAMUserID] = ExpectedFAMSession.FAMUserID "
				", [ExpectedActionID] = ExpectedFileSession.ActionID "
				"FROM[dbo].[ReportingRedactionAccuracy] "
				"INNER JOIN AttributeSetForFile FoundSet ON ReportingRedactionAccuracy.FoundAttributeSetForFileID = FoundSet.ID "
				"INNER JOIN FileTaskSession FoundFileSession ON FoundSet.FileTaskSessionID = FoundFileSession.ID "
				"INNER JOIN FAMSession FoundFAMSession ON FoundFileSession.FAMSessionID = FoundFAMSession.ID "
				"INNER JOIN AttributeSetForFile ExpectedSet ON ReportingRedactionAccuracy.ExpectedAttributeSetForFileID = ExpectedSet.ID "
				"INNER JOIN FileTaskSession ExpectedFileSession ON ExpectedSet.FileTaskSessionID = ExpectedFileSession.ID "
				"INNER JOIN FAMSession ExpectedFAMSession ON ExpectedFileSession.FAMSessionID = ExpectedFAMSession.ID "
			);

			queries.push_back("ALTER TABLE[dbo].[ReportingRedactionAccuracy] ALTER COLUMN [FoundDateTimeStamp] [DATETIME] NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingRedactionAccuracy] ALTER COLUMN [FoundFAMUserID] INT NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingRedactionAccuracy] ALTER COLUMN [ExpectedDateTimeStamp] [DATETIME] NOT NULL");
			queries.push_back("ALTER TABLE[dbo].[ReportingRedactionAccuracy] ALTER COLUMN [ExpectedFAMUserID] INT NOT NULL");
			queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_EXPECTED_FAMUSERID_WITH_INCLUDES);
			queries.push_back(gstrCREATE_REPORTING_REDACTION_ACCURACY_FOUND_FAMUSERID_WITH_INCLUDES);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46004");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion7(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 7;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back(gstrCREATE_REPORTING_HIM_STATS_V7);
			queries.push_back(gstrCREATE_FAMUSERID_WITH_INCLUDES_INDEX);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46055");

	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion8(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 8;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back("ALTER TABLE [dbo].[ReportingHIMStats] ADD [ID][int] IDENTITY(1,1) NOT NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingHIMStats] DROP CONSTRAINT [PK_ReportingHIMStats]");
			queries.push_back("ALTER TABLE [dbo].[ReportingHIMStats] ALTER COLUMN [PaginationID] [int] NULL");
			queries.push_back("ALTER TABLE [dbo].[ReportingHIMStats] ADD CONSTRAINT [PK_ReportingHIMStats] PRIMARY KEY CLUSTERED(ID)");
			queries.push_back(gstrCREATE_DESTFILE_WITH_INCLUDES_INDEX);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46594");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion9(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 9;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back("ALTER TABLE [dbo].[Workflow] DROP CONSTRAINT [FK_Workflow_OutputAttributeSet]");
			queries.push_back(gstrADD_WORKFLOW_OUTPUTATTRIBUTESET_FK);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46601");
	}
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion10(_ConnectionPtr ipConnection, long* pnNumSteps)
	{
		try
		{
			const int nNewSchemaVersion = 10;

			if (pnNumSteps != __nullptr)
			{
				*pnNumSteps += 1;
				return nNewSchemaVersion;
			}

			vector<string> queries;
			queries.push_back(gstrAdd_FKS_REPORTINGREDACTIONACCURACY);
			queries.push_back(gstrAdd_FKS_REPORTINGHIMSTATS);
			queries.push_back(gstrCreate_ReportingDataCaptureAccuracy_FKS);

			queries.emplace_back(GetVersionUpdateStatement(nNewSchemaVersion));
			long saveCommandTimeout = ipConnection->CommandTimeout;
			ipConnection->CommandTimeout = 0;
			executeVectorOfSQL(ipConnection, queries);
			ipConnection->CommandTimeout = saveCommandTimeout;

			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49587");
	}
}


//-------------------------------------------------------------------------------------------------
// CAttributeDBMgr
//-------------------------------------------------------------------------------------------------
CAttributeDBMgr::CAttributeDBMgr()
: m_ipFAMDB(__nullptr)
, m_ipDBConnection(__nullptr)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
{
}
//-------------------------------------------------------------------------------------------------
CAttributeDBMgr::~CAttributeDBMgr()
{
	try
	{
		m_ipFAMDB = __nullptr;
		m_ipDBConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38516");
}
//-------------------------------------------------------------------------------------------------
HRESULT CAttributeDBMgr::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CAttributeDBMgr::FinalRelease()
{
	try
	{
		// Release COM objects before the object is destructed
		m_ipFAMDB = __nullptr;
		m_ipDBConnection = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38517");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] =
	{
		&IID_IAttributeDBMgr,
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
STDMETHODIMP CAttributeDBMgr::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI38518", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t(gstrDESCRIPTION.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38519");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI38520", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38521");
}

//-------------------------------------------------------------------------------------------------
// IProductSpecificDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::raw_AddProductSpecificSchema( IFileProcessingDB* pDB,
															VARIANT_BOOL /*bOnlyTables*/,
															VARIANT_BOOL bAddUserTables )
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI38522", ipDB != __nullptr);

		// Create the connection object
		_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI38523", ipDBConnection != __nullptr);

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);
		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		VectorOfString tableCreationQueries = GetCurrentSchema( asCppBool(bAddUserTables) );
		executeVectorOfSQL(ipDBConnection, tableCreationQueries);

		// Set the default values for the DBInfo settings.
		map<string, string> mapDBInfoDefaultValues = getDBInfoDefaultValues();
		auto iterDBInfoValues = mapDBInfoDefaultValues.begin();
		for ( ; iterDBInfoValues != mapDBInfoDefaultValues.end(); ++iterDBInfoValues )
		{
			VARIANT_BOOL setIfExists =
				asVariantBool(iterDBInfoValues->first == gstrSCHEMA_VERSION_NAME);

			std::string schemaVersionName = iterDBInfoValues->first;
			std::string schemaVersion = iterDBInfoValues->second;
			ipDB->SetDBInfoSetting( schemaVersionName.c_str(),
									schemaVersion.c_str(),
									setIfExists,
									VARIANT_FALSE);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38524");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::raw_AddProductSpecificSchema80(IFileProcessingDB* /*pDB*/)
{
	// AttributeDBMgr did not exist in 8.0.
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::raw_RemoveProductSpecificSchema( IFileProcessingDB* pDB,
															   VARIANT_BOOL /*bOnlyTables*/,
															   VARIANT_BOOL bRetainUserTables,
															   VARIANT_BOOL *pbSchemaExists )
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		ASSERT_ARGUMENT("ELI38525", pbSchemaExists != __nullptr);

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI38526", ipDB != __nullptr);

		auto value = asString( ipDB->GetDBInfoSetting( gstrSCHEMA_VERSION_NAME.c_str(),
													   VARIANT_FALSE) );
		if ( value.empty() )
		{
			*pbSchemaExists = VARIANT_FALSE;
			return S_OK;
		}
		else
		{
			*pbSchemaExists = VARIANT_TRUE;
		}

		// Create the connection object
		ADODB::_ConnectionPtr ipDBConnection( __uuidof( Connection ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI38527", ipDBConnection != __nullptr );

		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString( strDatabaseServer,
															 strDatabaseName );
		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		VectorOfString tableNames = GetCurrentTableNames( asCppBool(bRetainUserTables) );
		dropTablesInVector(ipDBConnection, tableNames);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38528");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::raw_ValidateSchema( IFileProcessingDB* pDB )
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		// Update the FAMDB pointer if it is new.
		if (m_ipFAMDB != pDB)
		{
			m_ipFAMDB = pDB;
			m_ipFAMDB->GetConnectionRetrySettings( &m_nNumberOfRetries, &m_dRetryTimeout );

			// Reset the database connection
			m_ipDBConnection = __nullptr;
		}

		validateSchemaVersion();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR( "ELI38529" );
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any DBInfo row is removed, this code needs to be modified so that it does not treat
// the removed element(s) on an old schema versions as unrecognized.
STDMETHODIMP CAttributeDBMgr::raw_GetDBInfoRows(IVariantVector** ppDBInfoRows)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipDBInfoRows(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI38530", ipDBInfoRows != __nullptr);

		map<string, string> mapDBInfoValues = getDBInfoDefaultValues();
		auto iterDBInfoValues = mapDBInfoValues.begin();
		for ( ; iterDBInfoValues != mapDBInfoValues.end(); ++iterDBInfoValues )
		{
			ipDBInfoRows->PushBack(iterDBInfoValues->first.c_str());
		}

		*ppDBInfoRows = ipDBInfoRows.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38531");
}
//-------------------------------------------------------------------------------------------------
// WARNING: If any table is removed, this code needs to be modified so that it does not treat the
// removed element(s) on an old schema versions as unrecognized.
STDMETHODIMP CAttributeDBMgr::raw_GetTables(IVariantVector** ppTables)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IVariantVectorPtr ipTables(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI38532", ipTables != __nullptr);

		VectorOfString vecTablesNames = GetCurrentTableNames();
		auto iter = vecTablesNames.begin();
		for ( ; iter != vecTablesNames.end(); ++iter )
		{
			ipTables->PushBack( iter->c_str() );
		}

		*ppTables = ipTables.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38533");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP
CAttributeDBMgr::raw_UpdateSchemaForFAMDBVersion( IFileProcessingDB* pDB,
												  _Connection* pConnection,
												  long nFAMDBSchemaVersion,
												  long* pnProdSchemaVersion,
												  long* pnNumSteps,
												  IProgressStatus* /*pProgressStatus*/)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());

		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_ARGUMENT("ELI38534", ipDB != __nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_ARGUMENT("ELI38535", ipConnection != __nullptr);

		ASSERT_ARGUMENT("ELI38536", pnProdSchemaVersion != __nullptr);

		if (*pnProdSchemaVersion == 0)
		{
			string strVersion = asString( ipDB->GetDBInfoSetting(gstrSCHEMA_VERSION_NAME.c_str(), VARIANT_FALSE) );
			if (strVersion.empty())
			{
				if (nFAMDBSchemaVersion == dbSchemaVersionWhenAttributeCollectionWasIntroduced)
				{
					*pnProdSchemaVersion = 0;
				}
				else
				{
					return S_OK;
				}
			}
			else
			{
				*pnProdSchemaVersion = asLong(strVersion);
			}
		}

		// WARNING!!! - Fix up this switch when the version has been changed.
		switch (*pnProdSchemaVersion)
		{
			case 0:
				if (nFAMDBSchemaVersion == dbSchemaVersionWhenAttributeCollectionWasIntroduced)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion1(ipConnection, pnNumSteps);
				}
				// fall into the next case and update that version as well...

			case 1:
				*pnProdSchemaVersion = UpdateToSchemaVersion2(ipConnection, pnNumSteps);
				break;

			case 2:
				// Separated FAM workflows introduced
				if (nFAMDBSchemaVersion == 143)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion3(ipConnection, pnNumSteps);
				}
				break;

			case 3:
				if (nFAMDBSchemaVersion == 159)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion4(ipConnection, pnNumSteps);
				}
				break;

			case 4: 
				if (nFAMDBSchemaVersion == 165)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion5(ipConnection, pnNumSteps);
				}
				
			case 5:
				if (nFAMDBSchemaVersion == 165)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion6(ipConnection, pnNumSteps);
				}
				break;

			case 6:
				if (nFAMDBSchemaVersion == 166)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion7(ipConnection, pnNumSteps);
				}
				break;

			case 7:
				if (nFAMDBSchemaVersion == 171)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion8(ipConnection, pnNumSteps);
				}
				// Fall through to next case

			case 8:
				if (nFAMDBSchemaVersion == 171)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion9(ipConnection, pnNumSteps);
				}
				break;

			case 9:
				if (nFAMDBSchemaVersion == 179)
				{
					*pnProdSchemaVersion = UpdateToSchemaVersion10(ipConnection, pnNumSteps);
				}
				break;

			case 10:
				break;

			default:
			{
				UCLIDException ue("ELI38537", "Automatic updates are not supported for the current schema.");
				ue.addDebugInfo("FAM Schema Version", nFAMDBSchemaVersion, false);
				ue.addDebugInfo("Attribute Schema Version", *pnProdSchemaVersion, false);
				throw ue;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38538");
}

//-------------------------------------------------------------------------------------------------
// IAttributeProductDBMgr Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::put_FAMDB(IFileProcessingDB* newVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		ASSERT_ARGUMENT("ELI38541", newVal != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38540");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::get_FAMDB(IFileProcessingDB** retVal)
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState());
		ASSERT_ARGUMENT("ELI41635", retVal != __nullptr);

		IFileProcessingDBPtr ipTemp = m_ipFAMDB;
		*retVal = ipTemp.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41636");
}
//-------------------------------------------------------------------------------------------------

namespace
{
	// This function escapes any single quotes in the input.
	std::string SqlSanitizeInput( const std::string& input )
	{
		std::string sanitized( input );
		replaceVariable( sanitized, "'", "''" );	// replace single quote (') with ''
		return sanitized;
	}

	// NOTE: strAttributeSetName must be escaped for XML (apostrophe's escaped).
	longlong GetAttributeSetID(string strAttributeSetName, _ConnectionPtr ipConnection)
	{
		longlong llSetNameID = 0;
		try
		{
			try
			{
				executeCmdQuery(ipConnection, Util::Format(
					"SELECT TOP 1 [ID] FROM [dbo].[AttributeSetName] WHERE [Description]='%s'"
						, strAttributeSetName.c_str())
					, "ID", false, &llSetNameID);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39174");
		}
		catch (UCLIDException &ue)
		{
			UCLIDException uexOuter("ELI39175", "Attribute set name not found.", ue);
			uexOuter.addDebugInfo("Set name", strAttributeSetName);
			throw uexOuter;
		}

		return llSetNameID;
	}

	std::string GetInsertRootASFFStatement( longlong llSetNameID, long fileTaskSessionID )
	{
		std::string insert =
			"SET NOCOUNT ON \n"
			"DECLARE @AttributeSetName_ID AS BIGINT;\n"
			"DECLARE @AttributeSetForFile_ID AS BIGINT;\n"
			"DECLARE @FileTaskSessionID AS INT;\n"
			"\n";

		insert += Util::Format( "SELECT @AttributeSetName_ID=%lld;\n"
								"SELECT @FileTaskSessionID=%ld;\n",
								llSetNameID,
								fileTaskSessionID );
		insert +=
			"\n"
			"BEGIN TRY\n"
			"	INSERT INTO [AttributeSetForFile] ([FileTaskSessionID], [AttributeSetNameID])\n"
			"	OUTPUT INSERTED.ID \n"
			"	VALUES (@FileTaskSessionID, @AttributeSetName_ID);\n"
			"	SET NOCOUNT OFF\n"
			"END TRY\n"
			"BEGIN CATCH\n"
			"	SET NOCOUNT OFF\n"
			"	DECLARE @ErrorMessage NVARCHAR(4000);\n"
			"   DECLARE @ErrorSeverity INT;\n"
			"   DECLARE @ErrorState INT;\n"
			"\n"
			"    SELECT\n"
			"        @ErrorMessage = ERROR_MESSAGE(),\n"
			"        @ErrorSeverity = ERROR_SEVERITY(),\n"
			"        @ErrorState = ERROR_STATE();\n"
			"\n"
			"    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)\n"
			"END CATCH";

			return insert;
	}

	std::string insertAttributeQueryPreamble =
		"DECLARE @AttributeName_Name AS NVARCHAR(255); \n"
		"DECLARE @AttributeType_Type AS NVARCHAR(255); \n"
		"DECLARE @Attribute_Value AS NVARCHAR(MAX); \n"
		"DECLARE @Attribute_GUID AS UNIQUEIDENTIFIER; \n"
		"DECLARE @ParentAttribute_ID AS BIGINT; \n"
		"DECLARE @AttributeSetForFile_ID AS BIGINT; \n"
		"DECLARE @AttributeName_ID AS BIGINT; \n"
		"DECLARE @AttributeType_ID AS BIGINT; \n"
		"DECLARE @Attribute_ID AS BIGINT; \n"
		"DECLARE @TypeTable table(idx int IDENTITY(1,1), TypeName NVARCHAR(255)); \n"
		"DECLARE @ErrorMessage NVARCHAR(4000); \n"
		"DECLARE @ErrorSeverity INT; \n"
		"DECLARE @ErrorState INT; \n"
		"DECLARE @AttributeIDs TABLE (AttributeID BIGINT); \n";


	std::string GetInsertAttributeQuery( IAttributePtr ipAttribute,
										 longlong owningAttributeSetForFileID )
	{
		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		std::string value = SqlSanitizeInput(asString(ipValue->String));
		std::string attributeName = SqlSanitizeInput(asString(ipAttribute->GetName()));
		std::string attributeType = SqlSanitizeInput(asString(ipAttribute->GetType()));

		IIdentifiableObjectPtr ipIdentifiable(ipAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38642", __nullptr != ipIdentifiable);
		std::string guid = asString(ipIdentifiable->InstanceGUID);

		std::string insert;
		VectorOfString typeNames = Split( attributeType, '+' );
		for ( size_t i = 0; i < typeNames.size(); ++i )
		{
			std::string insertToTable =
						  Util::Format( "INSERT INTO @TypeTable (TypeName) VALUES ('%s'); \n",
										typeNames[i].c_str() );

			insert += insertToTable;
		}

		std::string args =
			Util::Format( "SELECT @AttributeName_Name='%s'; \n"
						  "SELECT @AttributeType_Type='%s'; \n"
						  "SELECT @Attribute_Value='%s'; \n"
						  "SELECT @Attribute_GUID='%s'; \n"
						  "SELECT @AttributeSetForFile_ID=%ld \n",
						  attributeName.c_str(),
						  attributeType.c_str(),
						  value.c_str(),
						  guid.c_str(),
						  owningAttributeSetForFileID );

		insert += args;
		insert +=
			  "SELECT @AttributeName_ID=null; \n"
			  "SELECT @AttributeType_ID=null; \n"
			  "SELECT @ParentAttribute_ID=null; \n"

			  "BEGIN TRY \n"
				  // AttributeName
				  "SELECT @AttributeName_ID = (SELECT TOP 1 [ID] FROM [dbo].[AttributeName] WHERE [Name]=@AttributeName_Name)\n"
				  "if @AttributeName_ID IS NULL\n"
				  "BEGIN \n"
				  "    INSERT INTO [dbo].[AttributeName] ([Name]) VALUES (@AttributeName_Name);\n"
				  "    SELECT @AttributeName_ID = SCOPE_IDENTITY()\n"
				  "END \n"
				  "\n"
				  // Attribute
				  "SELECT TOP 1 @ParentAttribute_ID=AttributeID FROM @AttributeIDs ORDER BY AttributeID DESC; \n"
				  "INSERT INTO [dbo].[Attribute] ([AttributeSetForFileID], [AttributeNameID], [Value], [ParentAttributeID], [GUID]) \n"
				  "VALUES (@AttributeSetForFile_ID, @AttributeName_ID, @Attribute_Value, @ParentAttribute_ID, @Attribute_GUID);\n"
				  "SELECT @Attribute_ID = SCOPE_IDENTITY()\n"
				  "\n"
				  "WHILE EXISTS (SELECT * FROM @TypeTable) \n"
				  "BEGIN \n"
				  "  SELECT @AttributeType_Type = MIN(TypeName) FROM @TypeTable \n"
					// AttributeType
					"SELECT @AttributeType_ID = (SELECT TOP 1 [ID] from [dbo].[AttributeType] WHERE [Type]=@AttributeType_Type) \n"
					"if @AttributeType_ID IS NULL AND @AttributeType_Type <> '' \n"
					"BEGIN \n"
					"    INSERT INTO [dbo].[AttributeType] ([Type]) VALUES (@AttributeType_Type);\n"
					"    SELECT @AttributeType_ID = SCOPE_IDENTITY()\n"
					"END \n"
					"\n"
					// AttributeInstanceType
					"if @AttributeType_ID IS NOT NULL AND not exists (SELECT * FROM [dbo].[AttributeInstanceType] WHERE [AttributeID]=@Attribute_ID AND [AttributeTypeID]=@AttributeType_ID)\n"
					"BEGIN \n"
					"    INSERT INTO [dbo].[AttributeInstanceType] ([AttributeID], [AttributeTypeID]) VALUES (@Attribute_ID, @AttributeType_ID)\n"
					"END \n"
					"\n"
					"DELETE FROM @TypeTable WHERE TypeName=@AttributeType_Type \n"
				  "END \n"
				  "\n"
				  "INSERT INTO @AttributeIDs VALUES(@Attribute_ID)\n"
			  "END TRY \n"
			  "BEGIN CATCH \n"
				"SELECT \n"
					"@ErrorMessage = ERROR_MESSAGE(), \n"
					"@ErrorSeverity = ERROR_SEVERITY(), \n"
					"@ErrorState = ERROR_STATE(); \n"
				"RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState) \n"
			  "END CATCH \n";
		return insert;
	}

	std::string popParentAttributeID =
		"SELECT TOP 1 @ParentAttribute_ID=AttributeID FROM @AttributeIDs ORDER BY AttributeID DESC; \n"
		"DELETE @AttributeIDs WHERE AttributeID = @ParentAttribute_ID; \n";

	std::string insertRasterZonePreamble =
		"INSERT INTO [dbo].[RasterZone] "
		"([AttributeID], [Top], [Left], [Bottom], [Right], "
		"[StartX], [StartY], [EndX], [EndY], [PageNumber], [Height]) "
		"VALUES ";

	std::string GetInsertRasterZoneStatement( IRasterZonePtr ipZone )
	{
		ILongRectanglePtr ipRect = ipZone->GetRectangularBounds( __nullptr );
		int top = ipRect->Top;
		int left = ipRect->Left;
		int bottom = ipRect->Bottom;
		int right = ipRect->Right;
		int startX = ipZone->StartX;
		int startY = ipZone->StartY;
		int endX = ipZone->EndX;
		int endY = ipZone->EndY;
		int pageNumber = ipZone->PageNumber;
		int height = ipZone->Height;

		std::string insert = Util::Format( "(@Attribute_ID, %d, %d, %d, %d, %d, %d, %d, %d, %d, %d)",
										   top,
										   left,
										   bottom,
										   right,
										   startX,
										   startY,
										   endX,
										   endY,
										   pageNumber,
										   height );

		return insert;
	}

	// This query gets one IUnknown Vector Of Attributes
	// A positive relative index is 1-based index from first stored to nth stored,
	// A negative relative index is 1-based index from last stored (-1) to nth most recently stored (-n)
	std::string GetQueryForAttributeSetForFile( long fileID,
												BSTR attributeSetName,
												long relativeIndex )
	{
		std::string query =
			Util::Format( "WITH attrSets AS ( \n"
						  "SELECT [asff].[VOA], "
						  "ROW_NUMBER() OVER (ORDER BY [asff].[ID] %s) AS \"RowNumber\" "
						  "FROM [AttributeSetForFile] asff \n"
						  "INNER JOIN [AttributeSetName] asn ON [asff].[AttributeSetNameID]=[asn].[ID] \n"
						  "INNER JOIN [FileTaskSession] fts ON [asff].[FileTaskSessionID]=[fts].[ID] \n"
						  "WHERE [fts].[FileID]=%d AND [asn].[Description]='%s' \n"
						  ") SELECT * FROM attrSets WHERE \"RowNumber\" = %d;",
						  relativeIndex < 0 ? "DESC" : "ASC",					// DESC is most recent, ASC is oldest
						  fileID,
						  SqlSanitizeInput(asString(attributeSetName)).c_str(),
						  abs( relativeIndex ) );								// offset (index) into result set, get Nth

		return query;
	}

	struct FieldFetcher
	{
		explicit FieldFetcher( FieldsPtr ipFields ):
		m_ipFields( ipFields )
		{
		}

		std::string GetString( const std::string& fieldName ) const
		{
			return getStringField( m_ipFields, fieldName );
		}

		long GetLong( const std::string& fieldName ) const
		{
			return getLongField( m_ipFields, fieldName );
		}

		long long GetLongLong(const std::string& fieldName ) const
		{
			return getLongLongField( m_ipFields, fieldName );
		}

	private:
		FieldsPtr	m_ipFields;
	};

	template <typename TPtr>
	TPtr AssignComPtr( TPtr ipInstance, const std::string& eliCode )
	{
		TPtr instance = ipInstance;
		ASSERT_RESOURCE_ALLOCATION( eliCode, __nullptr != instance );
		return instance;
	}

	template <typename TPtr>
	TPtr AssignComPtr( TPtr ipInstance, bool bCondition, const std::string& eliCode )
	{
		TPtr instance = ipInstance;
		ASSERT_RESOURCE_ALLOCATION( eliCode, bCondition );
		return instance;
	}

	template <typename TPtr>
	TPtr MakeIPtr( const GUID& clsId, const std::string& eliCode )
	{
		TPtr pT(clsId);
		ASSERT_RESOURCE_ALLOCATION( eliCode, __nullptr != pT );

		return pT;
	}

	void SetAttributeGuid( IAttributePtr ipAttribute, const std::string& guid )
	{
		UUID uuid;
		UuidFromString( (unsigned char*)guid.c_str(), &uuid );
		ipAttribute->SetGUID( &uuid );
	}

	bool RecordsInSet( const ADODB::_RecordsetPtr ipRecords )
	{
		return VARIANT_FALSE == ipRecords->adoEOF;
	}

	ADODB::_RecordsetPtr ExecuteCmd( const std::string& cmd, ADODB::_ConnectionPtr ipConnection )
	{
		return ipConnection->Execute( cmd.c_str(), __nullptr, adCmdText );
	}

	FieldsPtr GetFieldsForQuery( const std::string& query, ADODB::_ConnectionPtr ipConnection )
	{
		ADODB::_RecordsetPtr ipASFF = ExecuteCmd( query, ipConnection );
		if (!RecordsInSet(ipASFF))
		{
			UCLIDException ue("ELI38763", "Database record not found");
			ue.addDebugInfo("Query", query, true);
			throw ue;
		}

		FieldsPtr ipFields = AssignComPtr( ipASFF->Fields, "ELI38764" );
		return ipFields;
	}

	longlong ExecuteRootInsertASFF( const std::string& insert, ADODB::_ConnectionPtr ipConnection )
	{
		ADODB::_RecordsetPtr ipRS = ExecuteCmd( insert.c_str(), ipConnection );
		ASSERT_RESOURCE_ALLOCATION(	"ELI38886", VARIANT_FALSE == ipRS->adoEOF );

		FieldsPtr ipFields = AssignComPtr( ipRS->Fields, "ELI38887" );
		longlong ID = getLongLongField( ipFields, "ID" );
		ipRS->Close();

		return ID;
	}

	// Definition of empty:
	// the attribute Value is empty, the attribute Type is empty,
	// (iff true == storeRasterZones) RasterZones are NOT present, AND
	// all subattributes are similarly empty.
	bool AttributeIsEmpty( IAttributePtr ipAttribute,
						   bool bStoreRasterZones,
   						   bool bStoreEmptyAttributes )
	{
		if ( true == bStoreEmptyAttributes )
		{
			return false;	// Tell caller attribute is not empty, because caller doesn't care
		}

		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		auto strValue = asString( ipValue->String );
		if ( !strValue.empty() )
		{
			return false;
		}

		auto strType = asString( ipAttribute->Type );
		if ( !strType.empty() )
		{
			return false;
		}

		if ( true == bStoreRasterZones )
		{
			if ( asCppBool( ipValue->HasSpatialInfo() ) )
				return false;
		}

		IIUnknownVectorPtr ipSubAttrs = ipAttribute->GetSubAttributes();
		for ( long i = 0; i < ipSubAttrs->Size(); ++i )
		{
			IAttributePtr ipSubAttribute = AssignComPtr( ipSubAttrs->At(i), "ELI38997" );
			auto ret = AttributeIsEmpty( ipSubAttribute, bStoreRasterZones, bStoreEmptyAttributes );
			if ( false == ret )
			{
				return false;
			}
		}

		return true;
	}

}		// end of anonymous namespace


#define COMPRESSED_STREAM


// ------------------------------------------------------------------------------------------------
void CAttributeDBMgr::SaveVoaDataInASFF( _ConnectionPtr ipConnection, IIUnknownVector* pAttributes,
										 longlong llRootASFF_ID )
{
	try
	{
		std::string query = Util::Format( "SELECT * FROM [dbo].[AttributeSetForFile] "
										  "WHERE [ID] = %lld",
										  llRootASFF_ID );

		ADODB::_RecordsetPtr ipASFF( __uuidof(Recordset) );
		ASSERT_RESOURCE_ALLOCATION( "ELI38804", __nullptr != ipASFF );

		auto connectParam = _variant_t( (IDispatch*)ipConnection, true );
		ipASFF->Open( query.c_str(),
					  connectParam,
					  adOpenDynamic,
					  adLockOptimistic,
					  adCmdText );

		if (!RecordsInSet(ipASFF))
		{
			UCLIDException ue("ELI38805", "Database record not found");
			ue.addDebugInfo("Query", query, true);
			throw ue;
		}

		// Now prepare the VOA for save
		string storageManagerIID = asString(CLSID_AttributeStorageManager);
		IIUnknownVectorPtr pAttributesClone = pAttributes->PrepareForStorage( storageManagerIID.c_str() );

		IPersistStreamPtr ipPersistObj = pAttributesClone;

#ifdef UNCOMPRESSED_STREAM
		FieldsPtr ipFields = AssignComPtr( ipASFF->Fields, "ELI40361" );
		setIPersistObjToField( ipFields, "VOA", ipPersistObj );
		ipASFF->Update();
#endif

#ifdef COMPRESSED_STREAM
		CComSafeArray<BYTE> saData;
		saData.Attach(ZipUtil::CompressAttributes(ipPersistObj));

		_variant_t variantData;
		variantData.vt = VT_ARRAY|VT_UI1;
		variantData.parray = saData;
		ipASFF->Fields->GetItem( "VOA" )->PutValue( variantData );
		ipASFF->Update();
#endif
	}
	catch (...)
	{
		UCLIDException ue;
		std::string message(
			Util::Format("ADO exception while saving VOA to AttributeSetForFile, "
						 "ID: %lld", llRootASFF_ID ) );
		ue.asString( message );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CAttributeDBMgr::storeAttributeData( _ConnectionPtr ipConnection,
										  IIUnknownVectorPtr ipAttributes,
										  bool bStoreRasterZone,
										  bool bStoreEmptyAttributes,
										  long long llRootASFF_ID )
{
	std::string strInsertQuery = insertAttributeQueryPreamble
		+ buildStoreAttributeDataQuery( ipConnection,
										ipAttributes,
										bStoreRasterZone,
										bStoreEmptyAttributes,
										llRootASFF_ID);

	ExecuteCmd( strInsertQuery.c_str(), ipConnection );
}

std::string CAttributeDBMgr::buildStoreAttributeDataQuery( _ConnectionPtr ipConnection,
														   IIUnknownVectorPtr ipAttributes,
														   bool bStoreRasterZone,
														   bool bStoreEmptyAttributes,
														   long long llRootASFF_ID )
{
	std::string strInsertQuery;
	for ( long i = 0; i < ipAttributes->Size(); ++i )
	{
		IAttributePtr ipAttribute = AssignComPtr( ipAttributes->At(i), "ELI38693" );
		if ( true == AttributeIsEmpty( ipAttribute, bStoreRasterZone, bStoreEmptyAttributes ) )
		{
			continue;
		}

		strInsertQuery += GetInsertAttributeQuery( ipAttribute,
												   llRootASFF_ID);
		ISpatialStringPtr ipValue = AssignComPtr( ipAttribute->GetValue(), "ELI38714" );
		bool bHasSpatialInfo = asCppBool( ipValue->HasSpatialInfo() );
		if ( bHasSpatialInfo && true == bStoreRasterZone )
		{
			strInsertQuery += insertRasterZonePreamble;
			IIUnknownVectorPtr ipZones = ipValue->GetOriginalImageRasterZones();	// NOTE: can't return __nullptr, throws
			for ( long index = 0; index < ipZones->Size(); ++index )
			{
				IRasterZonePtr ipZone = AssignComPtr( ipZones->At(index), "ELI38669" );

				if (index != 0)
				{
					strInsertQuery += ",";
				}
				strInsertQuery += GetInsertRasterZoneStatement( ipZone );
			}
		}
		strInsertQuery += buildStoreAttributeDataQuery( ipConnection,
														ipAttribute->SubAttributes,
														bStoreRasterZone,
														bStoreEmptyAttributes,
														llRootASFF_ID);
		strInsertQuery += popParentAttributeID;
	}
	return strInsertQuery;
}

bool CAttributeDBMgr::CreateNewAttributeSetForFile_Internal( bool bDbLocked,
															 long nFileTaskSessionID,
  														     BSTR bstrAttributeSetName,
  														     IIUnknownVector* pAttributes,
  															 VARIANT_BOOL vbStoreDiscreteFields,
  															 VARIANT_BOOL vbStoreRasterZone,
  															 VARIANT_BOOL vbStoreEmptyAttributes )
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipAttributes(pAttributes);
			ASSERT_RESOURCE_ALLOCATION("ELI38959", ipAttributes != __nullptr);
			ASSERT_ARGUMENT("ELI38553", pAttributes != __nullptr);
			ASSERT_ARGUMENT("ELI38554", nFileTaskSessionID > 0 );

			std::string strSetName = SqlSanitizeInput(asString(bstrAttributeSetName));

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			TransactionGuard tg( ipConnection, adXactRepeatableRead, __nullptr );

			longlong llSetNameID = GetAttributeSetID( strSetName, ipConnection );
			auto strInsertRootASFF = GetInsertRootASFFStatement( llSetNameID, nFileTaskSessionID );
			longlong llRootASFF_ID = ExecuteRootInsertASFF( strInsertRootASFF, ipConnection );
			SaveVoaDataInASFF( ipConnection, ipAttributes, llRootASFF_ID );

			if (vbStoreDiscreteFields)
			{
				storeAttributeData(ipConnection,
					ipAttributes,
					asCppBool(vbStoreRasterZone),
					asCppBool(vbStoreEmptyAttributes),
					llRootASFF_ID);
			}

			tg.CommitTrans();

			END_ADO_CONNECTION_RETRY(ipConnection, getDBConnection, m_nNumberOfRetries,
				m_dRetryTimeout, "ELI39232");

			return true;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38557");
	}
	catch (const UCLIDException&)
	{
		if (!bDbLocked)
		{
			return false;
		}

		throw;
	}
}
// ------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetForFile( long nFileTaskSessionID,
														    BSTR bstrAttributeSetName,
														    IIUnknownVector* pAttributes,
  															VARIANT_BOOL vbStoreDiscreteFields,
															VARIANT_BOOL vbStoreRasterZone,
															VARIANT_BOOL vbStoreEmptyAttributes,
															VARIANT_BOOL vbCloseConnection)
{
	try
	{
		// Set connection to null on end of scope if requested to close it
		shared_ptr<void> closeConnection(__nullptr, [&](void*)
		{
			if (asCppBool(vbCloseConnection))
			{
				m_ipDBConnection = __nullptr;
			}
		});

		const bool bDbNotLocked = false;
		auto bRet = CreateNewAttributeSetForFile_Internal( bDbNotLocked,
														   nFileTaskSessionID,
														   bstrAttributeSetName,
														   pAttributes,
														   vbStoreDiscreteFields,
														   vbStoreRasterZone,
														   vbStoreEmptyAttributes );
		if ( !bRet )
		{
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dbLock( m_ipFAMDB,
																			 gstrMAIN_DB_LOCK );
			const bool bDbIsLocked = true;
			CreateNewAttributeSetForFile_Internal( bDbIsLocked,
												   nFileTaskSessionID,
												   bstrAttributeSetName,
												   pAttributes,
												   vbStoreDiscreteFields,
												   vbStoreRasterZone,
												   vbStoreEmptyAttributes );
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI39028");
}

bool CAttributeDBMgr::GetAttributeSetForFile_Internal( bool bDbLocked,
													   long fileID,
													   BSTR attributeSetName,
													   long relativeIndex,
													   IIUnknownVector** ppAttributes)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI38618", relativeIndex != 0);
			ASSERT_ARGUMENT("ELI38668", ppAttributes != __nullptr);

			// This needs to be allocated outside the BEGIN_ADO_CONNECTION_RETRY
			_ConnectionPtr ipConnection = __nullptr;

			BEGIN_ADO_CONNECTION_RETRY();

			// Get the connection for the thread and save it locally.
			ipConnection = getDBConnection();

			string strQuery = GetQueryForAttributeSetForFile(fileID, attributeSetName, relativeIndex);

#ifdef UNCOMPRESSED_STREAM
			FieldsPtr ipFields = GetFieldsForQuery( strQuery, ipConnection );
			IIUnknownVectorPtr ipAttributes = getIPersistObjFromField( ipFields, "VOA" );
			ASSERT_RESOURCE_ALLOCATION("ELI39173", ipAttributes != __nullptr);

			*ppAttributes = ipAttributes.Detach();
#endif

#ifdef COMPRESSED_STREAM

			TransactionGuard tg( ipConnection, adXactReadCommitted, __nullptr );

			FieldsPtr ipFields = GetFieldsForQuery( strQuery, ipConnection );
			ASSERT_RESOURCE_ALLOCATION("ELI46595", ipFields != __nullptr);

			FieldPtr ipVOAPtr = ipFields->GetItem("VOA");
			ASSERT_RESOURCE_ALLOCATION("ELI46596", ipVOAPtr != __nullptr);

			variant_t vtValue = ipVOAPtr->GetValue();

			tg.CommitTrans();
			if (vtValue.vt == VT_NULL)
			{
				// Try again in case this is just a temporary condition (e.g., record is being added)
				Sleep(500);

				ipFields = GetFieldsForQuery( strQuery, ipConnection );
				ASSERT_RESOURCE_ALLOCATION("ELI46617", ipFields != __nullptr);

				ipVOAPtr = ipFields->GetItem("VOA");
				ASSERT_RESOURCE_ALLOCATION("ELI46618", ipVOAPtr != __nullptr);

				vtValue = ipVOAPtr->GetValue();
			}

			IIUnknownVectorPtr ipAttributes;
			if (vtValue.vt == VT_NULL)
			{
				UCLIDException ue("ELI46597", "VOA field is null");
				ue.addDebugInfo("File ID", fileID);
				ue.addDebugInfo("Attribute set name", asString(attributeSetName));
				ue.addDebugInfo("Relative index", relativeIndex);
				throw ue;
			}
			else
			{
				CComSafeArray<BYTE> saData;
				saData.Attach(vtValue.Detach().parray);

				CComSafeArray<BYTE> saData2;
				saData2.Attach(ZipUtil::DecompressAttributes(saData));

				ipAttributes = readObjFromSAFEARRAY(saData2);
				ASSERT_RESOURCE_ALLOCATION("ELI39172", ipAttributes != __nullptr);
			}

			*ppAttributes = ipAttributes.Detach();
#endif
			END_ADO_CONNECTION_RETRY(ipConnection, getDBConnection, m_nNumberOfRetries,
				m_dRetryTimeout, "ELI39233");

			return true;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39029");
	}
	catch ( const UCLIDException&)
	{
		if ( !bDbLocked )
		{
			return false;
		}

		throw;
	}
}


//-------------------------------------------------------------------------------------------------
// relativeIndex: -1 for most recent, 1 for oldest
// decrement most recent value to get next most recent (-2)
// increment oldest value to get next oldest (2)
// Zero is an illegal relativeIndex value.
STDMETHODIMP CAttributeDBMgr::GetAttributeSetForFile(long fileID,
													 BSTR attributeSetName,
													 long relativeIndex,
													 VARIANT_BOOL vbCloseConnection,
													 IIUnknownVector** ppAttributes)
{
	try
	{
		// Set connection to null on end of scope if requested to close it
		shared_ptr<void> closeConnection(__nullptr, [&](void*)
		{
			if (asCppBool(vbCloseConnection))
			{
				m_ipDBConnection = __nullptr;
			}
		});

		const bool bDbNotLocked = false;
		auto bRet = GetAttributeSetForFile_Internal( bDbNotLocked,
													 fileID,
													 attributeSetName,
													 relativeIndex,
													 ppAttributes);
		if ( !bRet )
		{
			LockGuard<UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr> dbLock( m_ipFAMDB,
																			 gstrMAIN_DB_LOCK );
			const bool bDbIsLocked = true;
			GetAttributeSetForFile_Internal( bDbIsLocked,
											 fileID,
											 attributeSetName,
											 relativeIndex,
											 ppAttributes);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38619");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetName(BSTR name,
														long long* pAttributeSetNameID)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38630", name != __nullptr );
		ASSERT_ARGUMENT( "ELI38676", pAttributeSetNameID != __nullptr );

		std::string newName( SqlSanitizeInput(asString(name)) );
		std::string cmd( Util::Format( "INSERT INTO [dbo].[AttributeSetName] "
									   "(Description) OUTPUT INSERTED.ID VALUES ('%s');",
									   newName.c_str() ) );

		m_ipFAMDB->ExecuteCommandReturnLongLongResult( cmd.c_str(),
		 											   "ID",
													   pAttributeSetNameID );
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38629");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::RenameAttributeSetName(BSTR attributeSetName,
													 BSTR newName)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38627", attributeSetName != __nullptr );
		ASSERT_ARGUMENT( "ELI38628", newName != __nullptr );

		std::string currentName( SqlSanitizeInput(asString(attributeSetName)) );
		std::string changeNameTo( SqlSanitizeInput(asString(newName)) );

		std::string cmd( Util::Format( "UPDATE [dbo].[AttributeSetName] SET Description='%s' WHERE Description='%s';",
									   changeNameTo.c_str(),
									   currentName.c_str() ) );

		m_ipFAMDB->ExecuteCommandQuery( cmd.c_str() );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38626");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::DeleteAttributeSetName(BSTR attributeSetName)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38624", attributeSetName != __nullptr );

		std::string name( SqlSanitizeInput(asString(attributeSetName)) );
		ASSERT_ARGUMENT( "ELI38625", !name.empty() );

		std::string cmd( Util::Format( "DELETE FROM  [dbo].[AttributeSetName] "
									   "WHERE Description='%s';",
									   name.c_str() ) );

		m_ipFAMDB->ExecuteCommandQuery( cmd.c_str() );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38623");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::GetAllAttributeSetNames(IStrToStrMap** ippNames)
{
	try
	{
		ASSERT_ARGUMENT("ELI38617", __nullptr != ippNames);
		auto ipAttributeSetNames = MakeIPtr<IStrToStrMapPtr>(CLSID_StrToStrMap, "ELI38621");

		std::string query( "SELECT [ID], [Description] FROM [dbo].[AttributeSetName];" );
		ADODB::_RecordsetPtr pRecords = m_ipFAMDB->GetResultsForQuery( query.c_str() );
		while ( RecordsInSet(pRecords) )
		{
			FieldsPtr pFields = AssignComPtr( pRecords->Fields, "ELI40356" );

			std::string description = getStringField( pFields, "Description" );
			long long Id = getLongLongField( pFields, "ID" );

			std::string ID = Util::Format( "%lld", Id );
			ipAttributeSetNames->Set( description.c_str(), ID.c_str() );

			pRecords->MoveNext();
		}

		*ippNames = ipAttributeSetNames.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38620");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
ADODB::_ConnectionPtr CAttributeDBMgr::getDBConnection(bool bReset)
{
	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == __nullptr)
	{
		UCLIDException ue("ELI38542",
			"FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
	}

	// Check if the connection should be reset
	if (bReset && m_ipDBConnection != __nullptr)
	{
		// if the database is not closed close it
		if (m_ipDBConnection->State != adStateClosed)
		{
			// Do the close in a try catch so that if there is an exception it will be logged
			try
			{
				m_ipDBConnection->Close();
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI40165");
		}
		// Create a new connection
		m_ipDBConnection = __nullptr;
	}

	// Check if connection has been created
	if (m_ipDBConnection == __nullptr)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection));
		ASSERT_RESOURCE_ALLOCATION("ELI38543", m_ipDBConnection != __nullptr);
	}

	// if closed and Database server and database name are defined,  open the database connection
	if ( m_ipDBConnection->State == adStateClosed )
	{
		string strDatabaseServer = asString(m_ipFAMDB->DatabaseServer);
		string strDatabaseName = asString(m_ipFAMDB->DatabaseName);
		string strConnectionString = createConnectionString(strDatabaseServer, strDatabaseName);
		if ( !strDatabaseServer.empty() && !strDatabaseName.empty() )
		{
			m_ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified);

			// Get the command timeout from the FAMDB DBInfo table
			m_ipDBConnection->CommandTimeout =
				asLong(m_ipFAMDB->GetDBInfoSetting(gstrCOMMAND_TIMEOUT.c_str(), VARIANT_TRUE));
		}
	}

	return m_ipDBConnection;
}
//-------------------------------------------------------------------------------------------------
void CAttributeDBMgr::validateLicense()
{
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI38544", gstrDESCRIPTION);
}
//-------------------------------------------------------------------------------------------------
void CAttributeDBMgr::validateSchemaVersion()
{
	ASSERT_RESOURCE_ALLOCATION("ELI38545", m_ipFAMDB != __nullptr);

	// Get the Version from the FAMDB DBInfo table
	auto value = asString( m_ipFAMDB->GetDBInfoSetting( gstrSCHEMA_VERSION_NAME.c_str(),
													    asVariantBool(false) ) );
	// Check against expected version
	long versionNumber = value.empty() ? 0L : asLong(value);
	if (versionNumber != glSCHEMA_VERSION)
	{
		UCLIDException ue("ELI38546", "Attribute collection database schema is not current version!");
		ue.addDebugInfo("Expected", glSCHEMA_VERSION);
		ue.addDebugInfo("Database Version", value);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
map<string, string> CAttributeDBMgr::getDBInfoDefaultValues()
{
	map<string, string> mapDefaultValues;

	// WARNING: If any DBInfo row is removed, code needs to be modified so that
	// findUnrecognizedSchemaElements does not treat the element on old schema versions as
	// unrecognized.
	mapDefaultValues[gstrSCHEMA_VERSION_NAME] = asString(glSCHEMA_VERSION);

	return mapDefaultValues;
}
