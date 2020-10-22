// ShrinkLargePages.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CShrinkLargePagesApp:
// See ShrinkLargePages.cpp for the implementation of this class
//

class CShrinkLargePagesApp : public CWinApp
{
public:
	CShrinkLargePagesApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CShrinkLargePagesApp theApp;