// VariantPair.h : Declaration of the CVariantPair

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CVariantPair
class ATL_NO_VTABLE CVariantPair : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CVariantPair, &CLSID_VariantPair>,
	public ISupportErrorInfo,
	public IDispatchImpl<IVariantPair, &IID_IVariantPair, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CVariantPair();
	~CVariantPair();

DECLARE_REGISTRY_RESOURCEID(IDR_VARIANTPAIR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CVariantPair)
	COM_INTERFACE_ENTRY(IVariantPair)
	COM_INTERFACE_ENTRY2(IDispatch, IVariantPair)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IVariantPair
	STDMETHOD(get_VariantValue)(/*[out, retval]*/ VARIANT *pvtVal);
	STDMETHOD(put_VariantValue)(/*[in]*/ VARIANT newVal);
	STDMETHOD(get_VariantKey)(/*[out, retval]*/ VARIANT *pvtVal);
	STDMETHOD(put_VariantKey)(/*[in]*/ VARIANT newVal);
	STDMETHOD(SetKeyValuePair)(VARIANT vtKey, VARIANT vtVal);
	STDMETHOD(GetKeyValuePair)(VARIANT* pvtKey, VARIANT* pvtVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	////////////////
	// Variables
	////////////////
	_variant_t m_vtKey;
	_variant_t m_vtValue;
	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;

	///////////////
	// Methods
	///////////////
	// check license
	void validateLicense();
};

