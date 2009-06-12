
#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CHighlightedTextWindowPTH
class ATL_NO_VTABLE CHighlightedTextWindowPTH : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHighlightedTextWindowPTH, &CLSID_HighlightedTextWindowPTH>,
	public ISupportErrorInfo,
	public IDispatchImpl<IHighlightedTextWindowPTH, &IID_IHighlightedTextWindowPTH, &LIBID_UCLID_PARAGRAPHTEXTHANDLERSLib>,
	public IDispatchImpl<IParagraphTextHandler, &IID_IParagraphTextHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CHighlightedTextWindowPTH();
	~CHighlightedTextWindowPTH();

DECLARE_REGISTRY_RESOURCEID(IDR_HIGHLIGHTEDTEXTWINDOWPTH)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHighlightedTextWindowPTH)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IHighlightedTextWindowPTH)
	COM_INTERFACE_ENTRY(IParagraphTextHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
// IHighlightedTextWindowPTH
	STDMETHOD(SetInputManager)(/*[in]*/ IInputManager *pInputManager);
// IParagraphTextHandler
	STDMETHOD(raw_NotifyParagraphTextRecognized)(
		ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText);
	STDMETHOD(raw_GetPTHDescription)(BSTR *pstrDescription);
	STDMETHOD(raw_IsPTHEnabled)(VARIANT_BOOL *pbEnabled);
// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
//	CComQIPtr<IInputManager> m_ipInputManager;
	IInputManager* m_ipInputManager;
	void validateLicense();
};
