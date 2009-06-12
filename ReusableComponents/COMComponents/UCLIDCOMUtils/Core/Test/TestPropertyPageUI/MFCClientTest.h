// MFCClientTest.h : main header file for the MFCCLIENTTEST application
//

#if !defined(AFX_MFCCLIENTTEST_H__C92727C6_FC70_41FD_837E_892E7D1FC221__INCLUDED_)
#define AFX_MFCCLIENTTEST_H__C92727C6_FC70_41FD_837E_892E7D1FC221__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CMFCClientTestApp:
// See MFCClientTest.cpp for the implementation of this class
//

class CMFCClientTestApp : public CWinApp
{
public:
	CMFCClientTestApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMFCClientTestApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CMFCClientTestApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MFCCLIENTTEST_H__C92727C6_FC70_41FD_837E_892E7D1FC221__INCLUDED_)
