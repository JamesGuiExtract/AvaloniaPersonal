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

namespace Test		// TODO - temporary test framework
{
	void TestCreateNewAttributeSetForFile( CAttributeDBMgr* thisPtr )
	{
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38664", nullptr != ipAttribute);
	
		long baseID = 18;
		ipAttribute->Name = Util::Format("attributeName%d", baseID).c_str();
		ipAttribute->Type = Util::Format("attributeType%d", baseID).c_str();
		ISpatialStringPtr ipText(CLSID_SpatialString);
		ipText->ReplaceAndDowngradeToNonSpatial( Util::Format("attribute value (spatial string) #%d", baseID).c_str() );
		ipAttribute->Value = ipText;
		BSTR attributeSetName = L"AttributeSetName18";
		const long file_id = baseID;
	
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI38664", nullptr != ipAttributes);
		ipAttributes->PushBack(ipAttribute);
	
		thisPtr->CreateNewAttributeSetForFile( file_id, attributeSetName, ipAttributes, asVariantBool(false) );
	}


	void SaveStatement( const std::string& saveFileName, const std::string& statement )
	{
		if ( saveFileName.empty() )
			return;
#if 0
		std::ofstream ofile( saveFileName.c_str() );
		if ( ofile.is_open() )
		{
			ofile << statement;
			ofile.flush();
			ofile.close();
		}
#endif
	}
}


namespace
{

