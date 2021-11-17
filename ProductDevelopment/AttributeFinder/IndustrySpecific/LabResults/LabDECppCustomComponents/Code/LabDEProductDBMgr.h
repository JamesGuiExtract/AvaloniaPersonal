// LabDEProductDBMgr.h : Declaration of the CLabDEProductDBMgr

#pragma once
#include "resource.h"       // main symbols
#include "LabDECppCustomComponents.h"

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

// CLabDEProductDBMgr
class ATL_NO_VTABLE CLabDEProductDBMgr :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLabDEProductDBMgr, &CLSID_LabDEProductDBMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILabDEProductDBMgr, &IID_ILabDEProductDBMgr, &LIBID_UCLID_LABDECPPCUSTOMCOMPONENTSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IProductSpecificDBMgr, &__uuidof(IProductSpecificDBMgr), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>
{
public:
	CLabDEProductDBMgr();
	~CLabDEProductDBMgr();
	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_LABDEPRODUCTDBMGR)

	BEGIN_COM_MAP(CLabDEProductDBMgr)
		COM_INTERFACE_ENTRY(ILabDEProductDBMgr)
		COM_INTERFACE_ENTRY2(IDispatch, ILabDEProductDBMgr)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IProductSpecificDBMgr)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CLabDEProductDBMgr)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_PRODUCT_SPECIFIC_DB_MGRS)
	END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IProductSpecificDBMgr Methods
	STDMETHOD(raw_AddProductSpecificSchema)(_Connection* pConnection, IFileProcessingDB *pDB, VARIANT_BOOL bOnlyTables,
		VARIANT_BOOL bAddUserTables);
	STDMETHOD(raw_AddProductSpecificSchema80)(IFileProcessingDB *pDB);
	STDMETHOD(raw_RemoveProductSpecificSchema)(IFileProcessingDB *pDB,
		VARIANT_BOOL bOnlyTables, VARIANT_BOOL bRetainUserTables, VARIANT_BOOL *pbSchemaExists);
	STDMETHOD(raw_ValidateSchema)(IFileProcessingDB* pDB);
	STDMETHOD(raw_GetDBInfoRows)(IVariantVector** ppDBInfoRows);
	STDMETHOD(raw_GetTables)(IVariantVector** ppTables);
	STDMETHOD(raw_UpdateSchemaForFAMDBVersion)(IFileProcessingDB* pDB, _Connection* pConnection,
		long nFAMDBSchemaVersion, long* pnProdSchemaVersion, long* pnNumSteps,
		IProgressStatus* pProgressStatus);

// ILabDEProductDBMgr Methods
	STDMETHOD(put_FAMDB)(IFileProcessingDB* newVal);

private:

	//////////////
	// Variables
	//////////////

	// Pointer for the main FAMDB
	IFileProcessingDBPtr m_ipFAMDB;

	CppBaseApplicationRoleConnection::AppRoles m_currentRole;

	ApplicationRoleUtility m_roleUtility;

	ADODB::_ConnectionPtr m_ipDBConnection; 

	// Contains the number of times an attempt to reconnect. Each time the reconnect attempt times
	// out an exception will be logged.
	long m_nNumberOfRetries;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	//////////////
	// Methods
	//////////////

	// Returns CppBaseApplicationRoleConnection object, if it is NULL it is created using the 
	// DatabaseServer and DatabaseName from the m_ipFAMDB
	unique_ptr<CppBaseApplicationRoleConnection> getAppRoleConnection();

	// Puts all of the tables managed in the rvecTables vector
	void getLabDETables(std::vector<std::string>& rvecTables);

	// Throws an exception if the schema version in the database does not match current version
	// If bThrowIfMissing is true, an exception will be thrown if the version number is missing
	// from the database; if false if the DB setting is missing, it will be considered valid.
	void validateLabDESchemaVersion(bool bThrowIfMissing);

	void validateLicense();

	// Retrieves the set of SQL queries used to create the LabDE specific database tables.
	const vector<string> getTableCreationQueries(bool bAddUserTables);

	// Retrieves a map of each DBInfo value the LabDE specific DB component uses and its default
	// value.
	map<string, string> getDBInfoDefaultValues();

	// Adds old tables that are no longer in the database
	void addOldTables(vector<string>& vecTables);

};

OBJECT_ENTRY_AUTO(__uuidof(LabDEProductDBMgr), CLabDEProductDBMgr)
