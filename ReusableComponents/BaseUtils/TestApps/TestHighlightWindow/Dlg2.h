#if !defined(AFX_DLG2_H__8934050F_B97F_45FF_A4E9_E913DBCD5E47__INCLUDED_)
#define AFX_DLG2_H__8934050F_B97F_45FF_A4E9_E913DBCD5E47__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Dlg2.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// Dlg2 dialog

class Dlg2 : public CDialog
{
// Construction
public:
	Dlg2(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(Dlg2)
	enum { IDD = IDD_DIALOG2 };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(Dlg2)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized );
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(Dlg2)
	afx_msg void OnMove(int x, int y);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnSetfocusEdit1();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DLG2_H__8934050F_B97F_45FF_A4E9_E913DBCD5E47__INCLUDED_)
