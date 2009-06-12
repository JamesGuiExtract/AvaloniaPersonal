// TutorialVC3Dlg.h : header file
//

#if !defined(AFX_TUTORIALVC3DLG_H__7C5AF5E8_3D83_4E8F_829B_3CA02B82D727__INCLUDED_)
#define AFX_TUTORIALVC3DLG_H__7C5AF5E8_3D83_4E8F_829B_3CA02B82D727__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC3Dlg dialog

class CTutorialVC3Dlg : public CDialog
{
// Construction
public:
	CTutorialVC3Dlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTutorialVC3Dlg)
	enum { IDD = IDD_TUTORIALVC3_DIALOG };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTutorialVC3Dlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTutorialVC3Dlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnTest();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC3DLG_H__7C5AF5E8_3D83_4E8F_829B_3CA02B82D727__INCLUDED_)
