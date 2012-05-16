//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SelectFeaturesDlg.h
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

#include "Resource.h"

#include <string>
/////////////////////////////////////////////////////////////////////////////
// SelectFeaturesDlg dialog

class SelectFeaturesDlg : public CDialog
{
// Construction
public:
	std::string GetSourceDocName() const;
	SelectFeaturesDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(SelectFeaturesDlg)
	enum { IDD = IDD_DLG_SelectFeatures };
	CEdit	m_editSourceDoc;
	CString	m_cstrSourceDocName;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(SelectFeaturesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(SelectFeaturesDlg)
	afx_msg void OnBtnBrowse();
	virtual void OnOK();
	virtual void OnCancel();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
