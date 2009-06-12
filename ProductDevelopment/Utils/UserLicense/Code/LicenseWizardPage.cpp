// LicenseWizardPage.cpp : implementation file

#include "stdafx.h"
#include "UserLicense.h"
#include "LicenseWizardPage.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CLicenseWizardPage
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CLicenseWizardPage, CPropertyPage)
//-------------------------------------------------------------------------------------------------
CLicenseWizardPage::CLicenseWizardPage(UINT nIDTemplate)
	: CPropertyPage(nIDTemplate)
{
	try
	{
		// Disable the help button
		m_psp.dwFlags &= ~PSP_HASHELP;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23353");
}
//-------------------------------------------------------------------------------------------------
CLicenseWizardPage::~CLicenseWizardPage()
{
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CLicenseWizardPage, CPropertyPage)
END_MESSAGE_MAP()
//--------------------------------------------------------------------------------------------------