
#pragma once

// RVUIThread.h : header file
//

#include "DataAreaDlg.h"

#include <memory>
#include <Win32Event.h>
#include <UCLIDException.h>

/////////////////////////////////////////////////////////////////////////////
// RVUIThread thread

class RVUIThread : public CWinThread
{
	DECLARE_DYNCREATE(RVUIThread)

public:
	RVUIThread();
	~RVUIThread();

// Attributes
public:

	// Full path to image (and voa) file to be processed by Dialog
	std::string	m_strFileName;

	// the FAM Tag Manager pointer
	IFAMTagManagerPtr m_ipFAMTagManager;

	// Full path to INI file to be read and processed by Dialog
	//std::string	m_strINIFileName;
	RedactionUISettings m_UISettings;

	// Database pointer
	IFileProcessingDBPtr m_ipFAMDB;

// Operations
public:
	// Returns true if event is signaled, false if event times out
	bool	WaitForFileComplete(unsigned long ulWaitTimeMilliseconds);

	void	WaitForThreadInitialized();

	void	RequestTermination();

	// Holds exception caught at start
	UCLIDException	m_ue;
	bool			m_bExceptionThrown;
	bool			m_bTerminationRequested;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(RVUIThread)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(RVUIThread)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()

private:
	// Dialog used to review and approve redaction items
	std::auto_ptr<CDataAreaDlg> m_apDlg;

	// Event signaled when the Dialog finishes reviewing a file
	Win32Event	m_threadFileCompleteEvent;

	// Event signaled when Thread (and Dialog) finishes proper initialization
	Win32Event	m_threadInitCompleteEvent;

	// Set to true when WM_NEW_FILE_READY message actually received by 
	// PreTranslateMsg().  This will protect against WaitForFileComplete() 
	// timing out when it doesn't know a file is available.  The variable is 
	// declared volatile because reading and writing take place in different 
	// threads.
	volatile bool	m_bNewFileReceived;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
