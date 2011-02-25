#pragma once

#include "LeadUtils.h"

#include <TemporaryFileName.h>
#include <Win32Event.h>

#include <string>
#include <memory>
#include <map>
#include <deque>

using namespace std;

// Data stored for cached input files
class CachedFileData 
{
public:
	CachedFileData(const string& strPDFName);
	~CachedFileData();

	// Original filename passed to constructor and lower-case copy
	string							m_strOriginalFileName;
	string							m_strLCOriginalFileName;

	// Temporary file used as repository for converted input file or as file 
	// to be converted into the output PDF file
	auto_ptr<TemporaryFileName>	m_apTFN;

	// Timestamp of strLCOriginalFileName used to properly deal with image rescans
	CTime								m_tmOriginalFileModificationTime;

	// Waits for the pdf conversion to complete
	void waitForConversion();

private:

	typedef struct ConversionData {
		string strPdfName;
		string strTempName;
		Win32Event m_eventConverted;
	};

	ConversionData m_data;

	static UINT convertPdf(LPVOID pData);
};

//-------------------------------------------------------------------------------------------------
// Class used if input or output file is a PDF.  Constructor converts the input PDF into a 
// temporary file to be used by code that accesses file information such as number of pages, 
// image resolution, etc.  
// Destructor converts the temporary file into the output PDF.  The temporary TIF is also deleted.  
// This class should be used to prevent potential multi-threading problems.
class LEADUTILS_API PDFInputOutputMgr
{
public:
	// Manages automatic conversion of PDF images to and from temporary TIF.  No conversion 
	// is made if strOriginalName is not a PDF image.
	// If bFileUsedAsInput
	// - converts strOriginalName to temporary TIF
	// - provides filename of temporary file for subsequent use via getFileName()
	// - temporary file is automatically deleted when no longer needed 
	// Else
	// - creates empty temporary file for subsequent use
	// - temporary file is converted to PDF image ( strOriginalName ) in the destructor
	// - temporary file is automatically deleted after conversion to PDF
	PDFInputOutputMgr(const string& strOriginalName, bool bFileUsedAsInput,
		const string& strUserPassword = "", const string& strOwnerPassword = "",
		int nPermissions = 0);
	~PDFInputOutputMgr();

	// Returns name of temporary file if converted or original image
	// if not converted
	const string& getFileName();

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a file name information string to the caller
	//
	// PROMISE:	Returns the name of the original file, or if the file has been converted
	//			returns a string of the format "Original: <orig> As: <converted>"
	const string& getFileNameInformationString();

	// Returns true if the original file was used as input and false otherwise
	bool isInputFile() { return m_bOriginalUsedAsInput; }

	// Moves all active items to inactive queue then removes all items from the queue
	static void sFlushCache();

private:
	//////////
	// Methods
	//////////

	// Provides special handling for input PDF files only.
	// - Adds file to map of active files.  
	// - Converts strName to temporary TIF
	string addActiveFile(const string& strName);

	// Adds specified item to top of queue of inactive files.  Removes item at bottom of queue if 
	// queue was already at maximum capacity.
	static void	addItemToInactiveQueue(CachedFileData* pCFD);

	// Searches queue of inactive files and returns true if found.  Moves data structure back to 
	// map of active files if found and bMoveToActive == true.
	static bool isFileInactive(const string &strLCName, bool bMoveToActive);

	// Searches queue of inactive files and returns true if the file is found
	static bool isFileInactive(CachedFileData* pCFD);

	// Returns the pointer to the item from the map if it is in the map, or __nullptr otherwise
	// If bErase == true, then will erase the item from the map before returning the pointer.
	// NOTE: This method will search and possibly modify the static map. It must not be
	//		 called without first locking the static mutex
	CachedFileData* getFileFromMap(const string& strLCName, bool bErase = false);

	// Moves specified file from map of active files, if found, to top of deque of inactive files.
	// NOTE: This method will search and modify the static map as well as the inactive queue.
	//		 It must not be called without first locking the static mutex
	void removeActiveFile(const string &strLCName);

	// Removes last / oldest item from the collection of inactive files.
	static void	removeLastItem();

	///////////////
	// Data members
	///////////////

	// Temporary file for conversion to output PDF image
	auto_ptr<TemporaryFileName>	m_apTFN;

	// True if m_strOriginalName was an input file
	bool	m_bOriginalUsedAsInput;

	// Original filename provided to this class instance
	string	m_strOriginalName;

	// Filename provided to caller via getFileName()
	string	m_strWorkingName;

	// File name information string provided to caller via getFileNameInformationString()
	string m_strFileNameInformationString;

	// PDF security settings to apply to the output PDF
	string m_strUserPassword;
	string m_strOwnerPassword;
	int m_nPermissions;

	// Mutex used to protect the static collection objects
	static CMutex ms_mutex;

	// Collection of inactive files that can still be moved back to active status.
	// The maximum size of this deque is stored in m_nMaxCacheSize.
	static deque<CachedFileData *> ms_quInactiveFiles;

	// Collection of active files - not limited in size
	static map<string, CachedFileData *> ms_mapActiveFiles;

	// Maximum size of inactive files collection
	static int ms_nMaxCacheSize;
};
