// TestTFEDlg.h : header file
//

#if !defined(AFX_TESTTFEDLG_H__B73CC761_479F_467E_95A8_0367FEA091C7__INCLUDED_)
#define AFX_TESTTFEDLG_H__B73CC761_479F_467E_95A8_0367FEA091C7__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CTestTFEDlg dialog

class CTestTFEDlg : public CDialog
{
// Construction
public:
	CTestTFEDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTestTFEDlg)
	enum { IDD = IDD_TESTTFE_DIALOG };
	CComboBox	m_cmbFunctions;
	CButton	m_btnTest;
	CButton	m_btnClear;
	CString	m_zInput;
	CString	m_zOutput;
	CString	m_zParam;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestTFEDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestTFEDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButtonClear();
	afx_msg void OnEditchangeComboFunctions();
	virtual void OnOK();
	afx_msg void OnChangeEditInput();
	afx_msg void OnChangeEditParam();
	afx_msg void OnSelchangeComboFunctions();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Checks to see selected function requires additional parameters
	bool	isParameterMissing();

	// Loads combo box with available functions
	void	populateCombo();

	// Enables/disables Test button
	void	updateButtons();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTTFEDLG_H__B73CC761_479F_467E_95A8_0367FEA091C7__INCLUDED_)
