#if !defined(AFX_DLG4_H__0B76865F_F88A_44D9_8A0E_82DA7DA37134__INCLUDED_)
#define AFX_DLG4_H__0B76865F_F88A_44D9_8A0E_82DA7DA37134__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Dlg4.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// Dlg4 dialog

class Dlg4 : public CDialog
{
// Construction
public:
	Dlg4(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(Dlg4)
	enum { IDD = IDD_DIALOG4 };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(Dlg4)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(Dlg4)
		// NOTE: the ClassWizard will add member functions here
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DLG4_H__0B76865F_F88A_44D9_8A0E_82DA7DA37134__INCLUDED_)
