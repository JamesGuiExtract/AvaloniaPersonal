// AdvancedReplaceString.h : Declaration of the CAdvancedReplaceString

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\AFCore\Code\StringLoader.h"

#include <CachedObjectFromFile.h>

/////////////////////////////////////////////////////////////////////////////
// CAdvancedReplaceString
class ATL_NO_VTABLE CAdvancedReplaceString : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAdvancedReplaceString, &CLSID_AdvancedReplaceString>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAdvancedReplaceString, &IID_IAdvancedReplaceString, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CAdvancedReplaceString>
{
public:
	CAdvancedReplaceString();
	~CAdvancedReplaceString();

DECLARE_REGISTRY_RESOURCEID(IDR_ADVANCEDREPLACESTRING)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAdvancedReplaceString)
	COM_INTERFACE_ENTRY(IAdvancedReplaceString)
	COM_INTERFACE_ENTRY2(IDispatch, IAdvancedReplaceString)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CAdvancedReplaceString)
	PROP_PAGE(CLSID_AdvancedReplaceStringPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CAdvancedReplaceString)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAdvancedReplaceString
	STDMETHOD(get_SpecifiedOccurrence)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_SpecifiedOccurrence)(/*[in]*/ long newVal);
	STDMETHOD(get_ReplacementOccurrenceType)(/*[out, retval]*/ EReplacementOccurrenceType *pVal);
	STDMETHOD(put_ReplacementOccurrenceType)(/*[in]*/ EReplacementOccurrenceType newVal);
	STDMETHOD(get_AsRegularExpression)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_AsRegularExpression)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Replacement)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Replacement)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_StrToBeReplaced)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_StrToBeReplaced)(/*[in]*/ BSTR newVal);

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

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument,/*[in]*/ IProgressStatus *pProgressStatus);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////
	// Variables
	/////////////
	std::string m_strToBeReplaced;
	std::string m_strReplacement;
	bool m_bCaseSensitive;
	// whether or not the input strings will be treated as Regular Expression
	bool m_bAsRegularExpression;
	EReplacementOccurrenceType m_eOccurrenceType;
	// if m_eOccurrenceType is kSpecifiedOccurrence, m_nSpecifiedOccurrence must be defined
	long m_nSpecifiedOccurrence;

	bool m_bDirty;

	// This object will used to read the string to find from the file
	CachedObjectFromFile<std::string, StringLoader> m_cachedFindStringLoader;

	// This object will used to read the string to replace the find string from the file
	CachedObjectFromFile<std::string, StringLoader> m_cachedReplaceStringLoader;

	IMiscUtilsPtr m_ipMiscUtils;

	//////////////
	// Methods
	//////////////
	// Local method to adjust Value text either from an IAttribute via IAttributeModifyingRule
	// of IAFDocument via IDocumentPreprocessor
	void	modifyValue(ISpatialString* pText, IAFDocument* pAFDoc);

	// This function will read the actual string for replacement from file if the string specified 
	// in the edit box is a file name
	void	getStringsFromFiles(IAFDocument* pAFDoc, std::string& strFind, std::string& strReplaced);

	// Return the string to find
	string getFindString(std::string strFile);

	// Return the string to replace the find string
	string getReplaceString(std::string strFile);

	void	validateLicense();
};

