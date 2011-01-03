// AFConvertVOAToXMLTask.h : Declaration of the CAFConvertVOAToXMLTask

#pragma once

#include "resource.h"       // main symbols
#include <string>

#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\FPCategories.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAFConvertVOAToXMLTask
class ATL_NO_VTABLE CAFConvertVOAToXMLTask : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFConvertVOAToXMLTask, &CLSID_AFConvertVOAToXMLTask>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IAFConvertVOAToXMLTask, &IID_IAFConvertVOAToXMLTask, &LIBID_UCLID_AFFILEPROCESSORSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IFileProcessingTask, &IID_IFileProcessingTask, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CAFConvertVOAToXMLTask>
{
public:
	CAFConvertVOAToXMLTask();
	~CAFConvertVOAToXMLTask();

DECLARE_REGISTRY_RESOURCEID(IDR_AFCONVERTVOATOXMLTASK)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFConvertVOAToXMLTask)
	COM_INTERFACE_ENTRY(IAFConvertVOAToXMLTask)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IFileProcessingTask)
	COM_INTERFACE_ENTRY2(IDispatch, IAFConvertVOAToXMLTask)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IAccessRequired)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CAFConvertVOAToXMLTask)
	PROP_PAGE(CLSID_AFConvertVOAToXMLTaskPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CAttributeFinderEngine)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_PROCESSORS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAFConvertVOAToXMLTask
	STDMETHOD(get_VOAFile)(BSTR* pbstrVOAFile);
	STDMETHOD(put_VOAFile)(BSTR bstrVOAFile);
	STDMETHOD(get_XMLOutputHandler)(IUnknown** ppXMLOutputHandler);
	STDMETHOD(put_XMLOutputHandler)(IUnknown* pXMLOutputHandler);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IFileProcessingTask
	STDMETHOD(raw_Init)(long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB *pDB);
	STDMETHOD(raw_ProcessFile)(BSTR bstrFileFullName, long nFileID, long nTaskID,
		IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested, 
		EFileProcessingResult *pResult);
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_Close)();

// IAccessRequired
	STDMETHOD(raw_RequiresAdminAccess)(VARIANT_BOOL* pbResult);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////
	// Variables
	/////////////
	
	// The VOA file to convert from
	string m_strVOAFile;

	// The XML output handler to use for the conversion
	IOutputToXMLPtr m_ipXMLOutputHandler;

	bool m_bDirty;

	/////////////
	// Methods
	/////////////
	void validateLicense();

	IOutputToXMLPtr getXMLOutputHandler();
};

