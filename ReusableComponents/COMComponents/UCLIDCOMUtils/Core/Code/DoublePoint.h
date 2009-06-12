// DoublePoint.h : Declaration of the CDoublePoint

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CDoublePoint
class ATL_NO_VTABLE CDoublePoint : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDoublePoint, &CLSID_DoublePoint>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDoublePoint, &IID_IDoublePoint, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDoublePoint();

DECLARE_REGISTRY_RESOURCEID(IDR_DOUBLEPOINT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDoublePoint)
	COM_INTERFACE_ENTRY(IDoublePoint)
	COM_INTERFACE_ENTRY2(IDispatch, IDoublePoint)
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

// IDoublePoint
	STDMETHOD(get_X)(/*[out, retval]*/ double *pVal);
	STDMETHOD(put_X)(/*[in]*/ double newVal);
	STDMETHOD(get_Y)(/*[out, retval]*/ double *pVal);
	STDMETHOD(put_Y)(/*[in]*/ double newVal);

private:
	////////////
	// Variables
	///////////
	double m_dX;
	double m_dY;

	////////////
	// Methods
	///////////
	// check license
	void validateLicense();
};
