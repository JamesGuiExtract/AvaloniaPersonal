// ReplaceStringsPP.h : Declaration of the CReplaceStringsPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>
#include <string>

EXTERN_C const CLSID CLSID_ReplaceStringsPP;

/////////////////////////////////////////////////////////////////////////////
// CReplaceStringsPP
class ATL_NO_VTABLE CReplaceStringsPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReplaceStringsPP, &CLSID_ReplaceStringsPP>,
	public IPropertyPageImpl<CReplaceStringsPP>,
	public CDialogImpl<CReplaceStringsPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CReplaceStringsPP();

	enum {IDD = IDD_REPLACESTRINGSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REPLACESTRINGSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReplaceStringsPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CReplaceStringsPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CReplaceStringsPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD2, BN_CLICKED, OnClickedBtnAddReplacement)
	COMMAND_HANDLER(IDC_BTN_LOADFILE2, BN_CLICKED, OnClickedBtnLoadReplacementFromFile)
	COMMAND_HANDLER(IDC_BTN_MODIFY2, BN_CLICKED, OnClickedBtnModifyReplacement)
	COMMAND_HANDLER(IDC_BTN_REMOVE2, BN_CLICKED, OnClickedBtnRemoveReplacement)
	COMMAND_HANDLER(IDC_BTN_DOWN, BN_CLICKED, OnClickedBtnDown)
	COMMAND_HANDLER(IDC_BTN_UP, BN_CLICKED, OnClickedBtnUp)
	NOTIFY_HANDLER(IDC_LIST_REPLACE_STRING, LVN_KEYDOWN, OnKeydownListReplacement)
	NOTIFY_HANDLER(IDC_LIST_REPLACE_STRING, NM_DBLCLK, OnDblclkListReplacement)
	NOTIFY_HANDLER(IDC_LIST_REPLACE_STRING, LVN_ITEMCHANGED, OnItemchangedListReplacement)
	COMMAND_HANDLER(IDC_BTN_SAVEFILE2, BN_CLICKED, OnClickedBtnSaveFile)
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
	LRESULT OnClickedBtnAddReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnLoadReplacementFromFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModifyReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemoveReplacement(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnKeydownListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnDblclkListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemchangedListReplacement(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

private:
	/////////////
	// Variables
	/////////////
	UCLID_AFVALUEMODIFIERSLib::IReplaceStringsPtr m_ipInternalReplacements;

	ATLControls::CListViewCtrl m_listReplacement;
	ATLControls::CButton m_btnUp;
	ATLControls::CButton m_btnDown;
	ATLControls::CButton m_btnRemove;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnSaveFile;

	CXInfoTip m_infoTip;

	// contains the file header string got from MMiscUtils
	std::string m_strFileHeader;

	///////////
	// Methods
	///////////
	// check whether the string passed in already exists in the To Be Replaced column
	// if does not exists, return -1, else return the index of the item
	int existsStringToBeReplaced(const std::string& strToBeReplaced);
	// get item text based on its item index and the column index
	std::string getItemText(int nIndex, int nColumnNum);
	// get vector of replacements from ReplaceStrings object
	// and populate the list view
	void loadReplacements();
	// return true if OK is clicked and valid entries are provided
	// nItemIndex - if zEnt1 already exists in the list, this
	//				is the index of the existing item in the list
	bool promptForReplacements(CString& zEnt1, CString& zEnt2, int& nItemIndex);
	// save list items to m_ipInternalReplacements
	// Return true if there is at least one replacement info
	bool saveReplacements();

	// update up and down arrow buttons
	void updateUpAndDownButtons();

	// update Modify and Delete buttons
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
