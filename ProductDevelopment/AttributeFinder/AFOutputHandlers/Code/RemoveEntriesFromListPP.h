// RemoveEntriesFromListPP.h : Declaration of the CRemoveEntriesFromListPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_RemoveEntriesFromListPP;

/////////////////////////////////////////////////////////////////////////////
// CRemoveEntriesFromListPP
class ATL_NO_VTABLE CRemoveEntriesFromListPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRemoveEntriesFromListPP, &CLSID_RemoveEntriesFromListPP>,
	public IPropertyPageImpl<CRemoveEntriesFromListPP>,
	public CDialogImpl<CRemoveEntriesFromListPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRemoveEntriesFromListPP();
	~CRemoveEntriesFromListPP();

	enum {IDD = IDD_REMOVEENTRIESFROMLISTPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REMOVEENTRIESFROMLISTPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRemoveEntriesFromListPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRemoveEntriesFromListPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRemoveEntriesFromListPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_ADD, BN_CLICKED, OnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_LOAD, BN_CLICKED, OnClickedBtnLoad)
	COMMAND_HANDLER(IDC_BTN_MODIFY, BN_CLICKED, OnClickedBtnModify)
	COMMAND_HANDLER(IDC_BTN_REMOVE, BN_CLICKED, OnClickedBtnRemove)
	COMMAND_HANDLER(IDC_BTN_SAVE, BN_CLICKED, OnClickedBtnSave)
	NOTIFY_HANDLER(IDC_LIST_ENTRIES, LVN_ITEMCHANGED, OnItemListChanged)
	NOTIFY_HANDLER(IDC_LIST_ENTRIES, LVN_KEYDOWN, OnKeydownList)
	NOTIFY_HANDLER(IDC_LIST_ENTRIES, NM_DBLCLK, OnDblclkList)
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
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnLoad(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSave(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemListChanged(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnKeydownList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnDblclkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	////////////
	// Variables
	////////////
	UCLID_AFOUTPUTHANDLERSLib::IRemoveEntriesFromListPtr m_ipInternalObject;
	ATLControls::CListViewCtrl m_listEntries;
	CXInfoTip m_infoTip;

	//////////
	// Methods
	//////////
	// Load list into the list box
	void loadListValues();
	// Update Modify and Remove buttons' state
	void updateButtons();
	// Save current list to a file
	bool saveListValues();

	// ensure that this component is licensed
	void validateLicense();
};
