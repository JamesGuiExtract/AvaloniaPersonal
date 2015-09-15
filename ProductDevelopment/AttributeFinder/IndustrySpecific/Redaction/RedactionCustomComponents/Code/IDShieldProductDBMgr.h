// IDShieldProductDBMgr.h : Declaration of the CIDShieldProductDBMgr

#pragma once
#include "resource.h"       // main symbols
#include "RedactionCustomComponents.h"

#include <FPCategories.h>
#include <ADOUtils.h>

#include <vector>
#include <string>
#include <map>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CIDShieldProductDBMgr
class ATL_NO_VTABLE CIDShieldProductDBMgr :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIDShieldProductDBMgr, &CLSID_IDShieldProductDBMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIDShieldProductDBMgr, &IID_IIDShieldProductDBMgr, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IProductSpecificDBMgr, &__uuidof(IProductSpecificDBMgr), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>
{
public:
	CIDShieldProductDBMgr();
	~CIDShieldProductDBMgr();
	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_IDSHIELDPRODUCTDBMGR)

	BEGIN_COM_MAP(CIDShieldProductDBMgr)
		COM_INTERFACE_ENTRY(IIDShieldProductDBMgr)
		COM_INTERFACE_ENTRY2(IDispatch, ICategorizedComponent)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IProductSpecificDBMgr)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CIDShieldProductDBMgr)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_PRODUCT_SPECIFIC_DB_MGRS)
	END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IProductSpecificDBMgr Methods
	STDMETHOD(raw_AddProductSpecificSchema)(IFileProcessingDB *pDB,
		VARIANT_BOOL bOnlyTables, VARIANT_BOOL bAddUserTables);
	STDMETHOD(raw_AddProductSpecificSchema80)(IFileProcessingDB *pDB);
	STDMETHOD(raw_RemoveProductSpecificSchema)(IFileProcessingDB *pDB,
		VARIANT_BOOL bOnlyTables, VARIANT_BOOL bRetainUserTables, VARIANT_BOOL *pbSchemaExists);
	STDMETHOD(raw_ValidateSchema)(IFileProcessingDB* pDB);
	STDMETHOD(raw_GetDBInfoRows)(IVariantVector** ppDBInfoRows);
	STDMETHOD(raw_GetTables)(IVariantVector** ppTables);
	STDMETHOD(raw_UpdateSchemaForFAMDBVersion)(IFileProcessingDB* pDB,
		_Connection* pConnection, long nFAMDBSchemaVersion, long* pnProdSchemaVersion,
		long* pnNumSteps, IProgressStatus* pProgressStatus);

// IIDShieldProductDBMgr Methods
	STDMETHOD(AddIDShieldData)(BSTR bstrTaskClassGuid, long lFileID, double dDuration,
		double dOverheadTime, long lNumHCDataFound, long lNumMCDataFound, long lNumLCDataFound,
		long lNumCluesDataFound, long lTotalRedactions, long lTotalManualRedactions,
		long lNumPagesAutoAdvanced);
	STDMETHOD(GetResultsForQuery)(BSTR bstrQuery, _Recordset** ppVal);
	STDMETHOD(GetFileID)(BSTR bstrFileName, long* plFileID);
	STDMETHOD(Initialize)(IFileProcessingDB* pFAMDB);

private:
	// Variables

	// Pointer for the main FAMDB
	IFileProcessingDBPtr m_ipFAMDB;

	// This it the pointer to the database connection
	ADODB::_ConnectionPtr m_ipDBConnection; 

	// Contains the number of times an attempt to reconnect. Each time the reconnect attempt times
	// out an exception will be logged.
	long m_nNumberOfRetries;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	// Methods
	
	// Returns the m_ipDBConnection value, if it is NULL it is created using the 
	// DatabaseServer and DatabaseName from the m_ipFAMDB
	ADODB::_ConnectionPtr getDBConnection();

	// Puts all of the tables managed in the rvecTables vector
	void getIDShieldTables(std::vector<std::string>& rvecTables);

	// Throws an exception if the schema version in the database does not match current version
	// If bThrowIfMissing is true, an exception will be thrown if the version number is missing
	// from the database; if false if the DB setting is missing, it will be considered valid.
	void validateIDShieldSchemaVersion(bool bThrowIfMissing);

	void validateLicense();

	// Internal versions of external methods that may require database locking
	bool AddIDShieldData_Internal(bool bDBLocked, long nFileTaskSessionID,  long lNumHCDataFound,
		long lNumMCDataFound, long lNumLCDataFound, long lNumCluesDataFound, long lTotalRedactions,
		long lTotalManualRedactions, long lNumPagesAutoAdvanced);

	bool GetResultsForQuery_Internal(bool bDBLocked, BSTR bstrQuery, _Recordset** ppVal);

	bool GetFileID_Internal(bool bDBLocked, BSTR bstrFileName, long* plFileID);

	// Retrieves the set of SQL queries used to create the IDShield specific database tables.
	const vector<string> getTableCreationQueries();

	// Retrieves a map of each DBInfo value the IDShield specific DB component uses and its default
	// value.
	map<string, string> getDBInfoDefaultValues();
};

OBJECT_ENTRY_AUTO(__uuidof(IDShieldProductDBMgr), CIDShieldProductDBMgr)
