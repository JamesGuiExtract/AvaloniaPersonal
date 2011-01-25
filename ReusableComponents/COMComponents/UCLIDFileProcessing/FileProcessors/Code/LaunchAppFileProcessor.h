// LaunchAppFileProcessor.h : Declaration of the CLaunchAppFileProcessor

#ifndef __LAUNCHAPPFILEPROCESSOR_H_
#define __LAUNCHAPPFILEPROCESSOR_H_

#include "resource.h"       // main symbols

#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CLaunchAppFileProcessor
class ATL_NO_VTABLE CLaunchAppFileProcessor : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLaunchAppFileProcessor, &CLSID_LaunchAppFileProcessor>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILaunchAppFileProcessor, &IID_ILaunchAppFileProcessor, &LIBID_UCLID_FILEPROCESSORSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CLaunchAppFileProcessor>
{
public:
	CLaunchAppFileProcessor();
	~CLaunchAppFileProcessor();

DECLARE_REGISTRY_RESOURCEID(IDR_LAUNCHAPPFILEPROCESSOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLaunchAppFileProcessor)
	COM_INTERFACE_ENTRY(ILaunchAppFileProcessor)
	COM_INTERFACE_ENTRY2(IDispatch, ILaunchAppFileProcessor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CLaunchAppFileProcessor)
	PROP_PAGE(CLSID_LaunchAppFileProcessorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CLaunchAppFileProcessor)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILaunchAppFileProcessor
	STDMETHOD(get_CommandLine)(/*[out, retval]*/ BSTR *pRetVal);
	STDMETHOD(put_CommandLine)(/*[in]*/ BSTR newVal);

	STDMETHOD(get_WorkingDirectory)(/*[out, retval]*/ BSTR *pRetVal);
	STDMETHOD(put_WorkingDirectory)(/*[in]*/ BSTR newVal);

	STDMETHOD(get_IsBlocking)(/*[out, retval]*/ VARIANT_BOOL *pRetVal);
	STDMETHOD(put_IsBlocking)(/*[in]*/ VARIANT_BOOL newVal);

	STDMETHOD(get_Parameters)(BSTR* pRetVal);
	STDMETHOD(put_Parameters)(BSTR newVal);

	STDMETHOD(get_PropagateErrors)(VARIANT_BOOL* pbVal);
	STDMETHOD(put_PropagateErrors)(VARIANT_BOOL bVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(IFileRecord* pFileRecord, long nActionID,
		IFAMTagManager *pFAMTM, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
		VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	//////////////
	// Data
	//////////////
	bool m_bDirty;

	bool m_bBlocking;

	string m_strCmdLine;
	string m_strWorkingDir;
	string m_strParameters;

	bool m_bPropagateErrors;

	/////////////
	// Methods
	/////////////
	void validateLicense();
};

#endif //__LAUNCHAPPFILEPROCESSOR_H_
