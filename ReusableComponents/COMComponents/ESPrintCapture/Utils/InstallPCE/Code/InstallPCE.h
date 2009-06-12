// InstallPCE.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols
#include "Win32Event.h"

// CInstallPCEApp:
// See InstallPCE.cpp for the implementation of this class
//

class CInstallPCEApp : public CWinApp
{
public:
	CInstallPCEApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()

private:

	/////////////
	// Variables
	/////////////

	// An event to signal that the printer install process is complete
	static Win32Event ms_eventPrinterInstalled;

	/////////////
	// Methods
	/////////////

	// As a temporary fix for [IDSD:301], watches and closes the ActMask default printer prompt 
	// by answering "No" to not make ID Shield the default printer.
	static UINT closeDefaultPrinterPrompt(LPVOID pData);
};

extern CInstallPCEApp theApp;