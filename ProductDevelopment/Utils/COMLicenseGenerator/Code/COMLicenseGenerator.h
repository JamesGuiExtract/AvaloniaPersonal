// COMLicenseGenerator.h : main header file for the COMLICENSEGENERATOR application
//

#if !defined(AFX_COMLICENSEGENERATOR_H__E172DB46_486F_48E5_8A23_C94DB61A7DDB__INCLUDED_)
#define AFX_COMLICENSEGENERATOR_H__E172DB46_486F_48E5_8A23_C94DB61A7DDB__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CCOMLicenseGeneratorApp:
// See COMLicenseGenerator.cpp for the implementation of this class
//

class CCOMLicenseGeneratorApp : public CWinApp
{
public:
	CCOMLicenseGeneratorApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCOMLicenseGeneratorApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CCOMLicenseGeneratorApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_COMLICENSEGENERATOR_H__E172DB46_486F_48E5_8A23_C94DB61A7DDB__INCLUDED_)
