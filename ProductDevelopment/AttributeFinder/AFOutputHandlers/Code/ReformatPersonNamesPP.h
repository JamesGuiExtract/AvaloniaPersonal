// ReformatPersonNamesPP.h : Declaration of the CReformatPersonNamesPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_ReformatPersonNamesPP;

/////////////////////////////////////////////////////////////////////////////
// CReformatPersonNamesPP
class ATL_NO_VTABLE CReformatPersonNamesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReformatPersonNamesPP, &CLSID_ReformatPersonNamesPP>,
	public IPropertyPageImpl<CReformatPersonNamesPP>,
	public CDialogImpl<CReformatPersonNamesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CReformatPersonNamesPP();

	enum {IDD = IDD_REFORMATPERSONNAMESPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REFORMATPERSONNAMESPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReformatPersonNamesPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CReformatPersonNamesPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CReformatPersonNamesPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_NAME_FORMAT_INFO, BN_CLICKED, OnClickedNameFormatInfo)
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
	LRESULT OnClickedNameFormatInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CEdit m_editAttributeQuery;
	ATLControls::CButton m_chkReformatPersonSubAttributes;
	ATLControls::CEdit m_editFormatString;

	CXInfoTip m_infoTip;

	void validateLicense();
};

