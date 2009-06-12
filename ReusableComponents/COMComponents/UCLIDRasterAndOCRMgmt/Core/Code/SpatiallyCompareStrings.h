// SpatiallyCompareStrings.h : Declaration of the CSpatiallyCompareStrings

#ifndef __SPATIALLYCOMPARESTRINGS_H_
#define __SPATIALLYCOMPARESTRINGS_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSpatiallyCompareStrings
class ATL_NO_VTABLE CSpatiallyCompareStrings : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatiallyCompareStrings, &CLSID_SpatiallyCompareStrings>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISpatiallyCompareStrings, &IID_ISpatiallyCompareStrings, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<ISortCompare, &IID_ISortCompare, &LIBID_UCLID_COMUTILSLib>
{
public:
	CSpatiallyCompareStrings();

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALLYCOMPARESTRINGS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpatiallyCompareStrings)
	COM_INTERFACE_ENTRY(ISpatiallyCompareStrings)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ISpatiallyCompareStrings)
	COM_INTERFACE_ENTRY(ISortCompare)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

public:
// ISortCompare
	STDMETHOD(raw_LessThan)(IUnknown * pObj1, IUnknown * pObj2, VARIANT_BOOL * pbRetVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(/*[out, retval]*/ VARIANT_BOOL * pbValue);


private:
	void validateLicense();
};

#endif //__SPATIALLYCOMPARESTRINGS_H_
