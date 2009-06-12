// LongPoint.h : Declaration of the CLongPoint

#pragma once

#include "resource.h"       // main symbols

#include <comdef.h>

/////////////////////////////////////////////////////////////////////////////
// CLongPoint
class ATL_NO_VTABLE CLongPoint : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLongPoint, &CLSID_LongPoint>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILongPoint, &IID_ILongPoint, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLongPoint();

DECLARE_REGISTRY_RESOURCEID(IDR_LONGPOINT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLongPoint)
	COM_INTERFACE_ENTRY(ILongPoint)
	COM_INTERFACE_ENTRY2(IDispatch, ILongPoint)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

// ILongPoint
	STDMETHOD(get_X)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_X)(/*[in]*/ long newVal);
	STDMETHOD(get_Y)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Y)(/*[in]*/ long newVal);

private:
	////////////
	// Variables
	///////////
	long m_nX;
	long m_nY;

	////////////
	// Methods
	///////////
	// check license
	void validateLicense();
};