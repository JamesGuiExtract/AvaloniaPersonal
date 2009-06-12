#pragma once
// OpenSaveFileDlg.h : header file
//

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// OpenSaveFileDlg dialog

class OpenSaveFileDlg : public CDialog
{
// Construction
public:
	OpenSaveFileDlg(const CString& zOpenDirectory, 
					const CString& zFileExtension,	// ex. "FSD" or "FOD"
					bool bOpenDialog = true, 
					CWnd* pParent = NULL); 

// Dialog Data
	//{{AFX_DATA(OpenSaveFileDlg)
	enum { IDD = IDD_DLG_OpenSave };
	CListBox	m_listFiles;
	CString	m_zOpenDirectory;
	CString	m_zFileName;
	CEdit m_editFileName;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(OpenSaveFileDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(OpenSaveFileDlg)
	afx_msg void OnDblclkLISTFiles();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	afx_msg void OnSelchangeLISTFiles();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	CString m_zFileExtension;
	// whether this dialog is used as open or saveas dialog
	// true -- open dialog, false -- save as dialog
	bool m_bOpenDialog;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
