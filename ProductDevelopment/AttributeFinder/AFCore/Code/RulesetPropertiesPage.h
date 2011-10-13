#pragma once

// RuleSetPropertiesPage.h : header file
//
#include "resource.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRuleSetPropertiesPage dialog
/////////////////////////////////////////////////////////////////////////////
class CRuleSetPropertiesPage : public CPropertyPage
{
// Construction
public:
	// PROMISE: The properties of ipRuleSet will be modified as specified by the
	//			user ONLY if the user clicks the OK button to dismiss the dialog.
	CRuleSetPropertiesPage(UCLID_AFCORELib::IRuleSetPtr ipRuleSet,
		bool bReadOnly);
	~CRuleSetPropertiesPage();

	// Applies the properties in the property page to the ruleset.
	void Apply();

// Dialog Data
	enum { IDD = IDD_RULESET_PROPERTIES_PAGE };

// Overrides
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	virtual BOOL OnInitDialog();

	afx_msg void OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:

	// Hides the checkboxes that require an RDT license
	void hideCheckboxes();

	void setupCounterList();
	
	// throws and exception if an invalid serial number is found ( non numeric )
	void validateSerialList(const string &strSerialList);

	// Checks whether the specified check box counter item is available, and if so sets
	// rbIsCounterChecked accordingly.
	bool isCounterAvailable(int nCounterItem, bool &rbIsCounterChecked);

	// Returns true if the Rule Development Toolkit is licensed; false otherwise.
	bool isRdtLicensed();

	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	// Actual position in list of USB counter items
	int m_iIndexingCounterItem;
	int m_iPaginationCounterItem;
	int m_iRedactPagesCounterItem;
	int m_iRedactDocsCounterItem;

	CButton m_checkSwipingRule;
	CListCtrl	m_CounterList;
	CButton	m_checkboxForInternalUseOnly;
	CEdit m_editKeySerialNumbers;
	CEdit m_editFKBVersion;

	bool m_bReadOnly;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

