//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ShortcutSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Wayne Lenius
//
//==================================================================================================
#pragma once

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CShortcutSettings dialog

class CShortcutSettings : public CPropertyPage
{
	DECLARE_DYNCREATE(CShortcutSettings)

// Construction
public:
	CShortcutSettings();
	~CShortcutSettings();
	void createToolTips();

	int m_iShortcutSettingsPageIndex;	//will be set by Property sheet who owns this page


// Dialog Data
	//{{AFX_DATA(CShortcutSettings)
	enum { IDD = IDD_SHORTCUT_SETTINGS };
	CEdit	m_ctrlLineAngle;
	CEdit	m_ctrlRight;
	CEdit	m_ctrlReverse;
	CEdit	m_ctrlLine;
	CEdit	m_ctrlLess;
	CEdit	m_ctrlLeft;
	CEdit	m_ctrlGreater;
	CEdit	m_ctrlForward;
	CEdit	m_ctrlFinishSketch;
	CEdit	m_ctrlFinishPart;
	CEdit	m_ctrlDeleteSketch;
	CEdit	m_ctrlCustom;
	CEdit	m_ctrlCurve8;
	CEdit	m_ctrlCurve7;
	CEdit	m_ctrlCurve6;
	CEdit	m_ctrlCurve5;
	CEdit	m_ctrlCurve4;
	CEdit	m_ctrlCurve3;
	CEdit	m_ctrlCurve2;
	CEdit	m_ctrlCurve1;
	CString	m_zCurve1;
	CString	m_zCurve2;
	CString	m_zCurve3;
	CString	m_zCurve4;
	CString	m_zCurve5;
	CString	m_zCurve6;
	CString	m_zCurve7;
	CString	m_zCurve8;
	CString	m_zCustom;
	CString	m_zDeleteSketch;
	CString	m_zFinishPart;
	CString	m_zFinishSketch;
	CString	m_zForward;
	CString	m_zGreater;
	CString	m_zLeft;
	CString	m_zLess;
	CString	m_zLine;
	CString	m_zReverse;
	CString	m_zRight;
	CString	m_zLineAngle;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CShortcutSettings)
	public:
	virtual void OnFinalRelease();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnSetActive();
	virtual BOOL OnApply();
	virtual BOOL OnKillActive();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	CToolTipCtrl*	m_pToolTips;
		
	// Check if the input key code is alpha numeric, if not, clean the edit box
	void checkAlphaNumeric(CString &cstrEditCtrl, UINT ctrlID);
	
	// There should not be any duplicate shortcuts
	// Require: this function must be called after UpdateDate(TRUE) is called
	bool validateShortcuts();

	// Generated message map functions
	//{{AFX_MSG(CShortcutSettings)
	virtual BOOL OnInitDialog();
	afx_msg void OnButtonRestore();
	//}}AFX_MSG
	afx_msg void OnChangeCommand(UINT nID);
	DECLARE_MESSAGE_MAP()

private:
	void saveSettings();
	// if the pass-in shortcut already exists in the pass-in vector. If it's not,
	// push the shortcut into the vector
	bool findDuplicateShortcut(CString zShortcut, std::vector<CString> &vecExistingShortcuts);

	bool m_bInitialized;

	// whether or not any change made in this tab has been applied
	bool m_bApplied;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

