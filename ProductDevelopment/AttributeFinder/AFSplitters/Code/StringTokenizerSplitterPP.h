// StringTokenizerSplitterPP.h : Declaration of the CStringTokenizerSplitterPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

#include <string>

EXTERN_C const CLSID CLSID_StringTokenizerSplitterPP;

/////////////////////////////////////////////////////////////////////////////
// CStringTokenizerSplitterPP
class ATL_NO_VTABLE CStringTokenizerSplitterPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringTokenizerSplitterPP, &CLSID_StringTokenizerSplitterPP>,
	public IPropertyPageImpl<CStringTokenizerSplitterPP>,
	public CDialogImpl<CStringTokenizerSplitterPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CStringTokenizerSplitterPP(); 

	enum {IDD = IDD_STRINGTOKENIZERSPLITTERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGTOKENIZERSPLITTERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringTokenizerSplitterPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CStringTokenizerSplitterPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CStringTokenizerSplitterPP>)
	COMMAND_HANDLER(IDC_RADIO1, BN_CLICKED, OnClickedRadio1)
	COMMAND_HANDLER(IDC_RADIO2, BN_CLICKED, OnClickedRadio2)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BUTTON_ADD, BN_CLICKED, OnClickedButtonAdd)
	COMMAND_HANDLER(IDC_BUTTON_MODIFY, BN_CLICKED, OnClickedButtonModify)
	COMMAND_HANDLER(IDC_BUTTON_REMOVE, BN_CLICKED, OnClickedButtonRemove)
	NOTIFY_HANDLER(IDC_LIST_SUB_ATTRIBUTES, LVN_ITEMCHANGED, OnItemchangedListSubAttributes)
	NOTIFY_HANDLER(IDC_LIST_SUB_ATTRIBUTES, NM_DBLCLK, OnDblclkListSubAttributes)
	NOTIFY_HANDLER(IDC_LIST_SUB_ATTRIBUTES, LVN_KEYDOWN, OnKeydownListSubAttributes)
	COMMAND_HANDLER(IDC_BTN_DOWN, BN_CLICKED, OnClickedBtnDown)
	COMMAND_HANDLER(IDC_BTN_UP, BN_CLICKED, OnClickedBtnUp)
	COMMAND_HANDLER(IDC_EDIT_DELIMITER, EN_UPDATE, OnUpdateEditDelimiter)
	COMMAND_HANDLER(IDC_DELIMITER_HELP, BN_CLICKED, OnClickedDelimiterHelp)
	COMMAND_HANDLER(IDC_NAME_EXPRESSION_HELP, BN_CLICKED, OnClickedNameExpressionHelp)
	COMMAND_HANDLER(IDC_SUB_ATTRIBUTES_HELP, BN_CLICKED, OnClickedSubAttributesHelp)
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnClickedRadio1(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadio2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedButtonAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListSubAttributes(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnItemchangedListSubAttributes(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeydownListSubAttributes(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnUpdateEditDelimiter(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedDelimiterHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedNameExpressionHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSubAttributesHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// controls in the property page
	ATLControls::CRichEditCtrl m_reditNameExpressionHelp;
	ATLControls::CButton m_btnRadio1;
	ATLControls::CButton m_btnRadio2;
	ATLControls::CEdit m_editDelimiter;
	ATLControls::CStatic m_txtExpressionLabel;
	ATLControls::CEdit m_editNameExpression;
	ATLControls::CListViewCtrl m_listSubAttributeNameValues;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnRemove;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnUp;
	ATLControls::CButton m_btnDown;

	int existsStringToBeReplaced(const std::string& strAttributeName);
	bool promptForReplacements(CString& zEnt1, CString& zEnt2, int& nItemIndex);
	std::string getItemText(int nIndex, int nColumnNum);
	void updateUpAndDownButtons();

	CXInfoTip m_infoTip;

private:
	bool storeDelimiterField(UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj);
	bool storeFieldNameExpression(UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj);
	bool storeAttributeNameValues(UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj);

	// ensure that this component is licensed
	void validateLicense();
};
