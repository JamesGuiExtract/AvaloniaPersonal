// InsertCharactersPP.h : Declaration of the CInsertCharactersPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_InsertCharactersPP;

/////////////////////////////////////////////////////////////////////////////
// CInsertCharactersPP
class ATL_NO_VTABLE CInsertCharactersPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInsertCharactersPP, &CLSID_InsertCharactersPP>,
	public IPropertyPageImpl<CInsertCharactersPP>,
	public CDialogImpl<CInsertCharactersPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CInsertCharactersPP();
	~CInsertCharactersPP();

	enum {IDD = IDD_INSERTCHARACTERSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_INSERTCHARACTERSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInsertCharactersPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CInsertCharactersPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CInsertCharactersPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CMB_LENGTH_TYPE, CBN_SELCHANGE, OnSelchangeCmbLengthType)
	COMMAND_HANDLER(IDC_RADIO_APPEND, BN_CLICKED, OnClickedRadioAppend)
	COMMAND_HANDLER(IDC_RADIO_SPECIFIED, BN_CLICKED, OnClickedRadioSpecified)
	COMMAND_HANDLER(IDC_POSITION_INFO, BN_CLICKED, OnClickedPositionInfo)
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
	LRESULT OnSelchangeCmbLengthType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioAppend(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPositionInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CComboBox m_cmbLengthType;
	ATLControls::CEdit m_editNumOfCharsLong;
	ATLControls::CStatic m_picPositionInfo;

	CXInfoTip m_infoTip;

	// ensure that this component is licensed
	void validateLicense();
};
