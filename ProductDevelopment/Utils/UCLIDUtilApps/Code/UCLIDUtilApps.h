// UCLIDUtilApps.h : main header file for the UCLIDUTILAPPS application
//

#if !defined(AFX_UCLIDUTILAPPS_H__8EB23976_B3DA_4B38_9F50_392A577E7549__INCLUDED_)
#define AFX_UCLIDUTILAPPS_H__8EB23976_B3DA_4B38_9F50_392A577E7549__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CUCLIDUtilAppsApp:
// See UCLIDUtilApps.cpp for the implementation of this class
//

class CUCLIDUtilAppsApp : public CWinApp
{
public:
	CUCLIDUtilAppsApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDUtilAppsApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CUCLIDUtilAppsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UCLIDUTILAPPS_H__8EB23976_B3DA_4B38_9F50_392A577E7549__INCLUDED_)
