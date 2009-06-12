#pragma once

#include "LicenseWizardPage.h"

class CLicenseRequest;

//--------------------------------------------------------------------------------------------------
// CStep3Automatic
//--------------------------------------------------------------------------------------------------
class CStep3Automatic : public CLicenseWizardPage
{
	DECLARE_DYNAMIC(CStep3Automatic)

public:
	CStep3Automatic(CLicenseRequest &licenseRequest);
	virtual ~CStep3Automatic();

// Dialog Data
	enum { IDD = IDD_STEP3_AUTOMATIC };

// Overrides
	virtual BOOL OnSetActive();
	virtual BOOL OnInitDialog();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	DECLARE_MESSAGE_MAP()

// Message Handles
	afx_msg void OnClickedShowManualInstructions();
	afx_msg void OnClickedGenerateEmail();

private:

	//////////////
	// Variables
	//////////////

	// The current license request information.
	CLicenseRequest &m_licenseRequest;

	// Whether the email has been generated
	bool m_bEmailGenerated;

	// The instructions for sending the email (incorporates the product name)
	CString m_zSendInstructions;
};