	std::string GetInsertAttributeSetForFileStatement( IAttributePtr ipAttribute,
													   BSTR bstrAttributeSetName,
													   const std::string& parentAttributeID,
													   long fileID )
	{
		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		std::string value = asString(ipValue->String);
		std::string attributeName = asString(ipAttribute->GetName());
		std::string attributeType = asString(ipAttribute->GetType());
		std::string attributeSetName = asString(bstrAttributeSetName);

		IIdentifiableObjectPtr ipIdentifiable(ipAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI38642", nullptr != ipIdentifiable);			
		std::string guid = asString(ipIdentifiable->InstanceGUID);

		std::string insert = 
			"SET NOCOUNT ON\n"
			"DECLARE  @AttributeName_Name AS NVARCHAR(255);\n"
			"DECLARE  @AttributeType_Type AS NVARCHAR(255);\n"
			"DECLARE  @AttributeSetName_Description AS NVARCHAR(255);\n"
			"DECLARE  @Attribute_Value AS NVARCHAR(MAX);\n"
			"DECLARE  @Attribute_GUID AS UNIQUEIDENTIFIER;\n"
			"DECLARE  @Attribute_ParentAttributeID AS BIGINT;\n"
			"DECLARE  @FileID AS INT;\n"
			"\n"
			"DECLARE @AttributeSetName_ID AS BIGINT;\n"
			"DECLARE @AttributeSetForFile_ID AS BIGINT;\n"
			"DECLARE @AttributeName_ID AS BIGINT;\n"
			"DECLARE @AttributeType_ID AS BIGINT;\n"
			"DECLARE @Attribute_ID AS BIGINT;\n"
			"\n";
			
		insert += Util::Format( "SELECT @AttributeName_Name='%s';\n"
								"SELECT @AttributeType_Type='%s';\n"
								"SELECT @AttributeSetName_Description='%s';\n"
								"SELECT @Attribute_Value='%s';\n"
								"SELECT @Attribute_GUID='%s';\n"
								"SELECT @Attribute_ParentAttributeID=%s;\n"
								"SELECT @FileID=%d;\n",
								attributeName.c_str(),
								attributeType.c_str(),
								attributeSetName.c_str(),
								value.c_str(),
								guid.c_str(),
								parentAttributeID.c_str(),
								fileID );
			
		insert += 
			"\n"
			"SELECT @AttributeSetName_ID=null;\n"
			"SELECT @AttributeSetForFile_ID=null;\n"
			"SELECT @AttributeName_ID=null;\n"
			"SELECT @AttributeType_ID=null;\n"
			"SELECT @Attribute_ID=null;\n"
			"\n"
			"BEGIN TRY\n"
			"	SELECT @AttributeSetName_ID=(SELECT [ID] FROM [dbo].[AttributeSetName] "
			"WHERE Description=@AttributeSetName_Description)\n"
			"	if @AttributeSetName_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO [dbo].[AttributeSetName] ([Description]) VALUES (@AttributeSetName_Description);\n"
			"		SELECT @AttributeSetName_ID = SCOPE_IDENTITY()\n"
			"	end\n"
			"\n"	
			"	SELECT @AttributeSetForFile_ID=(SELECT ID FROM [dbo].[AttributeSetForFile] "
			"WHERE FileTaskSessionID=@FileID AND AttributeSetNameID=@AttributeSetName_ID)\n"
			"	if @AttributeSetForFile_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO AttributeSetForFile ([FileTaskSessionID], [AttributeSetNameID])\n"
			"			VALUES (@FileID, @AttributeSetName_ID);\n"
			"		SELECT @AttributeSetForFile_ID = SCOPE_IDENTITY();\n"
			"\n"		
			"	end\n"
			"	\n"
			"	SELECT @AttributeName_ID = (SELECT ID FROM [dbo].[AttributeName] WHERE [Name]=@AttributeName_Name)\n"
			"	if @AttributeName_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO [dbo].[AttributeName] ([Name]) VALUES (@AttributeName_Name);\n"
			"		SELECT @AttributeName_ID = SCOPE_IDENTITY()\n"
			"	end\n"
			"	\n"
			"	SELECT @AttributeType_ID = (SELECT [ID] from [dbo].[AttributeType] WHERE [Type]=@AttributeType_Type)\n"
			"	if @AttributeType_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO [dbo].[AttributeType] ([Type]) VALUES (@AttributeType_Type);\n"
			"		SELECT @AttributeType_ID = SCOPE_IDENTITY()\n"
			"	end\n"
			"\n"	
			"	SELECT @Attribute_ID = (SELECT ID FROM [dbo].[Attribute] WHERE [Value]=@Attribute_Value AND [GUID]=@Attribute_GUID)\n"
			"	if @Attribute_ID IS NULL\n"
			"	begin\n"
			"		INSERT INTO [dbo].[Attribute] ([AttributeSetForFileID], [AttributeNameID], [Value], [ParentAttributeID], [GUID]) \n"
			"			VALUES (@AttributeSetForFile_ID, @AttributeName_ID, @Attribute_Value, @Attribute_ParentAttributeID, @Attribute_GUID);\n"
			"		SELECT @Attribute_ID = SCOPE_IDENTITY()\n"
			"	end\n"
			"\n"
			"	if not exists (SELECT * FROM [dbo].[AttributeInstanceType] WHERE [AttributeID]=@Attribute_ID AND [AttributeTypeID]=@AttributeType_ID)\n"
			"	BEGIN\n"
			"		INSERT INTO [dbo].[AttributeInstanceType] ([AttributeID], [AttributeTypeID]) VALUES (@Attribute_ID, @AttributeType_ID)\n"
			"	END\n"
			"\n"	
			"	SELECT ID=@AttributeSetForFile_ID, AttributeID=@Attribute_ID\n"
			"	SET NOCOUNT OFF\n"
			"END TRY\n"
			"BEGIN CATCH\n"
			"	SET NOCOUNT OFF\n"
			"	DECLARE @ErrorMessage NVARCHAR(4000);\n"
			"    DECLARE @ErrorSeverity INT;\n"
			"    DECLARE @ErrorState INT;\n"
			"\n"
			"    SELECT\n"
			"        @ErrorMessage = ERROR_MESSAGE(),\n"
			"        @ErrorSeverity = ERROR_SEVERITY(),\n"
			"        @ErrorState = ERROR_STATE();\n"
			"\n"
			"    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState)\n"
			"END CATCH";

			Test::SaveStatement( "c:\\temp\\insertASFF.txt", insert );
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
		Test::SaveStatement( "c:\\temp\\insertRZ.txt", insert );
		return insert;
	}

	std::string GetQueryForAttributeSetForFile( long fileID, BSTR attributeSetName )
	{
		std::string query = 
			Util::Format( "SELECT attr.[Value], attr.[ParentAttributeID], attr.[GUID], \n"
			"attrT.[Type], attrN.[Name], asn.[Description], \n"
			"rz.[Top], rz.[Left], rz.[Bottom], rz.[Right], rz.[StartX], rz.[StartY], \n"
			"rz.[EndX], rz.[EndY], rz.[PageNumber], rz.[Height] \n"
			"FROM AttributeSetForFile asff \n"
			"LEFT JOIN AttributeSetName asn ON asff.AttributeSetNameID=asn.ID\n"
			"JOIN Attribute attr ON asff.ID=attr.AttributeSetForFileID\n"
			"JOIN AttributeName attrN ON attr.AttributeNameID=attrN.ID\n"
			"JOIN AttributeInstanceType ait ON ait.AttributeID=attr.ID\n"
			"JOIN AttributeType attrT ON ait.AttributeTypeID=attrT.ID\n"
			"LEFT JOIN RasterZone rz ON rz.AttributeID=attr.ID\n"
			"WHERE asff.FileTaskSessionID=%ld AND\n"
			"asn.Description='%s' ORDER BY attr.ID DESC;",
			fileID,
			asString(attributeSetName).c_str() );

		Test::SaveStatement( "c:\\temp\\queryASFF.txt", query );
		return query;
	}
}


namespace Test
{
	void TestRasterZoneInsert()
	{
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ipRect->SetBounds(0, 0, 100, 50);

		// Create a raster zone based on this rectangle.
		IRasterZonePtr ipRasterZone(CLSID_RasterZone);
		ipRasterZone->CreateFromLongRectangle(ipRect, 1);

		std::string insert = GetInsertRasterZoneStatement( "1", ipRasterZone );
	}

}

// TODO - add DB locking to all inserters and getters...
STDMETHODIMP CAttributeDBMgr::CreateNewAttributeSetForFile( long fileID,
														    BSTR bstrAttributeSetName,
														    IIUnknownVector* pAttributes,
															VARIANT_BOOL storeRasterZone )
{
	try
	{
		ASSERT_ARGUMENT("ELI38553", pAttributes != nullptr);
		ASSERT_ARGUMENT("ELI38554", fileID > 0 );

		_ConnectionPtr ipConnection = getDBConnection();
		TransactionGuard tg(ipConnection, adXactRepeatableRead, nullptr);

		for ( long i = 0; i < pAttributes->Size(); ++i )
		{
			IAttributePtr ipAttribute = pAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION( "ELI38693", ipAttribute != nullptr );
			ISpatialStringPtr ipValue = ipAttribute->GetValue();

			const std::string topLevelParentAttributeID( "null" );
			std::string insert = GetInsertAttributeSetForFileStatement( ipAttribute,
																		bstrAttributeSetName,
																		topLevelParentAttributeID,
																		fileID );

			ADODB::_RecordsetPtr ipRS = m_ipFAMDB->GetResultsForQuery( insert.c_str() );
			ASSERT_RESOURCE_ALLOCATION(	"ELI38670", VARIANT_FALSE == ipRS->adoEOF );

			FieldsPtr ipFields = ipRS->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI38631", ipFields != nullptr);

			long long parentID = getLongLongField( ipFields, "AttributeID" );
			ipRS->Close();

			if ( true == asCppBool(storeRasterZone) )
			{
				// Determine if there are 0..N associated raster zones to store
				IIUnknownVectorPtr ipZones = ipValue->GetOriginalImageRasterZones();
				for ( long index = 0; index < ipZones->Size(); ++index )
				{
					IRasterZonePtr ipZone = ipZones->At(index);
					ASSERT_RESOURCE_ALLOCATION("ELI38669", ipZone != nullptr);
	
					const std::string parentAttrID( Util::Format( "%ld", parentID ) );
					std::string zoneInsert = GetInsertRasterZoneStatement( parentAttrID, ipZone );
					m_ipFAMDB->GetResultsForQuery( zoneInsert.c_str() );
				}
			}

			// Now determine if there are 0..N associated sub attributes.
			IIUnknownVectorPtr ipSubAttrs = ipAttribute->GetSubAttributes();
			for ( long index = 0; index < ipSubAttrs->Size(); ++index )
			{
				IAttributePtr ipSubAttribute = ipSubAttrs->At( index );
				ASSERT_RESOURCE_ALLOCATION( "ELI38694", ipAttribute != nullptr );

				const std::string subAttributeParentID( Util::Format("%ld", parentID) );
				std::string subInsert = GetInsertAttributeSetForFileStatement( ipSubAttribute,
																			   bstrAttributeSetName,
																			   subAttributeParentID,
																			   fileID );
				m_ipFAMDB->GetResultsForQuery( subInsert.c_str() );
			}
		}

		tg.CommitTrans();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38557");

}


// relativeIndex: -1 for most recent, 1 for oldest
// decrement most recent value to get next most recent (-2)
// increment oldest value to get next oldest (2)
// Zero is an illegal relativeIndex value.
// TODO - support relativeIndex
// TODO - get associated RasterZone info
// TODO - get associated Subattribute info
STDMETHODIMP CAttributeDBMgr::GetAttributeSetForFile(IIUnknownVector** ppAttributes, 
													 long fileID, 
													 BSTR attributeSetName,
													 long relativeIndex)
{
	try
	{
		ASSERT_ARGUMENT("ELI38618", relativeIndex != 0);
		ASSERT_ARGUMENT("ELI38668", ppAttributes != nullptr);

		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI38664", nullptr != ipAttributes);
		
		std::string query( GetQueryForAttributeSetForFile( fileID, attributeSetName ) );
		
		ADODB::_RecordsetPtr ipRecords = m_ipFAMDB->GetResultsForQuery( query.c_str() );
		while ( VARIANT_FALSE == ipRecords->adoEOF )
		{
			FieldsPtr ipFields = ipRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI38622", ipFields != nullptr);

			IAttributePtr ipAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI38664", nullptr != ipAttribute);
		
			ipAttribute->Name = getStringField( ipFields, "Name" ).c_str();
			ipAttribute->Type = getStringField( ipFields, "Type" ).c_str();

			ISpatialStringPtr ipText(CLSID_SpatialString);
			ipText->ReplaceAndDowngradeToNonSpatial( getStringField( ipFields, "Value" ).c_str() );
			ipAttribute->Value = ipText;
		
			ipAttributes->PushBack(ipAttribute);
			ipRecords->MoveNext();
		}

		*ppAttributes = ipAttributes.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38619");
}

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

		m_ipFAMDB->ExecuteInsertReturnLongLongResult( cmd.c_str(), 
													  "ID",
													  pAttributeSetNameID );
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

		std::string cmd( Util::Format( "DELETE FROM  [dbo].[AttributeSetName] "
									   "WHERE Description='%s';", 
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

		IStrToStrMapPtr ipAttributeSetNames(CLSID_StrToStrMap);
		ASSERT_RESOURCE_ALLOCATION("ELI38621", ipAttributeSetNames != nullptr);

		while ( VARIANT_FALSE == pRecords->adoEOF )
		{
			FieldsPtr pFields = pRecords->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI38622", pFields != nullptr);

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

	//DeleteAttributeSetName( ::SysAllocString(L"as4") );
	//RenameAttributeSetName( ::SysAllocString(L"as2"), ::SysAllocString(L"asn2") );
	//CreateNewAttributeSetName( ::SysAllocString(L"asn4"), nullptr );
	/*
	long long ID = 0;
	CreateNewAttributeSetName( ::SysAllocString(L"asnTestInf4"), &ID );
	*/

	//Test::TestCreateNewAttributeSetForFile();
	
/*	
	IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI38664", nullptr != ipAttributes);

	GetAttributeSetForFile( &ipAttributes, 1, get_bstr_t("AttributeSetName1"), 1 );
*/
	Test::TestRasterZoneInsert();

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
