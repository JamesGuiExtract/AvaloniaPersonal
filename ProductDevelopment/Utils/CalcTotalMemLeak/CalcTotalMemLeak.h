// CalcTotalMemLeak.h : main header file for the CALCTOTALMEMLEAK application
//

#if !defined(AFX_CALCTOTALMEMLEAK_H__329E76FE_FD3E_466B_92A9_91C7C346A99B__INCLUDED_)
#define AFX_CALCTOTALMEMLEAK_H__329E76FE_FD3E_466B_92A9_91C7C346A99B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CCalcTotalMemLeakApp:
// See CalcTotalMemLeak.cpp for the implementation of this class
//

class CCalcTotalMemLeakApp : public CWinApp
{
public:
	CCalcTotalMemLeakApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCalcTotalMemLeakApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CCalcTotalMemLeakApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CALCTOTALMEMLEAK_H__329E76FE_FD3E_466B_92A9_91C7C346A99B__INCLUDED_)
