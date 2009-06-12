// LicenseTimeInfo.h : main header file for the LICENSETIMEINFO application
//

#if !defined(AFX_LICENSETIMEINFO_H__DAA34D44_9078_48FB_8280_0CF021A917F7__INCLUDED_)
#define AFX_LICENSETIMEINFO_H__DAA34D44_9078_48FB_8280_0CF021A917F7__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CLicenseTimeInfoApp:
// See LicenseTimeInfo.cpp for the implementation of this class
//

class CLicenseTimeInfoApp : public CWinApp
{
public:
	CLicenseTimeInfoApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CLicenseTimeInfoApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CLicenseTimeInfoApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_LICENSETIMEINFO_H__DAA34D44_9078_48FB_8280_0CF021A917F7__INCLUDED_)
