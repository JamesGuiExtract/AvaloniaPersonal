// EnumPart.h : Declaration of the CEnumPart

#pragma once

#include "resource.h"       // main symbols

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CEnumPart
class ATL_NO_VTABLE CEnumPart : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEnumPart, &CLSID_EnumPart>,
	public IDispatchImpl<IEnumPart, &IID_IEnumPart, &LIBID_UCLID_FEATUREMGMTLib>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEnumPartModifier, &IID_IEnumPartModifier, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{

public:
	CEnumPart();
	~CEnumPart();

DECLARE_REGISTRY_RESOURCEID(IDR_ENUMPART)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEnumPart)
	COM_INTERFACE_ENTRY(IEnumPart)
	COM_INTERFACE_ENTRY2(IDispatch, IEnumPart)
	COM_INTERFACE_ENTRY(IEnumPartModifier)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IEnumPart
public:
	STDMETHOD(next)(/*[out, retval]*/ IPart **pPart);
	STDMETHOD(reset)();

// IEnumPartModifier
	STDMETHOD(addPart)(IPart * pPart);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	///////////
	// Variables
	////////////
	bool m_bResetOnNext;
	std::vector<UCLID_FEATUREMGMTLib::IPartPtr>::iterator m_iter;
	std::vector<UCLID_FEATUREMGMTLib::IPartPtr> m_vecParts;
};
