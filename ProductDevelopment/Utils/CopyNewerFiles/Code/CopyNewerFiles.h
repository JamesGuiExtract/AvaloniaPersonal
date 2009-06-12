// CopyNewerFiles.h : main header file for the COPYNEWERFILES application
//

#if !defined(AFX_COPYNEWERFILES_H__CF173B70_BECB_4021_AC41_6BBE456666BC__INCLUDED_)
#define AFX_COPYNEWERFILES_H__CF173B70_BECB_4021_AC41_6BBE456666BC__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CCopyNewerFilesApp:
// See CopyNewerFiles.cpp for the implementation of this class
//

class CCopyNewerFilesApp : public CWinApp
{
public:
	CCopyNewerFilesApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CCopyNewerFilesApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CCopyNewerFilesApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_COPYNEWERFILES_H__CF173B70_BECB_4021_AC41_6BBE456666BC__INCLUDED_)
