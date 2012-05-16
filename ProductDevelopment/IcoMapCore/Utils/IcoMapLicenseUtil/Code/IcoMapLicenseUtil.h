// IcoMapLicenseUtil.h : main header file for the ICOMAPLICENSEUTIL application
//

#if !defined(AFX_ICOMAPLICENSEUTIL_H__A2B1A9CF_AA8C_4E90_8D26_60B4CC9DE470__INCLUDED_)
#define AFX_ICOMAPLICENSEUTIL_H__A2B1A9CF_AA8C_4E90_8D26_60B4CC9DE470__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CIcoMapLicenseUtilApp:
// See IcoMapLicenseUtil.cpp for the implementation of this class
//

class CIcoMapLicenseUtilApp : public CWinApp
{
public:
	CIcoMapLicenseUtilApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIcoMapLicenseUtilApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CIcoMapLicenseUtilApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_ICOMAPLICENSEUTIL_H__A2B1A9CF_AA8C_4E90_8D26_60B4CC9DE470__INCLUDED_)
