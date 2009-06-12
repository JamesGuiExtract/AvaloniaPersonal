
#pragma once

#include <SystemTray.h>

//-------------------------------------------------------------------------------------------------
// CGetNextELICodeDlg dialog
//-------------------------------------------------------------------------------------------------
enum ELocationIdentifierType
{
	kInvalidLocationIdentifierType,
	kExceptionLocationIdentifier,
	kMethodLocationIdentifier,
};

//-------------------------------------------------------------------------------------------------
// CGetNextELICodeDlg dialog
//-------------------------------------------------------------------------------------------------
class CGetNextELICodeDlg : public CDialog
{
// Construction
public:
	CGetNextELICodeDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CGetNextELICodeDlg)
	enum { IDD = IDD_GETNEXTELICODE_DIALOG };
	CEdit	m_EditELI;
	CString	m_Status;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGetNextELICodeDlg)
	public:
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL OnCommand(WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;
	CSystemTray m_SystemTrayIcon;
	LRESULT OnTrayNotification(WPARAM wParam, LPARAM lParam);

	void setStatusText(const CString& zText);
	void initializeLIGenerationProcess(ELocationIdentifierType eLIType, bool bDisplayDialogs = true);
	
	// Generated message map functions
	//{{AFX_MSG(CGetNextELICodeDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};
//-------------------------------------------------------------------------------------------------

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
