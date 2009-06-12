// MultipleObjSelectorPP.h : Declaration of the CMultipleObjSelectorPP

#pragma once

#include "resource.h"       // main symbols
#include "..\..\..\..\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#include <ImageButtonWithStyle.h>
#include <string>

EXTERN_C const CLSID CLSID_MultipleObjSelectorPP;

/////////////////////////////////////////////////////////////////////////////
// CMultipleObjSelectorPP
class ATL_NO_VTABLE CMultipleObjSelectorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMultipleObjSelectorPP, &CLSID_MultipleObjSelectorPP>,
	public IPropertyPageImpl<CMultipleObjSelectorPP>,
	public CDialogImpl<CMultipleObjSelectorPP>
{
public:
	CMultipleObjSelectorPP();

	enum {IDD = IDD_MULTIPLEOBJSELECTORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_MULTIPLEOBJSELECTORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMultipleObjSelectorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CMultipleObjSelectorPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CMultipleObjSelectorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BUTTON_DOWN, BN_CLICKED, OnClickedButtonDown)
	COMMAND_HANDLER(IDC_BUTTON_UP, BN_CLICKED, OnClickedButtonUp)
	COMMAND_HANDLER(IDC_BUTTON_CONFIG, BN_CLICKED, OnClickedButtonConfig)
	COMMAND_HANDLER(IDC_BUTTON_DELETE, BN_CLICKED, OnClickedButtonDelete)
	COMMAND_HANDLER(IDC_BUTTON_INSERT, BN_CLICKED, OnClickedButtonInsert)
	NOTIFY_HANDLER(IDC_LIST_OBJECTS, NM_DBLCLK, OnDblclkListObjects)
	NOTIFY_HANDLER(IDC_LIST_OBJECTS, LVN_ITEMCHANGED, OnItemChangedListObjects)
	NOTIFY_HANDLER(IDC_LIST_OBJECTS, NM_RCLICK, OnRClickListObjects)
	COMMAND_HANDLER(ID_EDIT_CUT, BN_CLICKED, OnEditCut)
	COMMAND_HANDLER(ID_EDIT_COPY, BN_CLICKED, OnEditCopy)
	COMMAND_HANDLER(ID_EDIT_PASTE, BN_CLICKED, OnEditPaste)
	COMMAND_HANDLER(ID_EDIT_DELETE, BN_CLICKED, OnEditDelete)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// Message handlers:
	STDMETHOD(Apply)(void);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedButtonDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonInsert(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedButtonConfig(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnItemChangedListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnRClickListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnEditCut(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditPaste(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);


private:
	////////////
	// Variables
	////////////
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	ATLControls::CButton m_btnInsert;
	ATLControls::CButton m_btnDelete;
	ATLControls::CButton m_btnConfig;
	ATLControls::CListViewCtrl m_listObjects;
	ATLControls::CStatic m_staticPrompt;

	// this vector will contain IObjectWithDescription objects
	UCLID_COMUTILSLib::IIUnknownVectorPtr m_ipObjects;
	
	// Cached copies of the category name and object type that were
	// retrieved from the multiple-object-holder
	std::string m_strCategoryName;
	std::string m_strObjectType;
	IID m_iid;
	int m_nRightClickIndex;

	UCLID_COMUTILSLib::IClipboardObjectManagerPtr m_ipClipboardMgr;

	// utility for handling list object double-click events
	UCLID_COMUTILSLib::IMiscUtilsPtr m_ipMiscUtils;

	///////////
	//Methods
	///////////
	// Disable or enable the buttons
	void updateButtonsStatus();

	void validateLicense();
};
