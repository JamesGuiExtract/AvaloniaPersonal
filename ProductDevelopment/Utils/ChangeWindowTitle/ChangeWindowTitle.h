// ChangeWindowTitle.h : main header file for the CHANGEWINDOWTITLE application
//

#if !defined(AFX_CHANGEWINDOWTITLE_H__8095EC9A_1B0D_4FC9_92C9_6DB4B00FFBB2__INCLUDED_)
#define AFX_CHANGEWINDOWTITLE_H__8095EC9A_1B0D_4FC9_92C9_6DB4B00FFBB2__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CChangeWindowTitleApp:
// See ChangeWindowTitle.cpp for the implementation of this class
//

class CChangeWindowTitleApp : public CWinApp
{
public:
	CChangeWindowTitleApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CChangeWindowTitleApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CChangeWindowTitleApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CHANGEWINDOWTITLE_H__8095EC9A_1B0D_4FC9_92C9_6DB4B00FFBB2__INCLUDED_)
