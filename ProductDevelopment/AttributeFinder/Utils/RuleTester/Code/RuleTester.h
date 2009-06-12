// RuleTester.h : main header file for the RULETESTER application
//

#if !defined(AFX_RULETESTER_H__C0E8B9CB_B1B2_45C6_9058_702FD9A0432B__INCLUDED_)
#define AFX_RULETESTER_H__C0E8B9CB_B1B2_45C6_9058_702FD9A0432B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CRuleTesterApp:
// See RuleTester.cpp for the implementation of this class
//

class CRuleTesterApp : public CWinApp
{
public:
	CRuleTesterApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRuleTesterApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CRuleTesterApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_RULETESTER_H__C0E8B9CB_B1B2_45C6_9058_702FD9A0432B__INCLUDED_)
