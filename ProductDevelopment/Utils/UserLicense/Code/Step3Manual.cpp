// Step3Manual.cpp : implementation file

#include "stdafx.h"
#include "UserLicense.h"
#include "Step3Manual.h"
#include "LicenseRequest.h"

#include <ClipboardManager.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const CString gzSEND_INSTRUCTIONS = "Click \"Send\" to send your email.  Extract Systems "
	"will send you back an email containing a license file and instructions on where to place the "
	"license file on your machine to activate ";

//--------------------------------------------------------------------------------------------------
// CStep3Manual
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CStep3Manual, CLicenseWizardPage)
//--------------------------------------------------------------------------------------------------
CStep3Manual::CStep3Manual(CLicenseRequest &licenseRequest)
	: CLicenseWizardPage(CStep3Manual::IDD)
	, m_licenseRequest(licenseRequest)
	, m_bCopyStepCompleted(false)
{
	try
	{
		m_apClipboardManager.reset(new ClipboardManager(this));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23354");
}
//--------------------------------------------------------------------------------------------------
CStep3Manual::~CStep3Manual()
{
	try
	{
		m_apClipboardManager.reset();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23355");
}
//--------------------------------------------------------------------------------------------------
void CStep3Manual::DoDataExchange(CDataExchange* pDX)
{
	CLicenseWizardPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_STATIC_SEND_INSTRUCTIONS, m_zSendInstructions);
	DDX_Control(pDX, IDC_BTN_COPY_SUBJECT, m_btnCopySubject);
	DDX_Control(pDX, IDC_BTN_COPY_RECIPIENT, m_btnCopyRecipient);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CStep3Manual, CLicenseWizardPage)
	ON_BN_CLICKED(IDC_BTN_COPY_BODY, &OnClickedCopyBody)
	ON_BN_CLICKED(IDC_BTN_COPY_SUBJECT, &OnClickedCopySubject)
	ON_BN_CLICKED(IDC_BTN_COPY_RECIPIENT, &OnClickedCopyRecipient)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CStep3Manual::OnInitDialog()
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23359");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
BOOL CStep3Manual::OnSetActive()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Enable the finish button if all the copy steps have been completed.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23363", pWizard != NULL);

		pWizard->SetWizardButtons(PSWIZB_BACK | (m_bCopyStepCompleted ? PSWIZB_FINISH : 
			PSWIZB_DISABLEDFINISH));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23343");

	return CLicenseWizardPage::OnSetActive();
}
//--------------------------------------------------------------------------------------------------
LRESULT CStep3Manual::OnWizardBack()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// If the the back button was pressed, go back to step 2 or step 3 automatic depending upon
	// which email system path was chosen in step 2.
	return (m_licenseRequest.m_bUseDesktopEmail ? IDD_STEP3_AUTOMATIC : IDD_STEP2);
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CStep3Manual::OnClickedCopyBody()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Copy the email body
		m_apClipboardManager->writeText(m_licenseRequest.createLicenseRequestText());
		m_btnCopySubject.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23344");
}
//--------------------------------------------------------------------------------------------------
void CStep3Manual::OnClickedCopySubject()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Copy the email subject
		m_apClipboardManager->writeText(m_licenseRequest.m_strEmailSubject);
		m_btnCopyRecipient.EnableWindow(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23345");
}
//--------------------------------------------------------------------------------------------------
void CStep3Manual::OnClickedCopyRecipient()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Copy the email recipient
		m_apClipboardManager->writeText(m_licenseRequest.m_strLicenseEmailAddress);
		m_bCopyStepCompleted = true;

		// Enable the finish button.
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23342", pWizard != NULL);

		pWizard->SetWizardButtons(PSWIZB_BACK | PSWIZB_FINISH);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23346");
}
//--------------------------------------------------------------------------------------------------