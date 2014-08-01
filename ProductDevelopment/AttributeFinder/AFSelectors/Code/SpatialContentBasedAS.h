// SpatialContentBasedAS.h : Declaration of the CSpatialContentBasedAS

#pragma once
#include "resource.h"       // main symbols
#include "..\\..\\AFCore\\Code\\AFCategories.h"
#include "AFSelectors.h"

#include <IdentifiableObject.h>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CSpatialContentBasedAS

class ATL_NO_VTABLE CSpatialContentBasedAS :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialContentBasedAS, &CLSID_SpatialContentBasedAS>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISpatialContentBasedAS, &IID_ISpatialContentBasedAS, &LIBID_UCLID_AFSELECTORSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IPersistStream,
	public IDispatchImpl<IAttributeSelector, &__uuidof(IAttributeSelector), &LIBID_UCLID_AFCORELib, /* wMajor = */ 1>,
	public IDispatchImpl<ICategorizedComponent, &__uuidof(ICategorizedComponent), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<ILicensedComponent, &__uuidof(ILicensedComponent), &LIBID_UCLID_COMLMLib, /* wMajor = */ 1>,
	public ISpecifyPropertyPagesImpl<CSpatialContentBasedAS>,
	public IDispatchImpl<IMustBeConfiguredObject, &__uuidof(IMustBeConfiguredObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	private CIdentifiableObject
{
public:
	CSpatialContentBasedAS();

	DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALCONTENTBASEDAS)


	BEGIN_COM_MAP(CSpatialContentBasedAS)
		COM_INTERFACE_ENTRY(ISpatialContentBasedAS)
		COM_INTERFACE_ENTRY2(IDispatch, IAttributeSelector)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAttributeSelector)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(IIdentifiableObject)
	END_COM_MAP()

	BEGIN_CATEGORY_MAP(CSpatialContentBasedAS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SELECTORS)
	END_CATEGORY_MAP()

	BEGIN_PROP_MAP(CSpatialContentBasedAS)
		PROP_PAGE(CLSID_SpatialContentBasedASPP)
	END_PROP_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// IAttributeSelector Methods
	STDMETHOD(raw_SelectAttributes)(IIUnknownVector * pAttrIn, IAFDocument * pAFDoc, IIUnknownVector * * pAttrOut);

	// ICategorizedComponent Methods
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

	// ILicensedComponent Methods
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// ISpatialContentBasedAS
	STDMETHOD(get_ConsecutiveRows)(long* pVal);
	STDMETHOD(put_ConsecutiveRows)(long newVal);
	STDMETHOD(get_MinPercent)(long* pVal);
	STDMETHOD(put_MinPercent)(long newVal);
	STDMETHOD(get_MaxPercent)(long* pVal);
	STDMETHOD(put_MaxPercent)(long newVal);
	STDMETHOD(get_Contains)(VARIANT_BOOL* pVal);
	STDMETHOD(put_Contains)(VARIANT_BOOL newVal);
	STDMETHOD(get_IncludeNonSpatial)(VARIANT_BOOL* pVal);
	STDMETHOD(put_IncludeNonSpatial)(VARIANT_BOOL newVal);

	// IMustBeConfiguredObject Methods
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * bConfigured);

	// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

private:
	//////////
	// Variables
	//////////

	// member variables
	bool m_bDirty; // dirty flag to indicate modified state of this object

	bool m_bContains;
	long m_lConsecutiveRows;
	long m_lMinPercent;
	long m_lMaxPercent;
	bool m_bIncludeNonSpatial;

	IImageUtilsPtr m_ipImageUtils;

	//////////
	// Methods
	//////////

	IImageUtilsPtr getImageUtils();

	// This function will be called for the list of attributes and for each set of subattributes
	void selectMatchingAttrs( IIUnknownVectorPtr ipAttributes, IIUnknownVectorPtr ipSelected );

	void validateLicense();

};

OBJECT_ENTRY_AUTO(__uuidof(SpatialContentBasedAS), CSpatialContentBasedAS)
