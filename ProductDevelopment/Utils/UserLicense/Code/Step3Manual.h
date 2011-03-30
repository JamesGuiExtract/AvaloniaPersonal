#pragma once

#include "LicenseWizardPage.h"
#include <ClipboardManager.h>

class CLicenseRequest;

#include <memory>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CStep3Manual
//--------------------------------------------------------------------------------------------------
class CStep3Manual : public CLicenseWizardPage
{
	DECLARE_DYNAMIC(CStep3Manual)

public:
	CStep3Manual(CLicenseRequest &licenseRequest);
	virtual ~CStep3Manual();

// Dialog Data
	enum { IDD = IDD_STEP3_MANUAL };

// Overrides
	virtual BOOL OnSetActive();
	virtual BOOL OnInitDialog();
	virtual LRESULT OnWizardBack();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	DECLARE_MESSAGE_MAP()

// Message handlers
	afx_msg void OnClickedCopyBody();
	afx_msg void OnClickedCopySubject();
	afx_msg void OnClickedCopyRecipient();

private:

	////////////////
	// Variables
	////////////////

	// Clipboard manager to handle placing data into the clipboard
	unique_ptr<ClipboardManager> m_apClipboardManager;

	// The current license request information.
	CLicenseRequest &m_licenseRequest;

	// The instructions for sending the email (incorporates the product name)
	CString m_zSendInstructions;

	// Whether the copy steps have been completed;
	bool m_bCopyStepCompleted;

	// Control variables
	CButton m_btnCopySubject;
	CButton m_btnCopyRecipient;
};
