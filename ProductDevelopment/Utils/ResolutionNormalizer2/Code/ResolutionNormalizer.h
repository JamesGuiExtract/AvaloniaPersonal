// ImageFormatConverter.h : main header file for the PROJECT_NAME application
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#include <string>

#include <KernelAPI.h>

// CResolutionNormalizerApp:
// See ResolutionNormalizer.cpp for the implementation of this class
class CResolutionNormalizerApp : public CWinApp
{
public:
	CResolutionNormalizerApp();

// Overrides
	virtual BOOL InitInstance();

private:
// Implementation

	// Top level function that encapsulates all processing for the file.
	void processFile(const std::string& strFileName, double dResFactor);

	// Initializes the Nuance engine
	void initNuanceEngineAndLicense();

	// Performs resolution normalizing on the specified file where all pages whose horizontal and
	// vertical DPI differ by more than dFactor. Normalize pages will have the DPI of both axes set
	// to the greater of the two.
	// pnPageCount returns the number of pages in the document
	// pnPagesUpdated returns the number of pages normalized.
	void normalizeResolution(const std::string& strFileName, double dResFactor, int *pnPageCount, int *pnPagesUpdated);

	// Performs resolution normalizing on the specified page of the specified file if the horizontal
	// and vertical DPI differ by more than dFactor. A normalized page will have the DPI of both
	// axes set to the greater of the two.
	// Returns true if normalization was required or false if the page was not modified.
	bool normalizePageResolution(const std::string& strFileName, HIMGFILE hImage, int nPage, double dResFactor);

	// Finalizes the specified file and validates by ensuring it can be opened and that its page
	// count is what it is expected to be.
	void finalizeAndValidateOutput(const std::string& strFileName, int nExpectedPageCount);

	// Displays command-line argument info.
	void usage(const std::string& strError);

	void validateLicense();
};

extern CResolutionNormalizerApp theApp;
