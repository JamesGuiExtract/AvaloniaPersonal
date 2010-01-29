// ExtractLine.h : Declaration of the CExtractLine

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <vector>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CExtractLine
class ATL_NO_VTABLE CExtractLine : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CExtractLine, &CLSID_ExtractLine>,
	public ISupportErrorInfo,
	public IDispatchImpl<IExtractLine, &IID_IExtractLine, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CExtractLine>
{
public:
	CExtractLine();
	~CExtractLine();

DECLARE_REGISTRY_RESOURCEID(IDR_EXTRACTLINE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CExtractLine)
	COM_INTERFACE_ENTRY(IExtractLine)
	COM_INTERFACE_ENTRY2(IDispatch,IExtractLine)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CExtractLine)
	PROP_PAGE(CLSID_ExtractLinePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CExtractLine)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IExtractLine
	STDMETHOD(get_IncludeLineBreak)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeLineBreak)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_LineNumbers)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_LineNumbers)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_EachLineAsUniqueValue)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_EachLineAsUniqueValue)(/*[in]*/ VARIANT_BOOL newVal);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

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
	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	bool m_bEachLineAsUniqueValue;
	bool m_bIncludeLineBreak;
	// this string needs to be parsed into individual line numbers
	string m_strLineNumbers;

	// vector for line numbers
	vector<long> m_vecLineNumbers;

	IMiscUtilsPtr m_ipMiscUtils;

	///////////
	// Methods
	///////////
	// Gets a new regular expression parser with the specified pattern
	IRegularExprParserPtr getParser(const string& strPattern);

	// parse the m_strLineNumber into individual line numbers
	// and store them in the m_vecLineNumbers
	void parseLineNumbers(const string& strLineNumbers);
	void validateLicense();
};
