// DocPreprocessorSequence.h : Declaration of the CDocPreprocessorSequence

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

/////////////////////////////////////////////////////////////////////////////
// CDocPreprocessorSequence
class ATL_NO_VTABLE CDocPreprocessorSequence : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocPreprocessorSequence, &CLSID_DocPreprocessorSequence>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMultipleObjectHolder, &IID_IMultipleObjectHolder, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CDocPreprocessorSequence>
{
public:
	CDocPreprocessorSequence();
	~CDocPreprocessorSequence();

DECLARE_REGISTRY_RESOURCEID(IDR_DOCPREPROCESSORSEQUENCE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocPreprocessorSequence)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY2(IDispatch, IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IMultipleObjectHolder)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CDocPreprocessorSequence)
	PROP_PAGE(CLSID_MultipleObjSelectorPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CDocPreprocessorSequence)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument,/*[in]*/ IProgressStatus *pProgressStatus);

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
	////////////
	// Variables
	////////////
	bool m_bDirty;
	
	// each entry in the vector below is expected to be of type 
	// IObjectWithDescription and the object contained therein is expected to 
	// be of type IOutputHandler
	IIUnknownVectorPtr m_ipDocPreprocessors;

	////////////
	// Methods
	////////////
	void validateLicense();
};
