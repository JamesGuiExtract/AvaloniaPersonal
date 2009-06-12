// LimitAsMidPartPP.h : Declaration of the CLimitAsMidPartPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_LimitAsMidPartPP;

/////////////////////////////////////////////////////////////////////////////
// CLimitAsMidPartPP
class ATL_NO_VTABLE CLimitAsMidPartPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLimitAsMidPartPP, &CLSID_LimitAsMidPartPP>,
	public IPropertyPageImpl<CLimitAsMidPartPP>,
	public CDialogImpl<CLimitAsMidPartPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLimitAsMidPartPP();

	enum {IDD = IDD_LIMITASMIDPARTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LIMITASMIDPARTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLimitAsMidPartPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CLimitAsMidPartPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLimitAsMidPartPP>)
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
