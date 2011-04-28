#include "stdafx.h"
#include "UserLicense.h"
#include "LicenseRequest.h"

#include <UCLIDException.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Externs
//-------------------------------------------------------------------------------------------------
const string gstrUNKNOWN = "(unknown)";

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string gstrEMAIL_SUBJECT = "License Request";
static const CString gzNEWLINE = "\r\n";

//--------------------------------------------------------------------------------------------------
// CLicenseRequest
//--------------------------------------------------------------------------------------------------
CLicenseRequest::CLicenseRequest(void)
	: m_zKey(_T(""))
	, m_zProductName(_T(""))
	, m_zVersion(_T(""))
	, m_zType(_T(""))
	, m_zName(_T(""))
	, m_zCompany(_T(""))
	, m_zPhone(_T(""))
	, m_zEmail(_T(""))
	, m_strLicenseEmailAddress("")
	, m_strEmailSubject(gstrEMAIL_SUBJECT)
	, m_bUseRegistrationKey(false)
	, m_bUseDesktopEmail(false)
{
}
//--------------------------------------------------------------------------------------------------
CLicenseRequest::~CLicenseRequest(void)
{
}
//-------------------------------------------------------------------------------------------------
string CLicenseRequest::createLicenseRequestText()
{
	string strRequest = "";
	
	// Fill in default values for product information that will potentially be empty.
	m_zProductName = asCppBool(m_zProductName.IsEmpty()) ? "None found" : m_zProductName;
	m_zVersion = asCppBool(m_zVersion.IsEmpty()) ? "N/A" : m_zVersion;
	m_zType = asCppBool(m_zType.IsEmpty()) ? "N/A" : m_zType;

	//put all the pieces together with "\r\n" as the delimiter
	//*NOTE*"\r\n" is REQUIRED to be able to paste into the License generation
	//tool that is used to make license files.
	strRequest = m_zKey + gzNEWLINE + 
		"Product: " + m_zProductName + gzNEWLINE +
		"Version: " + m_zVersion + gzNEWLINE +
		"Type: " + m_zType + gzNEWLINE +
		"Name: " + m_zName + gzNEWLINE + 
		"Company: " + m_zCompany + gzNEWLINE + 
		"Phone: " + m_zPhone + gzNEWLINE + 
		"Email: " + m_zEmail + gzNEWLINE;

	// append the registration key if applicable
	if (m_bUseRegistrationKey)
	{
		strRequest += "Registration Key: " + m_zRegistration;
	}

	return strRequest;
}
//-------------------------------------------------------------------------------------------------
void CLicenseRequest::createLicenseRequestEmail()
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{
			IExtractEmailMessagePtr ipMessage(CLSID_ExtractEmailMessage);
			ASSERT_RESOURCE_ALLOCATION("ELI32457", ipMessage != __nullptr);

			ipMessage->Subject = gstrEMAIL_SUBJECT.c_str();
			ipMessage->Body = createLicenseRequestText().c_str();
			ipMessage->AddRecipient(m_strLicenseEmailAddress.c_str());
			ipMessage->ShowInClient(VARIANT_FALSE);

			ipMessage = __nullptr;
		}
		CoUninitialize();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32459");
}
//-------------------------------------------------------------------------------------------------