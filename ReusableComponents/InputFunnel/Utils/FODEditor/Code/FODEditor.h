// FODEditor.h : main header file for the FODEDITOR application
//

#if !defined(AFX_FODEDITOR_H__97D59F71_2A32_4598_9347_0A193F69AB4A__INCLUDED_)
#define AFX_FODEDITOR_H__97D59F71_2A32_4598_9347_0A193F69AB4A__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CFODEditorApp:
// See FODEditor.cpp for the implementation of this class
//

class CFODEditorApp : public CWinApp
{
public:
	CFODEditorApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFODEditorApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CFODEditorApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FODEDITOR_H__97D59F71_2A32_4598_9347_0A193F69AB4A__INCLUDED_)
