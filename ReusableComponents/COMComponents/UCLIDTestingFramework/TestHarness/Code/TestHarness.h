// TestHarness.h : main header file for the TESTHARNESS application
//

#if !defined(AFX_TESTHARNESS_H__ACF715C9_8F56_4CE8_938B_66517D8EAA4E__INCLUDED_)
#define AFX_TESTHARNESS_H__ACF715C9_8F56_4CE8_938B_66517D8EAA4E__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTestHarnessApp:
// See TestHarness.cpp for the implementation of this class
//

class CTestHarnessApp : public CWinApp
{
public:
	CTestHarnessApp();
	~CTestHarnessApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestHarnessApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTestHarnessApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTHARNESS_H__ACF715C9_8F56_4CE8_938B_66517D8EAA4E__INCLUDED_)
