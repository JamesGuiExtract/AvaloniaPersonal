#pragma once

// SelectCountersDlg.h : header file
//
#include "resource.h"
#include "afxwin.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CRuleSetPropertiesDlg dialog

class CRuleSetPropertiesDlg : public CDialog
{
// Construction
public:
	// PROMISE: The properties of ipRuleSet will be modified as specified by the
	//			user ONLY if the user clicks the OK button to dismiss the dialog.
	CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, CWnd* pParent = __nullptr);   // standard constructor

// Dialog Data
	enum { IDD = IDD_RULESET_PROPERTIES_DLG };

// Overrides
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	virtual BOOL OnInitDialog();
	virtual void OnOK();

	afx_msg void OnCounterListItemChanged(NMHDR* pNMHDR, LRESULT* pResult);

	DECLARE_MESSAGE_MAP()

private:

	// Hides the checkboxes that require an RDT license
	void hideCheckboxes();

	void setupCounterList();
	
	// throws and exception if an invalid serial number is found ( non numeric )
	void validateSerialList(const string &strSerialList);

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
	CButton m_buttonOk;
	CButton m_buttonCancel;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

