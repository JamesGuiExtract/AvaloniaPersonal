#if !defined(AFX_DLG3_H__1F812996_B5B6_4377_BF35_0F52AB30E1CA__INCLUDED_)
#define AFX_DLG3_H__1F812996_B5B6_4377_BF35_0F52AB30E1CA__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Dlg3.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// Dlg3 dialog

class Dlg3 : public CDialog
{
// Construction
public:
	Dlg3(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(Dlg3)
	enum { IDD = IDD_DIALOG3 };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(Dlg3)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void OnActivate( UINT nState, CWnd* pWndOther, BOOL bMinimized );
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(Dlg3)
	afx_msg void OnMove(int x, int y);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DLG3_H__1F812996_B5B6_4377_BF35_0F52AB30E1CA__INCLUDED_)
