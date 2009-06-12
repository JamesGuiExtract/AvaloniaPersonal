// ConvertFPSFile.h : main header file for the ConvertFPSFile application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CConvertFPSFileApp:
// See ConvertFPSFile.cpp for the implementation of this class
//

class CConvertFPSFileApp : public CWinApp
{
public:
	CConvertFPSFileApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CConvertFPSFileApp theApp;