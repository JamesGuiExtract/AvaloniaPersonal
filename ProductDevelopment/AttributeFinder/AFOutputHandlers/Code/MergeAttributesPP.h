// MergeAttributesPP.h : Declaration of the CMergeAttributesPP

#pragma once
#include "resource.h"
#include "AFOutputHandlers.h"

#include <XInfoTip.h>

#include <string>
#include <vector>
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
		COMMAND_HANDLER(IDC_RADIO_SPECIFY_NAME, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_PRESERVE_NAME, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_SPECIFY_TYPE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_COMBINE_TYPES, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_SELECT_VALUE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_SPECIFY_VALUE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_PRESERVE_VALUE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_RADIO_SELECT_TYPE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_CHECK_TYPE_FROM_NAME, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_CHECK_PRESERVE_TYPE, BN_CLICKED, OnBnClickedCheckOrRadioBtn)
		COMMAND_HANDLER(IDC_BUTTON_EDIT_NAME_LIST, BN_CLICKED, OnBnClickedButtonEditNameList)
		COMMAND_HANDLER(IDC_BUTTON_EDIT_VALUE_LIST, BN_CLICKED, OnBnClickedButtonEditValueList)
		COMMAND_HANDLER(IDC_BUTTON_EDIT_TYPE_LIST, BN_CLICKED, OnBnClickedButtonEditTypeList)
	END_MSG_MAP()

	// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedCheckOrRadioBtn(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonEditNameList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonEditValueList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedButtonEditTypeList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

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
	ATLControls::CEdit m_editNameList;
	ATLControls::CButton m_btnEditNameList;
	ATLControls::CButton m_btnValueFromName;
	ATLControls::CButton m_btnSpecifyValue;
	ATLControls::CButton m_btnPreserveValue;
	ATLControls::CEdit m_editValueList;
	ATLControls::CButton m_btnEditValueList;
	ATLControls::CButton m_btnSpecifyType;
	ATLControls::CButton m_btnCombineType;
	ATLControls::CButton m_btnSelectType;
	ATLControls::CButton m_btnTypeFromName;
	ATLControls::CButton m_btnPreserveType;
	ATLControls::CEdit m_editTypeList;
	ATLControls::CButton m_btnEditTypeList;
	ATLControls::CEdit m_editSpecifiedName;
	ATLControls::CEdit m_editSpecifiedType;
	ATLControls::CEdit m_editSpecifiedValue;
	ATLControls::CButton m_btnPreserveAsSubAttributes;
	ATLControls::CButton m_btnCreateMergedRegion;
	ATLControls::CButton m_btnMergeIndividualZones;

	vector<string> m_vecNameMergePriority;
	bool m_bTreatNameListAsRegex;

	vector<string> m_vecValueMergePriority;
	bool m_bTreatValueListAsRegex;

	vector<string> m_vecTypeMergePriority;
	bool m_bTreatTypeListAsRegex;

	/////////////
	// Methods
	/////////////

	// Update the state of the buttons based on the items in the list
	void updateButtons();

	// Enables/disables controls based upon which radio button options are checked.
	void updateControls();

	// Updates editControl with the list values. If ipList is specified, it is the source of the
	// list values and they are applied to vecList as well. Otherwise, vecList is the souce of
	// the list values.
	void updateDelimetedList(vector<string>& vecList, ATLControls::CEdit& editControl,
		IVariantVectorPtr ipList = __nullptr);

	// validate license
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributesPP), CMergeAttributesPP)
