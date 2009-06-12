// RemoveCharactersPP.h : Declaration of the CRemoveCharactersPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>
#include <string>

EXTERN_C const CLSID CLSID_RemoveCharactersPP;

/////////////////////////////////////////////////////////////////////////////
// CRemoveCharactersPP
class ATL_NO_VTABLE CRemoveCharactersPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveCharactersPP, &CLSID_RemoveCharactersPP>,
	public IPropertyPageImpl<CRemoveCharactersPP>,
	public CDialogImpl<CRemoveCharactersPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRemoveCharactersPP();

	enum {IDD = IDD_REMOVECHARACTERSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVECHARACTERSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveCharactersPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRemoveCharactersPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRemoveCharactersPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHK_CASE4, BN_CLICKED, OnClickedChkCaseRemoveChar)
	COMMAND_HANDLER(IDC_CHK_OTHER_CHARS, BN_CLICKED, OnClickedChkOtherChars)
	COMMAND_HANDLER(IDC_RADIO_REMOVE_ALL, BN_CLICKED, OnClickedRadioRemoveAll)
	COMMAND_HANDLER(IDC_RADIO_SELECT_FOLLOW, BN_CLICKED, OnClickedRadioSelectFollowing)
	COMMAND_HANDLER(IDC_CONSOLIDATE_INFO, BN_CLICKED, OnClickedConsolidateInfo)
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
	LRESULT OnClickedChkCaseRemoveChar(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkOtherChars(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioRemoveAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSelectFollowing(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedConsolidateInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////////
	// Variables
	///////////////
	bool m_bCaseSensitive;
	CXInfoTip m_infoTip;

	///////////////
	// Methods
	///////////////
	// enable/disable check boxes for consolidate, trim leading and trim trailing
	void SetButtonStates();

	// ensure that this component is licensed
	void validateLicense();
};
