// FAMDBAdmin.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CFAMDBAdminApp:
// See FAMDBAdmin.cpp for the implementation of this class
//

class CFAMDBAdminApp : public CWinApp
{
public:
	CFAMDBAdminApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
private:

	void validateLicense();
};

extern CFAMDBAdminApp theApp;