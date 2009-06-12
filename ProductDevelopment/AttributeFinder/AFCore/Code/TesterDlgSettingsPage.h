
#pragma once

#include "resource.h"

class TesterConfigMgr;

#include <string>

// TesterDlgSettingsPage.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// TesterDlgSettingsPage dialog

class TesterDlgSettingsPage : public CPropertyPage
{
	DECLARE_DYNCREATE(TesterDlgSettingsPage)

// Construction
public:
	TesterDlgSettingsPage();
	~TesterDlgSettingsPage();
	void setTesterConfigMgr(TesterConfigMgr *pTesterConfigMgr);
	void setCurrentAttributeName(const std::string& strAttributeName);
	const std::string& getCurrentAttributeName() const;
	bool isAllAttributesScopeSet() const;

// Dialog Data
	//{{AFX_DATA(TesterDlgSettingsPage)
	enum { IDD = IDD_TESTERDLG_SETTINGS_PAGE };
	int		m_nScope;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(TesterDlgSettingsPage)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	// Generated message map functions
	//{{AFX_MSG(TesterDlgSettingsPage)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnRadioAll();
	afx_msg void OnRadioCurrent();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	TesterConfigMgr *m_pTesterConfigMgr;

	// name of the current attribute
	std::string m_strCurrentAttributeName;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
