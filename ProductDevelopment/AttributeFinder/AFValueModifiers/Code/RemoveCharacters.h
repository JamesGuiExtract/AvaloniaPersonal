// RemoveCharacters.h : Declaration of the CRemoveCharacters

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CRemoveCharacters
class ATL_NO_VTABLE CRemoveCharacters : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveCharacters, &CLSID_RemoveCharacters>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRemoveCharacters, &IID_IRemoveCharacters, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CRemoveCharacters>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>
{
public:
	CRemoveCharacters();
	~CRemoveCharacters();

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVECHARACTERS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveCharacters)
	COM_INTERFACE_ENTRY(IRemoveCharacters)
	COM_INTERFACE_ENTRY2(IDispatch, IRemoveCharacters)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IOutputHandler)
END_COM_MAP()

BEGIN_PROP_MAP(CRemoveCharacters)
	PROP_PAGE(CLSID_RemoveCharactersPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRemoveCharacters)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRemoveCharacters
	STDMETHOD(get_TrimTrailing)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_TrimTrailing)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_TrimLeading)(/*[out, retval]*/ VARIANT_BOOL  *pVal);
	STDMETHOD(put_TrimLeading)(/*[in]*/ VARIANT_BOOL  newVal);
	STDMETHOD(get_Consolidate)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_Consolidate)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_RemoveAll)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_RemoveAll)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_Characters)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Characters)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);

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

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector * pAttributes, IAFDocument * pDoc, 
		IProgressStatus *pProgressStatus);

private:
	/////////////
	// Methods
	////////////
	IIUnknownVectorPtr findPatternInString(IRegularExprParserPtr ipParser, _bstr_t _bstrInput, 
		_bstr_t _bstrPattern, bool bFindFirstMatchOnly = false);

	// translate the input string into a regual expression compatible string
	// for instance, input string = $abc%, RegEx string = \$abc\%
	// Note: if \t, \r and \n are found, they will not be translated
	std::string translateToRegExString(const std::string& strInput);

	void validateLicense();

	/////////////
	// Variables
	/////////////
	bool m_bCaseSensitive;
	std::string m_strCharactersDefined;
	bool m_bRemoveAll;
	bool m_bConsolidate;
	bool m_bTrimLeading;
	bool m_bTrimTrailing;

	IMiscUtilsPtr m_ipMiscUtils;

	bool m_bDirty;
};

