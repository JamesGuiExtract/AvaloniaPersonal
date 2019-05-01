// ESConvertUSSToTXT.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

//#include <afxmt.h>			// to define CMutex used by SafeNetLicenseMgr
#include <string>

class CESConvertUSSToTXTApp : public CWinApp
{
public:
	CESConvertUSSToTXTApp();

// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation
	DECLARE_MESSAGE_MAP()

	//-------------------------------------------------------------------------------------------------
	// Supported handling options for missing input file
	//-------------------------------------------------------------------------------------------------
	enum EHandlingType
	{
		kHandlingType_Exception,
		kHandlingType_NoOutput,
		kHandlingType_ZeroLengthOutput,
	};


	enum ECounterType {
		kIndexing,
		kPagination,
		kRedaction
	};

private:

	// Counter to decrement
	ECounterType m_eCounterToDecrement;

	// Writes the test from the strInputFileName USS file to the strOutputFileName TXT file
	void convertUSSFile(const std::string strInputFileName, const std::string strOutputFileName, 
		const EHandlingType eNoUSSFile);

	// Decrements the page-level redaction counter
	void decrementCounter(ISpatialStringPtr ipText);

	// Checks for disabling of USB keys
	bool usbCountersDisabled();

	// Checks for OCR_On_Server licensing
	void validateLicense();
};

extern CESConvertUSSToTXTApp theApp;
