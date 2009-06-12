#pragma once

//--------------------------------------------------------------------------------------------------
// CLicenseWizardPage dialog
//--------------------------------------------------------------------------------------------------
class CLicenseWizardPage : public CPropertyPage
{
	DECLARE_DYNAMIC(CLicenseWizardPage)

public:
	CLicenseWizardPage(UINT nIDTemplate);
	virtual ~CLicenseWizardPage();

	DECLARE_MESSAGE_MAP()
};
