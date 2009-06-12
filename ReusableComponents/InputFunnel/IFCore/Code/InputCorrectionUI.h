// InputCorrectionUI.h : Declaration of the CInputCorrectionUI
#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CInputCorrectionUI
class ATL_NO_VTABLE CInputCorrectionUI : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInputCorrectionUI, &CLSID_InputCorrectionUI>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputCorrectionUI, &IID_IInputCorrectionUI, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CInputCorrectionUI();

DECLARE_REGISTRY_RESOURCEID(IDR_INPUTCORRECTIONUI)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInputCorrectionUI)
	COM_INTERFACE_ENTRY(IInputCorrectionUI)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IInputCorrectionUI)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IInputCorrectionUI
	STDMETHOD(PromptForCorrection)(/*[in]*/ IInputValidator* pValidator, /*[in]*/ ITextInput* pTextInput, /*[out, retval]*/ VARIANT_BOOL *pbSuccess);
	STDMETHOD(get_ParentWndHandle)(long *pVal);
	STDMETHOD(put_ParentWndHandle)(long newVal);

private:
	// parent window handle
	long m_lParentWndHandle;

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};

