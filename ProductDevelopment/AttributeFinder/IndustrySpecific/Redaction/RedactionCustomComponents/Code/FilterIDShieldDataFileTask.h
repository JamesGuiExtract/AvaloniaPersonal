// FilterIDShieldDataFileTask.h : Declaration of the CFilterIDShieldDataFileTask

#pragma once

#include "resource.h"       // main symbols
#include "RedactionAppearanceDlg.h"

#include <FPCategories.h>

#include <string>
#include <set>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CFilterIDShieldDataFileTask
class ATL_NO_VTABLE CFilterIDShieldDataFileTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFilterIDShieldDataFileTask, &CLSID_FilterIDShieldDataFileTask>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IFilterIDShieldDataFileTask, &IID_IFilterIDShieldDataFileTask, &LIBID_UCLID_REDACTIONCUSTOMCOMPONENTSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CFilterIDShieldDataFileTask>
{
public:
	CFilterIDShieldDataFileTask();
	~CFilterIDShieldDataFileTask();

DECLARE_REGISTRY_RESOURCEID(IDR_FILTERIDSHIELD)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFilterIDShieldDataFileTask)
	COM_INTERFACE_ENTRY(IFilterIDShieldDataFileTask)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY2(IDispatch, IFilterIDShieldDataFileTask)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CFilterIDShieldDataFileTask)
	PROP_PAGE(CLSID_FilterIDShieldDataFileTaskPP)
END_PROP_MAP()


BEGIN_CATEGORY_MAP(CFilterIDShieldDataFileTask)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IFilterIDShieldDataFileTask
public:
	STDMETHOD(put_VOAFileToRead)(/*[in]*/ BSTR bstrVOAFileName);
	STDMETHOD(get_VOAFileToRead)(/*[out, retval]*/ BSTR *pbstrVOAFileName);
	STDMETHOD(put_DataTypes)(/*[in]*/ IVariantVector* pDataTypes);
	STDMETHOD(get_DataTypes)(/*[out, retval]*/ IVariantVector** ppDataTypes);
	STDMETHOD(put_VOAFileToWrite)(/*[in]*/ BSTR bstrVOAFileName);
	STDMETHOD(get_VOAFileToWrite)(/*[out, retval]*/ BSTR *pbstrVOAFileName);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//////////////
	// Variables
	//////////////

	// VOA File to read
	string m_strVOAFileToRead;

	// Data types to filter on
	set<string> m_setDataTypes;

	// VOA File to write
	string m_strVOAFileToWrite;

	bool m_bDirty;

	//////////////
	// Methods
	//////////////

	void validateLicense();

	// Returns a comma separated string of all the data types in the filter
	string getDataTypesAsString();

	// Returns a variant vector containing each of the data types as a string
	IVariantVectorPtr getDataTypesAsVariantVector();

	// Fills the internal data type set from the specified variant vector
	void fillDataTypeSet(IVariantVectorPtr ipDataTypes);

	// Fills the internal data type set from a comma separated string of data types
	void fillDataTypeSet(const string& strDataTypes);
};
