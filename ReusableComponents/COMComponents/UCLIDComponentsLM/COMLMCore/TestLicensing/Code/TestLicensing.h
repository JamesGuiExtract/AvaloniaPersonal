// TestLicensing.h : main header file for the TESTLICENSING application
//

#if !defined(AFX_TESTLICENSING_H__EB05E655_2A73_4548_84C7_42429B69CDA0__INCLUDED_)
#define AFX_TESTLICENSING_H__EB05E655_2A73_4548_84C7_42429B69CDA0__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTestLicensingApp:
// See TestLicensing.cpp for the implementation of this class
//

class CTestLicensingApp : public CWinApp
{
public:
	CTestLicensingApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestLicensingApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTestLicensingApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTLICENSING_H__EB05E655_2A73_4548_84C7_42429B69CDA0__INCLUDED_)
