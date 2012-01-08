// ImageFormatConverter.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

// Releases memory allocated by RecAPI (Nuance) calls. Create this object after the RecAPI call has
// allocated space for the object. MemoryType is the data type of the object to release when 
// RecMemoryReleaser goes out of scope.
template<typename MemoryType>
class RecMemoryReleaser
{
public:
	RecMemoryReleaser(MemoryType* pMemoryType);
	~RecMemoryReleaser();

private:
	MemoryType* m_pMemoryType;
};


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
