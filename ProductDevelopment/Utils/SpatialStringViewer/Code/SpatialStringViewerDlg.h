// SpatialStringViewerDlg.h : header file
//

#pragma once

#include <string>
#include <memory>
#include "PageRulerStatic.h"
#include "EditWithPageIndicators.h"

class FindRegExDlg;
class FontSizeDistributionDlg;
class SpatialStringViewerCfg;
class CharInfoDlg;
class WordLengthDistributionDlg;
class USSViewerToolBar;

/////////////////////////////////////////////////////////////////////////////
// CSpatialStringViewerDlg dialog

class CSpatialStringViewerDlg : public CDialog
{
// Construction
public:
	CSpatialStringViewerDlg(CWnd* pParent = NULL);	// standard constructor
	~CSpatialStringViewerDlg();

// Dialog Data
	//{{AFX_DATA(CSpatialStringViewerDlg)
	enum { IDD = IDD_SPATIALSTRINGVIEWER_DIALOG };
	CEditWithPageIndicators	m_editText;
	CString	m_zText;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSpatialStringViewerDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CSpatialStringViewerDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnCancel();
	afx_msg void OnFileClose();
	afx_msg void OnFileExit();
	afx_msg void OnFileOpen();
	afx_msg void OnHelpAboutUclidSpatialStringViewer();
	afx_msg void OnFileProperties();
	afx_msg void OnMnuFindRegexpr();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	afx_msg void OnMnuOpenCharInfo();
	afx_msg void OnMnuFontsizedistribution();
	afx_msg void OnMnuWordLengthDistribution();
	afx_msg void OnButtonFirstPage();
	afx_msg void OnButtonPrevPage();
	afx_msg void OnButtonNextPage();
	afx_msg void OnButtonLastPage();
	afx_msg void OnChangeGotoPage();
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	virtual BOOL OnHelpInfo(HELPINFO* pHelpInfo);
	//{{AFX_INSERT_LOCATION}}
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	// retrieves current cursor position
	int getCurrentCursorPosition();

	// retrieves entire document text
	std::string getEntireDocumentText();

	ISpatialStringPtr getSpatialString();

	// select/highlight a portion of text as specified 
	// by the start and end position
	void selectText(int nStartPos, int nEndPos);
	
private:
	//////////
	// Methods
	//////////

	// Returns strInputName if a USS file. 
	// Returns strInputName + ".uss" if the file exists.
	// Throws exception if strInputName + ".uss" is not found
	std::string getUSSFileName(const std::string strInputName) const;

	// load spatial information from the specified file m_strUSSFileName
	// If m_strUSSFileName == "", then m_ipSpatialString will be cleared out
	void loadSpatialStringFromFile();

	// resize the edit control to fit the client area of the dialog box
	void resizeEditControl();

	void setStatusBarText(const CString& zPage, const CString& zStartPos, 
		const CString& zEndPos, const CString& zConfidence, const CString& zPercentage,
		const CString& zPageConfidence);

	// Updates the window caption for the specified file
	void updateWindowCaption(const std::string& strFileName);

	// Create the tool bar for page movement
	void createToolBar();

	// Enables the toolbar page movement buttons appropriately
	void configureToolBarButtons();

	// Sets the Goto Edit box text based on the current page and the total number of pages
	void resetGoToPageText();

	// Gets the current page for the location of the cursor
	long getCurrentPage();

	// Gets the page for the given position
	long getPageAtPos(long nPos);

	// Used to update the status bar text
	void updateStatusBar();

	// Used to move to the first character of the next line for the current position
	// This does nothing if the current line is the first line or the last line
	void repositionToFirstCharOfNextLine();

	///////
	// Data
	///////

	// The currently loaded spatial information
	ISpatialStringPtr m_ipSpatialString;

	std::unique_ptr<SpatialStringViewerCfg> m_apSettings;

	// boolean variable to keep track of whether this dialog has
	// already been initialized.
	bool m_bInitialized;

	// Original SourceDocName property from USS file
	std::string	m_strOriginalSourceDoc;

	// The file name of the USS file that is opened
	std::string m_strUSSFileName;

	std::unique_ptr<FindRegExDlg> ma_pFindRegExDlg;

	std::unique_ptr<FontSizeDistributionDlg> ma_pFontSizeDistDlg;
	std::unique_ptr<CharInfoDlg> ma_pCharInfoDlg;
	std::unique_ptr<WordLengthDistributionDlg> ma_pWordLengthDistDlg;

	CStatusBar m_statusBar;

	// total number of characters in current document
	int m_nTotalNumChars;

	// The text that was last set in the Goto Edit box
	std::string m_strLastGoToPageText;

	// Toolbar
	std::unique_ptr<USSViewerToolBar> m_apToolBar;

	long m_nLastPageNumber;
};

