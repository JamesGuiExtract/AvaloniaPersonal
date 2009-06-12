// Step2.cpp : implementation file

#include "stdafx.h"
#include "UserLicense.h"
#include "Step2.h"
#include "LicenseRequest.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CStep2
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CStep2, CLicenseWizardPage)
//--------------------------------------------------------------------------------------------------
CStep2::CStep2(CLicenseRequest &licenseRequest)
	: CLicenseWizardPage(CStep2::IDD)
	, m_licenseRequest(licenseRequest)
{
}
//--------------------------------------------------------------------------------------------------
CStep2::~CStep2()
{
}
//--------------------------------------------------------------------------------------------------
void CStep2::DoDataExchange(CDataExchange* pDX)
{
	CLicenseWizardPage::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_RADIO_DESKTOP_EMAIL, m_btnDesktopEmail);
	DDX_Control(pDX, IDC_RADIO_WEB_EMAIL, m_btnWebEmail);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CStep2, CLicenseWizardPage)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CStep2::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CLicenseWizardPage::OnInitDialog();

		// Default to desktop email
		m_btnDesktopEmail.SetCheck(BST_CHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23331");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
BOOL CStep2::OnSetActive()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Enable both the next and back wizard buttons.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23336", pWizard != NULL);

		pWizard->SetWizardButtons(PSWIZB_BACK | PSWIZB_NEXT);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23335");

	return CLicenseWizardPage::OnSetActive();
}
//--------------------------------------------------------------------------------------------------
LRESULT CStep2::OnWizardNext()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Retrieve the user's email system selection.
		m_licenseRequest.m_bUseDesktopEmail = (m_btnDesktopEmail.GetCheck() == BST_CHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23348");

	// Depending on the user's selection, go to either the automatic or manual step 3 from here.
	return (m_licenseRequest.m_bUseDesktopEmail ? IDD_STEP3_AUTOMATIC : IDD_STEP3_MANUAL);
}
//--------------------------------------------------------------------------------------------------