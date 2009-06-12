// RDTConfig.h : main header file for the RDTCONFIG application
//

#if !defined(AFX_RDTCONFIG_H__B0E287A6_4037_4790_BA24_AEC4B8E0D6A8__INCLUDED_)
#define AFX_RDTCONFIG_H__B0E287A6_4037_4790_BA24_AEC4B8E0D6A8__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CRDTConfigApp:
// See RDTConfig.cpp for the implementation of this class
//

class CRDTConfigApp : public CWinApp
{
public:
	CRDTConfigApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRDTConfigApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CRDTConfigApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_RDTCONFIG_H__B0E287A6_4037_4790_BA24_AEC4B8E0D6A8__INCLUDED_)
