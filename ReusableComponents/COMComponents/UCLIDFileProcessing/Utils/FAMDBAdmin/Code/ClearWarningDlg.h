#pragma once
#include "afxwin.h"

#include <string>

using namespace std;

// ClearWarningDlg dialog
class ClearWarningDlg : public CDialog
{
	DECLARE_DYNAMIC(ClearWarningDlg)

public:
	ClearWarningDlg(const string& strCaption, const string& strTitle, CWnd* pParent = NULL);
	virtual ~ClearWarningDlg();

// Dialog Data
	enum { IDD = IDD_DIALOG_CLEAR_WARNING };

	// true if the retain actions checkbox was checked by the user; false otherwise
	bool getRetainActions();

	// Sets the caption and title text
	void setCaption(const string& strCaption);
	void setTitle(const string& strTitle);

protected:
	virtual BOOL OnInitDialog();
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	afx_msg void OnBnClickedYes();
	afx_msg void OnBnClickedNo();

	DECLARE_MESSAGE_MAP()
public:

	// Controls
	CStatic m_labelCaption;
	CButton m_checkRetainActions;

	// Data
	string m_strTitle;
	string m_strCaption;
	bool m_bRetainActions;
};
