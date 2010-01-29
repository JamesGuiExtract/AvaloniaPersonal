// ValueBeforeCluePP.h : Declaration of the CValueBeforeCluePP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>
#include <string>

EXTERN_C const CLSID CLSID_ValueBeforeCluePP;

/////////////////////////////////////////////////////////////////////////////
// CValueBeforeCluePP
class ATL_NO_VTABLE CValueBeforeCluePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CValueBeforeCluePP, &CLSID_ValueBeforeCluePP>,
	public IPropertyPageImpl<CValueBeforeCluePP>,
	public CDialogImpl<CValueBeforeCluePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CValueBeforeCluePP();

	enum {IDD = IDD_VALUEBEFORECLUEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_VALUEBEFORECLUEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CValueBeforeCluePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CValueBeforeCluePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CValueBeforeCluePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	NOTIFY_HANDLER(IDC_LIST_BEFORE_CLUE, LVN_ITEMCHANGED, OnListItemChanged)
	NOTIFY_HANDLER(IDC_LIST_BEFORE_CLUE, LVN_KEYDOWN, OnKeyDownList)
	NOTIFY_HANDLER(IDC_LIST_BEFORE_CLUE, NM_DBLCLK, OnDblclkListValue)
	COMMAND_HANDLER(IDC_BTN_ADD_BEFORE_CLUE, BN_CLICKED, OnClickedBtnAddBC)
	COMMAND_HANDLER(IDC_BTN_MODIFY_BEFORE_CLUE, BN_CLICKED, OnClickedBtnModifyBC)
	COMMAND_HANDLER(IDC_BTN_REMOVE_BEFORE_CLUE, BN_CLICKED, OnClickedBtnRemoveBC)
	COMMAND_HANDLER(IDC_CHK_CASE_BC, BN_CLICKED, OnClickedChkCaseBC)
	COMMAND_HANDLER(IDC_CHK_INCLUDE_CLUE_LINE_BC, BN_CLICKED, OnClickedChkClueLineBC)
	COMMAND_HANDLER(IDC_CHK_OTHER_PUNC_BC, BN_CLICKED, OnClickedChkOtherPuncBC)
	COMMAND_HANDLER(IDC_CHK_SPACES_BC, BN_CLICKED, OnClickedChkSpacesBC)
	COMMAND_HANDLER(IDC_CHK_STOP_AT_NEWLINE_BC, BN_CLICKED, OnClickedChkStopAtNewLineBC)
	COMMAND_HANDLER(IDC_CHK_OTHER_STOP_BC, BN_CLICKED, OnClickedChkStopForOtherBC)
	COMMAND_HANDLER(IDC_RADIO_CLUE_LINE_BC, BN_CLICKED, OnClickedRadioClueLineBC)
	COMMAND_HANDLER(IDC_RADIO_CLUE_TO_STRING_BC, BN_CLICKED, OnClickedRadioClueToStringBC)
	COMMAND_HANDLER(IDC_RADIO_NO_TYPE_BC, BN_CLICKED, OnClickedRadioNoTypeBC)
	COMMAND_HANDLER(IDC_RADIO_UPTO_XLINES_BC, BN_CLICKED, OnClickedRadioUptoXLinesBC)
	COMMAND_HANDLER(IDC_RADIO_UPTO_XWORDS_BC, BN_CLICKED, OnClickedRadioUptoXWordsBC)
	COMMAND_HANDLER(IDC_EDIT_NUM_OF_LINES_BC, EN_CHANGE, OnChangeEditNumOfLinesBC)
	COMMAND_HANDLER(IDC_EDIT_NUM_OF_WORDS_BC, EN_CHANGE, OnChangeEditNumOfWordsBC)
	COMMAND_HANDLER(IDC_EDIT_OTHER_PUNC_BC, EN_CHANGE, OnChangeEditOtherPuncsBC)
	COMMAND_HANDLER(IDC_EDIT_OTHER_STOP_BC, EN_CHANGE, OnChangeEditOtherStopsBC)
	COMMAND_HANDLER(IDC_EDIT_STRING_SPEC_BC, EN_CHANGE, OnChangeEditSpecStringBC)
	COMMAND_HANDLER(IDC_STOP_CHAR_HELP_VB, BN_CLICKED, OnClickedStopCharInfo)
	COMMAND_HANDLER(IDC_SEPERATOR_HELP_VB, BN_CLICKED, OnClickedSeperateCharInfo)
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
	LRESULT OnClickedBtnAddBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModifyBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkClueLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkOtherPuncBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkSpacesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkStopAtNewLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkStopForOtherBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioClueLineBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioClueToStringBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioNoTypeBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioUptoXLinesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioUptoXWordsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditNumOfLinesBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditNumOfWordsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditOtherPuncsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditOtherStopsBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditSpecStringBC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedStopCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSeperateCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	UCLID_AFVALUEFINDERSLib::ERuleRefiningType m_eRefiningType;
	bool m_bCaseSensitive;

	std::string m_strOtherPunctuations;
	std::string m_strOtherStops;
	std::string m_strSpecifiedString;
	long m_nNumOfWords;
	long m_nNumOfLines;
	bool m_bStopAtNewLine;
	bool m_bStopForOther;
	bool m_bSpacesAsPunctuations;
	bool m_bSpecifyOtherPunctuations;
	bool m_bIncludeClueLine;

	CXInfoTip m_infoTip;

	////////////
	// Methods
	////////////
	// disable all check boxes and edit boxes that associated with
	// current selected refining type
	void disableCheckAndEditBoxes();
	void selectRefiningType();
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
