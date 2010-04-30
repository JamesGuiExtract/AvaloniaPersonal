// BoxFinderPP.h : Declaration of the CBoxFinderPP


#pragma once
#include "resource.h"       // main symbols
#include "AFValueFinders.h"

#include <XInfoTip.h>
#include <ImageButtonWithStyle.h>

#include <string>
using namespace std;

////////////////////////////////////////
// CBoxFinderPP
////////////////////////////////////////
class ATL_NO_VTABLE CBoxFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CBoxFinderPP, &CLSID_BoxFinderPP>,
	public IPropertyPageImpl<CBoxFinderPP>,
	public CDialogImpl<CBoxFinderPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CBoxFinderPP();
	~CBoxFinderPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_BOXFINDERPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_BOXFINDERPP)

	BEGIN_COM_MAP(CBoxFinderPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CBoxFinderPP)
		MESSAGE_HANDLER(WM_LBUTTONUP, OnLButtonUp)
		COMMAND_HANDLER(IDC_RADIO_RETURN_BOX_AREA, BN_CLICKED, OnBnClickedFindType)
		COMMAND_HANDLER(IDC_RADIO_RETURN_TEXT, BN_CLICKED, OnBnClickedFindType)
		COMMAND_HANDLER(IDC_RADIO_ALL_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
		COMMAND_HANDLER(IDC_RADIO_FIRST_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
		COMMAND_HANDLER(IDC_RADIO_LAST_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
		COMMAND_HANDLER(IDC_RADIO_SPECIFIED_PAGES, BN_CLICKED, OnBnClickedSelectedPages)
		COMMAND_HANDLER(IDC_BTN_ADD_CLUE, BN_CLICKED, OnBnClickedBtnAddClue)
		COMMAND_HANDLER(IDC_BTN_REMOVE_CLUE, BN_CLICKED, OnBnClickedBtnRemoveClue)
		COMMAND_HANDLER(IDC_BTN_MODIFY_CLUE, BN_CLICKED, OnBnClickedBtnModifyClue)
		COMMAND_HANDLER(IDC_BTN_CLUE_UP, BN_CLICKED, OnBnClickedBtnUp)
		COMMAND_HANDLER(IDC_BTN_CLUE_DOWN, BN_CLICKED, OnBnClickedBtnDown)
		COMMAND_HANDLER(IDC_CLUE_DYNAMIC_LIST_HELP, STN_CLICKED, OnClickedClueDynamicListInfo)
		NOTIFY_HANDLER(IDC_LIST_CLUES, LVN_ITEMCHANGED, OnItemChangedListClues)
		NOTIFY_HANDLER(IDC_LIST_CLUES, NM_DBLCLK, OnDblclkListClues)
		CHAIN_MSG_MAP(IPropertyPageImpl<CBoxFinderPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnLButtonUp(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedFindType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedSelectedPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnAddClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnRemoveClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnModifyClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemChangedListClues(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);
	LRESULT OnDblclkListClues(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////

	CXInfoTip m_infoTip;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnRemove;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	ATLControls::CListViewCtrl m_listClues;
	ATLControls::CButton m_btnCluesAreRegularExpressions;
	ATLControls::CButton m_btnCluesAreCaseSensitive;
	ATLControls::CButton m_btnFirstBoxOnly;
	ATLControls::CStatic m_pictClueDiagram;
	CRect m_rectClueBitmap;
	ATLControls::CButton m_radioAllPages;
	ATLControls::CButton m_radioFirstPages;
	ATLControls::CButton m_radioLastPages;
	ATLControls::CButton m_radioSpecifiedPages;
	ATLControls::CEdit m_editFirstPageNums;
	ATLControls::CEdit m_editLastPageNums;
	ATLControls::CEdit m_editSpecifiedPageNums;
	ATLControls::CEdit m_editBoxWidthMin;
	ATLControls::CEdit m_editBoxWidthMax;
	ATLControls::CEdit m_editBoxHeightMin;
	ATLControls::CEdit m_editBoxHeightMax;
	ATLControls::CButton m_radioFindSpatialArea;
	ATLControls::CButton m_radioFindText;
	ATLControls::CEdit m_editAttributeText;
	ATLControls::CButton m_chkExcludeClueArea;
	ATLControls::CButton m_chkIncludeClueText;
	ATLControls::CButton m_chkIncludeLines;

	///////////
	// Methods
	///////////

	// Loads saved clues to the page
	void initializeClueList(IVariantVectorPtr ipClues);

	// Retrieve the clues from the screen and validate that one or more exist
	IVariantVectorPtr retrieveAndValidateClues();

	// Given the mouse location, determine which clue location was selected.
	EClueLocation getSelectedClueLocation(CPoint &ptMousePos);

	// Display the specified clue location to the page
	void displayClueLocation(EClueLocation eClueLocation);

	// Enables/checks/unchecks appropriate controls given the selection of the specified
	// pages selection radion button
	void onSelectPages(WORD wRadioId);

	// Enables/checks/unchecks appropriate controls given the selection of the specified
	// pages find type
	void onSelectFindType(WORD wRadioId);

	// Update the state of the clue buttons based on the items in the clue list
	void updateClueButtons();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(BoxFinderPP), CBoxFinderPP)