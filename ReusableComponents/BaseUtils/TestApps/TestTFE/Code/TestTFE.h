// TestTFE.h : main header file for the TESTTFE application
//

#if !defined(AFX_TESTTFE_H__1E13AE49_3487_4848_BE19_4C6558528110__INCLUDED_)
#define AFX_TESTTFE_H__1E13AE49_3487_4848_BE19_4C6558528110__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTestTFEApp:
// See TestTFE.cpp for the implementation of this class
//

class CTestTFEApp : public CWinApp
{
public:
	CTestTFEApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestTFEApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTestTFEApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTTFE_H__1E13AE49_3487_4848_BE19_4C6558528110__INCLUDED_)
