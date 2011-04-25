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
#include <StringTokenizer.h>
#include <Misc.h>

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
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
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

			vector<string> vecRecipients;
			parseRecipientAddress(vecRecipients);

			string strFileToMail = m_strFileToEmail;
			if (!m_strFileToEmail.empty())
			{
				validateFileOrFolderExistence(strFileToMail, "ELI32293");

				if (m_bZipFile)
				{
					strZipFileName = m_strFileToEmail + ".zip";
					CZipper z(strZipFileName.c_str());
					z.AddFileToZip(m_strFileToEmail.c_str());
					z.CloseZip();
					strFileToMail = strZipFileName;
				}
			}

			if (m_bShowInClient)
			{
				showInClient(ipEmailSettings, vecRecipients, strFileToMail);
			}
			else
			{
				sendEmail(ipEmailSettings, vecRecipients, strFileToMail);
			}
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
	bool bError = !strErrorMessage.empty();
	string strFirstLine;
	string strCaption;
	UINT uiType = MB_OK;
	if (bError)
	{
		strCaption = "Error";
		uiType |= MB_ICONERROR;
		strFirstLine = strErrorMessage;
		UCLIDException ue("ELI25108", strErrorMessage);
		ue.addDebugInfo("Recipient address", m_strEmailAddress);
		ue.addDebugInfo("File to send", m_strFileToEmail);
		ue.addDebugInfo("Command line", m_lpCmdLine);
		ue.log(m_strExceptionLog);
		
		// Don't display usage information if there is an exception log file specified.
		if (!m_strExceptionLog.empty())
		{
			return;
		}
	}
	else
	{
		strFirstLine = "Usage:";
		strCaption = "Usage";
		uiType |= MB_ICONINFORMATION;
	}

	string strUsage = strFirstLine + "\r\n";
	strUsage += "This application requires 1 argument\r\n";
	strUsage += "SYNTAX: EmailFile <Address(es)|/?> [FileToSend] [options] \r\n";
	strUsage += "Address(es): Email address(es) to send the file.\r\n";
	strUsage += "/? - Display the usage message.\r\n";
	strUsage += "    Multiple addresses should be separated by either ; or ,\r\n";
	strUsage += " <options>:\r\n";
	strUsage += "    /c - Configure email settings\r\n";
	strUsage += "    /subject <subject line> - Subject line for the email.\r\n";
	strUsage += "    /body <FileContainingText> - The file containing the body of the message.\r\n";
	strUsage += "    /client - Opens the email in the client application.\r\n";
	strUsage += "    /z - Zip the file that is being sent before emailing\r\n";
	strUsage += "        Note: This is ignored if no file is specified.\r\n";
	strUsage += "    /ef <exception log file> - File for logging exceptions.\r\n";
	strUsage +=  "\r\n";	
	strUsage += "FileToSend: Name of the file to send by email.\r\n";
	strUsage +=  "\r\n";
	strUsage += "If this application is run and the SMTP server has not been setup,\r\n";
	strUsage += "the UI for entering the SMTP server will be displayed\r\n";

	// display the message
	MessageBox(NULL, strUsage.c_str(), strCaption.c_str(), uiType);
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
	m_strBody = "";
	m_bZipFile = false;
	m_bConfigureSettings = false;
	m_bShowInClient = false;

	// Get the arguments
	for ( int i = 1; i < argc; i++ )
	{
		string strArg = argv[i];

		// Make the argument lower case for compare
		makeLowerCase(strArg);
		if (strArg == "/?")
		{
			displayUsage();
			return false;
		}
		else if ( strArg == "/c" )
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
		else if (strArg == "/client")
		{
			m_bShowInClient = true;
		}
		else if (strArg == "/body")
		{
			if (i+1 >= argc)
			{
				displayUsage("Missing file containing body of message.");
				return false;
			}
			string strTemp = buildAbsolutePath(argv[++i]);
			vector<string> vecLines = convertFileToLines(strTemp, false);
			size_t nCount = vecLines.size();
			if (nCount > 0)
			{
				m_strBody = vecLines[0];
				for(size_t i = 1; i < nCount; i++)
				{
					m_strBody += "\r\n";
					m_strBody += vecLines[i];
				}
			}
		}
		else if (strArg.find('/') == string::npos )
		{
			// Make sure the File or email addresses have not been specified
			if (!m_strFileToEmail.empty() && !m_strEmailAddress.empty())
			{
				displayUsage("Invalid argument specified: '" + string(argv[i]) + "'.");
				return false;
			}

			string strTemp = argv[i];

			// If no email address yet, assume this is the email address
			if (m_strEmailAddress.empty())
			{
				m_strEmailAddress = strTemp;
			}
			else
			{
				m_strFileToEmail = buildAbsolutePath(strTemp);
			}
		}
		else
		{
			displayUsage("Invalid argument specified: '" + string(argv[i]) + "'.");
			return false;
		}
	}
	// Make sure there is either /c or an email address
	if (m_bConfigureSettings || !m_strEmailAddress.empty())
	{
		return true;
	}
	displayUsage("Missing email address!");
	return false; 
}
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::parseRecipientAddress(vector<string>& rvecRecipients)
{
	rvecRecipients.clear();

	// Parse the email address for ,; separated email addresses
	StringTokenizer::sGetTokens(m_strEmailAddress, gstrEMAIL_ADDRESS_DELIMITERS,
		rvecRecipients, true);

	for(vector<string>::iterator it = rvecRecipients.begin(); it != rvecRecipients.end(); it++)
	{
		trim(*it, " ", " ");

		// Make sure the email address has a @ symbol
		if ( it->find("@") == string::npos )
		{
			UCLIDException ue("ELI25135", "Invalid email address!");
			ue.addDebugInfo("Email address", *it);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CEmailFileApp::getRecipientsAsVariantVector(const vector<string>& vecRecipients)
{

	// Put recipient on list of Recipients
	IVariantVectorPtr ipRecipients(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI25348", ipRecipients != __nullptr );

	for(vector<string>::const_iterator it = vecRecipients.begin();
		it != vecRecipients.end(); it++)
	{
		ipRecipients->PushBack(it->c_str());
	}

	return ipRecipients;
}
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::sendEmail(ISmtpEmailSettingsPtr ipEmailSettings,
	const vector<string>& vecRecipients, const string& strFile)
{
	// Email Message 
	IExtractEmailMessagePtr ipMessage(CLSID_ExtractEmailMessage);
	ASSERT_RESOURCE_ALLOCATION("ELI12603", ipMessage != __nullptr );

	ipMessage->EmailSettings = ipEmailSettings;

	if (!strFile.empty())
	{
		// Put file to send on list of files to be sent
		IVariantVectorPtr ipFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12604", ipFiles != __nullptr );

		ipFiles->PushBack(strFile.c_str());

		// Add file list to message
		ipMessage->Attachments = ipFiles;
	}

	// Add Recipients list to email message
	ipMessage->Recipients = getRecipientsAsVariantVector(vecRecipients);

	// Set the subject of the email
	if (m_strSubject.empty() && !strFile.empty())
	{
		// Create a subject line
		m_strSubject = "File: " + strFile;
	}
	ipMessage->Subject = m_strSubject.c_str();

	// If there is a message body, add it
	if (!m_strBody.empty())
	{
		ipMessage->Body = m_strBody.c_str();
	}

	// Send  the message
	ipMessage->Send();
}
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::showInClient(ISmtpEmailSettingsPtr ipSettings,
	const vector<string>& vecRecipients, const string& strFile)
{
	HINSTANCE hMapidll = NULL;
	try
	{
		try
		{
			hMapidll = LoadLibrary("MAPI32.dll");
			if (hMapidll == NULL)
			{
				UCLIDException uex("ELI32297", "Failed to load MAPI library.");
				uex.addWin32ErrorInfo();
				throw uex;
			}

			size_t nRecipCount = vecRecipients.size();

			MapiMessage message;
			ZeroMemory(&message, sizeof(MapiMessage));

			// Get the senders
			auto strSender = asString(ipSettings->SenderName);
			auto strSenderAddress = asString(ipSettings->SenderAddress);
			auto sender = buildMapiRecipient(true, strSender, strSenderAddress);
			message.lpOriginator = &sender;

			unique_ptr<MapiRecipDesc[]> pRecipients;
			if (nRecipCount > 0)
			{
				pRecipients.reset(new MapiRecipDesc[vecRecipients.size()]);
				ASSERT_RESOURCE_ALLOCATION("ELI32294", pRecipients.get() != __nullptr);
				for(size_t i=0; i < nRecipCount; i++)
				{
					const string& strTemp = vecRecipients[i];
					pRecipients[i] = buildMapiRecipient(false, strTemp, strTemp);
				}

				message.nRecipCount = nRecipCount;
				message.lpRecips = pRecipients.get();
			}

			if (!m_strSubject.empty())
			{
				message.lpszSubject = (char*) m_strSubject.c_str();
			}

			if (!m_strBody.empty())
			{
				message.lpszNoteText = (char*) m_strBody.c_str();
			}

			// Need this declared outside the if scope so that it still exists after the if statement
			MapiFileDesc file;

			if (!strFile.empty())
			{
				ZeroMemory(&file, sizeof(MapiFileDesc));
				file.nPosition = -1;
				file.lpszPathName = (char*) strFile.c_str();
				message.nFileCount = 1;
				message.lpFiles = &file;
			}

			LPMAPISENDMAIL mapiSendMail = (LPMAPISENDMAIL) GetProcAddress(hMapidll, "MAPISendMail");

			auto result = mapiSendMail(0, 0, &message, MAPI_DIALOG, 0);
			if (result != SUCCESS_SUCCESS && result != MAPI_E_USER_ABORT)
			{
				UCLIDException uex("ELI32295", "Failed while attempting to open message in email client.");
				uex.addDebugInfo("Error Code", result);
				uex.addDebugInfo("Error String", getMapiErrorCodeAsString(result));
				throw uex;
			}

			FreeLibrary(hMapidll);
			hMapidll = NULL;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32296");
	}
	catch(UCLIDException& uex)
	{
		if (hMapidll != NULL)
		{
			FreeLibrary(hMapidll);
		}
		
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
MapiRecipDesc CEmailFileApp::buildMapiRecipient(bool bSender,
	const string& strName, const string& strAddress)
{
	MapiRecipDesc recipient;
	ZeroMemory(&recipient, sizeof(MapiRecipDesc));
	recipient.lpszName = (char*) strName.c_str();
	recipient.lpszAddress = (char*) strAddress.c_str();
	recipient.ulRecipClass = bSender ? MAPI_ORIG : MAPI_TO;

	return recipient;
}
//-------------------------------------------------------------------------------------------------
string CEmailFileApp::getMapiErrorCodeAsString(ULONG ulErrorCode)
{
	switch(ulErrorCode)
	{
	case MAPI_USER_ABORT:
		return "MAPI_USER_ABORT";
	case MAPI_E_FAILURE:
		return "MAPI_E_FAILURE";
	case MAPI_E_LOGON_FAILURE:
		return "MAPI_E_LOGON_FAILURE";
	case MAPI_E_DISK_FULL:
		return "MAPI_E_DISK_FULL";
	case MAPI_E_INSUFFICIENT_MEMORY:
		return "MAPI_E_INSUFFICIENT_MEMORY";
	case MAPI_E_ACCESS_DENIED:
		return "MAPI_E_ACCESS_DENIED";
	case MAPI_E_TOO_MANY_SESSIONS:
		return "MAPI_E_TOO_MANY_SESSIONS";
	case MAPI_E_TOO_MANY_FILES:
		return "MAPI_E_TOO_MANY_FILES";
	case MAPI_E_TOO_MANY_RECIPIENTS:
		return "MAPI_E_TOO_MANY_RECIPIENTS";
	case MAPI_E_ATTACHMENT_NOT_FOUND: 
		return "MAPI_E_ATTACHMENT_NOT_FOUND";
	case MAPI_E_ATTACHMENT_OPEN_FAILURE: 
		return "MAPI_E_ATTACHMENT_OPEN_FAILURE";
	case MAPI_E_ATTACHMENT_WRITE_FAILURE:
		return "MAPI_E_ATTACHMENT_WRITE_FAILURE";
	case MAPI_E_UNKNOWN_RECIPIENT: 
		return "MAPI_E_UNKNOWN_RECIPIENT";
	case MAPI_E_BAD_RECIPTYPE: 
		return "MAPI_E_BAD_RECIPTYPE";
	case MAPI_E_NO_MESSAGES: 
		return "MAPI_E_NO_MESSAGES";
	case MAPI_E_INVALID_MESSAGE: 
		return "MAPI_E_INVALID_MESSAGE";
	case MAPI_E_TEXT_TOO_LARGE: 
		return "MAPI_E_TEXT_TOO_LARGE";
	case MAPI_E_INVALID_SESSION: 
		return "MAPI_E_INVALID_SESSION";
	case MAPI_E_TYPE_NOT_SUPPORTED: 
		return "MAPI_E_TYPE_NOT_SUPPORTED";
	case MAPI_E_AMBIGUOUS_RECIPIENT: 
		return "MAPI_E_AMBIGUOUS_RECIPIENT";
	case MAPI_E_MESSAGE_IN_USE: 
		return "MAPI_E_MESSAGE_IN_USE";
	case MAPI_E_NETWORK_FAILURE: 
		return "MAPI_E_NETWORK_FAILURE";
	case MAPI_E_INVALID_EDITFIELDS: 
		return "MAPI_E_INVALID_EDITFIELDS";
	case MAPI_E_INVALID_RECIPS: 
		return "MAPI_E_INVALID_RECIPS";
	case MAPI_E_NOT_SUPPORTED: 
		return "MAPI_E_NOT_SUPPORTED";
	default:
		return "Unrecognized Code";
	}
}
//-------------------------------------------------------------------------------------------------
void CEmailFileApp::validateLicense()
{
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI15248", "Email File" );
}
//-------------------------------------------------------------------------------------------------
