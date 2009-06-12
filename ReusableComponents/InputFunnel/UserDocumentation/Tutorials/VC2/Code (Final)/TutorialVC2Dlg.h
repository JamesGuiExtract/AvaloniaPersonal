// TutorialVC2Dlg.h : header file
//
//{{AFX_INCLUDES()
#include "inputmanager.h"
//}}AFX_INCLUDES

#if !defined(AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_)
#define AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#import "UCLIDExceptionMgmt.dll"
using namespace UCLIDEXCEPTIONMGMTLib;


/////////////////////////////////////////////////////////////////////////////
// CTutorialVC2Dlg dialog

class CTutorialVC2Dlg : public CDialog
{
// Construction
public:
	CTutorialVC2Dlg(CWnd* pParent = NULL);	// standard constructor
	~CTutorialVC2Dlg();

// Dialog Data
	//{{AFX_DATA(CTutorialVC2Dlg)
	enum { IDD = IDD_TUTORIALVC2_DIALOG };
	CString	m_zEndPoint;
	CString	m_zStartPoint;
	CString	m_zBearing;
	CString	m_zDistance;
	CInputManager	m_InputFunnel;
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
	afx_msg void OnHtwindow();
	afx_msg void OnSrwindow();
	afx_msg void OnSetfocusEditBearing();
	afx_msg void OnSetfocusEditDistance();
	afx_msg void OnSetfocusEditStart();
	afx_msg void OnKillfocusEditBearing();
	afx_msg void OnKillfocusEditDistance();
	afx_msg void OnKillfocusEditStart();
	afx_msg void OnSetfocusEditEnd();
	afx_msg void OnClose();
	afx_msg void OnNotifyInputReceivedInputmanager1(LPDISPATCH pTextInput);
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	long	m_lControlID;
	long	m_lKeyboardInputControlID;
	double	m_dStartX;
	double	m_dStartY;
	double	m_dBearingInRadians;
	double	m_dDistanceInFeet;

	ICOMUCLIDExceptionPtr m_ipException;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC2DLG_H__A636D800_E30F_428F_84CA_B1C31ED393EB__INCLUDED_)
