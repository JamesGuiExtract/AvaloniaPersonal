// LocateImageRegionPP.h : Declaration of the CLocateImageRegionPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>
#include <string>
#include <vector>
#include <map>

using namespace std;

EXTERN_C const CLSID CLSID_LocateImageRegionPP;

/////////////////////////////////////////////////////////////////////////////
// CLocateImageRegionPP
class ATL_NO_VTABLE CLocateImageRegionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLocateImageRegionPP, &CLSID_LocateImageRegionPP>,
	public IPropertyPageImpl<CLocateImageRegionPP>,
	public CDialogImpl<CLocateImageRegionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLocateImageRegionPP();

	enum {IDD = IDD_LOCATEIMAGEREGIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LOCATEIMAGEREGIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLocateImageRegionPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CLocateImageRegionPP)
	COMMAND_HANDLER(IDC_BTN_LOAD_LIST, BN_CLICKED, OnBnClickedBtnLoadList)
	COMMAND_HANDLER(IDC_BTN_SAVE_LIST, BN_CLICKED, OnBnClickedBtnSaveList)
	COMMAND_HANDLER(IDC_CLUE_DYNAMIC_LIST_HELP, STN_CLICKED, OnClickedClueDynamicListInfo)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLocateImageRegionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_OPT_LIST1, BN_CLICKED, OnClickedRadioList1)
	COMMAND_HANDLER(IDC_OPT_LIST2, BN_CLICKED, OnClickedRadioList2)
	COMMAND_HANDLER(IDC_OPT_LIST3, BN_CLICKED, OnClickedRadioList3)
	COMMAND_HANDLER(IDC_OPT_LIST4, BN_CLICKED, OnClickedRadioList4)
	COMMAND_HANDLER(IDC_BTN_ADD_LR, BN_CLICKED, OnClickedBtnAdd)
	COMMAND_HANDLER(IDC_BTN_REMOVE_LR, BN_CLICKED, OnClickedBtnRemove)
	COMMAND_HANDLER(IDC_BTN_MODIFY_LR, BN_CLICKED, OnClickedBtnModify)
	COMMAND_HANDLER(IDC_BTN_UP_LR, BN_CLICKED, OnClickedBtnUp)
	COMMAND_HANDLER(IDC_BTN_DOWN_LR, BN_CLICKED, OnClickedBtnDown)
	COMMAND_HANDLER(IDC_CHK_CASE_SENSITIVE_LR, BN_CLICKED, OnClickedChkCaseSensitive)
	COMMAND_HANDLER(IDC_CHK_AS_REGEXPR_LR, BN_CLICKED, OnClickedChkAsRegExpr)
	COMMAND_HANDLER(IDC_CHK_RESTRICT, BN_CLICKED, OnClickedChkRestrict)
	COMMAND_HANDLER(IDC_CMB_FIND_TYPE, CBN_SELCHANGE, OnCbnSelchangeCmbFindType)
	COMMAND_HANDLER(IDC_CMB_INSIDE, CBN_SELCHANGE, OnSelChangeInsideOutside)
	COMMAND_HANDLER(IDC_CMB_CONDITION1, CBN_SELCHANGE, OnSelChangeCondition);
	COMMAND_HANDLER(IDC_CMB_CONDITION2, CBN_SELCHANGE, OnSelChangeCondition);
	COMMAND_HANDLER(IDC_CMB_CONDITION3, CBN_SELCHANGE, OnSelChangeCondition);
	COMMAND_HANDLER(IDC_CMB_CONDITION4, CBN_SELCHANGE, OnSelChangeCondition);
	NOTIFY_HANDLER(IDC_LIST_CLUES, LVN_ITEMCHANGED, OnItemchangedList)
	NOTIFY_HANDLER(IDC_LIST_CLUES, NM_DBLCLK, OnDblclkClueList)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnItemchangedList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnClickedRadioList1(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioList2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioList3(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioList4(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkAsRegExpr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedChkRestrict(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnDblclkClueList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);
	LRESULT OnCbnSelchangeCmbFindType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnLoadList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnSaveList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeInsideOutside(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	////////////
	// Variables
	////////////

	// struct to hold each list's detail
	struct ListInfo
	{
		ListInfo()
			:m_bCaseSensitive(false),
			 m_bAsRegExpr(false),
			 m_bDisableRestriction(true), 
			 m_bRestrictSearch(false),
			 m_zListNumbers("")
		{
		}

		std::vector<std::string> m_vecClues;
		bool m_bCaseSensitive;
		bool m_bAsRegExpr;
		// whether or not to disable current restriction selection item
		bool m_bDisableRestriction;
		// whether or not to restrict current clues to be found within
		// found higher priority lists' boundary
		bool m_bRestrictSearch;
		// depending on what are defined lists that have higher priority.
		// For instance if current list is 3, and there are list 1 and 2 
		// defined, then the text shall be "1, 2"
		CString m_zListNumbers; 
	};

	struct RegionBoundary
	{
		RegionBoundary()
			: m_eRegion(kNoBoundary),
			  m_eSide(kNoBoundary),
			  m_eCondition(kNoCondition),
			  m_eDirection(kNoDirection),
			  m_dExpand(0),
			  m_eUnits(kInches)
		{}

		EBoundary m_eRegion; 
		EBoundary m_eSide; 
		EBoundaryCondition m_eCondition;
		EExpandDirection m_eDirection;
		double m_dExpand;
		EUnits m_eUnits;
	};

	struct BoundaryControls
	{
		ATLControls::CComboBox m_cmbSide;
		ATLControls::CComboBox m_cmbCondition;
		ATLControls::CComboBox m_cmbExpandDirection;
		ATLControls::CEdit m_editExpandNumber;
		ATLControls::CComboBox m_cmbExpandUnits;
	};

	// map for each list detail.
	// Only those non-empty lists will be listed in the map
	// i.e. if the list has an entry in the map, the list must 
	// not be empty. Otherwise, it'll be removed for the map
	std::map<EClueListIndex, ListInfo> m_mapListNameToInfo;

	// what's currently selected clue list
	EClueListIndex m_eCurrentSelectedClueList;

	// whether or not current list has been modified
	bool m_bCurrentListChanged;

	// icons to indicate whether the list is populated or empty
	ATLControls::CStatic m_picList1;
	ATLControls::CStatic m_picList2;
	ATLControls::CStatic m_picList3;
	ATLControls::CStatic m_picList4;

	ATLControls::CComboBox m_cmbFindType;

	ATLControls::CEdit m_editImageRegionText;

	// general settings combo boxes
	ATLControls::CComboBox m_cmbInsideOutside;
	ATLControls::CComboBox m_cmbIncludeExclude;
	ATLControls::CComboBox m_cmbIntersectingEntities;
	ATLControls::CComboBox m_cmbMatchMultiplePagesPerDocument;

	BoundaryControls m_ctrlBoundary[4];

	ATLControls::CListViewCtrl m_listClues;

	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnUp;
	ATLControls::CButton m_btnDown;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnRemove;
	ATLControls::CButton m_btnSaveList;

	ATLControls::CButton m_chkCaseSensitive;
	ATLControls::CButton m_chkAsRegExpr;
	ATLControls::CButton m_chkRestrictSearch;

	ATLControls::CStatic m_txtClueListNumbers;
	ATLControls::CStatic m_txtIncludeExcludeRegionOn;

	// contains the file header string got from MiscUtils
	std::string m_strFileHeader;

	CXInfoTip m_infoTip;

	// Map of the readable text to be associates with each EUnits enum value.
	map<EUnits, CString> m_mapUnitValues;

	//////////
	// Methods
	//////////

	// true if the specified opposing regions are valid in relation to each other; false otherwise.
	bool areValidOpposingRegions(const RegionBoundary& boundary1, const RegionBoundary& boundary2);

	// change the icon indicate whether the clue list is populated or empty
	void changeIcon(EClueListIndex eListIndex, bool bIsListEmpty);

	// remove any empty clue list entry in the map
	void cleanupClueLists();

	// Gets the name of the specified boundary (i.e. "Left", "Top", "Right", "Bottom")
	string getBoundaryName(EBoundary eRegionBoundary);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the number of spatial lines to expand the specified boundary side 
	//          (Top or Bottom). Returns true if successful.
	//          If unsuccessful, will display appropriate error message and return false.
	// PARAMS:  eBoundary - Boundary side to expand
    //          rdSpatialLines - The number of spatial lines to expand.
	bool getSpatialLines(EBoundary eBoundary, double &rdSpatialLines);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the the specified region boundary to the specified values.
	// PARAMS:  [See description of ILocateImageRegion::SetRegionBoundaries]
	void initBoundaries(EBoundary eRegionBoundary, 
						EBoundary eSide,
						EBoundaryCondition eCondition,
						EExpandDirection eExpandDirection,
						double dExpandNumber,
						EUnits eUnits);

	// setup clue lists
	void initClueList(EClueListIndex eListIndex, IVariantVectorPtr ipClues, 
						bool bCaseSensitive, bool bAsRegExpr, bool bRestrictSearch);

	// select one of the clue lists
	void selectClueList(EClueListIndex eListIndex);

	// setup controls
	void setupControls();

	// store all defined clue lists
	bool storeClueLists(UCLID_AFVALUEFINDERSLib::ILocateImageRegionPtr ipLocateRegion);

	// store what ever is the current clue list setting
	void storeCurrentListSettings();

	// If the UI settings are valid, sets the boundary and returns true; otherwise returns false.
	bool tryGetRegionBoundary(EBoundary eBoundary, RegionBoundary& boundary);

	// update all clue lists' Restrict Search to the defined region
	void updateAllRestrictSearchValue();

	// update list related buttons, such as Add, Remove, Up, etc.
	void updateListButtons();

	// update Restrict Search related controls for current list
	void updateRestrictSearchControl(EClueListIndex eListIndex, ListInfo& listInfo);

	// Updates the contents of the units combo box associated with and depending the selection of
	// the specified border condition combo box.
	void updateUnitsCombo(WORD wConditionCtrlID);

	void validateLicense();
};
