// NothingInputFinder.h : Declaration of the CNothingInputFinder

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CNothingInputFinder
class ATL_NO_VTABLE CNothingInputFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CNothingInputFinder, &CLSID_NothingInputFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputFinder, &IID_IInputFinder, &LIBID_UCLID_INPUTFINDERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CNothingInputFinder()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_NOTHINGINPUTFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CNothingInputFinder)
	COM_INTERFACE_ENTRY(IInputFinder)
	COM_INTERFACE_ENTRY2(IDispatch, IInputFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputFinder
	STDMETHOD(ParseString)(BSTR strInput, IIUnknownVector **ippTokenPositions);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// Check for license
	void validateLicense();
};
