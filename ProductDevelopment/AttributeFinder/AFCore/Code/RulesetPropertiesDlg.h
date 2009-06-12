#pragma once

// SelectCountersDlg.h : header file
//
#include "resource.h"
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CRuleSetPropertiesDlg dialog

class CRuleSetPropertiesDlg : public CDialog
{
// Construction
public:
	// PROMISE: The properties of ipRuleSet will be modified as specified by the
	//			user ONLY if the user clicks the OK button to dismiss the dialog.
	CRuleSetPropertiesDlg(UCLID_AFCORELib::IRuleSetPtr ipRuleSet, CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CRuleSetPropertiesDlg)
	enum { IDD = IDD_RULESET_PROPERTIES_DLG };
	CListCtrl	m_CounterList;
	CButton	m_checkboxForInternalUseOnly;
	CEdit m_editKeySerialNumbers;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRuleSetPropertiesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation

protected:
	// Generated message map functions
	//{{AFX_MSG(CRuleSetPropertiesDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnNMClickCounterList(NMHDR *pNMHDR, LRESULT *pResult);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	void setupCounterList();
	
	// throws and exception if an invalid serial number is found ( non numeric )
	void validateSerialList( const std::string &strSerialList );

	UCLID_AFCORELib::IRuleSetPtr m_ipRuleSet;

	// Actual position in list of USB counter items
	int m_iIndexingCounterItem;
	int m_iPaginationCounterItem;
	int m_iRedactPagesCounterItem;
	int m_iRedactDocsCounterItem;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

