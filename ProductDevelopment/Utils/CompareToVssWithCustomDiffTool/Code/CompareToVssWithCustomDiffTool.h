// CompareToVssWithCustomDiffTool.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols
#include "DiffStatusDlg.h"

//--------------------------------------------------------------------------------------------------
// CCompareToVssWithCustomDiffToolApp:
// See CompareToVssWithCustomDiffTool.cpp for the implementation of this class
//--------------------------------------------------------------------------------------------------
class CCompareToVssWithCustomDiffToolApp : public CWinApp
{
public:
	CCompareToVssWithCustomDiffToolApp();

	// Overrides
	virtual BOOL InitInstance();

private:
	DECLARE_MESSAGE_MAP()

	DiffStatusDlg m_diffStatusDlg;
};
//--------------------------------------------------------------------------------------------------

extern CCompareToVssWithCustomDiffToolApp theApp;