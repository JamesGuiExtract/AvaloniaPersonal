// TranslateToClosestValueInList.h : Declaration of the CTranslateToClosestValueInList

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedListLoader.h>

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CTranslateToClosestValueInList
class ATL_NO_VTABLE CTranslateToClosestValueInList : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTranslateToClosestValueInList, &CLSID_TranslateToClosestValueInList>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITranslateToClosestValueInList, &IID_ITranslateToClosestValueInList, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CTranslateToClosestValueInList>
{
public:
	CTranslateToClosestValueInList();
	~CTranslateToClosestValueInList();

DECLARE_REGISTRY_RESOURCEID(IDR_TRANSLATETOCLOSESTVALUEINLIST)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTranslateToClosestValueInList)
	COM_INTERFACE_ENTRY(ITranslateToClosestValueInList)
	COM_INTERFACE_ENTRY2(IDispatch, ITranslateToClosestValueInList)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CTranslateToClosestValueInList)
	PROP_PAGE(CLSID_TranslateToClosestValueInListPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CTranslateToClosestValueInList)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITranslateToClosestValueInList
	STDMETHOD(SaveValuesToFile)(/*[in]*/ BSTR strFileFullName);
	STDMETHOD(LoadValuesFromFile)(/*[in]*/ BSTR strFileFullName);
	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_ClosestValueList)(/*[out, retval]*/ IVariantVector* *pVal);
	STDMETHOD(put_ClosestValueList)(/*[in]*/ IVariantVector* newVal);
	STDMETHOD(get_IsForcedMatch)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsForcedMatch)(/*[in]*/ VARIANT_BOOL newVal);

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
	///////////////
	// Variables
	///////////////
	bool m_bCaseSensitive;
	bool m_bForceMatch;
	IVariantVectorPtr m_ipClosestValuesList;

	// Cached list loader object to read values from files
	CCachedListLoader m_cachedListLoader;

	bool m_bDirty;

	//////////
	// Methods
	///////////

	void validateLicense();
};

