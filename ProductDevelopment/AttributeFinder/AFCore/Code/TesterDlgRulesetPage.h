#pragma once

#include "resource.h"

#include <string>
using namespace std;
// TesterDlgRulesetPage.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// TesterDlgRulesetPage dialog

class TesterDlgRulesetPage : public CPropertyPage
{
	DECLARE_DYNCREATE(TesterDlgRulesetPage)

// Construction
public:
	void setRulesFileName(string strFileName);
	string getRulesFileName();
	TesterDlgRulesetPage();
	~TesterDlgRulesetPage();

// Dialog Data
	//{{AFX_DATA(TesterDlgRulesetPage)
	enum { IDD = IDD_TESTERDLG_RULESET_PAGE };
	CEdit	m_editRulesFileName;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(TesterDlgRulesetPage)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// Generated message map functions
	//{{AFX_MSG(TesterDlgRulesetPage)
	afx_msg void OnBtnBrowseRsd();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

