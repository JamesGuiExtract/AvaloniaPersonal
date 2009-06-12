// TutorialVC2Dlg.h : header file
//

#if !defined(AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_)
#define AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC2Dlg dialog

class CTutorialVC2Dlg : public CDialog
{
// Construction
public:
	CTutorialVC2Dlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTutorialVC2Dlg)
	enum { IDD = IDD_TUTORIALVC2_DIALOG };
	double	m_dBearingInRadians;
	double	m_dDistanceInFeet;
	CString	m_zEndPoint;
	CString	m_zStartPoint;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTutorialVC2Dlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTutorialVC2Dlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnCalculate();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_)
