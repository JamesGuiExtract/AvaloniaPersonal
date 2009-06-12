// UserLicense.h : main header file for the USERLICENSE application
//

#if !defined(AFX_USERLICENSE_H__591A9304_5555_4CCE_8738_E9AB07CCDD78__INCLUDED_)
#define AFX_USERLICENSE_H__591A9304_5555_4CCE_8738_E9AB07CCDD78__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CUserLicenseApp:
// See UserLicense.cpp for the implementation of this class
//

class CUserLicenseApp : public CWinApp
{
public:
	CUserLicenseApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUserLicenseApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CUserLicenseApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_USERLICENSE_H__591A9304_5555_4CCE_8738_E9AB07CCDD78__INCLUDED_)
