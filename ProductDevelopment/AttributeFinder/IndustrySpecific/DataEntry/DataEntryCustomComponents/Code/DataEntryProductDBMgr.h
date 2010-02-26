// DataEntryProductDBMgr.h : Declaration of the CDataEntryProductDBMgr

#pragma once
#include "resource.h"       // main symbols
#include "DataEntryCustomComponents.h"

#include <FPCategories.h>
#include <ADOUtils.h>

#include <string>
#include <vector>
#include <map>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CDataEntryProductDBMgr
class ATL_NO_VTABLE CDataEntryProductDBMgr :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDataEntryProductDBMgr, &CLSID_DataEntryProductDBMgr>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDataEntryProductDBMgr, &IID_IDataEntryProductDBMgr, &LIBID_UCLID_DATAENTRYCUSTOMCOMPONENTSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<IProductSpecificDBMgr, &__uuidof(IProductSpecificDBMgr), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>
{
public:
	CDataEntryProductDBMgr();
	~CDataEntryProductDBMgr();
	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_DATAENTRYPRODUCTDBMGR)

	BEGIN_COM_MAP(CDataEntryProductDBMgr)
		COM_INTERFACE_ENTRY(IDataEntryProductDBMgr)
		COM_INTERFACE_ENTRY2(IDispatch, IDataEntryProductDBMgr)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IProductSpecificDBMgr)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CDataEntryProductDBMgr)
		IMPLEMENTED_CATEGORY(CATID_FP_FAM_PRODUCT_SPECIFIC_DB_MGRS)
	END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IProductSpecificDBMgr Methods
	STDMETHOD(raw_AddProductSpecificSchema)(IFileProcessingDB *pDB);
	STDMETHOD(raw_RemoveProductSpecificSchema)(IFileProcessingDB *pDB);

// IDataEntryProductDBMgr Methods
	STDMETHOD(AddDataEntryData)(long lFileID, long nActionID, double lDuration, long* plInstanceID);
	STDMETHOD(put_FAMDB)(IFileProcessingDB* newVal);
	STDMETHOD(RecordCounterValues)(long* plInstanceToken, long lDataEntryDataInstanceID,
		IIUnknownVector* pAttributes);

private:

	//////////////
	// Variables
	//////////////

	// Pointer for the main FAMDB
	IFileProcessingDBPtr m_ipFAMDB;

	// This it the pointer to the database connection
	ADODB::_ConnectionPtr m_ipDBConnection; 

	// Flag to indicate if non recent DataEntryData records should be saved
	bool m_bStoreDataEntryProcessingHistory;

	// An IAFUtility instance to be used to execute attribute queries.
	IAFUtilityPtr m_ipAFUtility;

	// Maintains SQL commands to store counts from the data once an associated DataEntryTable entry
	// is available.
	map<long, vector<string> > m_mapVecCounterValueInsertionQueries;

	// The value of the next token to be assigned.
	volatile long m_lNextInstanceToken;

	// Contains the number of times an attempt to reconnect. Each time the reconnect attempt times
	// out an exception will be logged.
	long m_nNumberOfRetries;

	// Contains the time in seconds to keep retrying.  
	double m_dRetryTimeout;

	//////////////
	// Methods
	//////////////

	// Returns the m_ipDBConnection value, if it is NULL it is created using the 
	// DatabaseServer and DatabaseName from the m_ipFAMDB
	ADODB::_ConnectionPtr getDBConnection();

	// Puts all of the tables managed in the rvecTables vector
	void getDataEntryTables(std::vector<std::string>& rvecTables);

	// Throws an exception if the schema version in the database does not match current version
	void validateDataEntrySchemaVersion();

	// Method to check whether data entry counters are enabled in the database
	bool areCountersEnabled();

	// Returns m_ipAFUtility, after initializing it if necessary
	IAFUtilityPtr getAFUtility();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(DataEntryProductDBMgr), CDataEntryProductDBMgr)
