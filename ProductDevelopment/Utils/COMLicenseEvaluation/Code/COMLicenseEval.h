// COMLicenseEval.h : main header file for the COMLICENSEEVAL application
//

#if !defined(AFX_COMLICENSEEVAL_H__94C70165_D869_46B6_B362_7B507207A11E__INCLUDED_)
#define AFX_COMLICENSEEVAL_H__94C70165_D869_46B6_B362_7B507207A11E__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseEvalApp:
// See COMLicenseEval.cpp for the implementation of this class
//

class CCOMLicenseEvalApp : public CWinApp
{
public:
	CCOMLicenseEvalApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCOMLicenseEvalApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CCOMLicenseEvalApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_COMLICENSEEVAL_H__94C70165_D869_46B6_B362_7B507207A11E__INCLUDED_)
