// ESPrintManager.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#include <RegistryPersistenceMgr.h>

#include <string>
#include <memory>

using namespace std;

struct PrintedImageResults
{
	string ImageFile;
	string OriginalDocument;
};

// CESPrintManagerApp:
// See ESPrintManager.cpp for the implementation of this class
//
class CESPrintManagerApp : public CWinApp
{
public:
	CESPrintManagerApp();
	~CESPrintManagerApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()

private:
	// The application to launch after processing the INI file
	string m_strLaunchApplication;

	// Command line arguments for the application that will be launched
	string m_strCommandLineArgs;

	string m_strPrintedINIFile;
	auto_ptr<IConfigurationSettingsPersistenceMgr> m_apUserCfgMgr;

	//----------------------------------------------------------------------------------------------
	// methods
	//----------------------------------------------------------------------------------------------
	// Processes the INI file
	PrintedImageResults processPrintedINIFile();
	//----------------------------------------------------------------------------------------------
	// Launches the specified application with the tif image specified
	// in the INI file
	void launchApplication(const PrintedImageResults& results);
	//----------------------------------------------------------------------------------------------
	// Reads the Application and ApplicationArgs key from the registry to know what application
	// to launch after processing the INI file.  Will throw an exception if the Application
	// key is empty or if either the Application or ApplicationArgs key does not exist.
	void readSettingsFromRegistry();
};

extern CESPrintManagerApp theApp;