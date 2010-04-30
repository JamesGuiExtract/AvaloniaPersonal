// ValueFromListPP.h : Declaration of the CValueFromListPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_ValueFromListPP;

/////////////////////////////////////////////////////////////////////////////
// CValueFromListPP
class ATL_NO_VTABLE CValueFromListPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CValueFromListPP, &CLSID_ValueFromListPP>,
	public IPropertyPageImpl<CValueFromListPP>,
	public CDialogImpl<CValueFromListPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CValueFromListPP();

	enum {IDD = IDD_VALUEFROMLISTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_VALUEFROMLISTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CValueFromListPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CValueFromListPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CValueFromListPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, LVN_ITEMCHANGED, OnListItemChanged)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, NM_DBLCLK, OnDblClkList)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, LVN_KEYDOWN, OnKeyDownList)
	COMMAND_HANDLER(IDC_BTN_ADD_VALUE_TO_LIST, BN_CLICKED, OnClickedBtnAddValueToList)
	COMMAND_HANDLER(IDC_BTN_LOAD_VALUE_LIST_FILE, BN_CLICKED, OnClickedBtnLoadFromFile)
	COMMAND_HANDLER(IDC_BTN_SAVE_VALUE_LIST_FILE, BN_CLICKED, OnClickedBtnSaveToFile)
	COMMAND_HANDLER(IDC_BTN_MODIFY_VALUE_IN_LIST, BN_CLICKED, OnClickedBtnModifyValueInList)
	COMMAND_HANDLER(IDC_BTN_REMOVE_VALUE_FROM_LIST, BN_CLICKED, OnClickedBtnRemoveValueFromList)
	COMMAND_HANDLER(IDC_CHK_CASE_FROM_LIST, BN_CLICKED, OnClickedChkCaseValueFromList)
	COMMAND_HANDLER(IDC_CLUE_DYNAMIC_LIST_HELP, STN_CLICKED, OnClickedClueDynamicListInfo)
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
	LRESULT OnClickedBtnAddValueToList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnLoadFromFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSaveToFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModifyValueInList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveValueFromList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseValueFromList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblClkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	UCLID_AFVALUEFINDERSLib::IValueFromListPtr m_ipInternalObject;
	CXInfoTip m_infoTip;

	//////////
	// Methods
	//////////
	void loadListValues();
	bool saveListValues();
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
