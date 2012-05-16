#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardSaveAsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "resource.h"
#include "CurveToolManager.h"

class CurveWizardSaveAsDlg : public CDialog
{
// Construction
public:
	CurveWizardSaveAsDlg(CurveParameters curveParameters,CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CurveWizardSaveAsDlg)
	enum { IDD = IDD_DLG_CurveWizardSaveAs };
	CButton	m_btnCurve8;
	CButton	m_btnCurve7;
	CButton	m_btnCurve6;
	CButton	m_btnCurve5;
	CButton	m_btnCurve4;
	CButton	m_btnCurve3;
	CButton	m_btnCurve2;
	CButton	m_btnCurve1;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CurveWizardSaveAsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CurveWizardSaveAsDlg)
	virtual void OnCancel();
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	CurveParameters m_curveParameters;

	void configureButtons(void);
	ECurveToolID getSelectedCurveToolID(void);
	void saveSelectedCurveTool(void);
	void updateSelectedCurveTool(void);

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
