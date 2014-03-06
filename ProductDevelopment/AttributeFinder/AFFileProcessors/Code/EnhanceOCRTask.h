// EnhanceOCR.h : Declaration of the EnhanceOCRTask

#pragma once
#include "resource.h"       // main symbols
#include "AFFileProcessors.h"
#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\FPCategories.h"

#include <string>
#include <memory>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CEnhanceOCRTask
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CEnhanceOCRTask :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEnhanceOCRTask, &CLSID_EnhanceOCRTask>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IEnhanceOCRTask, &IID_IEnhanceOCRTask, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CEnhanceOCRTask>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
	public:
	CEnhanceOCRTask();
	~CEnhanceOCRTask();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_AFENHANCEOCRTASK)

	BEGIN_COM_MAP(CEnhanceOCRTask)
		COM_INTERFACE_ENTRY(IEnhanceOCRTask)
		COM_INTERFACE_ENTRY2(IDispatch, IEnhanceOCRTask)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IFileProcessingTask)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_PROP_MAP(CEnhanceOCRTask)
		PROP_PAGE(CLSID_EnhanceOCRTaskPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CEnhanceOCRTask)
		IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
	END_CATEGORY_MAP()

// IEnhanceOCRTask
	STDMETHOD(get_ConfidenceCriteria)(long *pVal);
	STDMETHOD(put_ConfidenceCriteria)(long newVal);
	STDMETHOD(get_FilterPackage)(EFilterPackage *pVal);
	STDMETHOD(put_FilterPackage)(EFilterPackage newVal);
	STDMETHOD(get_CustomFilterPackage)(BSTR *pVal);
	STDMETHOD(put_CustomFilterPackage)(BSTR newVal);
	STDMETHOD(get_PreferredFormatRegexFile)(BSTR *pVal);
	STDMETHOD(put_PreferredFormatRegexFile)(BSTR newVal);
	STDMETHOD(get_CharsToIgnore)(BSTR *pVal);
	STDMETHOD(put_CharsToIgnore)(BSTR newVal);
	STDMETHOD(get_OutputFilteredImages)(VARIANT_BOOL *pVal);
	STDMETHOD(put_OutputFilteredImages)(VARIANT_BOOL newVal);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(IFileRecord* pFileRecord, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_Standby)(VARIANT_BOOL* pVal);
	STDMETHOD(get_MinStackSize)(unsigned long *pnMinStackSize);

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	/////////////////
	// Variables
	/////////////////

	IEnhanceOCRPtr m_ipEnhanceOCR;

	/////////////////
	// Methods
	/////////////////

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(EnhanceOCRTask), CEnhanceOCRTask)