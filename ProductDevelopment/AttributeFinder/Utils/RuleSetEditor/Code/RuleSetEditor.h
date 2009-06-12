// RuleSetEditor.h : main header file for the RULESETEDITOR application
//

#if !defined(AFX_RULESETEDITOR_H__A755B909_D862_4A77_9C3F_F976F7A26ECF__INCLUDED_)
#define AFX_RULESETEDITOR_H__A755B909_D862_4A77_9C3F_F976F7A26ECF__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CRuleSetEditorApp:
// See RuleSetEditor.cpp for the implementation of this class
//

class CRuleSetEditorApp : public CWinApp
{
public:
	CRuleSetEditorApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRuleSetEditorApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CRuleSetEditorApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_RULESETEDITOR_H__A755B909_D862_4A77_9C3F_F976F7A26ECF__INCLUDED_)
