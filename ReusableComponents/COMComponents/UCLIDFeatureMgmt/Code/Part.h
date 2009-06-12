// Part.h : Declaration of the CPart

#pragma once

#include "resource.h"       // main symbols

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CPart
class ATL_NO_VTABLE CPart : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CPart, &CLSID_Part>,
	public ISupportErrorInfo,
	public IDispatchImpl<IPart, &IID_IPart, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CPart();
	~CPart();

DECLARE_REGISTRY_RESOURCEID(IDR_PART)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CPart)
	COM_INTERFACE_ENTRY(IPart)
	COM_INTERFACE_ENTRY2(IDispatch, IPart)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IPart
public:
	STDMETHOD(valueIsEqualTo)(/*[in]*/ IPart *pPart, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(setStartingPoint)(/*[in]*/ ICartographicPoint *pStartingPoint);
	STDMETHOD(addSegment)(/*[in]*/ IESSegment *pSegment);
	STDMETHOD(getNumSegments)(/*[out, retval]*/ long *plNumSegments);
	STDMETHOD(getSegments)(/*[out, retval]*/ IEnumSegment **pEnumSegment);
	STDMETHOD(getStartingPoint)(/*[out, retval]*/ ICartographicPoint **pStartingPoint);
	STDMETHOD(getEndingPoint)(ICartographicPoint **pEndingPoint);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	////////////
	// Variables
	////////////
	ICartographicPointPtr m_ptrStartingPoint;

	// all segments of this part
	std::vector<UCLID_FEATUREMGMTLib::IESSegmentPtr> m_vecSegments;
};

