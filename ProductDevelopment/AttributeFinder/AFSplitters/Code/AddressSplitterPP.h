// AddressSplitterPP.h : Declaration of the CAddressSplitterPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_AddressSplitterPP;

/////////////////////////////////////////////////////////////////////////////
// CAddressSplitterPP
class ATL_NO_VTABLE CAddressSplitterPP : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAddressSplitterPP, &CLSID_AddressSplitterPP>,
	public IPropertyPageImpl<CAddressSplitterPP>,
	public CDialogImpl<CAddressSplitterPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAddressSplitterPP();

	enum {IDD = IDD_ADDRESSSPLITTERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_ADDRESSSPLITTERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAddressSplitterPP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CAddressSplitterPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CAddressSplitterPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// Check licensing
	void validateLicense();
};
