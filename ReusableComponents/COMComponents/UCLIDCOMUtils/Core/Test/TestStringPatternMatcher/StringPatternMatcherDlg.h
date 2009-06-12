// StringPatternMatcherDlg.h : header file
//

#if !defined(AFX_STRINGPATTERNMATCHERDLG_H__E88C73ED_2B8D_4CE8_AF70_E66F08DF2EBC__INCLUDED_)
#define AFX_STRINGPATTERNMATCHERDLG_H__E88C73ED_2B8D_4CE8_AF70_E66F08DF2EBC__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CStringPatternMatcherDlg dialog

class CStringPatternMatcherDlg : public CDialog
{
// Construction
public:
	CStringPatternMatcherDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CStringPatternMatcherDlg)
	enum { IDD = IDD_STRINGPATTERNMATCHER_DIALOG };
	CListCtrl	m_listMatches;
	CString	m_zInput;
	CString	m_zPattern;
	int		m_iOption;
	CString	m_zMatch1Expr;
	CString	m_zMatch2Expr;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CStringPatternMatcherDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CStringPatternMatcherDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	virtual void OnOK();
	afx_msg void OnButtonTestRegex();
	afx_msg void OnButtonClear();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// reg expr parser
	IRegularExprParserPtr m_ipRegExpr;

	// string pattern matcher
	IStringPatternMatcherPtr m_ipStringPatternMatcher;
	
	std::map<std::string, std::string> doUCLIDMatch();
	std::map<std::string, std::string> doRegExMatch();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STRINGPATTERNMATCHERDLG_H__E88C73ED_2B8D_4CE8_AF70_E66F08DF2EBC__INCLUDED_)
