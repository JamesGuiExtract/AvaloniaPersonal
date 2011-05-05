// ExtractTRPDlg.h : header file
//
#pragma once

#include "TimeRollbackPreventer.h"

#include <Win32Event.h>
#include <string>

using namespace std;

// Title of the window that receives the time rollback prevention & licensing related messages
const std::string gstrTRP_WINDOW_TITLE = "Extract Systems TRP Window";

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
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()

private:
	// Time Rollback Preventor object and associated event
	TimeRollbackPreventer m_TRP;
};
