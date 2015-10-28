#pragma once

#include "resource.h"

/////////////////////////////////////////////////////////////////////////////
// CCounterEditDlg dialog
/////////////////////////////////////////////////////////////////////////////
class CCounterEditDlg : public CDialog
{
	DECLARE_DYNAMIC(CCounterEditDlg)

public:
	CCounterEditDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CCounterEditDlg();

// Dialog Data
	enum { IDD = IDD_EDIT_COUNTER };

	CString m_zCaption;
	CString m_zCounterID;
	CString m_zCounterName;

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()

	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickOK();

private:

	//////////////
	// Variables
	//////////////

	CEdit m_editCounterID;
	CEdit m_editCounterName;
};
