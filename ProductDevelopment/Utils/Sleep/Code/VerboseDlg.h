// VerboseDlg.h - Definition of the CVerboseDlg class
#pragma once
#include "Resource.h"

#include <string>

using namespace std;

// CESImageFinderDlg dialog
class CVerboseDlg : public CDialog
{
// Construction
public:
	CVerboseDlg(CWnd* pParent = NULL);

	// function to set the sleep time label
	void setSleepTime(unsigned long);

// Dialog Data
	enum { IDD = IDD_SLEEP_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	
	//----------------------------------------------------------------------------------------------
	// these functions handle the button clicks
	afx_msg void OnCancel();
	afx_msg void OnOK();
	//----------------------------------------------------------------------------------------------
	// other message handlers
	virtual BOOL OnInitDialog();
	//----------------------------------------------------------------------------------------------

// Implementation
protected:
	HICON m_hIcon;

	DECLARE_MESSAGE_MAP()

};
