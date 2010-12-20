
#pragma once

#include "resource.h"       // main symbols
#include "FileProcessors.h"
#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

#include <string>

//--------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessor
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSetActionStatusFileProcessor :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSetActionStatusFileProcessor, &CLSID_SetActionStatusFileProcessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISetActionStatusFileProcessor, &IID_ISetActionStatusFileProcessor, &LIBID_UCLID_FILEPROCESSORSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileProcessingTask, &__uuidof(IFileProcessingTask), &LIBID_UCLID_FILEPROCESSINGLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CSetActionStatusFileProcessor>
{
public:
	CSetActionStatusFileProcessor();
	~CSetActionStatusFileProcessor();

	DECLARE_REGISTRY_RESOURCEID(IDR_SETACTIONSTATUSFILEPROCESSOR)

	BEGIN_COM_MAP(CSetActionStatusFileProcessor)
		COM_INTERFACE_ENTRY(ISetActionStatusFileProcessor)
		COM_INTERFACE_ENTRY2(IDispatch, ISetActionStatusFileProcessor)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IFileProcessingTask)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CSetActionStatusFileProcessor)
		IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
	END_CATEGORY_MAP()

	BEGIN_PROP_MAP(CSetActionStatusFileProcessor)
		PROP_PAGE(CLSID_SetActionStatusFileProcessorPP)
	END_PROP_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ISetActionStatusFileProcessor
	STDMETHOD(get_ActionName)(/*[out, retval]*/ BSTR *pbstrRetVal);
	STDMETHOD(put_ActionName)(/*[in]*/ BSTR bstrNewVal);
	STDMETHOD(get_ActionStatus)(/*[out, retval]*/ long *pRetVal);
	STDMETHOD(put_ActionStatus)(/*[in]*/ long newVal);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nActionID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

	// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * bConfigured);

private:
	// flag to keep track of dirty state of this object
	bool m_bDirty;

	// member variables representing the state of this object
	std::string m_strActionName;
	EActionStatus m_eActionStatus;

	// method to validate license
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(SetActionStatusFileProcessor), CSetActionStatusFileProcessor)
