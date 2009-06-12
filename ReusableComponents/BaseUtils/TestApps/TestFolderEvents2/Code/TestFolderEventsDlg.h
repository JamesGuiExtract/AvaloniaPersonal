// TestFolderEventsDlg.h : header file
//

#pragma once

#include <memory>
#include <ThreadSafeLogFile.h>

/////////////////////////////////////////////////////////////////////////////
// CTestFolderEventsDlg dialog

class CTestFolderEventsDlg : public CDialog
{
// Construction
public:
	CTestFolderEventsDlg(CWnd* pParent = NULL);	// standard constructor
	~CTestFolderEventsDlg();

// Dialog Data
	//{{AFX_DATA(CTestFolderEventsDlg)
	enum { IDD = IDD_TESTFOLDEREVENTS_DIALOG };
	CString	m_zFolder;
	BOOL	m_bRecursive;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestFolderEventsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CTestFolderEventsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	virtual void OnCancel();
	afx_msg void OnButtonBrowse();
	afx_msg void OnButtonStart();
	afx_msg void OnCheckRecursive();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	ThreadSafeLogFile*	getLogFile();
	const std::string&	getLogFileName() const;

	std::auto_ptr<ThreadSafeLogFile> m_apLogFile;

	// Thread-related items
	friend UINT asyncListeningThread(LPVOID pData);
	void startListening();
	CWinThread*	m_pListenerThread;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
