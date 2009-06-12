// ShowStringCharPosition.h : main header file for the SHOWSTRINGCHARPOSITION application
//

#if !defined(AFX_SHOWSTRINGCHARPOSITION_H__7C9B7788_F5A5_4F63_9295_40013738BA84__INCLUDED_)
#define AFX_SHOWSTRINGCHARPOSITION_H__7C9B7788_F5A5_4F63_9295_40013738BA84__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CShowStringCharPositionApp:
// See ShowStringCharPosition.cpp for the implementation of this class
//

class CShowStringCharPositionApp : public CWinApp
{
public:
	CShowStringCharPositionApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CShowStringCharPositionApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CShowStringCharPositionApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SHOWSTRINGCHARPOSITION_H__7C9B7788_F5A5_4F63_9295_40013738BA84__INCLUDED_)
