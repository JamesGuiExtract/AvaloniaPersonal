// PadValue.h : Declaration of the CPadValue
#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CPadValue
class ATL_NO_VTABLE CPadValue : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CPadValue, &CLSID_PadValue>,
	public ISupportErrorInfo,
	public IDispatchImpl<IPadValue, &IID_IPadValue, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	//public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CPadValue>
{
public:
	CPadValue();
	~CPadValue();

DECLARE_REGISTRY_RESOURCEID(IDR_PADVALUE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CPadValue)
	COM_INTERFACE_ENTRY(IPadValue)
	COM_INTERFACE_ENTRY2(IDispatch, IPadValue)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CPadValue)
	PROP_PAGE(CLSID_PadValuePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CRemoveCharacters)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// IPadValue
	STDMETHOD(get_PadLeft)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_PadLeft)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_PaddingCharacter)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_PaddingCharacter)(/*[in]*/ long newVal);
	STDMETHOD(get_RequiredSize)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_RequiredSize)(/*[in]*/ long newVal);
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

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
	void validateLicense();

	/////////////
	// Variables
	/////////////
	bool m_bDirty;

	long m_nRequiredSize;
	long m_nPaddingCharacter;

	// if true the left side is padded with the padding Character until it is the Required Size
	bool m_bPadLeft;
};

