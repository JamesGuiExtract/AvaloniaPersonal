#pragma once

////////////////////////////////////////////////////////////////////////////////////////////////////
// CAlertLevelDlg dialog
////////////////////////////////////////////////////////////////////////////////////////////////////
class CAlertLevelDlg : public CDialog
{
	DECLARE_DYNAMIC(CAlertLevelDlg)

public:
	CAlertLevelDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CAlertLevelDlg();

	static bool GetAlertLevel(CWnd *pParent, CString zTitle,
		DWORD &rdwAlertLevel, DWORD &rdwAlertMultiple);

// Dialog Data
	enum { IDD = IDD_ALERT_LEVEL_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	DECLARE_MESSAGE_MAP()

private:
	
	//////////////
	// Variables
	//////////////

	DWORD m_dwAlertLevel;
	DWORD m_dwAlertMultiple;
	CString m_zTitle;
};
