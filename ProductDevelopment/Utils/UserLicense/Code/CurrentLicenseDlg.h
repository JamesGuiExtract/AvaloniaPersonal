#pragma once
#include <afxwin.h>

//--------------------------------------------------------------------------------------------------
// CCurrentLicenseDlg dialog
//--------------------------------------------------------------------------------------------------
class CCurrentLicenseDlg : public CDialog
{
public:
	CCurrentLicenseDlg(CWnd* pParent = NULL);
	virtual ~CCurrentLicenseDlg();

	enum { IDD = IDD_CURRENT_LICENSE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()
};


