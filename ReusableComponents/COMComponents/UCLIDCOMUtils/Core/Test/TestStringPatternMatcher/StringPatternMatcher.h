// StringPatternMatcher.h : main header file for the STRINGPATTERNMATCHER application
//

#if !defined(AFX_STRINGPATTERNMATCHER_H__A7CF8BD7_4743_4F15_BF48_5698FE524131__INCLUDED_)
#define AFX_STRINGPATTERNMATCHER_H__A7CF8BD7_4743_4F15_BF48_5698FE524131__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CStringPatternMatcherApp:
// See StringPatternMatcher.cpp for the implementation of this class
//

class CStringPatternMatcherApp : public CWinApp
{
public:
	CStringPatternMatcherApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CStringPatternMatcherApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CStringPatternMatcherApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STRINGPATTERNMATCHER_H__A7CF8BD7_4743_4F15_BF48_5698FE524131__INCLUDED_)
