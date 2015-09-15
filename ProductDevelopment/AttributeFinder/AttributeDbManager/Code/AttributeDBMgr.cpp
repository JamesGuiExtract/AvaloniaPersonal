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
#include "DefinedTypes.h"

using namespace ADODB;
using namespace std;

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
	const long glSCHEMA_VERSION = 1;
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

	VectorOfString GetCurrentSchema( bool bAddUserTables = true )
	{
		return GetSchema_v1( bAddUserTables );
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
				break;

			case 1:
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

STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetForFile( long fileID,
														    BSTR /*bstrAttributeSetName*/,
														    IIUnknownVector* ipAttributes,
															VARIANT_BOOL /*storeRasterZone*/ )
{
	try
	{
		ASSERT_ARGUMENT("ELI38553", ipAttributes != nullptr);
		ASSERT_ARGUMENT("ELI38554", fileID > 0 );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38557");

}

// relativeIndex: -1 for most recent, 1 for oldest
// decrement most recent value to get next most recent (-2)
// increment oldest value to get next oldest (2)
// Zero is an illegal relativeIndex value.
STDMETHODIMP CAttributeDBMgr::GetAttributeSetForFile(IIUnknownVector** /*ippAttributes*/, 
													 long /*fileID*/, 
													 BSTR /*attributeSetName*/,
													 long relativeIndex)
{
	try
	{
		ASSERT_ARGUMENT("ELI38618", relativeIndex != 0);
		return Error("Not implemented");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38619");
}

STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetName(BSTR name, 
														long long* pAttributeSetNameID)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38630", name != nullptr );

		std::string newName( asString(name) );
		std::string cmd( Util::Format( "INSERT INTO [dbo].[AttributeSetName] (Description) VALUES ('%s');",
									   newName.c_str() ) );

		m_ipFAMDB->ExecuteCommandQuery( cmd.c_str() );

		if ( nullptr != pAttributeSetNameID )
		{
			*pAttributeSetNameID = 0;

			std::string query( Util::Format( "SELECT ID FROM [dbo].[AttributeSetName] WHERE Description='%s';",
											 newName.c_str() ) );

			ADODB::_RecordsetPtr pRecords = m_ipFAMDB->GetResultsForQuery( query.c_str() );
			if ( VARIANT_FALSE == pRecords->adoEOF )
			{
				FieldsPtr pFields = pRecords->Fields;
				ASSERT_RESOURCE_ALLOCATION("ELI38631", pFields != nullptr);

				long long Id = getLongLongField( pFields, "ID" );
				*pAttributeSetNameID = Id;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38629");
}

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

STDMETHODIMP CAttributeDBMgr::DeleteAttributeSetName(BSTR attributeSetName)
{
	try
	{
		ASSERT_ARGUMENT( "ELI38624", attributeSetName != nullptr );

		std::string name( asString(attributeSetName) );
		ASSERT_ARGUMENT( "ELI38625", !name.empty() );

		std::string cmd( Util::Format( "DELETE FROM  [dbo].[AttributeSetName] WHERE Description='%s';", 
									   name.c_str() ) );

		m_ipFAMDB->ExecuteCommandQuery( cmd.c_str() );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38623");
}

STDMETHODIMP CAttributeDBMgr::GetAllAttributeSetNames(IStrToStrMap** ippNames)
{
	try
	{
		ASSERT_ARGUMENT("ELI38617", nullptr != ippNames);

		std::string query( "SELECT [ID], [Description] FROM [dbo].[AttributeSetName];" );

		ADODB::_RecordsetPtr pRecords = m_ipFAMDB->GetResultsForQuery( query.c_str() );

		IStrToStrMapPtr pAttributeSetNames(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI38621", pAttributeSetNames != nullptr);

		while ( VARIANT_FALSE == pRecords->adoEOF )
		{
			FieldsPtr pFields = pRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI38622", pFields != nullptr);

			std::string description = getStringField( pFields, "Description" );
			long long Id = getLongLongField( pFields, "ID" );
			std::string ID = Util::Format( "%lld", Id );

			pAttributeSetNames->Set( ID.c_str(), description.c_str() );
			
			pRecords->MoveNext();
		}

		*ippNames = pAttributeSetNames.Detach();
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
													    asVariantBool(true) ) );
	// Check against expected version
	if (asLong(value) != glSCHEMA_VERSION)
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
