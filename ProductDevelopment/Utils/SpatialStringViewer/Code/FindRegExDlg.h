// FindRegExDlg.h : header file
//

#pragma once

#include "resource.h"
#include "SpatialStringViewerDlg.h"
#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\CPPLetter.h"

#include <string>
#include <vector>

using namespace std;

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
	CButton m_chkCaseSensitive;
	CEdit	m_editToFontSize;
	CEdit	m_editFromFontSize;
	CComboBox	m_cmbIncludeFontSize;
	CEdit	m_editPatterns;
	CEdit	m_editRangeTo;
	CEdit	m_editRangeFrom;
	CButton	m_btnFind;
	CButton m_btnPrevious;
	CButton m_btnResetSearch;
	CButton	m_chkAsRegEx;
	BOOL	m_bCaseSensitive;
	CString	m_zRangeFrom;
	CString	m_zRangeTo;
	CString	m_zPatterns;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(FindRegExDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL


// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(FindRegExDlg)
	virtual BOOL OnInitDialog();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg void OnBtnFind();
	afx_msg void OnBtnPrevious();
	afx_msg void OnBtnResetFind();
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg void OnChkRange();
	afx_msg void OnChkFontsizerange();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	////////////
	// Variables
	////////////
	bool m_bShiftDown;

	IRegularExprParserPtr m_ipRegExpr;

	// the actual start searching position in the iput text
	size_t m_nOffSet;

	// pointing to the spatial string viewer dlg
	CSpatialStringViewerDlg* m_pSSVDlg;

	SpatialStringViewerCfg* m_pCfgDlg;

	vector<string> m_vecPatterns;

	// the dialog height
	int m_nDlgHeight;

	// Data that is set when a search is initiated and keeps track of the current search state
	bool m_bSearchStarted;
	string m_strSearchText;
	vector<pair<size_t, size_t>> m_vecMatches;
	vector<pair<size_t, size_t>>::iterator m_currentSearchResult;

	// Min and max font range if it has been specified
	long m_nFontMin;
	long m_nFontMax;

	///////////
	// Methods
	///////////
	// according to the search scope and search position defined by user, 
	// get the appropriate input text for search
	string calculateSearchText();

	// Finds all matches for the text
	void computeMatches();

	// convert block of patterns into a vector of patterns
	void readPatterns(const string& strPatternsText);

	// if the Search Range is selected, make sure the From and To values are valid
	bool validateFromToValue();

	// Gets the start and end position for the search (may be the entire document)
	size_t getSearchStartPosition(size_t inputLength);
	size_t getSearchEndPosition(size_t inputLength);

	// Checks the font size and search range settings to ensure they are valid
	// Note: Check font size settings will set the m_nFontMin and m_nFontMax
	bool checkFontSizeSettings();
	bool checkSearchRangeSettings();

	IRegularExprParserPtr getRegExParser();

	void getStartAndEndPositionForMatch(IObjectPairPtr ipMatchPair, long& nStart, long& nEnd);

	bool initializeSearch();
	void updateUIForSearch();
	void updateRangeEnableStates();

	void selectCurrentSearchResult();
};