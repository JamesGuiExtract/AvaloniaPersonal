
//#include "afxwin.h"

#pragma once
// PromptValuesDlg.h : header file
//


/////////////////////////////////////////////////////////////////////////////
// PromptValuesDlg dialog

class PromptValuesDlg : public CDialog
{
// Construction
public:
	PromptValuesDlg(CWnd* pParent = NULL);   // standard constructor

	// whether or not to enable the Quarter
	void enableQuarter(bool bEnable){m_bEnableQuarter = bEnable ? TRUE : FALSE;}
	// whether or not to enable the Quarter-Quarter
	void enableQuarterQuarter(bool bEnable) {m_bEnableQQ = bEnable ? TRUE : FALSE;}
	// whether or not to enable the Quarter-Quarter-Quarter
	void enableQuarterQuarterQuarter(bool bEnable) {m_bEnableQQQ = bEnable ? TRUE : FALSE;}

	void setStaticSelectText ( CString zSelectText );

// Dialog Data
	//{{AFX_DATA(PromptValuesDlg)
	enum { IDD = IDD_DLG_PROMPT };
	CButton m_btnUseExisting;
	CButton	m_btnSectionGT36;
	CButton	m_btnQQQQ;
	CButton	m_btnQQQ;
	CButton	m_btnQQ;
	CButton	m_btnQ;
	CEdit	m_editSection;
	CEdit	m_editQQ;
	CEdit	m_editQQQ;
	CEdit	m_editQuarter;
	BOOL	m_bDrawQuarter;
	BOOL	m_bDrawQQ;
	BOOL	m_bDrawQQQ;
	BOOL	m_bDrawQQQQ;
	BOOL	m_bSectionGT36;
	BOOL	m_bUseExisting;
	CStatic m_staticSelectText;
	//}}AFX_DATA

	static int m_cmbRangeDir;
	static int m_cmbTownshipDir;
	static CString m_zTownship;
	static CString m_zCountyCode;
	static CString m_zRange;
	static CString m_zSection;
	static CString m_zQQ;
	static CString m_zQQQ;
	static CString m_zQuarter;


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(PromptValuesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL


// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(PromptValuesDlg)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnCancel();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	BOOL m_bEnableQuarter;
	BOOL m_bEnableQQ;
	BOOL m_bEnableQQQ;

	CString m_zStaticSelectText;
	afx_msg void OnStnClickedStaticSelectText();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

