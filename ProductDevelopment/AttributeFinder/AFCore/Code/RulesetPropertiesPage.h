#pragma once

// RuleSetPropertiesPage.h : header file
//
#include "resource.h"
#include "CounterInfo.h"

#include <string>
#include <map>

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

// Windows Message Handlers
	afx_msg void OnClickedBtnAddCounter();
	afx_msg void OnClickedBtnEditCounter();
	afx_msg void OnClickedBtnDeleteCounter();
	afx_msg void OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnBnClickedCheckSpecifyOcrParameters();
	afx_msg void OnBnClickedBtnOcrparameters();
	afx_msg void OnBnClickedBtnImportOcrParameters();

	DECLARE_MESSAGE_MAP()

private:

	// Hides the checkboxes that require an RDT license
	void hideCheckboxes();

	void setupCounterList();

	// Displays dialog to allow creation/editing of a custom counter.
	// Pass -1 as the list index to add a new counter or the ID of an existing counter to edit.
	void addEditCounter(int nListIndex);

	// Checks whether the specified counter is available in m_CounterList, and if so sets
	// rbIsCounterChecked accordingly.
	bool isCounterAvailable(int nCounterId, bool &rbIsCounterChecked);

	// Gets the CounterInfo instance associated with the specified row index in m_CounterList.
	CounterInfo& getCounterFromList(long nIndex);

	// Returns true if the specified name has not already been used; false if it has not.
	bool isCounterNameUsed(const char* szName);

	// Gets the index of the currently selected row in m_CounterList or -1 if no row is currently
	// selected.
	int getSelectedItem();
	
	// Updates the width of the counter name column to use the remaining space in the grid.
	// (Ensures proper sizing after scroll bar as appeared/disappeared.)
	void updateGridWidth();

	// Returns true if the Rule Development Toolkit is licensed; false otherwise.
	bool isRdtLicensed();

	const std::string chooseFile();

	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	// A map counter ID to CounterInfo for all counters that appear in the grid (standard and
	// custom).
	map<long, CounterInfo> m_mapCounters;

	CButton m_btnAddCounter;
	CButton m_btnEditCounter;
	CButton m_btnDeleteCounter;
	CButton m_checkSwipingRule;
	CListCtrl	m_CounterList;
	CButton	m_checkboxForInternalUseOnly;
	CEdit m_editFKBVersion;
	CButton m_checkSpecifiedOCRParameters;
	CButton m_btnEditOCRParameters;
	CButton m_btnImportOCRParameters;

	bool m_bReadOnly;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
