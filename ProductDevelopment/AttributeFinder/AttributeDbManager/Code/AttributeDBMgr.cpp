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
	// This function escapes any single quotes in the input.
	std::string SqlSanitizeInput( const std::string& input )
	{
		std::string sanitized( input );
		replaceVariable( sanitized, "'", "''" );	// replace single quote (') with ''
		return sanitized;
	}


	// Split a string into a vector of string, using delimiter char.
	// For an empty string, or a string that doesn't have the delimiter
	// character in it, this routine will return a vector with only the 
	// original source string in it.
	// Note that this routine makes use of well-known std::string behavior:
	// string::substr( start, len ) - if len > string::size(), the entire
	// string from startPos to end is returned.
	VectorOfString Split( const std::string& source, const char delimiter )
	{
		VectorOfString results;
		size_t pos = 0;
		size_t startPos = 0;
		while ( true )
		{
			pos = source.find( delimiter, startPos );
			const auto length = pos - startPos;
			results.push_back( source.substr( startPos, length ) );
			if ( pos == std::string::npos)
			{
				break;
			}

			startPos = pos + 1;
		}

		return std::move( results );
	}

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

	std::string GetInsertAttributeQuery( IAttributePtr ipAttribute,
										 longlong parentAttributeID,
										 longlong owningAttributeSetForFileID )
	{
		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		std::string value = SqlSanitizeInput(asString(ipValue->String));
		std::string attributeName = SqlSanitizeInput(asString(ipAttribute->GetName()));
		std::string attributeType = SqlSanitizeInput(asString(ipAttribute->GetType()));

		IIdentifiableObjectPtr ipIdentifiable(ipAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38642", nullptr != ipIdentifiable);			
		std::string guid = asString(ipIdentifiable->InstanceGUID);

		std::string insert = 
						  "SET NOCOUNT ON \n"
						  "DECLARE  @AttributeName_Name AS NVARCHAR(255); \n"
						  "DECLARE  @AttributeType_Type AS NVARCHAR(255); \n"
						  "DECLARE  @Attribute_Value AS NVARCHAR(MAX); \n"
						  "DECLARE  @Attribute_GUID AS UNIQUEIDENTIFIER; \n"
						  "DECLARE  @Attribute_ParentAttributeID AS BIGINT; \n"
						  
						  "DECLARE @AttributeSetForFile_ID AS BIGINT; \n"
						  "DECLARE @AttributeName_ID AS BIGINT; \n"
						  "DECLARE @AttributeType_ID AS BIGINT; \n"
						  "DECLARE @Attribute_ID AS BIGINT; \n"
						  "DECLARE @TypeTable table(idx int IDENTITY(1,1), TypeName NVARCHAR(255)) \n";

		VectorOfString typeNames = Split( attributeType, '+' );
		for ( size_t i = 0; i < typeNames.size(); ++i )
		{
			std::string insertToTable =
						  Util::Format( "INSERT INTO @TypeTable (TypeName) VALUES ('%s') \n", 
										typeNames[i].c_str() );

			insert += insertToTable;
		}

		std::string parentID = parentAttributeID <= 0 ? "null" : Util::Format("%lld", parentAttributeID );
		std::string args = 
			Util::Format( "SELECT @AttributeName_Name='%s'; \n"
						  "SELECT @AttributeType_Type='%s'; \n"
						  "SELECT @Attribute_Value='%s'; \n"
						  "SELECT @Attribute_GUID='%s'; \n"
						  "SELECT @Attribute_ParentAttributeID=%s; \n"
						  "SELECT @AttributeSetForFile_ID=%ld \n",
						  attributeName.c_str(),
						  attributeType.c_str(),
						  value.c_str(),
						  guid.c_str(),
						  parentID.c_str(),
						  owningAttributeSetForFileID );

		insert += args;
		insert += 		  "SELECT @AttributeName_ID=null; \n"
						  "SELECT @AttributeType_ID=null; \n"

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
  						  "INSERT INTO [dbo].[Attribute] ([AttributeSetForFileID], [AttributeNameID], [Value], [ParentAttributeID], [GUID]) \n"
						  "VALUES (@AttributeSetForFile_ID, @AttributeName_ID, @Attribute_Value, @Attribute_ParentAttributeID, @Attribute_GUID);\n"
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
		return ipConnection->Execute( cmd.c_str(), nullptr, adCmdText );
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
void CAttributeDBMgr::SaveVoaDataInASFF( IIUnknownVector* pAttributes, longlong llRootASFF_ID )
{
	try
	{
		std::string query = Util::Format( "SELECT * FROM [dbo].[AttributeSetForFile] "
										  "WHERE [ID] = %lld",
										  llRootASFF_ID );

		ADODB::_RecordsetPtr ipASFF( __uuidof(Recordset) );
		ASSERT_RESOURCE_ALLOCATION( "ELI38804", nullptr != ipASFF );

		auto connectParam = _variant_t( (IDispatch*)getDBConnection(), true );
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

		FieldsPtr ipFields = AssignComPtr( ipASFF->Fields, "ELI38764" );

		// Now prepare the VOA for save
		string storageManagerIID = asString(CLSID_AttributeStorageManager);
		IIUnknownVectorPtr pAttributesClone = pAttributes->PrepareForStorage( storageManagerIID.c_str() );

		IPersistStreamPtr ipPersistObj = pAttributesClone;

#ifdef UNCOMPRESSED_STREAM
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
// ------------------------------------------------------------------------------------------------
long long CAttributeDBMgr::SaveAttribute( IAttributePtr ipAttribute, 
										  bool bStoreRasterZone,
										  const std::string& strInsert )
{
	ADODB::_RecordsetPtr ipRS = ExecuteCmd( strInsert.c_str(), getDBConnection() );
	ASSERT_RESOURCE_ALLOCATION(	"ELI38670", VARIANT_FALSE == ipRS->adoEOF );

	FieldsPtr ipFields = AssignComPtr( ipRS->Fields, "ELI38631" );
	long long llParentID = getLongLongField( ipFields, "AttributeID" );

	ipRS->Close();

	ISpatialStringPtr ipValue = AssignComPtr( ipAttribute->GetValue(), "ELI38714" );
	bool bHasSpatialInfo = asCppBool( ipValue->HasSpatialInfo() );
	if ( bHasSpatialInfo && true == bStoreRasterZone )
	{
		IIUnknownVectorPtr ipZones = ipValue->GetOriginalImageRasterZones();	// NOTE: can't return nullptr, throws
		for ( long index = 0; index < ipZones->Size(); ++index )
		{
			IRasterZonePtr ipZone = AssignComPtr( ipZones->At(index), "ELI38669" );

			const std::string parentAttrID( Util::Format( "%lld", llParentID ) );
			std::string zoneInsert = GetInsertRasterZoneStatement( parentAttrID, ipZone );

			ExecuteCmd( zoneInsert.c_str(), getDBConnection() );
		}
	}

	return llParentID;
}
//-------------------------------------------------------------------------------------------------
void CAttributeDBMgr::storeAttributeData( IIUnknownVectorPtr ipAttributes, 
										  bool bStoreRasterZone,
										  bool bStoreEmptyAttributes,
										  long long llRootASFF_ID, 
										  long long llParentAttrID/* = 0*/ )
{
	for ( long i = 0; i < ipAttributes->Size(); ++i )
	{
		IAttributePtr ipAttribute = AssignComPtr( ipAttributes->At(i), "ELI38693" );
		if ( true == AttributeIsEmpty( ipAttribute, bStoreRasterZone, bStoreEmptyAttributes ) )
		{
			continue;
		}

		std::string strInsertQuery = GetInsertAttributeQuery( ipAttribute, 
															  llParentAttrID,
															  llRootASFF_ID);
		long long llAttrID = SaveAttribute( ipAttribute, 
											bStoreRasterZone,
											strInsertQuery );

		IIUnknownVectorPtr ipSubAttrs = ipAttribute->GetSubAttributes();
		storeAttributeData(ipSubAttrs, bStoreRasterZone, bStoreEmptyAttributes, llRootASFF_ID, llAttrID);
	}
}


bool CAttributeDBMgr::CreateNewAttributeSetForFile_Internal( bool bDbLocked,
															 long nFileTaskSessionID,
  														     BSTR bstrAttributeSetName,
  														     IIUnknownVector* pAttributes,
  															 VARIANT_BOOL vbStoreRasterZone,
  															 VARIANT_BOOL vbStoreEmptyAttributes )
{
	try
	{
		try
		{
			IIUnknownVectorPtr ipAttributes(pAttributes);
			ASSERT_RESOURCE_ALLOCATION("ELI38959", ipAttributes != nullptr);
			ASSERT_ARGUMENT("ELI38553", pAttributes != nullptr);
			ASSERT_ARGUMENT("ELI38554", nFileTaskSessionID > 0 );

			std::string strSetName = SqlSanitizeInput(asString(bstrAttributeSetName));

			TransactionGuard tg( getDBConnection(), adXactRepeatableRead, nullptr );

			longlong llSetNameID = GetAttributeSetID( strSetName, getDBConnection() );
			auto strInsertRootASFF = GetInsertRootASFFStatement( llSetNameID, nFileTaskSessionID );
			longlong llRootASFF_ID = ExecuteRootInsertASFF( strInsertRootASFF, getDBConnection() );
			SaveVoaDataInASFF( ipAttributes, llRootASFF_ID );

			storeAttributeData( ipAttributes, 
								asCppBool(vbStoreRasterZone), 
								asCppBool(vbStoreEmptyAttributes), 
								llRootASFF_ID );

			tg.CommitTrans();

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
															VARIANT_BOOL vbStoreRasterZone,
															VARIANT_BOOL vbStoreEmptyAttributes )
{
	try
	{
		const bool bDbNotLocked = false;
		auto bRet = CreateNewAttributeSetForFile_Internal( bDbNotLocked, 
														   nFileTaskSessionID,
														   bstrAttributeSetName,
														   pAttributes,
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
			ASSERT_ARGUMENT("ELI38668", ppAttributes != nullptr);

			auto strQuery( GetQueryForAttributeSetForFile( fileID, attributeSetName, relativeIndex ) );

#ifdef UNCOMPRESSED_STREAM	
			FieldsPtr ipFields = GetFieldsForQuery( strQuery, getDBConnection() );
			IIUnknownVectorPtr ipAttributes = getIPersistObjFromField( ipFields, "VOA" );
			ASSERT_RESOURCE_ALLOCATION("ELI39173", ipAttributes != nullptr);

			*ppAttributes = ipAttributes.Detach();
#endif		

#ifdef COMPRESSED_STREAM
			FieldsPtr ipFields = GetFieldsForQuery( strQuery, getDBConnection() );

			CComSafeArray<BYTE> saData;
			saData.Attach(ipFields->GetItem("VOA")->GetValue().parray);

			CComSafeArray<BYTE> saData2;
			saData2.Attach(ZipUtil::DecompressAttributes(saData));

			IIUnknownVectorPtr ipAttributes = readObjFromSAFEARRAY(saData2);
			ASSERT_RESOURCE_ALLOCATION("ELI39172", ipAttributes != nullptr);

			*ppAttributes = ipAttributes.Detach();
#endif
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
													 IIUnknownVector** ppAttributes)
{
	try
	{
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
		ASSERT_ARGUMENT( "ELI38630", name != nullptr );
		ASSERT_ARGUMENT( "ELI38676", pAttributeSetNameID != nullptr );

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
