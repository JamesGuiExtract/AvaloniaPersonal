// ReplaceStrings.h : Declaration of the CReplaceStrings

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\AFCore\Code\StringLoader.h"

#include <CachedListLoader.h>

/////////////////////////////////////////////////////////////////////////////
// CReplaceStrings
class ATL_NO_VTABLE CReplaceStrings : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReplaceStrings, &CLSID_ReplaceStrings>,
	public ISupportErrorInfo,
	public IDispatchImpl<IReplaceStrings, &IID_IReplaceStrings, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CReplaceStrings>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>
{
public:
	CReplaceStrings();
	~CReplaceStrings();

DECLARE_REGISTRY_RESOURCEID(IDR_REPLACESTRINGS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReplaceStrings)
	COM_INTERFACE_ENTRY(IReplaceStrings)
	COM_INTERFACE_ENTRY2(IDispatch, IReplaceStrings)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IOutputHandler)
END_COM_MAP()

BEGIN_PROP_MAP(CReplaceStrings)
	PROP_PAGE(CLSID_ReplaceStringsPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CReplaceStrings)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IReplaceStrings
	STDMETHOD(SaveReplaceInfoToFile)(/*[in]*/ BSTR strFileFullName, /*[in]*/ BSTR cDelimiter);
	STDMETHOD(LoadReplaceInfoFromFile)(/*[in]*/ BSTR strFileFullName, /*[in]*/ BSTR cDelimiter);
	STDMETHOD(get_Replacements)(/*[out, retval]*/ IIUnknownVector* *pVal);
	STDMETHOD(put_Replacements)(/*[in]*/ IIUnknownVector* newVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_AsRegularExpr)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AsRegularExpr)(/*[in]*/ VARIANT_BOOL newVal);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument *pDocument, IProgressStatus *pProgressStatus);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector *pAttributes, IAFDocument *pDoc, 
		IProgressStatus *pProgressStatus);

private:
	////////////
	// Variables
	////////////
	bool m_bIsCaseSensitive;
	bool m_bAsRegExpr;
	IIUnknownVectorPtr m_ipReplaceInfos;

	IMiscUtilsPtr m_ipMiscUtils;

	bool m_bDirty;

	// Cached list loader object to read values from files
	CCachedListLoader m_cachedListLoader;

	// progress status object
	IProgressStatusPtr m_ipProgressStatus;

	// counter to keep track of the number of the current attribute that is being modified
	long m_lCurrentAttributeNumber;

	// Total number of attributes to be modified (count of each attribute and its
	// subattributes)
	// NOTE: this is different from the number of replacements that will be made
	long m_lAttributeCount;

	// weighted value for the number of progress items per replacement string
	// NOTE: the value of 1 was chosen, because progress status is not updated
	// at a level deeper than the individual replacement strings.
	static const long ms_lPROGRESS_ITEMS_PER_REPLACEMENT = 1;

	//////////
	// Methods
	//////////
	// Local method to adjust Value text either from an IAttribute via IAttributeModifyingRule
	// of IAFDocument via IDocumentPreprocessor
	void	modifyValue(ISpatialString* pText, IAFDocument* pDocument, 
		IProgressStatus* pProgressStatus);
	// Local method to do the replacement of the string list
	void	replaceValue(ISpatialStringPtr ipInputText, IIUnknownVectorPtr ipReplaceInfos, 
		IProgressStatus* pProgressStatus);

	void validateLicense();
};
