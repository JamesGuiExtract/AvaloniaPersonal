#if !defined(AFX_LINEHEIGHTRESULTSDLG_H__6A0B862C_3F3F_4E9B_81D2_E3CE87715AC4__INCLUDED_)
#define AFX_LINEHEIGHTRESULTSDLG_H__6A0B862C_3F3F_4E9B_81D2_E3CE87715AC4__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// LineHeightResultsDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CLineHeightResultsDlg dialog

class CLineHeightResultsDlg : public CDialog
{
// Construction
public:
	CLineHeightResultsDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CLineHeightResultsDlg)
	enum { IDD = IDD_VIEWLINEHEIGHTRESULTS };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLineHeightResultsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CLineHeightResultsDlg)
		// NOTE: the ClassWizard will add member functions here
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_LINEHEIGHTRESULTSDLG_H__6A0B862C_3F3F_4E9B_81D2_E3CE87715AC4__INCLUDED_)
