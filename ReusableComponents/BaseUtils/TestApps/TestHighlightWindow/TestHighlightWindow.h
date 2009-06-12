// TestHighlightWindow.h : main header file for the TESTHIGHLIGHTWINDOW application
//

#if !defined(AFX_TESTHIGHLIGHTWINDOW_H__EF192B1E_41C7_4A19_8B95_243765821DF5__INCLUDED_)
#define AFX_TESTHIGHLIGHTWINDOW_H__EF192B1E_41C7_4A19_8B95_243765821DF5__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CTestHighlightWindowApp:
// See TestHighlightWindow.cpp for the implementation of this class
//

class CTestHighlightWindowApp : public CWinApp
{
public:
	CTestHighlightWindowApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestHighlightWindowApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CTestHighlightWindowApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTHIGHLIGHTWINDOW_H__EF192B1E_41C7_4A19_8B95_243765821DF5__INCLUDED_)
