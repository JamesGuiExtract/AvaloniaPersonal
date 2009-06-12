#pragma once

// PromptDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// PromptDlg dialog

#include "BaseUtils.h"
#include "Resource.h"

class EXPORT_BaseUtils PromptDlg : public CDialog
{
private:
	CString m_zTitle;

public:
	PromptDlg(const CString& zTitle, const CString& zPrompt, 
		const CString& zInput = "", bool bAllowEmptyString = false, 
		bool bRemoveLeadingWhitespace = true, bool bRemoveTrailingWhitespace = true, 
		CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(PromptDlg)
	enum { IDD = IDD_PROMPT_DLG };
	CButton	m_btnOK;
	CEdit	m_editInput;
	CString	m_zPrompt;
	CString	m_zInput;
	//}}AFX_DATA

	// override DoModal so that the correct resource template
	// is always used.
	virtual int DoModal();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(PromptDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(PromptDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnChangeEditInput();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// If false, OK button will be disabled when text is empty
	bool	m_bAllowEmptyString;

	// If true, leading whitespace will be removed from result text
	bool	m_bRemoveLeadingWhitespace;

	// If true, trailing whitespace will be removed from result text
	bool	m_bRemoveTrailingWhitespace;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
