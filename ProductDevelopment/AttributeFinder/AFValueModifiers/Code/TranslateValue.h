// TranslateValue.h : Declaration of the CTranslateValue

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedListLoader.h>

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CTranslateValue
class ATL_NO_VTABLE CTranslateValue : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTranslateValue, &CLSID_TranslateValue>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITranslateValue, &IID_ITranslateValue, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CTranslateValue>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>
{
public:
	CTranslateValue();
	~CTranslateValue();

DECLARE_REGISTRY_RESOURCEID(IDR_TRANSLATEVALUE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTranslateValue)
	COM_INTERFACE_ENTRY(ITranslateValue)
	COM_INTERFACE_ENTRY2(IDispatch, ITranslateValue)
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

BEGIN_PROP_MAP(CTranslateValue)
	PROP_PAGE(CLSID_TranslateValuePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CTranslateValue)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITranslateValue
	STDMETHOD(get_TranslationStringPairs)(/*[out, retval]*/ IIUnknownVector* *pVal);
	STDMETHOD(put_TranslationStringPairs)(/*[in]*/ IIUnknownVector* pVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(LoadTranslationsFromFile)(/*[in]*/ BSTR strFileFullName, /*[in]*/ BSTR strDelimiter);
	STDMETHOD(SaveTranslationsToFile)(/*[in]*/ BSTR strFileFullName, /*[in]*/ BSTR cDelimiter);
	STDMETHOD(get_TranslateFieldType)(/*[out, retval]*/ ETranslateFieldType* newVal);
	STDMETHOD(put_TranslateFieldType)(/*[in]*/ ETranslateFieldType newVal);

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
	//////////////
	// Methods
	///////////////
	void loadFromFile(const std::string& strFileName, const char& cDelimiter);
	void validateLicense();

	//////////////
	// Variables
	/////////////
	// map holds translate-from and translate-to string pairs
	IIUnknownVectorPtr m_ipTranslationStringPairs;

	// whether case-sensitive or not
	bool m_bCaseSensitive;

	ETranslateFieldType m_eTranslateFieldType;

	// Cached list loader object to read values from files
	CCachedListLoader m_cachedListLoader;

	bool m_bDirty;
};

