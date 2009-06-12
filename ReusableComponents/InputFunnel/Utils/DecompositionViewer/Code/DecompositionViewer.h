// DecompositionViewer.h : main header file for the DECOMPOSITIONVIEWER application
//

#if !defined(AFX_DECOMPOSITIONVIEWER_H__EEAE2FD5_6158_43C3_9E10_E4F6BECB12A9__INCLUDED_)
#define AFX_DECOMPOSITIONVIEWER_H__EEAE2FD5_6158_43C3_9E10_E4F6BECB12A9__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CDecompositionViewerApp:
// See DecompositionViewer.cpp for the implementation of this class
//

class CDecompositionViewerApp : public CWinApp
{
public:
	CDecompositionViewerApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDecompositionViewerApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CDecompositionViewerApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DECOMPOSITIONVIEWER_H__EEAE2FD5_6158_43C3_9E10_E4F6BECB12A9__INCLUDED_)
