// CPUHog.h : main header file for the CPUHOG application
//

#if !defined(AFX_CPUHOG_H__4F2B8965_7686_4A70_9173_9A8BBD59BC8C__INCLUDED_)
#define AFX_CPUHOG_H__4F2B8965_7686_4A70_9173_9A8BBD59BC8C__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CCPUHogApp:
// See CPUHog.cpp for the implementation of this class
//

class CCPUHogApp : public CWinApp
{
public:
	CCPUHogApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCPUHogApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CCPUHogApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CPUHOG_H__4F2B8965_7686_4A70_9173_9A8BBD59BC8C__INCLUDED_)
