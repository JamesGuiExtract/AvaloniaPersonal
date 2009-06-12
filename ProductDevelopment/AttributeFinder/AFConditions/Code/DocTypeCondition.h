// DocTypeCondition.h : Declaration of the CDocTypeCondition

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
/////////////////////////////////////////////////////////////////////////////
// DocTypeCondition
class ATL_NO_VTABLE CDocTypeCondition : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDocTypeCondition, &CLSID_DocTypeCondition>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<IDocTypeCondition, &IID_IDocTypeCondition, &LIBID_UCLID_AFCONDITIONSLib>,
	public IDispatchImpl<IAFCondition, &IID_IAFCondition, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CDocTypeCondition>
{
public:
	CDocTypeCondition();
	~CDocTypeCondition();

DECLARE_REGISTRY_RESOURCEID(IDR_DOCTYPECONDITION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDocTypeCondition)
	COM_INTERFACE_ENTRY(IDocTypeCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDocTypeCondition)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(IAFCondition)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CDocTypeCondition)
	PROP_PAGE(CLSID_DocTypeConditionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CDocTypeCondition)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_CONDITIONS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDocTypeCondition
	STDMETHOD(get_Types)(IVariantVector **ppVec);
	STDMETHOD(put_Types)(IVariantVector *pVec);
	STDMETHOD(get_AllowTypes)(VARIANT_BOOL* pVal);
	STDMETHOD(put_AllowTypes)(VARIANT_BOOL newVal);
	STDMETHOD(get_MinConfidence)(EDocumentConfidenceLevel* pVal);
	STDMETHOD(put_MinConfidence)(EDocumentConfidenceLevel newVal);
	STDMETHOD(get_Category)(BSTR *pRetVal);
	STDMETHOD(put_Category)(BSTR pNewVal);

// IAFCondition
	STDMETHOD(raw_ProcessCondition)(IAFDocument *pAFDoc, VARIANT_BOOL* pbRetVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

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
	// flag to keep track of whether object is dirty
	bool		m_bDirty;

	// This contains the list of DocType strings that this 
	// condition will allow (or not allow)
	IVariantVectorPtr m_ipTypes;

	// Selected category of document types
	std::string	m_strCategory;

	bool m_bAllowTypes;

	EDocumentConfidenceLevel m_eMinConfidence;

	IAFUtilityPtr m_ipAFUtility;

	///////////////
	// Methods
	///////////////

	// Check licensing
	void validateLicense();
	void clear();
};

