#pragma once

#include "LicenseWizardPage.h"

class CLicenseRequest;

//--------------------------------------------------------------------------------------------------
// CStep2
//--------------------------------------------------------------------------------------------------
class CStep2 : public CLicenseWizardPage
{
	DECLARE_DYNAMIC(CStep2)

public:
	CStep2(CLicenseRequest &licenseRequest);
	virtual ~CStep2();

// Dialog Data
	enum { IDD = IDD_STEP2 };

// Overrides
	virtual BOOL OnInitDialog();
	virtual BOOL OnSetActive();
	virtual LRESULT OnWizardNext();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	DECLARE_MESSAGE_MAP()
	
private:

	//////////////
	// Variables
	//////////////

	// The current license request information.
	CLicenseRequest &m_licenseRequest;

	// Control variables
	CButton m_btnDesktopEmail;
	CButton m_btnWebEmail;
};
