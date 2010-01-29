// StringTokenizerModifier.h : Declaration of the CStringTokenizerModifier

#pragma once
#include "resource.h"       // main symbols

#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CStringTokenizerModifier
class ATL_NO_VTABLE CStringTokenizerModifier : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringTokenizerModifier, &CLSID_StringTokenizerModifier>,
	public ISupportErrorInfo,
	public IDispatchImpl<IStringTokenizerModifier, &IID_IStringTokenizerModifier, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CStringTokenizerModifier>
{
public:
	CStringTokenizerModifier();
	~CStringTokenizerModifier();

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGTOKENIZERMODIFIER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringTokenizerModifier)
	COM_INTERFACE_ENTRY(IStringTokenizerModifier)
	COM_INTERFACE_ENTRY2(IDispatch,IStringTokenizerModifier)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CStringTokenizerModifier)
	PROP_PAGE(CLSID_StringTokenizerModifierPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CStringTokenizerModifier)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IStringTokenizerModifier
	STDMETHOD(get_TextInBetween)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_TextInBetween)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_NumberOfTokensRequired)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumberOfTokensRequired)(/*[in]*/ long newVal);
	STDMETHOD(get_NumberOfTokensType)(/*[out, retval]*/ ENumOfTokensType *pVal);
	STDMETHOD(put_NumberOfTokensType)(/*[in]*/ ENumOfTokensType newVal);
	STDMETHOD(get_ResultExpression)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_ResultExpression)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Delimiter)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Delimiter)(/*[in]*/ BSTR newVal);

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

private:
	/////////////
	// Methods
	////////////
	// This function will take the strToBeReplacedStart and strToBeReplacedEnd
	// , replace all the token place holders with real tokens
	// ipTokens -- vector of tokenized spatial strings
	// strToBeReplacedStart -- the string that contains a number
	// strToBeReplacedEnd -- the string that contains a number, this string can be empty
	// Example: tokens are "ABC", "123", "DEF", "456", "GHI", "789"
	// 1)
	// strToBeReplacedStart = 2, strToBeReplacedEnd = 4
	// Replacement will be "123DEF456"
	// 2)
	// strToBeReplacedStart = 5, strToBeReplacedEnd = ""
	// Replacement will be "GHI"
	ISpatialStringPtr getReplacement(IIUnknownVectorPtr ipTokens, 
							   const string& strToBeReplacedStart,
							   const string& strToBeReplacedEnd);
	
	IRegularExprParserPtr getRegexParser(const string& strPattern = "");

	void validateLicense();

	// validate the syntex of the result expression
	bool validateExpression(const string& strExpr);

	/////////////
	// Variables
	/////////////
	bool m_bDirty;

	string m_strDelimiter;
	string m_strResultExpression;
	string m_strTextInBetween;
	ENumOfTokensType m_eNumOfTokensType;
	long m_nNumOfTokensRequired;

	IMiscUtilsPtr m_ipMiscUtils;
};
