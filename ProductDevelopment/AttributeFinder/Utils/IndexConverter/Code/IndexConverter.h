// IndexConverter.h : main header file for the INDEXCONVERTER application
//

#if !defined(AFX_INDEXCONVERTER_H__F9713DC1_BA0C_439E_9C16_A01588C28856__INCLUDED_)
#define AFX_INDEXCONVERTER_H__F9713DC1_BA0C_439E_9C16_A01588C28856__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CIndexConverterApp:
// See IndexConverter.cpp for the implementation of this class
//

class CIndexConverterApp : public CWinApp
{
public:
	CIndexConverterApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIndexConverterApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CIndexConverterApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Checks license
	void validateLicense();
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_INDEXCONVERTER_H__F9713DC1_BA0C_439E_9C16_A01588C28856__INCLUDED_)
