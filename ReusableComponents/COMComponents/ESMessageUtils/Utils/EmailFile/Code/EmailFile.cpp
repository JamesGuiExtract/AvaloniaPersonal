// EmailFile.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "EmailFile.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <LicenseMgmt.h>
#include <zipper.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>

#include <iostream>
#include <string>
#include <vector>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrEMAIL_ADDRESS_DELIMITERS = ";,";

//-------------------------------------------------------------------------------------------------
// CEmailFileApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CEmailFileApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CEmailFileApp construction
//-------------------------------------------------------------------------------------------------
CEmailFileApp::CEmailFileApp()
{
}

// The one and only CEmailFileApp object

CEmailFileApp theApp;

//-------------------------------------------------------------------------------------------------
// CEmailFileApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CEmailFileApp::InitInstance()
{
	bool bZipFile = false;
	string strZipFileName = "";
	string strLocalExceptionLog = "";

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		try
		{
			// set exception handling dialog
			UCLIDExceptionDlg ue_dlg;
			UCLIDException::setExceptionHandler(&ue_dlg);

			if ( !getAndValidateArguments( __argc, __argv ) )
			{
				return FALSE;
			}

			// Load license files ( this is need for IVariantVector )
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
			validateLicense();

			// Email Settings
			ISmtpEmailSettingsPtr ipEmailSettings(CLSID_SmtpEmailSettings);
			ASSERT_RESOURCE_ALLOCATION("ELI12601", ipEmailSettings != __nullptr );
			ipEmailSettings->LoadSettings(VARIANT_FALSE);

			// If there is no SMTP server set the UI needs to be displayed to enter that information
			string strServer = asString(ipEmailSettings->Server);
			if ( strServer.empty() || m_bConfigureSettings)
			{
				// Get the UI Pointer
				IConfigurableObjectPtr ipUI = ipEmailSettings;
				ASSERT_RESOURCE_ALLOCATION("ELI12606", ipUI != __nullptr );

				if (ipUI->RunConfiguration() == VARIANT_TRUE)
				{
					// Reload the settings if configuration was successful
					ipEmailSettings->LoadSettings(VARIANT_FALSE);
				}
				else
				{
					// If the settings window was cancelled then just return
					return FALSE;
				}

				// if the File or address were not defined there is nothing to do
				if (m_strFileToEmail.empty() || m_strEmailAddress.empty())
				{
					return FALSE;
				}
			}

			// Email Message 
			IExtractEmailMessagePtr ipMessage(CLSID_ExtractEmailMessage);
			ASSERT_RESOURCE_ALLOCATION("ELI12603", ipMessage != __nullptr );

			ipMessage->EmailSettings = ipEmailSettings;

			// Put file to send on list of files to be sent
			IVariantVectorPtr ipFiles(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI12604", ipFiles != __nullptr );
			if (m_bZipFile)
			{
				strZipFileName = m_strFileToEmail + ".zip";
				CZipper z(strZipFileName.c_str());
				z.AddFileToZip(m_strFileToEmail.c_str());
				z.CloseZip();
				ipFiles->PushBack( _bstr_t(strZipFileName.c_str()));
			}
			else
			{
				ipFiles->PushBack( _bstr_t(m_strFileToEmail.c_str()));
			}

			// Add file list to message
			ipMessage->Attachments = ipFiles;

			// Add Recipients list to email message
			ipMessage->Recipients = parseRecipientAddress();

			// Set the subject of the email
			if (m_strSubject.empty())
			{
				// Create a subject line
				m_strSubject = "File: " + ((bZipFile) ? strZipFileName : m_strFileToEmail);
			}
			ipMessage->Subject = m_strSubject.c_str();

			// Send  the message
			ipMessage->Send();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25047");
	}
	catch(UCLIDException &ue)
	{
		// Deal with the exception
		if (m_strExceptionLog.empty())
		{
			// If not logged locally, it should be displayed
			ue.display();
		}
		else
		{
			// Log the exception
			ue.log( m_strExceptionLog, false );
		}
	}

	// Clean up the zip file
	if ( m_bZipFile )
	{
		try
		{
			try
			{
				if (isValidFile (strZipFileName) )
				{
					deleteFile( strZipFileName );
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25134");
		}
		catch(UCLIDException &ue)
		{
			// Deal with the exception
			if (m_strExceptionLog.empty())
			{
				// If not logged locally, it should be displayed
				ue.display();
			}
			else
			{
				// Log the exception
				ue.log( m_strExceptionLog, false );
			}
		}
	}
	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::displayUsage(const string &strErrorMessage)
{
	// Log exception in the approriate file if there is an error message.
	string strErrorLine = "";
	if (!strErrorMessage.empty())
	{
		strErrorLine += strErrorMessage + "\r\n";
		UCLIDException ue("ELI25108", strErrorMessage);
		ue.addDebugInfo("Recipient address", m_strEmailAddress);
		ue.addDebugInfo("File to send", m_strFileToEmail);
		ue.addDebugInfo("Command line", m_lpCmdLine);
		ue.log(m_strExceptionLog);
	}
	
	// Don't display usage information if there is an exception log file specified.
	if (!m_strExceptionLog.empty())
	{
		return;
	}
	// ensure that there is at least one argument, and no more than 
	// 2 arguments other than options
	string strUsage = "";
	strUsage += "ERROR: Invalid number of arguments!\r\n" ;
	strUsage += strErrorLine;
	strUsage += "\r\n";
	strUsage += "This application requires 2 arguments\r\n";
	strUsage += "SYNTAX: EmailFile <options> arg1 arg2\r\n";
	strUsage += " <options>:\r\n";
	strUsage += "\t/c - Configure email settings\r\n";
	strUsage += "\t/z - Zip the file that is being sent before emailing\r\n";
	strUsage += "\t/subject <subject line> - Subject line for the email.\r\n";
	strUsage += "\t/ef <exception log file> - File for logging exceptions.\r\n";
	strUsage +=  "\r\n";	
	strUsage += "Arg1:\tName of the file to send by email.\r\n";
	strUsage += "Arg2:\tEmail address(es) to send the file.\r\n";
	strUsage += "\tMultiple addresses should be separated by either ; or ,\r\n";
	strUsage +=  "\r\n";
	strUsage += "If this application is run and the SMTP server has not been setup,\r\n";
	strUsage += "the UI for entering the SMTP server will be displayed\r\n";

	// display the message
	AfxMessageBox(strUsage.c_str(), MB_ICONINFORMATION);
}
//-------------------------------------------------------------------------------------------------
bool CEmailFileApp::getAndValidateArguments(int argc, char* argv[])
{
	int nPosArg1 = 1;

	// Initialize options
	m_strExceptionLog = "";
	m_strSubject = "";
	m_strFileToEmail = "";
	m_strEmailAddress = "";
	m_bZipFile = false;
	m_bConfigureSettings = false;

	// Get the arguments
	for ( int i = 1; i < argc; i++ )
	{
		string strArg = argv[i];

		// Make the argument lower case for compare
		makeLowerCase(strArg);
		if ( strArg == "/c" )
		{
			m_bConfigureSettings = true;
		}
		else if (strArg == "/z")
		{
			m_bZipFile = true;
		}
		else if (strArg == "/ef")
		{
			if (i+1 >= argc)
			{
				displayUsage("Missing exception file name!");
				return false;
			}

			m_strExceptionLog = argv[++i];
		}
		else if (strArg == "/subject")
		{
			if (i+1 >= argc)
			{
				displayUsage("Missing subject text!");
				return false;
			}

			m_strSubject = argv[++i];
		}
		else if (strArg.find('/') == string::npos )
		{
			// Make sure the File or email addresses have not been specified
			if (!m_strFileToEmail.empty() && !m_strEmailAddress.empty())
			{
				displayUsage("Invalid arguments found!");
				return false;
			}

			// Since options are expected to be first there are no more options
			m_strFileToEmail = argv[i];
			
			if ( i + 1 >= argc)
			{
				displayUsage("Missing recipient address!");
				return false;
			}

			// Get the email address
			m_strEmailAddress = argv[++i];
		}
		else
		{
			displayUsage("Invalid arguments specified.");
			return false;
		}
	}
	// Make sure there is either /c and/or both filename and email address
	if (m_bConfigureSettings || !m_strFileToEmail.empty() && !m_strEmailAddress.empty())
	{
		return true;
	}
	displayUsage("Missing filename or email address!");
	return false; 
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CEmailFileApp::parseRecipientAddress()
{
	// Put recipient on list of Recipients
	IVariantVectorPtr ipRecipients(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI25348", ipRecipients != __nullptr );

	// Parse the email address for ,; separated email addresses
	int iStart = 0;
	int iEnd = m_strEmailAddress.find_first_of(gstrEMAIL_ADDRESS_DELIMITERS);
	int nLength = m_strEmailAddress.length();
	do
	{
		// Get the address
		string strEmailAddress = m_strEmailAddress.substr(iStart, iEnd);
		if (!strEmailAddress.empty() )
		{
			trim(strEmailAddress," ", " ");

			// Make sure the email address has a @ symbol
			if ( strEmailAddress.find("@") == string::npos )
			{
				UCLIDException ue("ELI25135", "Invalid email address!");
				ue.addDebugInfo("Email address", strEmailAddress);
				throw ue;
			}
			ipRecipients->PushBack(strEmailAddress.c_str());
		}

		// Find the end of the next address
		if (iEnd != string::npos)
		{
			iStart = iEnd + 1;
			iEnd = m_strEmailAddress.find_first_of(gstrEMAIL_ADDRESS_DELIMITERS, iStart);
		}
		else
		{
			// No more addresses so exit loop
			break;
		}
	}
	while (iStart < nLength);

	// Return list of recipients
	return ipRecipients;
}
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::validateLicense()
{
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI15248", "Email File" );
}
//-------------------------------------------------------------------------------------------------
