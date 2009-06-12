// GetNextELICode.h : main header file for the GETNEXTELICODE application
//

#if !defined(AFX_GETNEXTELICODE_H__DB35F766_392F_4680_B16F_66F8F1CC5637__INCLUDED_)
#define AFX_GETNEXTELICODE_H__DB35F766_392F_4680_B16F_66F8F1CC5637__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CGetNextELICodeApp:
// See GetNextELICode.cpp for the implementation of this class
//

class CGetNextELICodeApp : public CWinApp
{
public:
	CGetNextELICodeApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGetNextELICodeApp)
	public:
	virtual BOOL InitInstance();
	virtual BOOL OnCmdMsg(UINT nID, int nCode, void* pExtra, AFX_CMDHANDLERINFO* pHandlerInfo);
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CGetNextELICodeApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_GETNEXTELICODE_H__DB35F766_392F_4680_B16F_66F8F1CC5637__INCLUDED_)
