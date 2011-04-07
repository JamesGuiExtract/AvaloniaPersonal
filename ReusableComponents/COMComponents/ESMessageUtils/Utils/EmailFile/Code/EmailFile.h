// EmailFile.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#include <string>
#include <vector>
#include <MAPI.h>

using namespace std;
// CEmailFileApp:
// See EmailFile.cpp for the implementation of this class
//

class CEmailFileApp : public CWinApp
{
public:
	CEmailFileApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()

private:
	// Variables
	
	// Used for the name of the exception log file passed with the /ef switch
	string m_strExceptionLog;

	// Used for the subject passed with the /subject
	string m_strSubject;

	// File to email
	string m_strFileToEmail;

	// Email address to send to
	string m_strEmailAddress;

	string m_strBody;

	bool m_bZipFile;

	bool m_bConfigureSettings;

	bool m_bShowInClient;

	// Methods

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Displays usage information for this executable.
	void displayUsage(const string& strErrorMessage = "");
	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the member variables based on the command line arguments. If there are missing
	// arguments a usage window will be displayed if the /ef switch has not been parsed or has a 
	// missing file name.
	bool getAndValidateArguments(int argc, char* argv[]);
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: Fills the vector with all of the Email addresses in the
	// m_strEmailAddress member variable. The addresses will be validated to contain '@' symbol.
	void parseRecipientAddress(vector<string>& rvecRecipients);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To add each entry in the recipient vector to an IVariantVector and return it
	IVariantVectorPtr getRecipientsAsVariantVector(const vector<string>& vecRecipients);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To send an email using the IExtractEmailMessage object
	void sendEmail(ISmtpEmailSettingsPtr ipEmailSettings,
		const vector<string>& vecRecipients, const string& strFile);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To build an email structure using Mapi32.dll structures and display in client
	void showInClient(ISmtpEmailSettingsPtr ipEmailSettings,
		const vector<string>& vecRecipients, const string& strFile);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To fill the MapiRecipDesc struct with the recipient data and return it
	MapiRecipDesc buildMapiRecipient(bool bSender, const string& strName, const string& strAddress);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To convert the mapi error code to a the string representation of the value.
	string getMapiErrorCodeAsString(ULONG ulErrorCode);

	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this executable is unlicensed. Returns true otherwise.
	static void validateLicense();
};

extern CEmailFileApp theApp;