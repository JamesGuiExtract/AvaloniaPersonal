// ObjectPair.h : Declaration of the CObjectPair

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CObjectPair
class ATL_NO_VTABLE CObjectPair : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjectPair, &CLSID_ObjectPair>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IObjectPair, &IID_IObjectPair, &LIBID_UCLID_COMUTILSLib>
{
public:
	CObjectPair();
	~CObjectPair();

DECLARE_REGISTRY_RESOURCEID(IDR_OBJECTPAIR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjectPair)
	COM_INTERFACE_ENTRY(IObjectPair)
	COM_INTERFACE_ENTRY2(IDispatch, IObjectPair)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IObjectPair
	STDMETHOD(get_Object2)(/*[out, retval]*/ IUnknown* *pVal);
	STDMETHOD(put_Object2)(/*[in]*/ IUnknown* newVal);
	STDMETHOD(get_Object1)(/*[out, retval]*/ IUnknown* *pVal);
	STDMETHOD(put_Object1)(/*[in]*/ IUnknown* newVal);

private:
	// check license
	void validateLicense();

	///////////////
	// Variables
	///////////////
	IUnknownPtr m_ipObject1;
	IUnknownPtr m_ipObject2;
};
