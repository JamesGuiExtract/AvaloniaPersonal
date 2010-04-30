// TranslateValuePP.h : Declaration of the CTranslateValuePP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

#include <string>
using namespace std;

EXTERN_C const CLSID CLSID_TranslateValuePP;

/////////////////////////////////////////////////////////////////////////////
// CTranslateValuePP
class ATL_NO_VTABLE CTranslateValuePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTranslateValuePP, &CLSID_TranslateValuePP>,
	public IPropertyPageImpl<CTranslateValuePP>,
	public CDialogImpl<CTranslateValuePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTranslateValuePP();

	enum {IDD = IDD_TRANSLATEVALUEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_TRANSLATEVALUEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTranslateValuePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CTranslateValuePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CTranslateValuePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD, BN_CLICKED, OnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_LOAD_FROM_FILE, BN_CLICKED, OnClickedBtnLoadFromFile)
	COMMAND_HANDLER(IDC_BTN_MODIFY, BN_CLICKED, OnClickedBtnModify)
	COMMAND_HANDLER(IDC_BTN_REMOVE, BN_CLICKED, OnClickedBtnRemove)
	COMMAND_HANDLER(IDC_RADIO_TRANSLATE_VALUE, BN_CLICKED, OnClickedRadioTranslateValue)
	COMMAND_HANDLER(IDC_RADIO_TRANSLATE_TYPE, BN_CLICKED, OnClickedRadioTranslateType)
	COMMAND_HANDLER(IDC_CHK_CASE1, BN_CLICKED, OnClickedChkCaseSensitive)
	NOTIFY_HANDLER(IDC_LIST_TRANS_VALUE, NM_DBLCLK, OnDblclkListTransValue)
	NOTIFY_HANDLER(IDC_LIST_TRANS_VALUE, LVN_KEYDOWN, OnKeydownListTransValue)
	NOTIFY_HANDLER(IDC_LIST_TRANS_VALUE, LVN_ITEMCHANGED, OnListItemChanged)
	COMMAND_HANDLER(IDC_BTN_SAVE_TO_FILE, BN_CLICKED, OnClickedBtnSaveFile)
	COMMAND_HANDLER(IDC_CLUE_DYNAMIC_LIST_HELP, STN_CLICKED, OnClickedClueDynamicListInfo)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnLoadFromFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioTranslateValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioTranslateType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListTransValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeydownListTransValue(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnListItemChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedBtnSaveFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	/////////////////
	// Variables
	/////////////////
	// a temp translate value object, which will be used internally. 
	// It will not affect the translate value object that's associated with
	// this proprety page unless Apply is called
	UCLID_AFVALUEMODIFIERSLib::ITranslateValuePtr m_ipInternalTransValue;

	ATLControls::CButton m_radioTranslateValue;
	ATLControls::CButton m_radioTranslateType;

	CXInfoTip m_infoTip;

	// Contains the file header string got from MiscUtils
	string m_strFileHeader;

	////////////////
	// Methods
	///////////////
	// check whether the string passed in already exists in the translate from column
	// if does not exists, return -1, else return the index of the item
	int existsTranslateFromString(const string& strTranslateFrom);
	// store all items in the list into a map
	bool saveAllTranslationPairsFromList();
	// get item text based on its item index and the column index
	string getItemText(int nIndex, int nColumnNum);
	// get vector of translations from TranslateValue object
	// and populate the list view
	void loadTranslations();
	// return true if OK is clicked and valid entries are provided
	// nItemIndex - if zEnt1 already exists in the list, this
	//				is the index of the existing item in the list
	bool promptForTranslations(CString& zEnt1, CString& zEnt2, int& nItemIndex);
	
	// update Modify and Delete buttons
	void updateButtons();

	// ensure that this component is licensed
	void validateLicense();
};
