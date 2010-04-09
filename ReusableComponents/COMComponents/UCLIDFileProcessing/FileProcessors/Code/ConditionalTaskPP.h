// ConditionalTaskPP.h : Declaration of the CConditionalTaskPP


#pragma once
#include "resource.h"       // main symbols
#include "FileProcessors.h"
#include <ImageButtonWithStyle.h>

EXTERN_C const CLSID CLSID_ConditionalTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CConditionalTaskPP
class ATL_NO_VTABLE CConditionalTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConditionalTaskPP, &CLSID_ConditionalTaskPP>,
	public IPropertyPageImpl<CConditionalTaskPP>,
	public CDialogImpl<CConditionalTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CConditionalTaskPP();
	~CConditionalTaskPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_CONDITIONALTASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_CONDITIONALTASKPP)

	BEGIN_COM_MAP(CConditionalTaskPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CConditionalTaskPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CConditionalTaskPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_BTN_SELECT_CONDITION, BN_CLICKED, OnBtnSelectCondition)
		MESSAGE_HANDLER(WM_LBUTTONDBLCLK, OnLButtonDblClk) 
		COMMAND_HANDLER(IDC_BTN_ADD, BN_CLICKED, OnClickedBtnAdd)
		COMMAND_HANDLER(IDC_BTN_ADD2, BN_CLICKED, OnClickedBtnAdd)
		COMMAND_HANDLER(IDC_BTN_REMOVE, BN_CLICKED, OnClickedBtnRemove)
		COMMAND_HANDLER(IDC_BTN_REMOVE2, BN_CLICKED, OnClickedBtnRemove)
		COMMAND_HANDLER(IDC_BTN_MODIFY, BN_CLICKED, OnClickedBtnModify)
		COMMAND_HANDLER(IDC_BTN_MODIFY2, BN_CLICKED, OnClickedBtnModify)
		COMMAND_HANDLER(IDC_BTN_UP, BN_CLICKED, OnClickedBtnUp)
		COMMAND_HANDLER(IDC_BTN_UP2, BN_CLICKED, OnClickedBtnUp)
		COMMAND_HANDLER(IDC_BTN_DOWN, BN_CLICKED, OnClickedBtnDown)
		COMMAND_HANDLER(IDC_BTN_DOWN2, BN_CLICKED, OnClickedBtnDown)
		NOTIFY_HANDLER(IDC_LIST_TRUE, LVN_ITEMCHANGED, OnItemChangedList)
		NOTIFY_HANDLER(IDC_LIST_FALSE, LVN_ITEMCHANGED, OnItemChangedList)
		NOTIFY_HANDLER(IDC_LIST_TRUE, NM_DBLCLK, OnDblclkList)
		NOTIFY_HANDLER(IDC_LIST_FALSE, NM_DBLCLK, OnDblclkList)
		NOTIFY_HANDLER(IDC_LIST_TRUE, NM_RCLICK, OnRClickList)
		NOTIFY_HANDLER(IDC_LIST_FALSE, NM_RCLICK, OnRClickList)
		COMMAND_HANDLER(IDC_EDIT_CUT, BN_CLICKED, OnEditCut)
		COMMAND_HANDLER(IDC_EDIT_COPY, BN_CLICKED, OnEditCopy)
		COMMAND_HANDLER(IDC_EDIT_PASTE, BN_CLICKED, OnEditPaste)
		COMMAND_HANDLER(IDC_EDIT_DELETE, BN_CLICKED, OnEditDelete)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// Message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBtnSelectCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemChangedList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnDblclkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnRClickList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnEditCut(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditPaste(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEditDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

	// added as per P16 #2583 and changed per FlexIDSCore #4257
	LRESULT OnLButtonDblClk(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// Clears selection state for specified list
	void	clearListSelection(ATLControls::CListViewCtrl &rList);

	// Gets position and dimensions of the dialog item with the specified resource ID
	void	getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow);

	// Returns index of position in rList at which to insert a new task
	int		getInsertPosition(ATLControls::CListViewCtrl &rList);

	// Validates wID against nID1 or nID2.  Updates rList to be the list control 
	// associated with wID.  Updates ipCollection to be the collection of tasks 
	// associated with wID.
	// REQUIRES: wID == nID1 OR wID == nID2
	void	getListAndTasks(WORD wID, int nID1, int nID2, ATLControls::CListViewCtrl& rList, 
		IIUnknownVectorPtr &ripCollection);

	// Creates the MiscUtils object if necessary and returns it
	IMiscUtilsPtr getMiscUtils();

	// Provides the appropriate collection from underlying file processor object
	IIUnknownVectorPtr getFalseTasks();
	IIUnknownVectorPtr getTrueTasks();

	// Prepares controls and associated data members
	void	prepareControls();

	// Updates the specified list from the specified collection.  Selects the 
	// specified task if iTaskForSelection > -1.
	void	refreshTasks(ATLControls::CListViewCtrl &rList, IIUnknownVectorPtr ipCollection, 
		int iTaskForSelection = -1);

	// Based on which list was provided, calls updateButtons() with the appropriate 
	// Add, Remove, Modify, Up and Down buttons
	void	setButtonStates(ATLControls::CListViewCtrl &rList);

	// Enables / disables the specified buttons based on selection state of the specified list
	void	updateButtons(ATLControls::CListViewCtrl &rList, ATLControls::CButton &rbtnAdd, 
		ATLControls::CButton &rbtnRemove, ATLControls::CButton &rbtnModify, 
		CImageButtonWithStyle &rbtnUp, CImageButtonWithStyle &rbtnDown);

	// Ensures that this component is licensed
	void	validateLicense();

	// Ensures that object settings are valid for save
	// Requires:
	// - a defined FAM Condition AND
	//    - at least one Enabled task in True list OR
	//    - at least one Enabled task in False list
	void	validateSettings();

	// Removes the selected items from the specified list and related collection
	// Requires:
	//		- at least one item is selected in the specified list
	//		- ipCollection is an IUnknownVector of objects that underlies the specified list
	void removeSelectedItems(ATLControls::CListViewCtrl &rList, IIUnknownVectorPtr ipCollection);

	///////
	// Data
	///////
	// Various controls
	ATLControls::CEdit m_editConditionDescription;
	ATLControls::CButton m_btnSelectCondition;
	ATLControls::CListViewCtrl m_listTrueTasks;
	ATLControls::CListViewCtrl m_listFalseTasks;

	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnRemove;
	ATLControls::CButton m_btnModify;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	
	ATLControls::CButton m_btnAdd2;
	ATLControls::CButton m_btnRemove2;
	ATLControls::CButton m_btnModify2;
	CImageButtonWithStyle m_btnUp2;
	CImageButtonWithStyle m_btnDown2;

	IMiscUtilsPtr m_ipMiscUtils;

	// Clipboard manager for handling cut/copy/paste of conditional tasks
	UCLID_COMUTILSLib::IClipboardObjectManagerPtr m_ipClipboardMgr;

	// Sets the id of the last clicked resource (used for context menu items)
	int m_iLastClickedResourceID;

	// Underlying file processor object to hold the settings
	UCLID_FILEPROCESSORSLib::IConditionalTaskPtr m_ipConditionalTaskFP;
};

OBJECT_ENTRY_AUTO(__uuidof(ConditionalTaskPP), CConditionalTaskPP)