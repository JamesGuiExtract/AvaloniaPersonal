// EnumSegment.h : Declaration of the CEnumSegment

#pragma once

#include "resource.h"       // main symbols

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CEnumSegment
class ATL_NO_VTABLE CEnumSegment : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEnumSegment, &CLSID_EnumSegment>,
	public IDispatchImpl<IEnumSegment, &IID_IEnumSegment, &LIBID_UCLID_FEATUREMGMTLib>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEnumSegmentModifier, &IID_IEnumSegmentModifier, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{

public:
	CEnumSegment();
	~CEnumSegment();

DECLARE_REGISTRY_RESOURCEID(IDR_ENUMSEGMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEnumSegment)
	COM_INTERFACE_ENTRY(IEnumSegment)
	COM_INTERFACE_ENTRY2(IDispatch, IEnumSegment)
	COM_INTERFACE_ENTRY(IEnumSegmentModifier)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IEnumSegment
public:
	STDMETHOD(next)(IESSegment **pSegment);
	STDMETHOD(reset)();

// IEnumSegmentModifier
	STDMETHOD(addSegment)(IESSegment * pSegment);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	/////////////
	// Variables
	/////////////
	bool m_bResetOnNext;
	std::vector<UCLID_FEATUREMGMTLib::IESSegmentPtr> m_vecSegments;
	std::vector<UCLID_FEATUREMGMTLib::IESSegmentPtr>::iterator m_iter;
};

