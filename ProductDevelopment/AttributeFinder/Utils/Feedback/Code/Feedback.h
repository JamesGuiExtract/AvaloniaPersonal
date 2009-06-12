// Feedback.h : main header file for the FEEDBACK application
//

#if !defined(AFX_FEEDBACK_H__4CEEC398_57B3_4EA7_8B53_C2D4CC1B549F__INCLUDED_)
#define AFX_FEEDBACK_H__4CEEC398_57B3_4EA7_8B53_C2D4CC1B549F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CFeedbackApp:
// See Feedback.cpp for the implementation of this class
//

class CFeedbackApp : public CWinApp
{
	typedef enum EFeedbackDialogSelected
	{
		kNoDialog,
		kConfigure,
		kPackage,
		kChoice,
		kUnpackage,
	}	EFeedbackDialogSelected;

public:
	CFeedbackApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFeedbackApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CFeedbackApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FEEDBACK_H__4CEEC398_57B3_4EA7_8B53_C2D4CC1B549F__INCLUDED_)
