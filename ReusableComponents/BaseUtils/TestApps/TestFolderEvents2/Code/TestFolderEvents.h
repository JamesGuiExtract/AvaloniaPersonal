// TestFolderEvents.h : main header file for the TESTFOLDEREVENTS application
//

#if !defined(AFX_TESTFOLDEREVENTS_H__807ABD13_DFC1_44E4_80E3_D8AC89128AE7__INCLUDED_)
#define AFX_TESTFOLDEREVENTS_H__807ABD13_DFC1_44E4_80E3_D8AC89128AE7__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTestFolderEventsApp:
// See TestFolderEvents.cpp for the implementation of this class
//

class CTestFolderEventsApp : public CWinApp
{
public:
	CTestFolderEventsApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestFolderEventsApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTestFolderEventsApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTFOLDEREVENTS_H__807ABD13_DFC1_44E4_80E3_D8AC89128AE7__INCLUDED_)
