// ValueAfterCluePP.h : Declaration of the CValueAfterCluePP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

#include <string>

EXTERN_C const CLSID CLSID_ValueAfterCluePP;

/////////////////////////////////////////////////////////////////////////////
// CValueAfterCluePP
class ATL_NO_VTABLE CValueAfterCluePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CValueAfterCluePP, &CLSID_ValueAfterCluePP>,
	public IPropertyPageImpl<CValueAfterCluePP>,
	public CDialogImpl<CValueAfterCluePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CValueAfterCluePP();

	enum {IDD = IDD_VALUEAFTERCLUEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_VALUEAFTERCLUEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CValueAfterCluePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CValueAfterCluePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CValueAfterCluePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	NOTIFY_HANDLER(IDC_LIST_AFTER_CLUE, LVN_ITEMCHANGED, OnListItemChanged)
	NOTIFY_HANDLER(IDC_LIST_AFTER_CLUE, LVN_KEYDOWN, OnKeyDownList)
	NOTIFY_HANDLER(IDC_LIST_AFTER_CLUE, NM_DBLCLK, OnDblclkListValue)
	COMMAND_HANDLER(IDC_BTN_ADD_AFTER_CLUE, BN_CLICKED, OnClickedBtnAddAC)
	COMMAND_HANDLER(IDC_BTN_MODIFY_AFTER_CLUE, BN_CLICKED, OnClickedBtnModifyAC)
	COMMAND_HANDLER(IDC_BTN_REMOVE_AFTER_CLUE, BN_CLICKED, OnClickedBtnRemoveAC)
	COMMAND_HANDLER(IDC_CHK_CASE_AC, BN_CLICKED, OnClickedChkCaseAC)
	COMMAND_HANDLER(IDC_CHK_INCLUDE_CLUE_LINE_AC, BN_CLICKED, OnClickedChkClueLineAC)
	COMMAND_HANDLER(IDC_CHK_OTHER_PUNC_AC, BN_CLICKED, OnClickedChkOtherPuncAC)
	COMMAND_HANDLER(IDC_CHK_SPACES_AC, BN_CLICKED, OnClickedChkSpacesAC)
	COMMAND_HANDLER(IDC_CHK_STOP_AT_NEWLINE_AC, BN_CLICKED, OnClickedChkStopAtNewLineAC)
	COMMAND_HANDLER(IDC_CHK_OTHER_STOP_AC, BN_CLICKED, OnClickedChkStopForOtherAC)
	COMMAND_HANDLER(IDC_RADIO_CLUE_LINE_AC, BN_CLICKED, OnClickedRadioClueLineAC)
	COMMAND_HANDLER(IDC_RADIO_CLUE_TO_STRING_AC, BN_CLICKED, OnClickedRadioClueToStringAC)
	COMMAND_HANDLER(IDC_RADIO_NO_TYPE_AC, BN_CLICKED, OnClickedRadioNoTypeAC)
	COMMAND_HANDLER(IDC_RADIO_UPTO_XLINES_AC, BN_CLICKED, OnClickedRadioUptoXLinesAC)
	COMMAND_HANDLER(IDC_RADIO_UPTO_XWORDS_AC, BN_CLICKED, OnClickedRadioUptoXWordsAC)
	COMMAND_HANDLER(IDC_EDIT_NUM_OF_LINES_AC, EN_CHANGE, OnChangeEditNumOfLinesAC)
	COMMAND_HANDLER(IDC_EDIT_NUM_OF_WORDS_AC, EN_CHANGE, OnChangeEditNumOfWordsAC)
	COMMAND_HANDLER(IDC_EDIT_OTHER_PUNC_AC, EN_CHANGE, OnChangeEditOtherPuncsAC)
	COMMAND_HANDLER(IDC_EDIT_OTHER_STOP_AC, EN_CHANGE, OnChangeEditOtherStopsAC)
	COMMAND_HANDLER(IDC_EDIT_STRING_SPEC_AC, EN_CHANGE, OnChangeEditSpecStringAC)
	COMMAND_HANDLER(IDC_STOP_CHAR_HELP_VA, BN_CLICKED, OnClickedStopCharInfo)
	COMMAND_HANDLER(IDC_SEPARATOR_HELP_VA, BN_CLICKED, OnClickedSeparateCharInfo)
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
	LRESULT OnClickedBtnAddAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModifyAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkClueLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkOtherPuncAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkSpacesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkStopAtNewLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkStopForOtherAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioClueLineAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioClueToStringAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioNoTypeAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioUptoXLinesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioUptoXWordsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditNumOfLinesAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditNumOfWordsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditOtherPuncsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditOtherStopsAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnChangeEditSpecStringAC(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeyDownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedStopCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSeparateCharInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

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
