#pragma once

// Prompt2Dlg.h : header file
//

#include "BaseUtils.h"
#include "Resource.h"

/////////////////////////////////////////////////////////////////////////////
// Prompt2Dlg dialog

class EXPORT_BaseUtils Prompt2Dlg : public CDialog
{
// Construction
public:
	// PROMISE: If bDefaultFocusInput1 == true, the input1 field will
	//			be selected+highlighted by default when the dialog box is displayed.
	//			If bDefaultFocusInput1 == false, the input2 field will be
	//			be selected+highlighted by default when the dialog box is displayed.
	Prompt2Dlg(const CString& zTitle, 
		       const CString& zPrompt1 = "", 
			   const CString& zInput1 = "",
		       const CString& zPrompt2 = "", 
			   const CString& zInput2 = "",
			   bool bDefaultFocusInput1 = true,
			   const CString& zHeader = "",
			   CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(Prompt2Dlg)
	enum { IDD = IDD_PROMPT2_DLG };
	CEdit	m_editInput2;
	CEdit	m_editInput1;
	CString	m_zInput1;
	CString	m_zInput2;
	CString	m_zPrompt1;
	CString	m_zPrompt2;
	CString m_zHeader;
	//}}AFX_DATA

	// override DoModal so that the correct resource template
	// is always used.
	virtual int DoModal();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(Prompt2Dlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(Prompt2Dlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnChangeEditInput1();
	afx_msg void OnBnClickOK();

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	///////////
	//Variables
	//////////
	CString m_zTitle;
	bool m_bDefaultFocusInput1;
	///////////
	//Methods
	//////////
	void updateControl();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
