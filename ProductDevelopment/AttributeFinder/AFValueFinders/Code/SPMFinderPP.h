// SPMFinderPP.h : Declaration of the CSPMFinderPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_SPMFinderPP;

/////////////////////////////////////////////////////////////////////////////
// CSPMFinderPP
class ATL_NO_VTABLE CSPMFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSPMFinderPP, &CLSID_SPMFinderPP>,
	public IPropertyPageImpl<CSPMFinderPP>,
	public CDialogImpl<CSPMFinderPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSPMFinderPP();

	enum {IDD = IDD_SPMFINDERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_SPMFINDERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSPMFinderPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CSPMFinderPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CSPMFinderPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_SPM, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_RADIO_FILE, BN_CLICKED, OnClickedRadioFile)
	COMMAND_HANDLER(IDC_RADIO_TEXT, BN_CLICKED, OnClickedRadioText)
	COMMAND_HANDLER(IDC_BTN_OPEN_NOTEPAD, BN_CLICKED, OnClickedBtnOpenNotepad)
	COMMAND_HANDLER(IDC_PATTERN_TEXT, BN_CLICKED, OnClickedPatternTextInfo)
	COMMAND_HANDLER(IDC_PATTERN_FILE_INFO, BN_CLICKED, OnClickedPatternFileInfo)
	COMMAND_HANDLER(IDC_CHK_STORE_RULE_WORKED, BN_CLICKED, OnClickedCheckStoreRuleWorked)
	COMMAND_HANDLER(IDC_MIN_SCORE_INFO, BN_CLICKED, OnClickedMinScoreInfo)
	COMMAND_HANDLER(IDC_BUTTON_SELECT_DATA_SCORER, BN_CLICKED, OnClickedButtonSelectDataScorer)
	COMMAND_HANDLER(IDC_BTN_ADD_SPM, BN_CLICKED, OnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_MODIFY_SPM, BN_CLICKED, OnClickedBtnModify)
	COMMAND_HANDLER(IDC_BTN_REMOVE_SPM, BN_CLICKED, OnClickedBtnRemove)
	COMMAND_HANDLER(IDC_PREPROCESSOR_INFO, BN_CLICKED, OnClickedPreprocessorInfo)
	COMMAND_HANDLER(IDC_MIN_FIRST_SCORE_INFO, BN_CLICKED, OnClickedMinFirstScoreInfo)
	NOTIFY_HANDLER(IDC_LIST_PREPROCESSORS, NM_DBLCLK, OnDblclkListPreprocessor)
	NOTIFY_HANDLER(IDC_LIST_PREPROCESSORS, LVN_ITEMCHANGED, OnListItemChanged)
	COMMAND_HANDLER(IDC_RADIO_RETURN_FIRST_MATCH, BN_CLICKED, OnClickedRadioMatch )
	COMMAND_HANDLER(IDC_RADIO_RETURN_ALL_MATCHES, BN_CLICKED, OnClickedRadioMatch )
	COMMAND_HANDLER(IDC_RADIO_RETURN_BEST_MATCH, BN_CLICKED, OnClickedRadioMatch )
	COMMAND_HANDLER(IDC_RADIO_RETURN_FIRST_OR_BEST, BN_CLICKED, OnClickedRadioMatch )
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
	COMMAND_HANDLER(IDC_RULEID_TAG_INFO, BN_CLICKED, OnClickedRuleIDTagInfo)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioText(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnOpenNotepad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPatternTextInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPatternFileInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedPreprocessorInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckStoreRuleWorked(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedMinScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonSelectDataScorer(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedMinFirstScoreInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListPreprocessor(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedRadioMatch(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRuleIDTagInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////
	// Variables
	/////////////
	ATLControls::CButton m_radioPatternText;
	ATLControls::CEdit m_editPatternText;
	ATLControls::CEdit m_editRuleFile;
	ATLControls::CEdit m_editRuleWorkedName;
	ATLControls::CStatic m_txtDefineRuleWorkedName; 
	ATLControls::CButton m_chkStoreRuleWorked;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnOpenNotepad;
	ATLControls::CEdit m_editMinMatchScore;
	ATLControls::CEdit m_editMinFirstMatchScore;
	ATLControls::CStatic m_txtMinScoreLabel;
	ATLControls::CStatic m_txtMinFirstScoreLabel;
	ATLControls::CButton m_radioReturnFirstMatch;
	ATLControls::CButton m_radioReturnBestMatch;
	ATLControls::CButton m_radioReturnAllMatches;
	ATLControls::CButton m_radioReturnFirstOrBest;
	ATLControls::CListViewCtrl m_listPreprocessor;
	ATLControls::CButton m_chkIgnoreMissingTags;
	ATLControls::CButton m_btnSelectDocTag;

	bool m_bIsPatternsFromFile;
	bool m_bStoreRuleWorked;
	IObjectWithDescriptionPtr m_ipDataScorer;
	CXInfoTip m_infoTip;

	/////////////
	// Methods
	/////////////
	// whether or not the user has the RDT license in order to
	// define their own patterns (or pattern files)
	bool isRDTLicensed();
	bool usingDataScorer();
	bool storeRulesFile(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder);
	bool storeRulesText(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder);
	void storeRuleWorked(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder);
	void storeDataScorerInfo(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder);
	void storePreprocessors(UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSPMFinder);
	void updateControls();
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
