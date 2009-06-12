// LimitAsLeftPartPP.h : Declaration of the CLimitAsLeftPartPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_LimitAsLeftPartPP;

/////////////////////////////////////////////////////////////////////////////
// CLimitAsLeftPartPP
class ATL_NO_VTABLE CLimitAsLeftPartPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLimitAsLeftPartPP, &CLSID_LimitAsLeftPartPP>,
	public IPropertyPageImpl<CLimitAsLeftPartPP>,
	public CDialogImpl<CLimitAsLeftPartPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLimitAsLeftPartPP();

	enum {IDD = IDD_LIMITASLEFTPARTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LIMITASLEFTPARTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLimitAsLeftPartPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CLimitAsLeftPartPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLimitAsLeftPartPP>)
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
