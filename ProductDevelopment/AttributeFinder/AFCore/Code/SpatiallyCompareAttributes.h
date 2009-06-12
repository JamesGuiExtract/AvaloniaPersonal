// SpatiallyCompareAttributes.h : Declaration of the CSpatiallyCompareAttributes

#ifndef __SPATIALLYCOMPAREATTRIBUTES_H_
#define __SPATIALLYCOMPAREATTRIBUTES_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSpatiallyCompareAttributes
class ATL_NO_VTABLE CSpatiallyCompareAttributes : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatiallyCompareAttributes, &CLSID_SpatiallyCompareAttributes>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISpatiallyCompareAttributes, &IID_ISpatiallyCompareAttributes, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ISortCompare, &IID_ISortCompare, &LIBID_UCLID_COMUTILSLib>
{
public:
	CSpatiallyCompareAttributes();

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALLYCOMPAREATTRIBUTES)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpatiallyCompareAttributes)
	COM_INTERFACE_ENTRY(ISpatiallyCompareAttributes)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ISpatiallyCompareAttributes)
	COM_INTERFACE_ENTRY(ISortCompare)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISpatiallyCompareAttributes
public:
// ISortCompare
	STDMETHOD(raw_LessThan)(IUnknown * pObj1, IUnknown * pObj2, VARIANT_BOOL * pbRetVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL * pbValue);


private:
	void validateLicense();
};

#endif //__SPATIALLYCOMPAREATTRIBUTES_H_
