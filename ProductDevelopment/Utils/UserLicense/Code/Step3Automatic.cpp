// Step3Automatic.cpp : implementation file
//

#include "stdafx.h"
#include "UserLicense.h"
#include "Step3Automatic.h"
#include "LicenseRequest.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const CString gzSEND_INSTRUCTIONS = "Click \"Send\" to send your email.  Extract Systems "
	"will send you back an email containing a license file and instructions on where to place the "
	"license file on your machine to activate ";

//--------------------------------------------------------------------------------------------------
// CStep3Automatic
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CStep3Automatic, CLicenseWizardPage)
//--------------------------------------------------------------------------------------------------
CStep3Automatic::CStep3Automatic(CLicenseRequest &licenseRequest)
	: CLicenseWizardPage(CStep3Automatic::IDD)
	, m_licenseRequest(licenseRequest)
	, m_bEmailGenerated(false)
{
}
//--------------------------------------------------------------------------------------------------
CStep3Automatic::~CStep3Automatic()
{
}
//--------------------------------------------------------------------------------------------------
void CStep3Automatic::DoDataExchange(CDataExchange* pDX)
{
	CLicenseWizardPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_STATIC_SEND_INSTRUCTIONS, m_zSendInstructions);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CStep3Automatic, CLicenseWizardPage)
	ON_BN_CLICKED(IDC_BTN_SHOW_MANUAL_INSTRUCTIONS, &CStep3Automatic::OnClickedShowManualInstructions)
	ON_BN_CLICKED(IDC_BTN_GENERATE_EMAIL, &CStep3Automatic::OnClickedGenerateEmail)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CStep3Automatic::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CLicenseWizardPage::OnInitDialog();

		// Incoporate the product name into the send email instructions.
		if (m_licenseRequest.m_zProductName == gstrUNKNOWN.c_str())
		{
			m_zSendInstructions = gzSEND_INSTRUCTIONS + "your product.";
		}
		else
		{
			m_zSendInstructions = gzSEND_INSTRUCTIONS + m_licenseRequest.m_zProductName + ".";
		}
			
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23358");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
BOOL CStep3Automatic::OnSetActive()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Set the wizard to have a back and finish button, but disable the finish button until
		// the user has generated a license request email.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23338", pWizard != NULL);

		pWizard->SetWizardButtons(PSWIZB_BACK | (m_bEmailGenerated ? PSWIZB_FINISH : 
			PSWIZB_DISABLEDFINISH));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23339");

	return CLicenseWizardPage::OnSetActive();
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CStep3Automatic::OnClickedShowManualInstructions()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// If the user wants to use the manual steps to create an email, go to the next wizard page.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI24211", pWizard != NULL);

		pWizard->PressButton(PSBTN_NEXT);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24213");
}
//--------------------------------------------------------------------------------------------------
void CStep3Automatic::OnClickedGenerateEmail()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Generate the license request email.
		m_licenseRequest.createLicenseRequestEmail();

		m_bEmailGenerated = true;

		// Enable the finish button.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI24212", pWizard != NULL);

		pWizard->SetWizardButtons(PSWIZB_BACK | PSWIZB_FINISH);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23340");
}
//--------------------------------------------------------------------------------------------------