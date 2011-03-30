// FindingRuleConditionPP.h : Declaration of the CFindingRuleConditionPP


#pragma once
#include "resource.h"       // main symbols
#include "AFConditions.h"

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CFindingRuleConditionPP
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CFindingRuleConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFindingRuleConditionPP, &CLSID_FindingRuleConditionPP>,
	public IPropertyPageImpl<CFindingRuleConditionPP>,
	public CDialogImpl<CFindingRuleConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFindingRuleConditionPP();
	~CFindingRuleConditionPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_FINDINGRULECONDITIONPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_FINDINGRULECONDITIONPP)

	BEGIN_COM_MAP(CFindingRuleConditionPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CFindingRuleConditionPP)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_COMBO_OBJ, CBN_SELCHANGE, OnSelChangeCombo)
		COMMAND_HANDLER(IDC_BTN_CONFIGURE, BN_CLICKED, OnConfigure)
		CHAIN_MSG_MAP(IPropertyPageImpl<CFindingRuleConditionPP>)
	END_MSG_MAP()

// Windows message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnSelChangeCombo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnConfigure(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	///////////
	// Variables
	////////////

	ATLControls::CComboBox m_cmbRules;
	ATLControls::CStatic m_txtMustBeConfigured;
	ATLControls::CButton m_btnConfig;

	ICategoryManagerPtr m_ipCategoryManager;

	// Stores map of registered attribute finding rule names to their associated ProgIDs
	IStrToStrMapPtr	m_ipRuleMap;

	///////////
	// Methods
	///////////

	ICategoryManagerPtr getCategoryManager();

	// Retrieves association between registered Object names and ProgIDs
	IStrToStrMapPtr	getRuleMap();

	// PROMISE: Retrieve an instantiation of the currently selected AttributeFindingRule
	// ARGS:	For efficiency's sake, you can pass in an existing instance of the 
	//			FindingRuleObject.  If __nullptr, getSelectedAFRule will obtain an instantiation
	//			itself.
	IAttributeFindingRulePtr getSelectedAFRule(
		UCLID_AFCONDITIONSLib::IFindingRuleConditionPtr ipFindingRuleCondition = __nullptr);

	// PROMISE: Updates the configuration requirement message and configuration button enabled
	//			status based on the currently selected rule.  
	// RETURNS: true if configuration is required at this time and false otherwise.
	bool updateRequiresConfig(IAttributeFindingRulePtr ipRule);

	void validateLicense();
};


OBJECT_ENTRY_AUTO(__uuidof(FindingRuleConditionPP), CFindingRuleConditionPP)
