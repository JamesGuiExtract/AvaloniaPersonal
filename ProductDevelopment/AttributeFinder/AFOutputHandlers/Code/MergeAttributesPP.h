// MergeAttributesPP.h : Declaration of the CMergeAttributesPP

#pragma once
#include "resource.h"
#include "AFOutputHandlers.h"

#include <XInfoTip.h>
#include <ImageButtonWithStyle.h>

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CMergeAttributesPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CMergeAttributesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMergeAttributesPP, &CLSID_MergeAttributesPP>,
	public IPropertyPageImpl<CMergeAttributesPP>,
	public CDialogImpl<CMergeAttributesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMergeAttributesPP();
	~CMergeAttributesPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_MERGEATTRIBUTESPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_MERGEATTRIBUTESPP)

	BEGIN_COM_MAP(CMergeAttributesPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CMergeAttributesPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CMergeAttributesPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_RADIO_SPECIFY_NAME, BN_CLICKED, OnBnClickedRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_PRESERVE_NAME, BN_CLICKED, OnBnClickedRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_SPECIFY_TYPE, BN_CLICKED, OnBnClickedRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_COMBINE_TYPES, BN_CLICKED, OnBnClickedRadioBtn)
		COMMAND_HANDLER(IDC_BTN_ADD_NAME, BN_CLICKED, OnBnClickedBtnAdd)
		COMMAND_HANDLER(IDC_BTN_REMOVE_NAME, BN_CLICKED, OnBnClickedBtnRemove)
		COMMAND_HANDLER(IDC_BTN_MODIFY_NAME, BN_CLICKED, OnBnClickedBtnModify)
		COMMAND_HANDLER(IDC_BTN_NAME_UP, BN_CLICKED, OnBnClickedBtnUp)
		COMMAND_HANDLER(IDC_BTN_NAME_DOWN, BN_CLICKED, OnBnClickedBtnDown)
		NOTIFY_HANDLER(IDC_LIST_NAMES, LVN_ITEMCHANGED, OnItemChangedList)
		NOTIFY_HANDLER(IDC_LIST_NAMES, NM_DBLCLK, OnDblclkList)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedRadioBtn(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnItemChangedList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);
	LRESULT OnDblclkList(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

private:

	/////////////
	// Variables
	/////////////

	// Controls
	ATLControls::CEdit m_editAttributeQuery;
	ATLControls::CEdit m_editOverlapPercent;
	ATLControls::CButton m_btnSpecifyName;
	ATLControls::CButton m_btnPreserveName;
	ATLControls::CButton m_btnSpecifyType;
	ATLControls::CButton m_btnCombineType;
	ATLControls::CButton m_btnPreserveType;
	ATLControls::CEdit m_editSpecifiedName;
	ATLControls::CEdit m_editSpecifiedType;
	ATLControls::CEdit m_editSpecifiedValue;
	ATLControls::CListViewCtrl m_listNameMergePriority;
	ATLControls::CButton m_btnAdd;
	ATLControls::CButton m_btnModify;
	ATLControls::CButton m_btnRemove;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	ATLControls::CButton m_btnPreserveAsSubAttributes;
	ATLControls::CButton m_btnCreateMergedRegion;
	ATLControls::CButton m_btnMergeIndividualZones;

	/////////////
	// Methods
	/////////////

	// Populates the specified list with the specified vector of values
	void initializeList(ATLControls::CListViewCtrl &listControl, IVariantVectorPtr ipEntries);

	// Returns a vector of entries in the specified list
	IVariantVectorPtr retrieveListValues(ATLControls::CListViewCtrl &listControl);

	// Update the state of the buttons based on the items in the list
	void updateButtons();

	// Enables/disables controls based upon which radio button options are checked.
	void updateControls();

	// validate license
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributesPP), CMergeAttributesPP)
