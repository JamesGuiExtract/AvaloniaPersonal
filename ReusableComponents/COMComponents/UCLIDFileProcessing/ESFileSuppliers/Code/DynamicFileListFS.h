// DynamicFileListFS.h : Declaration of the CDynamicFileListFS

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESFileSuppliers.h"
#include "Win32Event.h"

#include <string>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CDynamicFileListFS
class ATL_NO_VTABLE CDynamicFileListFS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDynamicFileListFS, &CLSID_DynamicFileListFS>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDynamicFileListFS, &IID_IDynamicFileListFS, &LIBID_EXTRACT_FILESUPPLIERSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileSupplier, &IID_IFileSupplier, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CDynamicFileListFS>
{
public:
	CDynamicFileListFS();
	~CDynamicFileListFS();

	enum {IDD = IDD_DYNAMICFILELISTFSPP};

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_DYNAMICFILELISTFS)

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

BEGIN_COM_MAP(CDynamicFileListFS)
	COM_INTERFACE_ENTRY(IDynamicFileListFS)
	COM_INTERFACE_ENTRY2(IDispatch, IDynamicFileListFS)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IFileSupplier)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CDynamicFileListFS)
	PROP_PAGE(CLSID_DynamicFileListFSPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CDynamicFileListFS)
	IMPLEMENTED_CATEGORY(CATID_FP_FILE_SUPPLIERS)
END_CATEGORY_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

	// IDynamicFileList
	STDMETHOD(get_FileName)(/*[out, retval]*/ BSTR *strFileName);
	STDMETHOD(put_FileName)(/*[in]*/ BSTR strFileName);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IFileSupplier
	STDMETHOD(raw_Start)( IFileSupplierTarget *pTarget, IFAMTagManager *pFAMTM );
	STDMETHOD(raw_Stop)();
	STDMETHOD(raw_Pause)();
	STDMETHOD(raw_Resume)();

private:

	/////////////
	// Variables
	/////////////

	// File name that contains the image file list
	std::string m_strFileName;

	Win32Event m_stopEvent;
	Win32Event m_pauseEvent;
	Win32Event m_resumeEvent;
	Win32Event m_supplyingDoneOrStoppedEvent;

	bool m_bDirty;

	/////////////
	// Methods
	/////////////

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(DynamicFileListFS), CDynamicFileListFS)
