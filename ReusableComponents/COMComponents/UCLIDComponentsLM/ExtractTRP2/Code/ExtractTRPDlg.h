// ExtractTRPDlg.h : header file
//
#pragma once

#include <TimeRollbackPreventer.h>
#include <Win32Event.h>

// CExtractTRPDlg dialog
class CExtractTRPDlg : public CDialog
{
// Construction
public:
	CExtractTRPDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_EXTRACTTRP_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg LRESULT OnGetAuthenticationCode(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnStateIsValid(WPARAM wParam, LPARAM lParam);
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

private:
	// Time Rollback Preventor object and associated event
	Win32Event m_stateIsInvalidEvent;
	TimeRollbackPreventer m_TRP;

	// Returns true if TRP state is valid, false otherwise
	bool	stateIsValid();
};
