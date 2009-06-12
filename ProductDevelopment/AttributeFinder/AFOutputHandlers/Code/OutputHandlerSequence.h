// OutputHandlerSequence.h : Declaration of the COutputHandlerSequence

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// COutputHandlerSequence
class ATL_NO_VTABLE COutputHandlerSequence : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COutputHandlerSequence, &CLSID_OutputHandlerSequence>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMultipleObjectHolder, &IID_IMultipleObjectHolder, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<COutputHandlerSequence>
{
public:
	COutputHandlerSequence();
	~COutputHandlerSequence();

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPUTHANDLERSEQUENCE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COutputHandlerSequence)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputHandler)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IMultipleObjectHolder)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(COutputHandlerSequence)
	PROP_PAGE(CLSID_MultipleObjSelectorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(COutputHandlerSequence)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);
	STDMETHOD(raw_Clone)(IUnknown * * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IMultipleObjectHolder
	STDMETHOD(raw_GetObjectCategoryName)(/*[out, retval]*/BSTR *pstrCategoryName);
	STDMETHOD(get_ObjectsVector)(/*[out, retval]*/IIUnknownVector* *pVal);
	STDMETHOD(put_ObjectsVector)(/*[in]*/IIUnknownVector *newVal);
	STDMETHOD(raw_GetObjectType)(/*[out, retval]*/BSTR *pstrObjectType);
	STDMETHOD(raw_GetRequiredIID)(/*[out, retval]*/ IID *riid);

private:
	/////////////
	// Methods
	/////////////
	void validateLicense();

	/////////////
	// Variables
	/////////////
	// each entry in the vector below is expected to be of type 
	// IObjectWithDescription and the object contained therein is expected to 
	// be of type IOutputHandler
	IIUnknownVectorPtr m_ipOutputHandlers;

	// flag to keep track of whether object is dirty
	bool m_bDirty;
};
