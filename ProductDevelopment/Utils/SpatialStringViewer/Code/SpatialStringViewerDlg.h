// SpatialStringViewerDlg.h : header file
//

#pragma once

#include <string>
#include <memory>

class FindRegExDlg;
class FontSizeDistributionDlg;
class SpatialStringViewerCfg;
class CharInfoDlg;
class WordLengthDistributionDlg;

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
	CEdit	m_editText;
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
		const CString& zEndPos, const CString& zPercentage);

	// Updates the window caption for the specified file
	void updateWindowCaption(const std::string& strFileName);

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
};

