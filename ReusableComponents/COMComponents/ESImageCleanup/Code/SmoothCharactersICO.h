// SmoothCharactersICO.h : Declaration of the CSmoothCharactersICO

#pragma once
#include "resource.h"       // main symbols
#include "ICCategories.h"
#include "stdafx.h"
#include "ESImageCleanup.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CSmoothCharactersICO

class ATL_NO_VTABLE CSmoothCharactersICO :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSmoothCharactersICO, &CLSID_SmoothCharactersICO>,
	public ISupportErrorInfo,
	public IPersistStream,
	public ISmoothCharactersICO,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IImageCleanupOperation, &IID_IImageCleanupOperation, &LIBID_ESImageCleanupLib>,
	public ISpecifyPropertyPagesImpl<CSmoothCharactersICO>
{
public:
	CSmoothCharactersICO();
	~CSmoothCharactersICO();

	DECLARE_REGISTRY_RESOURCEID(IDR_SMOOTHCHARACTERSICO)

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	BEGIN_COM_MAP(CSmoothCharactersICO)
		COM_INTERFACE_ENTRY(ISmoothCharactersICO)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IImageCleanupOperation)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CSmoothCharactersICO)
		PROP_PAGE(CLSID_SmoothCharactersICOPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CSmoothCharactersICO)
		IMPLEMENTED_CATEGORY(CATID_ICO_CLEANING_OPERATIONS)
	END_CATEGORY_MAP()

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IImageCleanupOperation Methods
	STDMETHOD(Perform)(void* pciRepair);

	// ISmoothCharactersICO Methods
	STDMETHOD(get_SmoothType)(long* plSmoothType);
	STDMETHOD(put_SmoothType)(long lSmoothType);

private:
	// Variables
	bool m_bDirty;

	long m_lSmoothType;

	// Methods
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To validate the image cleanup license
	//----------------------------------------------------------------------------------------------
	void validateLicense();

};
OBJECT_ENTRY_AUTO(__uuidof(SmoothCharactersICO), CSmoothCharactersICO)