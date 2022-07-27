// ImageFormatConverter.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

// CImageFormatConverterApp:
// See ImageFormatConverter.cpp for the implementation of this class
//

class CImageFormatConverterApp : public CWinApp
{
public:
	CImageFormatConverterApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CImageFormatConverterApp theApp;

IImageFormatConverterPtr getImageFormatConverter();
