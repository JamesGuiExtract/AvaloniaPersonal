#if !defined(AFX_FINDREGEXDLG_H__BEA398E6_E78A_45F8_971B_FA65A94A2120__INCLUDED_)
#define AFX_FINDREGEXDLG_H__BEA398E6_E78A_45F8_971B_FA65A94A2120__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// FindRegExDlg.h : header file
//

#include "resource.h"
#include "SpatialStringViewerDlg.h"
#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\CPPLetter.h"

#include <string>
#include <vector>

class SpatialStringViewerCfg;

/////////////////////////////////////////////////////////////////////////////
// FindRegExDlg dialog

class FindRegExDlg : public CDialog
{
// Construction
public:
	FindRegExDlg(CSpatialStringViewerDlg* pSSVDlg,
				 SpatialStringViewerCfg* pCfgDlg,
				 CWnd* pParent = NULL);   // standard constructor
	~FindRegExDlg();

// Dialog Data
	//{{AFX_DATA(FindRegExDlg)
	enum { IDD = IDD_DLG_FIND_REGEXPR };
	CButton	m_chkRange;
	CButton	m_chkFontSizeRange;
	CEdit	m_editToFontSize;
	CEdit	m_editFromFontSize;
	CComboBox	m_cmbIncludeFontSize;
	CEdit	m_editPatterns;
	CButton	m_btnAdvance;
	CEdit	m_editRangeTo;
	CEdit	m_editRangeFrom;
	CButton	m_btnFind;
	CButton	m_chkAsRegEx;
	CButton	m_chkTreatAsMultiRegex;
	CButton m_radOr;
	CButton m_radAnd;
	CButton m_radBegin;
	CButton m_radCurPos;
	BOOL	m_bCaseSensitive;
	CString	m_zRangeFrom;
	CString	m_zRangeTo;
	int		m_nSearchPos;
	int		m_nOperator;
	CString	m_zPatterns;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(FindRegExDlg)
	public:
	virtual BOOL DestroyWindow();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL


// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(FindRegExDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnFind();
	afx_msg void OnBtnAdvance();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg void OnChkRange();
	afx_msg void OnChkFontsizerange();
	afx_msg void OnChkAsRegex();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	IRegularExprParserPtr m_ipRegExpr;

	// the actual start searching position in the iput text
	int m_nOffSet;

	// pointing to the spatial string viewer dlg
	CSpatialStringViewerDlg* m_pSSVDlg;

	SpatialStringViewerCfg* m_pCfgDlg;

	std::vector<std::string> m_vecPatterns;

	// the dialog height
	int m_nDlgHeight;

	bool m_bShowAdvanced;

	///////////
	// Methods
	///////////
	// according to the search scope and search position defined by user, 
	// get the appropriate input text for search
	std::string calculateSearchText();

	// whether or not found pattern(s) in the input text
	bool foundPatternsInText(const std::string& strInputText, 
							 int &nFoundStartPos,
							 int &nFoundEndPos);

	// convert block of patterns into a vector of patterns
	void readPatterns(const std::string& strPatternsText);

	// show or hide detail settings
	void showDetailSettings(bool bShow);

	// if the Search Range is selected, make sure the From and To values are valid
	bool validateFromToValue();

	bool letterInFontSizeRange(const CPPLetter& letter);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FINDREGEXDLG_H__BEA398E6_E78A_45F8_971B_FA65A94A2120__INCLUDED_)
