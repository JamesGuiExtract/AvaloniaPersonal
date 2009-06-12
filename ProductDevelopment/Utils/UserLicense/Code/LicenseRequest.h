#pragma once

#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Externs
//-------------------------------------------------------------------------------------------------
extern const string gstrUNKNOWN;

//--------------------------------------------------------------------------------------------------
// CLicenseRequest
//--------------------------------------------------------------------------------------------------
class CLicenseRequest
{
public:
	CLicenseRequest(void);
	~CLicenseRequest(void);

	//////////////
	// Variables
	//////////////

	CString	m_zKey;
	CString	m_zProductName;
	CString m_zVersion;
	CString m_zType;
	CString m_zName;
	CString m_zCompany;
	CString m_zPhone;
	CString m_zEmail;
	CString m_zRegistration;
	string m_strLicenseEmailAddress;
	const string m_strEmailSubject;
	bool m_bUseRegistrationKey;
	bool m_bUseDesktopEmail;

	//////////////
	// Methods
	//////////////

	// Creates the license request string using the data from the class variables.
	string createLicenseRequestText();

	// Automatically generates an email addressed to the appropriate email address with the
	// license request filled in.
	void createLicenseRequestEmail();
};
//--------------------------------------------------------------------------------------------------