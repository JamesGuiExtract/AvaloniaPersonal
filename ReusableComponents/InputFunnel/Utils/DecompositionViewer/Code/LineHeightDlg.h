#if !defined(AFX_LINEHEIGHTDLG_H__F6687CA9_A00C_4FB5_85FA_1F9E41C83B8D__INCLUDED_)
#define AFX_LINEHEIGHTDLG_H__F6687CA9_A00C_4FB5_85FA_1F9E41C83B8D__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// LineHeightDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CLineHeightDlg dialog

class CLineHeightDlg : public CDialog
{
// Construction
public:
	CLineHeightDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CLineHeightDlg)
	enum { IDD = IDD_VIEWLINEHEIGHTRESULTS };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLineHeightDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CLineHeightDlg)
		// NOTE: the ClassWizard will add member functions here
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_LINEHEIGHTDLG_H__F6687CA9_A00C_4FB5_85FA_1F9E41C83B8D__INCLUDED_)
