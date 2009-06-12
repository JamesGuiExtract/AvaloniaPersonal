// ImageCleanupSettingsEditor.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols


// CImageCleanupSettingsEditorApp:
// See ImageCleanupSettingsEditor.cpp for the implementation of this class
//

class CImageCleanupSettingsEditorApp : public CWinApp
{
public:
	CImageCleanupSettingsEditorApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CImageCleanupSettingsEditorApp theApp;