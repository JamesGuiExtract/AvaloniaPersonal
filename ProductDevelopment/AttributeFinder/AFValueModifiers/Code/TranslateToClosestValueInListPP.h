// TranslateToClosestValueInListPP.h : Declaration of the CTranslateToClosestValueInListPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_TranslateToClosestValueInListPP;

/////////////////////////////////////////////////////////////////////////////
// CTranslateToClosestValueInListPP
class ATL_NO_VTABLE CTranslateToClosestValueInListPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTranslateToClosestValueInListPP, &CLSID_TranslateToClosestValueInListPP>,
	public IPropertyPageImpl<CTranslateToClosestValueInListPP>,
	public CDialogImpl<CTranslateToClosestValueInListPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTranslateToClosestValueInListPP();

	enum {IDD = IDD_TRANSLATETOCLOSESTVALUEINLISTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_TRANSLATETOCLOSESTVALUEINLISTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTranslateToClosestValueInListPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CTranslateToClosestValueInListPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CTranslateToClosestValueInListPP>)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, LVN_ITEMCHANGED, OnListItemChanged)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, NM_DBLCLK, OnDblClkList)
	NOTIFY_HANDLER(IDC_LIST_VALUE_LIST, LVN_KEYDOWN, OnKeyDownList)
	COMMAND_HANDLER(IDC_BTN_ADD_VALUE, BN_CLICKED, OnClickedBtnAddValue)
	COMMAND_HANDLER(IDC_BTN_LOAD_FILE_VALUES, BN_CLICKED, OnClickedBtnLoadFileValues)
	COMMAND_HANDLER(IDC_BTN_MODIFY_VALUE, BN_CLICKED, OnClickedBtnModifyValue)
	COMMAND_HANDLER(IDC_BTN_REMOVE_VALUE, BN_CLICKED, OnClickedBtnRemoveValue)
	COMMAND_HANDLER(IDC_CHK_CASE3, BN_CLICKED, OnClickedChkValueCaseSensitive)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHK_FORCE, BN_CLICKED, OnClickedChkForceMatch)
	COMMAND_HANDLER(IDC_BTN_SAVE_FILE_VALUES, BN_CLICKED, OnClickedBtnSaveFile)
	COMMAND_HANDLER(IDC_CLUE_DYNAMIC_LIST_HELP, STN_CLICKED, OnClickedClueDynamicListInfo)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnAddValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnLoadFileValues(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModifyValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkValueCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkForceMatch(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseValueFromList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblClkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	////////////
	// Variables
	////////////
	UCLID_AFVALUEMODIFIERSLib::ITranslateToClosestValueInListPtr m_ipInternalValueList;
	CXInfoTip m_infoTip;

	///////////
	// Methods
	///////////
	void loadListValues();
	// save translation values
	bool saveListValues();
	// enable/disable Remove and Modify buttons
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
