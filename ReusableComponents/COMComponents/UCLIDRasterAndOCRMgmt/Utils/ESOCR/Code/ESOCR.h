// ESOCR.h : main header file for the ESOCR application
//

#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CESOCRApp:
// See ESOCR.cpp for the implementation of this class
//

class CESOCRApp : public CWinApp
{
public:
	CESOCRApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CESOCRApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CESOCRApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	bool			m_bNoError;

	// OCR engine
	IOCREnginePtr	m_ipOCREngine;

	// OCRUtils object
	IOCRUtilsPtr	m_ipOCRUtils;

	// Displays a usage message and sets the no error flag to false
	void displayUsage();

	// Create OCR engine and initialize private license
	void	prepareOCREngine();

	// Throws exception if:
	//    multi-page image
	//    image height > 8.0 inches
	//    image width  > 8.5 inches
	void	validateImage(const std::string& strImagePath);
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
