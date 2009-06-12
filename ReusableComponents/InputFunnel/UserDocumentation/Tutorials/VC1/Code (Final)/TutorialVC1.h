// TutorialVC1.h : main header file for the TUTORIALVC1 application
//

#if !defined(AFX_TUTORIALVC1_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_)
#define AFX_TUTORIALVC1_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC1App:
// See TutorialVC1.cpp for the implementation of this class
//

class CTutorialVC1App : public CWinApp
{
public:
	CTutorialVC1App();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTutorialVC1App)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTutorialVC1App)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TUTORIALVC1_H__FB3F6595_7DD0_46F6_A425_3A9A659EE4D1__INCLUDED_)
