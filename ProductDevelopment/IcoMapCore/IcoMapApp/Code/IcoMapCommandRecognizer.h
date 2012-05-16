// IcoMapCommandRecognizer.h : Declaration of the CIcoMapCommandRecognizer

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CIcoMapCommandRecognizer
class ATL_NO_VTABLE CIcoMapCommandRecognizer : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIcoMapCommandRecognizer, &CLSID_IcoMapCommandRecognizer>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:

DECLARE_REGISTRY_RESOURCEID(IDR_ICOMAPCOMMANDRECOGNIZER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CIcoMapCommandRecognizer)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	void validateLicense();
};
