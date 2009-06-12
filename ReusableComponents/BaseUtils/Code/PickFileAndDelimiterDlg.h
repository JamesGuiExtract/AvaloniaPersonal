#pragma once

#include "BaseUtils.h"
#include "Resource.h"

/////////////////////////////////////////////////////////////////////////////
// PickFileAndDelimiterDlg dialog

class EXPORT_BaseUtils PickFileAndDelimiterDlg : public CDialog
{
// Construction
public:
	PickFileAndDelimiterDlg(const CString& zFileName="",
							const CString& zDlimiter="",
							bool bShowDelimiter=true,
							bool bOpenFile = true,
							CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(PickFileAndDelimiterDlg)
	enum { IDD = IDD_DLG_SELECT_FILE };
	CStatic	m_promptDelimiter;
	CEdit	m_editFileName;
	CEdit	m_editDelimiter;
	CString	m_zDelimiter;
	CString	m_zFileName;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(PickFileAndDelimiterDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

public:
	
	// override DoModal so that the correct resource template
	// is always used.
	virtual int DoModal();

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(PickFileAndDelimiterDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnBrowse();
	virtual void OnOK();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// whether or not to show the delimiter edit box
	bool m_bShowDelimiter;

	// whether or not this dialog box is used as open file or save file dialog
	bool m_bOpenFile;

	// Whether or not output file was specified via Browse.
	// This protects against stealth overwrite of file whose name was preloaded
	// into the edit box.
	bool m_bConfirmed;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

