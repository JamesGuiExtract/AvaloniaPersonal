// ConvertFAMDB.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CConvertFAMDBApp:
// See ConvertFAMDB.cpp for the implementation of this class
//

class CConvertFAMDBApp : public CWinApp
{
public:
	CConvertFAMDBApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
private:

	void validateLicense();
};

extern CConvertFAMDBApp theApp;