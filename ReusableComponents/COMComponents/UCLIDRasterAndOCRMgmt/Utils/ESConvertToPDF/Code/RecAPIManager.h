#pragma once

#include <KernelAPI.h>
#include <string>

using namespace std;

class CESConvertToPDFApp;

/////////////////////////////
// CRecAPIManager class
// Encapsulates initialization, memory management and cleanup of Nuance API calls for processing
// a document. Each instance allows one set of HPAGE instances with OCR data to be returned. These
// need not be all pages of a document, but if more pages need to be loaded at a later time, they
// need to be loaded with a new class instance.
/////////////////////////////
class CRecAPIManager
{
public:
	CRecAPIManager(CESConvertToPDFApp *pApp, const string& strFileName);
	~CRecAPIManager();

	// Gets the number of pages in the loaded file.
	int getPageCount();

	// Gets the format info for the loaded file.
	void getImageInfo(IMG_INFO &rimgInfo, IMF_FORMAT &rimgFormat);

	// OCRs the specified pages and returns HPAGE instances for those pages that includes OCR.
	HPAGE* getOCRedPages(int nStartPage, int nPageCount);

	// The HIMGFILE handles for the loaded file.
	HIMGFILE m_hFile;

private:

	CESConvertToPDFApp *m_pApp;
	int m_nLoadedPageCount;
	HPAGE* m_pPages;

	///////////////////////
	/// Private methods
	///////////////////////

	void init();
	// Applies RecAPI settings for OCR, format and security based on the command-line params.
	void applySettings();
	// Opens the specified image file for reading.
	void openImageFile(const string& strFileName);
};

