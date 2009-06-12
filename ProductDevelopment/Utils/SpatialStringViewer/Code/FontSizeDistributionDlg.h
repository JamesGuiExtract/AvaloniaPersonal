#if !defined(AFX_FONTSIZEDISTRIBUTIONDLG_H__14DFACF3_37FA_44D1_A950_C502E93E7C61__INCLUDED_)
#define AFX_FONTSIZEDISTRIBUTIONDLG_H__14DFACF3_37FA_44D1_A950_C502E93E7C61__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// FontSizeDistributionDlg.h : header file
//
#include "SpatialStringViewerDlg.h"

/////////////////////////////////////////////////////////////////////////////
// FontSizeDistributionDlg dialog

class FontSizeDistributionDlg : public CDialog
{
// Construction
public:
	FontSizeDistributionDlg(CSpatialStringViewerDlg* pSSVDlg,
				 SpatialStringViewerCfg* pCfgDlg,
				 CWnd* pParent = NULL);

	void refreshDistribution();

// Dialog Data
	//{{AFX_DATA(FontSizeDistributionDlg)
	enum { IDD = IDD_DLG_FONTSIZEDISTRIBUTION };
	CStatic	m_staticNumChars;
	CListCtrl	m_listFontSizeDistribution;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(FontSizeDistributionDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(FontSizeDistributionDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
private:

///////////////
// Variables
///////////////
	CSpatialStringViewerDlg* m_pSSVDlg;
	SpatialStringViewerCfg* m_pCfgDlg;

///////////////
// Methods
///////////////
	
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FONTSIZEDISTRIBUTIONDLG_H__14DFACF3_37FA_44D1_A950_C502E93E7C61__INCLUDED_)
