// IcoMapLicenseUtilDlg.h : header file
//

#pragma once


#include "SafeNetLicenseCfg.h"

/////////////////////////////////////////////////////////////////////////////
// CIcoMapLicenseUtilDlg dialog

class CIcoMapLicenseUtilDlg : public CDialog
{
// Construction
public:
	CIcoMapLicenseUtilDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CIcoMapLicenseUtilDlg)
	enum { IDD = IDD_ICOMAPLICENSEUTIL_DIALOG };
	CStatic	m_staticLimitText;
	CEdit	m_editIcoMapUserLimit;
	CButton	m_btnServerStatus;
	CStatic	m_staticLMText;
	CEdit	m_editLMServer;
	CButton m_checkNodeLocked;
	CButton m_checkConcurrent;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIcoMapLicenseUtilDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CIcoMapLicenseUtilDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnLicenseType();
	virtual void OnOK();
	afx_msg void OnServerStatus();
	afx_msg void OnChangeLmServer();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	// Methods
	void updateControls();

	
	// Variables
	SafeNetLicenseCfg m_snlcSafeNetCfg;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

