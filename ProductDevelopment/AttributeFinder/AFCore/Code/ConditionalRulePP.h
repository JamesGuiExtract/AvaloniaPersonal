// ConditionalRulePP.h : Declaration of the CConditionalRulePP

#ifndef __CONDITIONALRULEPP_H_
#define __CONDITIONALRULEPP_H_

#include "resource.h"       // main symbols
#include <string>
#include "AFCategories.h"

EXTERN_C const CLSID CLSID_ConditionalRulePP;

/////////////////////////////////////////////////////////////////////////////
// CConditionalRulePP
class ATL_NO_VTABLE CConditionalRulePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CConditionalRulePP, &CLSID_ConditionalRulePP>,
	public IPropertyPageImpl<CConditionalRulePP>,
	public CDialogImpl<CConditionalRulePP>
{
public:
	CConditionalRulePP();

	enum {IDD = IDD_CONDITIONALRULEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_CONDITIONALRULEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CConditionalRulePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CConditionalRulePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CConditionalRulePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_CONFIG_CONDITION, BN_CLICKED, OnClickedBtnConfigCondition)
	COMMAND_HANDLER(IDC_BTN_CONFIG_RULE, BN_CLICKED, OnClickedBtnConfigRule)
	COMMAND_HANDLER(IDC_CMB_CONDITION, CBN_SELCHANGE, OnSelChangeCmbCondition)
	COMMAND_HANDLER(IDC_CMB_RULE, CBN_SELCHANGE, OnSelChangeCmbRule)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnConfigCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnConfigRule(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeCmbCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnSelChangeCmbRule(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////////
	// Methods
	///////////////
	IStrToStrMapPtr populateCombo(ATLControls::CComboBox& m_cmbObjects, const std::string& strCategoryName);
	ICategorizedComponentPtr createSelectedObject(ATLControls::CComboBox& m_cmbObjects, IStrToStrMapPtr ipMap);
	ICategorizedComponentPtr createObjectFromName(std::string strName, IStrToStrMapPtr ipMap);
	// This method will display(or hide) to the ui a reminder to configure the components 
	// that still need to be configured
	void showReminder();
	// This method will enable or disablt the config buttons based on whether the current objects
	// are configurable
	void toggleConfigButtons();
	void configureComponent(ICategorizedComponentPtr& ipObject);
	bool objectMustBeConfigured(IUnknownPtr ipObject);
	///////////////
	// Variables
	///////////////
	ATLControls::CComboBox m_cmbTrueFalse;
	ATLControls::CComboBox m_cmbCondition;
	ATLControls::CComboBox m_cmbRule;

	ATLControls::CButton m_btnConfigCondition;
	ATLControls::CButton m_btnConfigRule;

	UCLID_AFCORELib::IAFConditionPtr m_ipCondition;
	ICategorizedComponentPtr m_ipRule;


	// Used to get the object map
	ICategoryManagerPtr m_ipCategoryMgr;

	// Stores association between registered Object names and ProgIDs
	IStrToStrMapPtr	m_ipConditionMap;
	IStrToStrMapPtr	m_ipRuleMap;
};

#endif //__CONDITIONALRULEPP_H_
