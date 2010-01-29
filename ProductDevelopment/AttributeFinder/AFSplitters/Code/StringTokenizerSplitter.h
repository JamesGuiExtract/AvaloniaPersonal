// StringTokenizerSplitter.h : Declaration of the CStringTokenizerSplitter

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CStringTokenizerSplitter
class ATL_NO_VTABLE CStringTokenizerSplitter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringTokenizerSplitter, &CLSID_StringTokenizerSplitter>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IStringTokenizerSplitter, &IID_IStringTokenizerSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CStringTokenizerSplitter>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFSPLITTERSLib>
{
public:
	CStringTokenizerSplitter();
	~CStringTokenizerSplitter();

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGTOKENIZERSPLITTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringTokenizerSplitter)
	COM_INTERFACE_ENTRY(IAttributeSplitter)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeSplitter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IStringTokenizerSplitter)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_PROP_MAP(CStringTokenizerSplitter)
	PROP_PAGE(CLSID_StringTokenizerSplitterPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CStringTokenizerSplitter)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
END_CATEGORY_MAP()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IStringTokenizerSplitter
	STDMETHOD(get_Delimiter)(/*[out, retval]*/ short *pVal);
	STDMETHOD(put_Delimiter)(/*[in]*/ short newVal);
	STDMETHOD(get_SplitType)(/*[out, retval]*/ EStringTokenizerSplitType *pVal);
	STDMETHOD(put_SplitType)(/*[in]*/ EStringTokenizerSplitType newVal);
	STDMETHOD(get_FieldNameExpression)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_FieldNameExpression)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_AttributeNameAndValueExprVector)(/*[out, retval]*/ IIUnknownVector* *pVal);
	STDMETHOD(put_AttributeNameAndValueExprVector)(/*[in]*/ IIUnknownVector *newVal);
	STDMETHOD(IsValidSubAttributeValueExpression)(/*[in]*/ BSTR strExpr, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(IsValidSubAttributeName)(/*[in]*/ BSTR strName, /*[out, retval]*/ VARIANT_BOOL *pbValue);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

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
	char m_cDelimiter;
	std::string m_strFieldNameExpression;
	IIUnknownVectorPtr m_ipVecNameValuePair;
	EStringTokenizerSplitType m_eSplitType;

	IMiscUtilsPtr m_ipMiscUtils;

	// Gets a regular expression parser with the specified pattern
	IRegularExprParserPtr getRegExParser(const string& strPattern);
	
	// flag to keep track of whether object is dirty
	bool m_bDirty;

	// ensure that this component is licensed
	void validateLicense();

	// reset member variables to some reasonable default state
	void resetVariablesToDefault();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To validate whether the field name expression (such as "Field_%d")
	//			is a valid expression.
	// PROMISE: an exception will be thrown if the field name expression
	//			is not valid.
	void validateFieldNameExpression(const std::string& strFieldNameExpr);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To validate whether the sub-attribute value expression (such as "%1_%2")
	//			is a valid expression.
	// PROMISE: An exception will be thrown if the strExpr is not a valid
	//			sub-attribute value expression.
	void validateSubAttributeValueExpression(const std::string& strExpr);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To validate whether the sub-attribute name (such as "FirstName")
	//			is a valid name.
	// PROMISE: An exception will be thrown if the strName is not a valid
	//			sub-attribute name.
	void validateSubAttributeName(const std::string& strName);
	//---------------------------------------------------------------------------------------------
};
