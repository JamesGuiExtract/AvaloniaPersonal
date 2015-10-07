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
using namespace std;

namespace Logging
{
	std::string CurrentDateTimeAsString()
	{
		SYSTEMTIME st;
		GetLocalTime( &st );

		return Util::Format( "%04d-%02d-%02d_%02d-%02d", 
							 st.wYear,
							 st.wMonth,
							 st.wDay,
							 st.wHour, 
							 st.wMinute );
	}

	std::string MakeFilename()
	{
		return Util::Format( "c:\\temp\\Log_%s.txt", CurrentDateTimeAsString().c_str() );
	}

	struct Log
	{
		static Log& Instance()
		{
			static Log instance;
			return instance;
		}

		void Write( const std::string& msg )
		{
			_file.write( msg.c_str(), msg.size() );
			_file.flush();
		}

		~Log()
		{
			_file.flush();
			_file.close();
		}

	private:
		std::ofstream _file;

		Log():
		_file( MakeFilename().c_str() )
		{
			ASSERT_RUNTIME_CONDITION( "ELI38787", _file.is_open(), "Log open failed" );
		}
	};

//#define LOGGING_ENABLED
	void WriteToLog( const char* 
#ifdef LOGGING_ENABLED
						formatSpec
#endif
						, ... )
	{
#ifdef LOGGING_ENABLED
#pragma message("Logging is enabled...")
		size_t size = 8 * 1024;
		std::vector<char> buffer( size, '\0' );

		std::string format = Util::Format( "%s\n", formatSpec );

		va_list args;
		va_start( args, formatSpec );
		::vsnprintf_s( &buffer[0], size, size, format.c_str(), args );

		std::string msg( buffer.data() );
		Log::Instance().Write( msg );
#endif
	}

}	// end of namespace Logging



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
	const long glSCHEMA_VERSION = 2;
	const long dbSchemaVersionWhenAttributeCollectionWasIntroduced = 129;


	VectorOfString GetCurrentTableNames( bool excludeUserTables = false )
	{
		VectorOfString names;

		if ( !excludeUserTables )
		{
			names.push_back( gstrATTRIBUTE_SET_NAME );
		}

		names.push_back( gstrATTRIBUTE_SET_FOR_FILE );
		names.push_back( gstrATTRIBUTE_NAME );
		names.push_back( gstrATTRIBUTE_TYPE );
		names.push_back( gstrATTRIBUTE_INSTANCE_TYPE );
		names.push_back( gstrATTRIBUTE );
		names.push_back( gstrRASTER_ZONE );

		return names;
	}

	VectorOfString GetTables_v1( bool bAddUserTables )
	{
		VectorOfString tables;
	
		if ( bAddUserTables )
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


	std::string GetVersionInsertStatement( long schemaVersion )
	{
		return "INSERT INTO [DBInfo] ([Name], [Value]) VALUES ('" + 
				gstrSCHEMA_VERSION_NAME + "', '" + asString(schemaVersion) + "' )";
	}

	std::string GetVersionUpdateStatement(long schemaVersion )
	{
		char buffer[255];
		_snprintf_s( buffer, 
					 sizeof(buffer), 
					 sizeof(buffer) - 1, 
					 "UPDATE [DBInfo] SET Value='%d' where Name='AttributeCollectionSchemaVersion';",
					 schemaVersion );

		return buffer;
	}

	template <typename T>
	void AppendToVector( T& dest, const T& source )
	{
		dest.insert( dest.end(), source.begin(), source.end() );
	}


	VectorOfString GetSchema_v1( bool bAddUserTables )
	{
		VectorOfString queries = GetTables_v1( bAddUserTables );
		AppendToVector( queries, GetIndexes_v1() );
		AppendToVector( queries, GetForeignKeys_v1() );

		return queries;
	}

	VectorOfString GetSchema_v2( bool bAddUserTables )
	{
		VectorOfString queries = GetSchema_v1( bAddUserTables );
		queries.push_back( gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN );

		return queries;
	}

	VectorOfString GetCurrentSchema( bool bAddUserTables = true )
	{
		return GetSchema_v2( bAddUserTables );
	}

	
	//-------------------------------------------------------------------------------------------------
	// Schema update functions
	//-------------------------------------------------------------------------------------------------
	int UpdateToSchemaVersion1( _ConnectionPtr ipConnection, long* pnNumSteps )
	{
		try
		{
			const int nNewSchemaVersion = 1;
	
			if (pnNumSteps != nullptr)
			{
				*pnNumSteps += 3;
				return nNewSchemaVersion;
			}
	
			vector<string> queries = GetSchema_v1( true );
			queries.emplace_back( GetVersionInsertStatement( nNewSchemaVersion ) );
			executeVectorOfSQL(ipConnection, queries);
	
			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38511");
	}

	int UpdateToSchemaVersion2( _ConnectionPtr ipConnection, long* pnNumSteps )
	{
		try
		{
			const int nNewSchemaVersion = 2;

			if (pnNumSteps != nullptr)
			{
				*pnNumSteps += 3;
				return nNewSchemaVersion;
			}
	
			vector<string> queries;
			queries.push_back( gstrADD_ATTRIBUTE_SET_FOR_FILE_VOA_COLUMN );
			queries.emplace_back( GetVersionUpdateStatement( nNewSchemaVersion ) );
			executeVectorOfSQL(ipConnection, queries);
	
			return nNewSchemaVersion;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38888");
	}

}		// end of anonymous namespace
//-------------------------------------------------------------------------------------------------


//-------------------------------------------------------------------------------------------------
// CAttributeDBMgr
//-------------------------------------------------------------------------------------------------
CAttributeDBMgr::CAttributeDBMgr()	
: m_ipFAMDB(nullptr)
, m_ipDBConnection(nullptr)
, m_nNumberOfRetries(0)
, m_dRetryTimeout(0.0)
{
}
//-------------------------------------------------------------------------------------------------
CAttributeDBMgr::~CAttributeDBMgr()
{
	try
	{
		m_ipFAMDB = nullptr;
		m_ipDBConnection = nullptr;
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
		m_ipFAMDB = nullptr;
		m_ipDBConnection = nullptr;
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

		ASSERT_ARGUMENT("ELI38518", pstrComponentDescription != nullptr);

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

		ASSERT_ARGUMENT("ELI38520", pbValue != nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI38522", ipDB != nullptr);

		// Create the connection object
		_ConnectionPtr ipDBConnection(__uuidof( Connection ));
		ASSERT_RESOURCE_ALLOCATION("ELI38523", ipDBConnection != nullptr);

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
									setIfExists);
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

		ASSERT_ARGUMENT("ELI38525", pbSchemaExists != nullptr);

		// Make DB a smart pointer
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI38526", ipDB != nullptr);

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
		ASSERT_RESOURCE_ALLOCATION( "ELI38527", ipDBConnection != nullptr );
		
		string strDatabaseServer = asString(ipDB->DatabaseServer);
		string strDatabaseName = asString(ipDB->DatabaseName);

		// create the connection string
		string strConnectionString = createConnectionString( strDatabaseServer, 
															 strDatabaseName );
		ipDBConnection->Open( strConnectionString.c_str(), "", "", adConnectUnspecified );

		VectorOfString tableNames = GetCurrentTableNames( asCppBool(bRetainUserTables) );
		dropTablesInVector( ipDBConnection, tableNames );

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
			m_ipDBConnection = nullptr;
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
		ASSERT_RESOURCE_ALLOCATION("ELI38530", ipDBInfoRows != nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI38532", ipTables != nullptr);

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
		ASSERT_ARGUMENT("ELI38534", ipDB != nullptr);

		_ConnectionPtr ipConnection(pConnection);
		ASSERT_ARGUMENT("ELI38535", ipConnection != nullptr);

		ASSERT_ARGUMENT("ELI38536", pnProdSchemaVersion != nullptr);

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
		ASSERT_ARGUMENT("ELI38541", newVal != nullptr);

		// Only update if it is a new value
		if (m_ipFAMDB != newVal)
		{
			m_ipFAMDB = newVal;
			m_ipFAMDB->GetConnectionRetrySettings(&m_nNumberOfRetries, &m_dRetryTimeout);
		
			// Reset the database connection
			m_ipDBConnection = nullptr;
		}
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38540");
}
//-------------------------------------------------------------------------------------------------

namespace
{
	void SaveStatement( const std::string& saveFileName, 
						const std::string& 
#ifdef LOGGING_ENABLED
						statement 
#endif
					  )
	{
		if ( saveFileName.empty() )
			return;
#ifdef LOGGING_ENABLED
		std::ofstream ofile( saveFileName.c_str() );
		if ( ofile.is_open() )
		{
			ofile << statement;
			ofile.flush();
			ofile.close();
		}
#endif
	}
	// This function escapes any single quotes in the input.
	std::string SqlSanitizeInput( const std::string& input )
	{
		std::string sanitized( input );
		replaceVariable( sanitized, "'", "''" );	// replace single quote (') with ''
		return sanitized;
	}


	std::string GetInsertRootASFFStatement( BSTR bstrAttributeSetName, long fileID )
	{
		std::string attributeSetName = SqlSanitizeInput(asString(bstrAttributeSetName));

		std::string insert = 
			"SET NOCOUNT ON \n"
			"DECLARE @AttributeSetName_Description AS NVARCHAR(255);\n"
			"DECLARE @FileID AS INT;\n"
			"DECLARE @AttributeSetName_ID AS BIGINT;\n"
			"DECLARE @AttributeSetForFile_ID AS BIGINT;\n"
			"DECLARE @FileTaskSessionID AS BIGINT;\n"
			"\n";

		insert += Util::Format( "SELECT @AttributeSetName_Description='%s';\n"
								"SELECT @FileID=%d;\n",
								attributeSetName.c_str(),
								fileID );
		insert += 
			"\n"
			"BEGIN TRY\n"
			"	SELECT @AttributeSetName_ID=(SELECT TOP 1 [ID] FROM [dbo].[AttributeSetName] "
			"		WHERE [Description]=@AttributeSetName_Description)\n"
			"	if @AttributeSetName_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO [dbo].[AttributeSetName] ([Description]) VALUES (@AttributeSetName_Description);\n"
			"		SELECT @AttributeSetName_ID = SCOPE_IDENTITY()\n"
			"	end\n"
			"\n"
			"	SELECT @FileTaskSessionID=(SELECT TOP 1 [ID] from [dbo].[FileTaskSession] \n"
			"		WHERE [FileID]=@FileID ORDER BY [ID] DESC) \n"
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

			static int counter = 0;
			SaveStatement( Util::Format("c:\\temp\\insertRootASFF%04d.txt", counter).c_str(), insert );
			++counter;

			return insert;
	}

	std::string GetInsertAttribute( IAttributePtr ipAttribute,
									BSTR bstrAttributeSetName,
									longlong parentAttributeID,
									longlong owningAttributeSetForFileID )
	{
		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		std::string value = SqlSanitizeInput(asString(ipValue->String));
		std::string attributeName = SqlSanitizeInput(asString(ipAttribute->GetName()));
		std::string attributeType = SqlSanitizeInput(asString(ipAttribute->GetType()));
		std::string attributeSetName = SqlSanitizeInput(asString(bstrAttributeSetName));

		IIdentifiableObjectPtr ipIdentifiable(ipAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38642", nullptr != ipIdentifiable);			
		std::string guid = asString(ipIdentifiable->InstanceGUID);

		Logging::WriteToLog( "%s- Value: %s, attributeName: %s, attributeType: %s, "
							 "attributeSetName: %s, parentAttributeID: %lld, GUID: %s, ASFF ID: %lld",
							 __FUNCTION__,
							 value.c_str(), 
							 attributeName.c_str(),
							 attributeType.c_str(),
							 attributeSetName.c_str(),
							 parentAttributeID,
							 guid.c_str(),
							 owningAttributeSetForFileID );

		std::string insert = 
						  "SET NOCOUNT ON \n"
						  "DECLARE  @AttributeName_Name AS NVARCHAR(255); \n"
						  "DECLARE  @AttributeType_Type AS NVARCHAR(255); \n"
						  "DECLARE  @AttributeSetName_Description AS NVARCHAR(255); \n"
						  "DECLARE  @Attribute_Value AS NVARCHAR(MAX); \n"
						  "DECLARE  @Attribute_GUID AS UNIQUEIDENTIFIER; \n"
						  "DECLARE  @Attribute_ParentAttributeID AS BIGINT; \n"
						  
						  "DECLARE @AttributeSetName_ID AS BIGINT; \n"
						  "DECLARE @AttributeSetForFile_ID AS BIGINT; \n"
						  "DECLARE @AttributeName_ID AS BIGINT; \n"
						  "DECLARE @AttributeType_ID AS BIGINT; \n"
						  "DECLARE @Attribute_ID AS BIGINT; \n";

		std::string parentID = parentAttributeID == 0 ? "null" : Util::Format("%lld", parentAttributeID );
		std::string args = 
			Util::Format( "SELECT @AttributeName_Name='%s'; \n"
						  "SELECT @AttributeType_Type='%s'; \n"
						  "SELECT @AttributeSetName_Description='%s'; \n"
						  "SELECT @Attribute_Value='%s'; \n"
						  "SELECT @Attribute_GUID='%s'; \n"
						  "SELECT @Attribute_ParentAttributeID=%s; \n"
						  "SELECT @AttributeSetForFile_ID=%ld",
						  attributeName.c_str(),
						  attributeType.c_str(),
						  attributeSetName.c_str(),
						  value.c_str(),
						  guid.c_str(),
						  parentID.c_str(),
						  owningAttributeSetForFileID );

		insert += args;
		insert += 		  "SELECT @AttributeSetName_ID=null; \n"
						  "SELECT @AttributeName_ID=null; \n"
						  "SELECT @AttributeType_ID=null; \n"

						  "BEGIN TRY \n"
						  "SELECT @AttributeSetName_ID=(SELECT TOP 1 [ID] FROM [dbo].[AttributeSetName] \n"
						  "WHERE Description=@AttributeSetName_Description)\n"
						  "if @AttributeSetName_ID IS NULL\n"
						  "begin\n"
						  "    INSERT INTO [dbo].[AttributeSetName] ([Description]) VALUES (@AttributeSetName_Description);\n"
						  "    SELECT @AttributeSetName_ID = SCOPE_IDENTITY()\n"
						  "end\n"
						  "\n"	
						  "SELECT @AttributeName_ID = (SELECT TOP 1 [ID] FROM [dbo].[AttributeName] WHERE [Name]=@AttributeName_Name)\n"
						  "if @AttributeName_ID IS NULL\n"
						  "begin\n"
						  "    INSERT INTO [dbo].[AttributeName] ([Name]) VALUES (@AttributeName_Name);\n"
						  "    SELECT @AttributeName_ID = SCOPE_IDENTITY()\n"
						  "end\n"
						  "\n"
						  "SELECT @AttributeType_ID = (SELECT TOP 1 [ID] from [dbo].[AttributeType] WHERE [Type]=@AttributeType_Type)\n"
						  "if @AttributeType_ID IS NULL\n"
						  "begin\n"
						  "    INSERT INTO [dbo].[AttributeType] ([Type]) VALUES (@AttributeType_Type);\n"
						  "    SELECT @AttributeType_ID = SCOPE_IDENTITY()\n"
						  "end\n"
						  "\n"	
						  "INSERT INTO [dbo].[Attribute] ([AttributeSetForFileID], [AttributeNameID], [Value], [ParentAttributeID], [GUID]) \n"
						  "VALUES (@AttributeSetForFile_ID, @AttributeName_ID, @Attribute_Value, @Attribute_ParentAttributeID, @Attribute_GUID);\n"
						  "SELECT @Attribute_ID = SCOPE_IDENTITY()\n"
						  "\n"
						  "if not exists (SELECT * FROM [dbo].[AttributeInstanceType] WHERE [AttributeID]=@Attribute_ID AND [AttributeTypeID]=@AttributeType_ID)\n"
						  "BEGIN\n"
						  "    INSERT INTO [dbo].[AttributeInstanceType] ([AttributeID], [AttributeTypeID]) VALUES (@Attribute_ID, @AttributeType_ID)\n"
						  "END\n"
						  "\n"	
						  "SELECT AttributeID=@Attribute_ID\n"
						  "SET NOCOUNT OFF\n"
						"END TRY \n"
						"BEGIN CATCH \n"
							"SET NOCOUNT OFF \n"
							"DECLARE @ErrorMessage NVARCHAR(4000); \n"
							"DECLARE @ErrorSeverity INT; \n"
							"DECLARE @ErrorState INT; \n"
							"SELECT \n"
								"@ErrorMessage = ERROR_MESSAGE(), \n"
								"@ErrorSeverity = ERROR_SEVERITY(), \n"
								"@ErrorState = ERROR_STATE(); \n"
							"RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState) \n"
						"END CATCH \n";

		static int counter = 0;
		SaveStatement( Util::Format("c:\\temp\\insertSubAttribute%04d.txt", counter).c_str(), insert );
		++counter;

		return insert;
	}


	std::string GetInsertRasterZoneStatement( const std::string& attributeID,
											  IRasterZonePtr ipZone )
	{
		ILongRectanglePtr ipRect = ipZone->GetRectangularBounds( nullptr );
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

		std::string insert = Util::Format( "INSERT INTO [dbo].[RasterZone] "
										   "([AttributeID], [Top], [Left], [Bottom], [Right], "
										   "[StartX], [StartY], [EndX], [EndY], [PageNumber], [Height]) "
										   "VALUES (%s, %d, %d, %d, %d, %d, %d, %d, %d, %d, %d);",
										   attributeID.c_str(),
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

		static int counter = 0;
		SaveStatement( Util::Format("c:\\temp\\insertRZ%04d.txt", counter).c_str(), insert );
		++counter;
		return insert;
	}

	// This query gets one top-level attribute.
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
						  asString(attributeSetName).c_str(),
						  abs( relativeIndex ) );								// offset (index) into result set, get Nth

		static int counter = 0;
		SaveStatement( Util::Format("c:\\temp\\queryASFF%04d.txt", counter).c_str(), query );
		++counter;
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
		ASSERT_RESOURCE_ALLOCATION( eliCode, nullptr != instance );
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
		ASSERT_RESOURCE_ALLOCATION( eliCode, nullptr != pT );
		
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
		Logging::WriteToLog( "%s- executing command...", __FUNCTION__ );
		return ipConnection->Execute( cmd.c_str(), nullptr, adCmdText );
	}

	FieldsPtr GetFieldsForQuery( const std::string& query, ADODB::_ConnectionPtr ipConnection )
	{
		ADODB::_RecordsetPtr ipASFF = ExecuteCmd( query, ipConnection );
		ASSERT_RUNTIME_CONDITION( "ELI38763", 
								  RecordsInSet(ipASFF), 
								  Util::Format( "Insert failed: %s", query.c_str() ) );

		FieldsPtr ipFields = AssignComPtr( ipASFF->Fields, "ELI38764" );
		return ipFields;
	}

	longlong ExecuteRootInsertASFF( const std::string& insert, ADODB::_ConnectionPtr ipConnection )
	{
		Logging::WriteToLog( "\n%s- insert statement: %s", __FUNCTION__, insert.c_str() );

		ADODB::_RecordsetPtr ipRS = ExecuteCmd( insert.c_str(), ipConnection );
		ASSERT_RESOURCE_ALLOCATION(	"ELI38886", VARIANT_FALSE == ipRS->adoEOF );

		FieldsPtr ipFields = AssignComPtr( ipRS->Fields, "ELI38887" );
		longlong ID = getLongLongField( ipFields, "ID" );
		Logging::WriteToLog( "%s- Returned Root ASFF ID: %lld", __FUNCTION__, ID );

		ipRS->Close();

		return ID;
	}

}		// end of anonymous namespace

// ------------------------------------------------------------------------------------------------
void CAttributeDBMgr::SaveVoaDataInASFF( IIUnknownVector* pAttributes, longlong rootASFF_ID )
{
//	if ( 0 == pAttributes->Size() )
//		return;

	try
	{
		std::string query = Util::Format( "SELECT * FROM [dbo].[AttributeSetForFile] "
										  "WHERE [ID] = %lld",
										  rootASFF_ID );
		Logging::WriteToLog( "%s- set up to write VOA, query is: %s", __FUNCTION__, query.c_str() );

		ADODB::_RecordsetPtr ipASFF( __uuidof(Recordset) );
		ASSERT_RESOURCE_ALLOCATION( "ELI38804", nullptr != ipASFF );

		auto connectParam = _variant_t( (IDispatch*)getDBConnection(), true );
		ipASFF->Open( query.c_str(), 
					  connectParam, 
					  adOpenDynamic, 
					  adLockOptimistic, 
					  adCmdText );
		ASSERT_RUNTIME_CONDITION( "ELI38805", 
								  RecordsInSet(ipASFF), 
								  Util::Format( "Insert failed: %s", query.c_str() ) );

		FieldsPtr ipFields = AssignComPtr( ipASFF->Fields, "ELI38764" );

		// Now prepare the VOA for save
		string storageManagerIID = asString(CLSID_AttributeStorageManager);
		IIUnknownVectorPtr pAttributesClone = pAttributes->PrepareForStorage( storageManagerIID.c_str() );

		IPersistStreamPtr ipPersistObj = pAttributesClone;
		// TODO - compress the spatial string!
		setIPersistObjToField( ipFields, "VOA", ipPersistObj );
		ipASFF->Update();
	}
	catch (...)
	{
		UCLIDException ue;
		std::string message( 
			Util::Format("ADO exception while saving VOA to AttributeSetForFile, "
						 "ID: %lld", rootASFF_ID ) );
		Logging::WriteToLog( message.c_str() );
		ue.asString( message );
		throw ue;
	}
}
// ------------------------------------------------------------------------------------------------
long long CAttributeDBMgr::SaveAttribute( IAttributePtr ipAttribute, 
										  VARIANT_BOOL storeRasterZone,
										  const std::string& insert )
{
	Logging::WriteToLog( "\n%s-", __FUNCTION__ );

	ADODB::_RecordsetPtr ipRS = ExecuteCmd( insert.c_str(), getDBConnection() );
	ASSERT_RESOURCE_ALLOCATION(	"ELI38670", VARIANT_FALSE == ipRS->adoEOF );

	FieldsPtr ipFields = AssignComPtr( ipRS->Fields, "ELI38631" );
	long long parentID = getLongLongField( ipFields, "AttributeID" );
	Logging::WriteToLog( "%s- Returned Parent ID: %lld", __FUNCTION__, parentID );

	ipRS->Close();

	ISpatialStringPtr ipValue = AssignComPtr( ipAttribute->GetValue(), "ELI38714" );
	bool hasSpatialInfo = asCppBool( ipValue->HasSpatialInfo() );
	bool storeInfo = asCppBool(storeRasterZone);
	if ( hasSpatialInfo && true == storeInfo )
	{
		IIUnknownVectorPtr ipZones = ipValue->GetOriginalImageRasterZones();	// NOTE: can't return nullptr, throws
		Logging::WriteToLog( "%s- Number of Raster Zones: %d", __FUNCTION__, ipZones->Size() );
		for ( long index = 0; index < ipZones->Size(); ++index )
		{
			IRasterZonePtr ipZone = AssignComPtr( ipZones->At(index), "ELI38669" );

			const std::string parentAttrID( Util::Format( "%lld", parentID ) );
			std::string zoneInsert = GetInsertRasterZoneStatement( parentAttrID, ipZone );

			Logging::WriteToLog( "%s- ^^^^^^^^^ Saving raster zone[%d]: %s", __FUNCTION__, index, zoneInsert.c_str() );
			ExecuteCmd( zoneInsert.c_str(), getDBConnection() );
		}
	}

	return parentID;
}
// ------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetForFile( long fileID,
														    BSTR bstrAttributeSetName,
														    IIUnknownVector* pAttributes,
															VARIANT_BOOL storeRasterZone )
{
	longlong rootASFF_ID = 0;

	try
	{
		ASSERT_ARGUMENT("ELI38553", pAttributes != nullptr);
		ASSERT_ARGUMENT("ELI38554", fileID > 0 );

		Logging::WriteToLog( "%s- starting, fileID: %d, attribute set name: %s, Size: %d",
							 __FUNCTION__,
							 fileID,
							 asString(bstrAttributeSetName).c_str(),
							 pAttributes->Size() );

		TransactionGuard tg( getDBConnection(), adXactRepeatableRead, nullptr );

		auto insertRootASFF = GetInsertRootASFFStatement( bstrAttributeSetName, fileID );
		rootASFF_ID = ExecuteRootInsertASFF( insertRootASFF, getDBConnection() );
		SaveVoaDataInASFF( pAttributes, rootASFF_ID );

		for ( long i = 0; i < pAttributes->Size(); ++i )
		{
			Logging::WriteToLog( "\n%s- __________ Saving parent attribute[%d]", __FUNCTION__, i );
			IAttributePtr ipAttribute = AssignComPtr( pAttributes->At(i), "ELI38693" );

			const longlong topLevelParentAttributeID = 0;
			std::string insert = GetInsertAttribute( ipAttribute, 
													 bstrAttributeSetName, 
													 topLevelParentAttributeID,
													 rootASFF_ID );
			long long parentID = SaveAttribute( ipAttribute, 
												storeRasterZone,
												insert );

			IIUnknownVectorPtr ipSubAttrs = ipAttribute->GetSubAttributes();
			Logging::WriteToLog( "%s- ----- Saving subattributes, Size: %d", __FUNCTION__, ipSubAttrs->Size() );
			for ( long index = 0; index < ipSubAttrs->Size(); ++index )
			{
				Logging::WriteToLog( "%s- **** Saving sub-attribute[%d]", __FUNCTION__, index );

				IAttributePtr ipSubAttribute = AssignComPtr( ipSubAttrs->At( index ), "ELI38717" );
				std::string cmd = GetInsertAttribute( ipSubAttribute, 
													  bstrAttributeSetName, 
													  parentID, 
													  rootASFF_ID );
				SaveAttribute( ipAttribute, 
							   storeRasterZone,
							   cmd );
			}
		}

		tg.CommitTrans();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38557");
}
//-------------------------------------------------------------------------------------------------
// relativeIndex: -1 for most recent, 1 for oldest
// decrement most recent value to get next most recent (-2)
// increment oldest value to get next oldest (2)
// Zero is an illegal relativeIndex value.
STDMETHODIMP CAttributeDBMgr::GetAttributeSetForFile(IIUnknownVector** ppAttributes, 
													 long fileID, 
													 BSTR attributeSetName,
													 long relativeIndex)
{
	try
	{
		ASSERT_ARGUMENT("ELI38618", relativeIndex != 0);
		ASSERT_ARGUMENT("ELI38668", ppAttributes != nullptr);

		auto query( GetQueryForAttributeSetForFile( fileID, attributeSetName, relativeIndex ) );

		FieldsPtr ipFields = GetFieldsForQuery( query, getDBConnection() );
		IPersistStreamPtr ipStream = getIPersistObjFromField( ipFields, "VOA" );

		*ppAttributes = (IIUnknownVectorPtr)ipStream.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38619");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetName(BSTR name, 
														long long* pAttributeSetNameID)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38630", name != nullptr );
		ASSERT_ARGUMENT( "ELI38676", pAttributeSetNameID != nullptr );

		std::string newName( asString(name) );
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
		ASSERT_ARGUMENT( "ELI38627", attributeSetName != nullptr );
		ASSERT_ARGUMENT( "ELI38628", newName != nullptr );

		std::string currentName( asString(attributeSetName) );
		std::string changeNameTo( asString(newName) );

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
		ASSERT_ARGUMENT( "ELI38624", attributeSetName != nullptr );

		std::string name( asString(attributeSetName) );
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
		ASSERT_ARGUMENT("ELI38617", nullptr != ippNames);
		auto ipAttributeSetNames = MakeIPtr<IStrToStrMapPtr>(CLSID_StrToStrMap, "ELI38621");

		std::string query( "SELECT [ID], [Description] FROM [dbo].[AttributeSetName];" );
		ADODB::_RecordsetPtr pRecords = m_ipFAMDB->GetResultsForQuery( query.c_str() );
		while ( RecordsInSet(pRecords) )
		{
			FieldsPtr pFields = AssignComPtr( pRecords->Fields, "ELI38622" );

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
ADODB::_ConnectionPtr CAttributeDBMgr::getDBConnection()
{
	// If the FAMDB is not set throw an exception
	if (m_ipFAMDB == nullptr)
	{
		UCLIDException ue("ELI38542",
			"FAMDB pointer has not been initialized! Unable to open connection.");
		throw ue;
	}

	// Check if connection has been created
	if (m_ipDBConnection == nullptr)
	{
		m_ipDBConnection.CreateInstance(__uuidof( Connection));
		ASSERT_RESOURCE_ALLOCATION("ELI38543", m_ipDBConnection != nullptr);
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
	ASSERT_RESOURCE_ALLOCATION("ELI38545", m_ipFAMDB != nullptr);

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
//-------------------------------------------------------------------------------------------------
