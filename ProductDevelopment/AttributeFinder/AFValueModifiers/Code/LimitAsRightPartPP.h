// LimitAsRightPartPP.h : Declaration of the CLimitAsRightPartPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_LimitAsRightPartPP;

/////////////////////////////////////////////////////////////////////////////
// CLimitAsRightPartPP
class ATL_NO_VTABLE CLimitAsRightPartPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLimitAsRightPartPP, &CLSID_LimitAsRightPartPP>,
	public IPropertyPageImpl<CLimitAsRightPartPP>,
	public CDialogImpl<CLimitAsRightPartPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLimitAsRightPartPP();

	enum {IDD = IDD_LIMITASRIGHTPARTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LIMITASRIGHTPARTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLimitAsRightPartPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CLimitAsRightPartPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLimitAsRightPartPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:

	// ensure that this component is licensed
	void validateLicense();
};
