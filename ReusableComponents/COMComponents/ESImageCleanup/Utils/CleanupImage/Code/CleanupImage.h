// CleanupImage.h : main header file for the CleanupImage application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CCleanupImageApp:
// See CleanupImage.cpp for the implementation of this class
//

class CCleanupImageApp : public CWinApp
{
public:
	CCleanupImageApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CCleanupImageApp theApp;