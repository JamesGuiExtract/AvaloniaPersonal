#pragma once

//--------------------------------------------------------------------------------------------------
// CLicenseWizard
//--------------------------------------------------------------------------------------------------
class CLicenseWizard : public CPropertySheet
{
	DECLARE_DYNAMIC(CLicenseWizard)

public:
	CLicenseWizard(UINT nIDCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);
	CLicenseWizard(LPCTSTR pszCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);
	virtual ~CLicenseWizard();

// Overrides
	virtual BOOL OnInitDialog();
	virtual int DoModal();

// Message handlers
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnOpenLicenseFolder();

	DECLARE_MESSAGE_MAP()

private:
	CButton m_BtnOpenLicenseFolder;
};
//--------------------------------------------------------------------------------------------------