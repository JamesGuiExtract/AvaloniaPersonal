#if !defined(AFX_DLG1_H__0AD1DEBB_4CD9_4DFB_A4EA_201C17263035__INCLUDED_)
#define AFX_DLG1_H__0AD1DEBB_4CD9_4DFB_A4EA_201C17263035__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Dlg1.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// Dlg1 dialog

class Dlg1 : public CDialog
{
// Construction
public:
	Dlg1(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(Dlg1)
	enum { IDD = IDD_DIALOG1 };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(Dlg1)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized );
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(Dlg1)
	afx_msg void OnSetfocusEdit1();
	afx_msg void OnSetfocusEdit2();
	afx_msg void OnSetfocusEdit3();
	afx_msg void OnSetfocusEdit4();
	virtual BOOL OnInitDialog();
	afx_msg void OnMove(int x, int y);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DLG1_H__0AD1DEBB_4CD9_4DFB_A4EA_201C17263035__INCLUDED_)
