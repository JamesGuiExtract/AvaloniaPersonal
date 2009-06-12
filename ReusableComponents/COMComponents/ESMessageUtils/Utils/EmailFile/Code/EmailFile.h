// EmailFile.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#include <string>

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

	bool m_bZipFile;

	bool m_bConfigureSettings;

	// Methods

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Displays usage information for this executable.
	void displayUsage(const string& strErrorMessage);
	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the member variables based on the command line arguments. If there are missing
	// arguments a usage window will be displayed if the /ef switch has not been parsed or has a 
	// missing file name.
	bool getAndValidateArguments(int argc, char* argv[]);
	
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns IVariantVectorPtr that contains all of the Email addresses in the
	// m_strEmailAddress member variable.
	IVariantVectorPtr CEmailFileApp::parseRecipientAddress();

	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this executable is unlicensed. Returns true otherwise.
	static void validateLicense();
};

extern CEmailFileApp theApp;