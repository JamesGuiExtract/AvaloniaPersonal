//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	KeyboardInputSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <EDirectionType.h>

#include <vector>

/////////////////////////////////////////////////////////////////////////////
// KeyboardInputSettingsDlg dialog

class KeyboardInputSettingsDlg : public CPropertyPage
{
	DECLARE_DYNCREATE(KeyboardInputSettingsDlg)

// Construction
public:
	KeyboardInputSettingsDlg();
	~KeyboardInputSettingsDlg();
	void createToolTips();

	int m_iKeyboardInputSettingsPageIndex;	//will be set by Property sheet who owns this page

// Dialog Data
	//{{AFX_DATA(KeyboardInputSettingsDlg)
	enum { IDD = IDD_KEYBOARD_SETTINGS };
	CEdit	m_ctrlWest;
	CEdit	m_ctrlSW;
	CEdit	m_ctrlSE;
	CEdit	m_ctrlSouth;
	CEdit	m_ctrlNW;
	CEdit	m_ctrlNE;
	CEdit	m_ctrlNorth;
	CEdit	m_ctrlEast;
	CString	m_editEast;
	CString	m_editNorth;
	CString	m_editNE;
	CString	m_editNW;
	CString	m_editSouth;
	CString	m_editSE;
	CString	m_editSW;
	CString	m_editWest;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(KeyboardInputSettingsDlg)
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
	CToolTipCtrl *m_pToolTips;
	//check if the input key code is alpha numeric, if not, clean the edit box
	void checkAlphaNumeric(CString &cstrEditCtrl, UINT ctrlID);
	//there should be no duplicate key codes exist
	//Require: this function must be called after UpdateDate(TRUE) is called
	bool validateKeyCodes();

	// Generated message map functions
	//{{AFX_MSG(KeyboardInputSettingsDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	afx_msg void OnUpdateDirection(UINT nID);
	DECLARE_MESSAGE_MAP()

	// Generated OLE dispatch map functions
	//{{AFX_DISPATCH(KeyboardInputSettingsDlg)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()
	DECLARE_INTERFACE_MAP()

private:
	void saveSettings();
	// if the pass-in key code already exists in the pass-in vector. If it's not,
	// push the key code into the vector
	bool findDuplicateKey(CString zKeyCode, std::vector<CString> &vecExistingKeyCodes);
	// make the direction shortcut persistent
	void setDirectionShortcut(EDirectionType eDirectionType, CString zShortcut);

	bool m_bInitialized;

	// whether or not any change made in this tab has been applied
	bool m_bApplied;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

