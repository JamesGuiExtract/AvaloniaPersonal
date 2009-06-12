// USBLicenseKeyManager.h : main header file for the USBLICENSEKEYMANAGER application
//

#if !defined(AFX_USBLICENSEKEYMANAGER_H__DFD0E94D_904E_4851_A3FB_B8B461224A02__INCLUDED_)
#define AFX_USBLICENSEKEYMANAGER_H__DFD0E94D_904E_4851_A3FB_B8B461224A02__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CUSBLicenseKeyManagerApp:
// See USBLicenseKeyManager.cpp for the implementation of this class
//

class CUSBLicenseKeyManagerApp : public CWinApp
{
public:
	CUSBLicenseKeyManagerApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUSBLicenseKeyManagerApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CUSBLicenseKeyManagerApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_USBLICENSEKEYMANAGER_H__DFD0E94D_904E_4851_A3FB_B8B461224A02__INCLUDED_)
