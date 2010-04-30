// RemoveEntriesFromList.h : Declaration of the CRemoveEntriesFromList

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedListLoader.h>

/////////////////////////////////////////////////////////////////////////////
// CRemoveEntriesFromList
class ATL_NO_VTABLE CRemoveEntriesFromList : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveEntriesFromList, &CLSID_RemoveEntriesFromList>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IRemoveEntriesFromList, &IID_IRemoveEntriesFromList, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CRemoveEntriesFromList>
{
public:
	CRemoveEntriesFromList();
	~CRemoveEntriesFromList();

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVEENTRIESFROMLIST)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveEntriesFromList)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputHandler)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IRemoveEntriesFromList)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRemoveEntriesFromList)
	PROP_PAGE(CLSID_RemoveEntriesFromListPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRemoveEntriesFromList)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// IRemoveEntriesFromList
	STDMETHOD(SaveEntriesToFile)(/*[in]*/ BSTR strFileFullName);
	STDMETHOD(LoadEntriesFromFile)(/*[in]*/ BSTR strFileFullName);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_EntryList)(/*[out, retval]*/ IVariantVector* *pVal);
	STDMETHOD(put_EntryList)(/*[in]*/ IVariantVector* newVal);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * bConfigured);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	///////////////
	// Variables
	///////////////
	bool m_bCaseSensitive;

	IVariantVectorPtr m_ipEntriesList;

	// Cached list loader object to read values from files
	CCachedListLoader m_cachedListLoader;

	bool m_bDirty;

	//////////
	// Methods
	//////////
	void validateLicense();
};

