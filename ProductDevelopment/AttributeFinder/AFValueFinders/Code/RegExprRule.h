
#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedObjectFromFile.h>
#include <RegExLoader.h>

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CRegExprRule
class ATL_NO_VTABLE CRegExprRule : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRegExprRule, &CLSID_RegExprRule>,
	public IPersistStream,
	public ISupportErrorInfo,
	public ISpecifyPropertyPagesImpl<CRegExprRule>,
	public IDispatchImpl<IRegExprRule, &IID_IRegExprRule, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRegExprRule();
	~CRegExprRule();

DECLARE_REGISTRY_RESOURCEID(IDR_REGEXPRRULE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

BEGIN_COM_MAP(CRegExprRule)
	COM_INTERFACE_ENTRY(IRegExprRule)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IRegExprRule)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CRegExprRule)
	PROP_PAGE(CLSID_RegExprRulePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRegExprRule)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRegExprRule
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Pattern)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Pattern)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RegExpFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RegExpFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_IsRegExpFromFile)(/*[out, retval]*/ BOOL *pVal);
	STDMETHOD(put_IsRegExpFromFile)(/*[in]*/ BOOL newVal);
	STDMETHOD(get_CreateSubAttributesFromNamedMatches)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CreateSubAttributesFromNamedMatches)(/*[in]*/ VARIANT_BOOL newVal);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute* pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

private:	
	bool m_bCaseSensitive;

	bool m_bAddCapturesAsSubAttributes;
	
	bool m_bIsRegExpFromFile;
	std::string m_strRegExpFileName;
	IAFUtilityPtr	m_ipAFUtility;
	IMiscUtilsPtr m_ipMiscUtils;

	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	// the regular expression pattern
	std::string m_strPattern;

	// Use CachedObjectFromFile so that the regular expression is re-loaded from disk only when the
	// RegEx file is modified.
	CachedObjectFromFile<string, RegExLoader> m_cachedRegExLoader;

	void validateLicense();
	
	// Returns m_ipAFUtility, after initializing it if necessary
	IAFUtilityPtr getAFUtility();
	
	// Returns m_ipMiscUtils, after initializing it if necessary
	IMiscUtilsPtr getMiscUtils();

	// Returns the regular expression whether from a file or in a string
	std::string getRegularExpr(IAFDocumentPtr ipAFDoc);

	// Returns m_strRegExpFileName after all tags and functions have been expanded
	std::string getRegExpFileName(IAFDocumentPtr ipAFDoc);

	// Creates an attribute with the Name from the ipToken->Name and a value created from the
	// sub string of ipInput with start and end from the ipToken
	IAttributePtr createAttribute(ITokenPtr ipToken, ISpatialStringPtr ipInput);

	// Gets the regular expression parser
	IRegularExprParserPtr getParser();

	// Parses the text and returns an IUnknownVector of attributes
	IIUnknownVectorPtr parseText(IAFDocumentPtr ipAFDoc);
};
