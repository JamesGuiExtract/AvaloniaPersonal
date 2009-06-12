// Licensing.h : main header file for the Extract.Licensing DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CLicensingApp
// See Licensing.cpp for the implementation of this class
//

class CLicensingApp : public CWinApp
{
public:
	CLicensingApp();

// Overrides
public:
	virtual BOOL InitInstance();

	DECLARE_MESSAGE_MAP()
};
