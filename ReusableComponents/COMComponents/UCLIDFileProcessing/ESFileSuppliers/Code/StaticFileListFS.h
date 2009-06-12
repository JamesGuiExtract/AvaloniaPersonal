// StaticFileListFS.h : Declaration of the CStaticFileListFS

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESFileSuppliers.h"
#include "Win32Event.h"

#include <vector>
#include <string>


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CStaticFileListFS
class ATL_NO_VTABLE CStaticFileListFS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStaticFileListFS, &CLSID_StaticFileListFS>,
	public ISupportErrorInfo,
	public IDispatchImpl<IStaticFileListFS, &IID_IStaticFileListFS, &LIBID_EXTRACT_FILESUPPLIERSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IFileSupplier, &IID_IFileSupplier, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CStaticFileListFS>
{
public:
	CStaticFileListFS();
	~CStaticFileListFS();

	enum {IDD = IDD_STATICFILELISTFSPP};

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_REGISTRY_RESOURCEID(IDR_STATICFILELISTFS)

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	BEGIN_COM_MAP(CStaticFileListFS)
		COM_INTERFACE_ENTRY(IStaticFileListFS)
		COM_INTERFACE_ENTRY2(IDispatch, IStaticFileListFS)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IFileSupplier)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

BEGIN_PROP_MAP(CStaticFileListFS)
	PROP_PAGE(CLSID_StaticFileListFSPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CStaticFileListFS)
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

	// IStaticFileList
	STDMETHOD(get_FileList)(/*[out, retval]*/ IVariantVector* *pVal);
	STDMETHOD(put_FileList)(/*[in]*/ IVariantVector* newVal);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IFileSupplier
	STDMETHOD(raw_Start)( IFileSupplierTarget *pTarget, IFAMTagManager *pFAMTM);
	STDMETHOD(raw_Stop)();
	STDMETHOD(raw_Pause)();
	STDMETHOD(raw_Resume)();

private:

	/////////////
	// Methods
	/////////////

	void validateLicense();

	//clear all the variables
	void clear();

	/////////////
	// Variables
	/////////////

	// individually defined list of files
	std::vector<std::string> m_vecFileList;

	Win32Event m_stopEvent;
	Win32Event m_pauseEvent;
	Win32Event m_resumeEvent;
	Win32Event m_supplyingDoneOrStoppedEvent;

	bool m_bDirty;
};

OBJECT_ENTRY_AUTO(__uuidof(StaticFileListFS), CStaticFileListFS)
