// AttributeDBMgr.h : Declaration of the CAttributeDBMgr

#pragma once
#include "resource.h"       // main symbols
#include "AttributeDBMgrComponents.h"

#include <FPCategories.h>
#include <ADOUtils.h>
#include <CppApplicationRoleConnection.h>

#include <string>
#include <vector>
#include <map>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif
#include <ApplicationRoleUtility.h>

// CAttributeDBMgr
class ATL_NO_VTABLE CAttributeDBMgr :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttributeDBMgr, &CLSID_AttributeDBMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeDBMgr, &IID_IAttributeDBMgr, &LIBID_AttributeDbMgrComponentsLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IProductSpecificDBMgr, &__uuidof(IProductSpecificDBMgr), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>
{
public:
	CAttributeDBMgr();
	~CAttributeDBMgr();
	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTEDBMGR)

	BEGIN_COM_MAP(CAttributeDBMgr)
		COM_INTERFACE_ENTRY(IAttributeDBMgr)
		COM_INTERFACE_ENTRY2(IDispatch, IAttributeDBMgr)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IProductSpecificDBMgr)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CAttributeDBMgr)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_PRODUCT_SPECIFIC_DB_MGRS)
	END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IProductSpecificDBMgr Methods
	STDMETHOD(raw_AddProductSpecificSchema)( _Connection* pConnection,
                                            IFileProcessingDB *pDB,
											VARIANT_BOOL bOnlyTables,
											VARIANT_BOOL bAddUserTables);
	STDMETHOD(raw_AddProductSpecificSchema80)(IFileProcessingDB *pDB);
	STDMETHOD(raw_RemoveProductSpecificSchema)(_Connection* pConnection,
											   IFileProcessingDB *pDB,
											   VARIANT_BOOL bOnlyTables,
											   VARIANT_BOOL bRetainUserTables,
											   VARIANT_BOOL *pbSchemaExists);
	STDMETHOD(raw_ValidateSchema)(IFileProcessingDB* pDB);
	STDMETHOD(raw_GetDBInfoRows)(IVariantVector** ppDBInfoRows);
	STDMETHOD(raw_GetTables)(IVariantVector** ppTables);
	STDMETHOD(raw_UpdateSchemaForFAMDBVersion)(IFileProcessingDB* pDB,
											   _Connection* pConnection,
											   long nFAMDBSchemaVersion,
											   long* pnProdSchemaVersion,
											   long* pnNumSteps,
											   IProgressStatus* pProgressStatus);
// IAttributeDBMgr Methods
	STDMETHOD(put_FAMDB)(IFileProcessingDB* newVal);
	STDMETHOD(get_FAMDB)(IFileProcessingDB** retVal);

	STDMETHOD(CreateNewAttributeSetForFile)(long nFileTaskSessionID,
											BSTR bstrAttributeSetName,
											IIUnknownVector* pAttributes,
											VARIANT_BOOL vbStoreDiscreteFields,
											VARIANT_BOOL vbStoreRasterZone,
											VARIANT_BOOL vbStoreEmptyAttributes,
											VARIANT_BOOL vbCloseConnection);

	// relativeIndex: -1 for most recent, 1 for oldest
	// decrement most recent value to get next most recent (-2)
	// increment oldest value to get next oldest (2)
	// Zero is an illegal relativeIndex value.
	STDMETHOD(GetAttributeSetForFile)(long nFileID,
									  BSTR bstrAttributeSetName,
									  long nRelativeIndex,
									  VARIANT_BOOL vbCloseConnection,
									  IIUnknownVector** pAttributes);

	STDMETHOD(CreateNewAttributeSetName)(BSTR name,
										 long long* pllAttributeSetNameID);

	STDMETHOD(RenameAttributeSetName)(BSTR bstrAttributeSetName,
									  BSTR bstrNewName);

	STDMETHOD(DeleteAttributeSetName)(BSTR bstrAttributeSetName);

	STDMETHOD(GetAllAttributeSetNames)(IStrToStrMap** ppNames);

private:

	//////////////
	// Variables
	//////////////

	// Pointer for the main FAMDB
	IFileProcessingDBPtr m_ipFAMDB;

	// This it the pointer to the database connection
	shared_ptr<CppBaseApplicationRoleConnection> m_ipDBConnection;

	// Contains the number of times an attempt to reconnect. Each time the reconnect attempt times
	// out an exception will be logged.
	long m_nNumberOfRetries;

	// Contains the time in seconds to keep retrying.
	double m_dRetryTimeout;

	//////////////
	// Methods
	//////////////

	// Returns a CppBaseApplicationRoleConnection object that contains the m_ipDBConnection value, 
	// if it is NULL it is created using the DatabaseServer and DatabaseName from the m_ipFAMDB
	// if bReset is true the current connection in m_ipDBConnection is set to NULL and recreated
	// and make the default false
	shared_ptr<CppBaseApplicationRoleConnection> getAppRoleConnection(bool bReset = false);

	ApplicationRoleUtility m_roleUtility;

	CppBaseApplicationRoleConnection::AppRoles m_currentRole;

	// Puts all of the tables managed in the rvecTables vector
	std::vector<std::string> getAttributeTables();

	// Throws an exception if the schema version in the database does not match current version
	void validateSchemaVersion(/*bool bThrowIfMissing*/);

	void validateLicense();

	// Retrieves a map of each DBInfo value the component uses and its default value.
	std::map<std::string, std::string> getDBInfoDefaultValues();

	// This method sets the VOA field in the AttributeSetForFile table.
	void SaveVoaDataInASFF( _ConnectionPtr ipConnection, IIUnknownVector* pAttributes,
		long long llRootASFF_ID );

	// Stores the discrete data for the specified vector of attributes (including all descendants).
	void storeAttributeData(_ConnectionPtr ipConnection,
							IIUnknownVectorPtr ipAttributes,
							bool bStoreRasterZone,
							bool bStoreEmptyAttributes,
							long long llRootASFF_ID);

	// Recursively builds the query to store discrete data for the specified vector of attributes
	std::string buildStoreAttributeDataQuery(_ConnectionPtr ipConnection,
											IIUnknownVectorPtr ipAttributes,
											bool bStoreRasterZone,
											bool bStoreEmptyAttributes,
											long long llRootASFF_ID);

	bool CreateNewAttributeSetForFile_Internal( bool bDbLocked,
												long nFileTaskSessionID,
  												BSTR bstrAttributeSetName,
  												IIUnknownVector* pAttributes,
												VARIANT_BOOL vbStoreDiscreteFields,
  												VARIANT_BOOL vbStoreRasterZone,
  												VARIANT_BOOL vbStoreEmptyAttributes );

	bool CAttributeDBMgr::GetAttributeSetForFile_Internal( bool bDbLocked,
													       long fileID,
													       BSTR attributeSetName,
													       long relativeIndex,
														   IIUnknownVector** ppAttributes);
};

OBJECT_ENTRY_AUTO(__uuidof(AttributeDBMgr), CAttributeDBMgr)
