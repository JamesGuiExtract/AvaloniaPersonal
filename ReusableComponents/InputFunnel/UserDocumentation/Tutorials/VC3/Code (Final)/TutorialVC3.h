// TutorialVC3.h : main header file for the TUTORIALVC3 application
//

#if !defined(AFX_TUTORIALVC3_H__D6C56845_7EA4_4B2D_8979_C3B2147F3196__INCLUDED_)
#define AFX_TUTORIALVC3_H__D6C56845_7EA4_4B2D_8979_C3B2147F3196__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC3App:
// See TutorialVC3.cpp for the implementation of this class
//

class CTutorialVC3App : public CWinApp
{
public:
	CTutorialVC3App();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTutorialVC3App)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTutorialVC3App)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC3_H__D6C56845_7EA4_4B2D_8979_C3B2147F3196__INCLUDED_)
