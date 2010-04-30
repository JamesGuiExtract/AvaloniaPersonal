// ValueFromList.h : Declaration of the CValueFromList

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCategories.h"

#include <CachedListLoader.h>

/////////////////////////////////////////////////////////////////////////////
// CValueFromList
class ATL_NO_VTABLE CValueFromList : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CValueFromList, &CLSID_ValueFromList>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<IValueFromList, &IID_IValueFromList, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CValueFromList>
{
public:
	CValueFromList();
	~CValueFromList();

DECLARE_REGISTRY_RESOURCEID(IDR_VALUEFROMLIST)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CValueFromList)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY2(IDispatch, IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IValueFromList)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CValueFromList)
	PROP_PAGE(CLSID_ValueFromListPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CValueFromList)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
	IMPLEMENTED_CATEGORY(CATID_INPUTFUNNEL_INPUT_VALIDATORS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// IValueFromList
	STDMETHOD(LoadListFromFile)(/*[in]*/ BSTR strFileFullName);
	STDMETHOD(SaveListToFile)(/*[in]*/ BSTR strFileFullName);	
	STDMETHOD(get_ValueList)(/*[out, retval]*/ IVariantVector* *pVal);
	STDMETHOD(put_ValueList)(/*[in]*/ IVariantVector* newVal);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);

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
	// Variables
	/////////////
	IVariantVectorPtr m_ipValueList;
	bool m_bCaseSensitive;

	// Cached list loader object to read values from files
	CCachedListLoader m_cachedListLoader;

	// whether the current object is modified
	bool m_bDirty;

	///////////
	// Methods
	///////////
	void validateLicense();
};

