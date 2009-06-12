// CopyNewerFilesDlg.h : header file
//

#pragma once

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CCopyNewerFilesDlg dialog

class CCopyNewerFilesDlg : public CDialog
{
// Construction
public:
	CCopyNewerFilesDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CCopyNewerFilesDlg)
	enum { IDD = IDD_COPYNEWERFILES_DIALOG };
	CEdit	m_editFolder;
	CButton	m_btnCopy;
	BOOL	m_bPrompt;
	CString	m_zDestination;
	CString	m_zSource;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCopyNewerFilesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CCopyNewerFilesDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButtonCopy();
	afx_msg void OnButtonDestination();
	afx_msg void OnButtonSource();
	afx_msg void OnCheckPrompt();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Copies files from source folder to destination folder
	void	doCopy(std::string strSourceFolder, std::string strDestinationFolder);

	// Opens the log file
	void	openLogFile();

	// Set current folder text
	void	setFolderText(std::string strFolder);

	// Adds status string to Log File
	void	updateStatus(std::string strStatus);

	// File to receive status messages
	CStdioFile	m_fLogFile;

	// Flag indicating that log file is open
	bool	m_bLogFileOpen;

	// Flag indicating that log file could not be opened
	// and will not be written to
	bool	m_bNoLogFile;

	// Status counters
	long	m_lCountSameTime;
	long	m_lCountSourceOlder;
	long	m_lCountNoSourceRead;
	long	m_lCountNoSourceWriteTime;
	long	m_lCountNoDestinationWriteTime;
	long	m_lCountNoDestinationRead;
	long	m_lCountDestinationTooLong;
	long	m_lCountCopyFailure;
	long	m_lCountCopySuccess;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
