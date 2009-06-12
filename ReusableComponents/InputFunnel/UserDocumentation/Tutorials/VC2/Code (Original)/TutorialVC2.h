// TutorialVC2.h : main header file for the TUTORIAlVC2 application
//

#if !defined(AFX_TUTORIALVC2_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_)
#define AFX_TUTORIALVC2_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC2App:
// See TutorialVC2.cpp for the implementation of this class
//

class CTutorialVC2App : public CWinApp
{
public:
	CTutorialVC2App();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTutorialVC2App)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTutorialVC2App)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC2_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_)
