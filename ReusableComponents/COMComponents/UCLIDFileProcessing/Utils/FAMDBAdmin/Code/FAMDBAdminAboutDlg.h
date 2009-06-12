//-------------------------------------------------------------------------------------------------
// CFAMDBAdminAboutDlg dialog used for About Flex Index
//-------------------------------------------------------------------------------------------------

#pragma once

#include "resource.h"
#include "afxwin.h"

// FAMDBAdminAboutDlg.h : header file
//

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminAboutDlg dialog
//-------------------------------------------------------------------------------------------------
class CFAMDBAdminAboutDlg : public CDialog
{
// Construction
public:
	CFAMDBAdminAboutDlg();

// Dialog Data
	//{{AFX_DATA(CFAMDBAdminAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFAMDBAdminAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CFAMDBAdminAboutDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
