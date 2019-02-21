#pragma once

#include <l_bitmap.h>

#include <string>

using namespace std;

///////////////////////////
// CLocalPDFOptions
//
// Allows for different FILEPDFOPTIONS to be applied within a local scope, then automatically
// reverted as the instance goes out-of-scope.
// NOTE: Because L_SetPDFOptions applies settings per thread, conflicts between threads should not
// be an issue unless multiple threads were passing settings into a shared instance.
///////////////////////////
class CLocalPDFOptions
{
public:
	CLocalPDFOptions(void);
	~CLocalPDFOptions(void);

	// Defaults to the currently used FILEPDFOPTIONS
	FILEPDFOPTIONS m_pdfOptions;

	// Defaults to the currently used RASTERIZEDOCOPTIONS
	RASTERIZEDOCOPTIONS m_pdfRasterizeDocOptions;

	// Applies the current m_pdfOptions.
	void ApplyPDFOptions(const string &strELICode, const string &strErrorDescription);

private:
	FILEPDFOPTIONS m_pdfOptionsOriginal;
	RASTERIZEDOCOPTIONS m_pdfRasterizeDocOptionsOriginal;
	bool m_bInitialized;
};

