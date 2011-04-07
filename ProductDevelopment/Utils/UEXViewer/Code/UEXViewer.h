// UEXViewer.h : main header file for the UEXVIEWER application
//

#if !defined(AFX_UEXVIEWER_H__E57508BF_C589_4401_ABBC_CCB904063427__INCLUDED_)
#define AFX_UEXVIEWER_H__E57508BF_C589_4401_ABBC_CCB904063427__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CUEXViewerApp:
// See UEXViewer.cpp for the implementation of this class
//

class CUEXViewerApp : public CWinApp
{
public:
	CUEXViewerApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUEXViewerApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CUEXViewerApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	// A temporary .uex file that should be deleted on exit.
	string m_strTemporaryFile;
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_UEXVIEWER_H__E57508BF_C589_4401_ABBC_CCB904063427__INCLUDED_)
