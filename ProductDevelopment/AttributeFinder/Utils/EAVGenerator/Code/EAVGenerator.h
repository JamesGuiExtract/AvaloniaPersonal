// EAVGenerator.h : main header file for the EAVGENERATOR application
//

#if !defined(AFX_EAVGENERATOR_H__1EBE6171_2418_4A0C_8EB1_283CBB73565D__INCLUDED_)
#define AFX_EAVGENERATOR_H__1EBE6171_2418_4A0C_8EB1_283CBB73565D__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CEAVGeneratorApp:
// See EAVGenerator.cpp for the implementation of this class
//

class CEAVGeneratorApp : public CWinApp
{
public:
	CEAVGeneratorApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CEAVGeneratorApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CEAVGeneratorApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_EAVGENERATOR_H__1EBE6171_2418_4A0C_8EB1_283CBB73565D__INCLUDED_)
