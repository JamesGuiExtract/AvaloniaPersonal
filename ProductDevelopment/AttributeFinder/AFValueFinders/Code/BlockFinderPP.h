// BlockFinderPP.h : Declaration of the CBlockFinderPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_BlockFinderPP;

/////////////////////////////////////////////////////////////////////////////
// CBlockFinderPP
class ATL_NO_VTABLE CBlockFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CBlockFinderPP, &CLSID_BlockFinderPP>,
	public IPropertyPageImpl<CBlockFinderPP>,
	public CDialogImpl<CBlockFinderPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CBlockFinderPP();

	enum {IDD = IDD_BLOCKFINDERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_BLOCKFINDERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CBlockFinderPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CBlockFinderPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CBlockFinderPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD_BF, BN_CLICKED, OnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_MODIFY_BF, BN_CLICKED, OnClickedBtnModify)
	COMMAND_HANDLER(IDC_BTN_REMOVE_BF, BN_CLICKED, OnClickedBtnRemove)
	COMMAND_HANDLER(IDC_RADIO_BLOCK_WITH_CLUE, BN_CLICKED, OnClickedRadioBlockWithClue)
	COMMAND_HANDLER(IDC_RADIO_FIND_ALL, BN_CLICKED, OnClickedRadioFindAll)
	NOTIFY_HANDLER(IDC_LIST_CLUES, LVN_ITEMCHANGED, OnItemchangedListClues)
	NOTIFY_HANDLER(IDC_LIST_CLUES, NM_DBLCLK, OnDblclkListClues)
	COMMAND_HANDLER(IDC_BLOCK_SEPARATOR_HELP_BF, BN_CLICKED, OnClickedSeparatorInfo)
	COMMAND_HANDLER(IDC_CLUES_HELP_BF, BN_CLICKED, OnClickedCluesInfo)
	COMMAND_HANDLER(IDC_PART_WORD_HELP_BF, BN_CLICKED, OnClickedWordPartInfo)
	COMMAND_HANDLER(IDC_MAX_HELP_BF, BN_CLICKED, OnClickedMaxCluesInfo)
	COMMAND_HANDLER(IDC_RADIO_DEFINE_BLOCKS_SEPARATOR, BN_CLICKED, OnClickedRadioDefineSeparator)
	COMMAND_HANDLER(IDC_RADIO_DEFINE_BLOCKS_BEGINEND, BN_CLICKED, OnClickedRadioDefineBeginEnd)
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
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioBlockWithClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioFindAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemchangedListClues(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnDblclkListClues(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedSeparatorInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCluesInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedWordPartInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedMaxCluesInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

	LRESULT OnClickedRadioDefineSeparator(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioDefineBeginEnd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	//////////////
	// Methods
	//////////////
	bool storeSeparator(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder);
	bool storeMinNumber(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder);

	bool storeBlockBegin(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder);
	bool storeBlockEnd(UCLID_AFVALUEFINDERSLib::IBlockFinderPtr ipBlockFinder);

	// enable/disable 'Find blocks containing...' check box related controls
	void updateControlStates();
	// enable/disable Remove/Modify buttons
	void updateButtonStates();

	// ensure that this component is licensed
	void validateLicense();

	//////////////
	// Variables
	//////////////
	ATLControls::CEdit m_editMinNumOfClues;
	ATLControls::CListViewCtrl m_listClues;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnRemove;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_chkAsRegExpr;
	ATLControls::CButton m_chkFindMax;
	ATLControls::CButton m_chkPartOfWord;
	ATLControls::CEdit m_editSeparator;
	ATLControls::CButton m_chkAllIfNoSeparator;

	ATLControls::CButton m_radioDefineSeparator;
	ATLControls::CButton m_radioDefineBeginEnd;
	ATLControls::CEdit m_editBlockBegin;
	ATLControls::CEdit m_editBlockEnd;
	ATLControls::CButton m_chkPairBeginEnd;

	CXInfoTip m_infoTip;
};
