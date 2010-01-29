
#pragma once
#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_RegExprRulePP;

/////////////////////////////////////////////////////////////////////////////
// CRegExprRulePP
class ATL_NO_VTABLE CRegExprRulePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRegExprRulePP, &CLSID_RegExprRulePP>,
	public IPropertyPageImpl<CRegExprRulePP>,
	public CDialogImpl<CRegExprRulePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRegExprRulePP();

	enum {IDD = IDD_REGEXPRRULEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REGEXPRRULEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRegExprRulePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRegExprRulePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRegExprRulePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_EDIT_PATTERN, EN_CHANGE, OnChangeEditPattern)
	COMMAND_HANDLER(IDC_CHK_REG_EXP_CASE, BN_CLICKED, OnClickedChkCaseRegExpr)
	COMMAND_HANDLER(IDC_BTN_BROWSE_REG_EXP, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_RADIO_FILE, BN_CLICKED, OnClickedRadioFile)
	COMMAND_HANDLER(IDC_RADIO_TEXT, BN_CLICKED, OnClickedRadioText)
	COMMAND_HANDLER(IDC_BTN_OPEN_NOTEPAD, BN_CLICKED, OnClickedBtnOpenNotepad)
	COMMAND_HANDLER(IDC_REG_EXP_FILE_INFO, BN_CLICKED, OnClickedRegExpFileInfo)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
	COMMAND_HANDLER(IDC_CHK_NAMED_MATCHES_AS_SUBATTRIBUTES, BN_CLICKED, OnClickedNamedMatchesAsSubAttributes);

END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
		BOOL& bHandled);
	LRESULT OnChangeEditPattern(WORD wNotifyCode, WORD wID, 
		HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseRegExpr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnOpenNotepad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRegExpFileInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedNamedMatchesAsSubAttributes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CButton m_radioPatternText;
	ATLControls::CEdit m_editPatternText;
	ATLControls::CEdit m_editRegExpFile;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnOpenNotepad;
	ATLControls::CButton m_btnSelectDocTag;
	ATLControls::CButton m_checkCreateSubAttributesFromMatches;

	CXInfoTip m_infoTip;

	bool m_bIsRegExpFromFile;

	bool storePattern(UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule);
	bool storePatternFile(UCLID_AFVALUEFINDERSLib::IRegExprRulePtr ipRegExprRule);
	void updateControls();

	// ensure that this component is licensed
	void validateLicense();
};
