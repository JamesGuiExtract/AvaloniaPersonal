// SimpleMDIFramework.h : main header file for the SIMPLEMDIFRAMEWORK application
//

#if !defined(AFX_SIMPLEMDIFRAMEWORK_H__9134C930_CDD8_4210_9B4B_4AEAFF1D1F0D__INCLUDED_)
#define AFX_SIMPLEMDIFRAMEWORK_H__9134C930_CDD8_4210_9B4B_4AEAFF1D1F0D__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkApp:
// See SimpleMDIFramework.cpp for the implementation of this class
//

class CSimpleMDIFrameworkApp : public CWinApp
{
public:
	CSimpleMDIFrameworkApp();
	~CSimpleMDIFrameworkApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSimpleMDIFrameworkApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation
	COleTemplateServer m_server;
		// Server object for document creation
	//{{AFX_MSG(CSimpleMDIFrameworkApp)
	afx_msg void OnAppAbout();
	afx_msg void OnIcomap();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SIMPLEMDIFRAMEWORK_H__9134C930_CDD8_4210_9B4B_4AEAFF1D1F0D__INCLUDED_)
