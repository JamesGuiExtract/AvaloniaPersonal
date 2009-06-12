// RunRules.h : main header file for the RUNRULES application
//

#if !defined(AFX_RUNRULES_H__86E49EB3_0773_4BDB_86F7_F5F91C2B9A31__INCLUDED_)
#define AFX_RUNRULES_H__86E49EB3_0773_4BDB_86F7_F5F91C2B9A31__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CRunRulesApp:
// See RunRules.cpp for the implementation of this class
//

class CRunRulesApp : public CWinApp
{
public:
	CRunRulesApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRunRulesApp)
	public:
	virtual BOOL InitInstance();
	virtual int Run();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CRunRulesApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_RUNRULES_H__86E49EB3_0773_4BDB_86F7_F5F91C2B9A31__INCLUDED_)
