
#include "stdafx.h"
#include "PDFInputOutputMgr.h"
#include "MiscLeadUtils.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrLEAD_UTILS_FOLDER		= "\\LeadUtils";
const string gstrCACHE_SIZE_KEY		= "FileCacheSize";
const string gstrDEFAULT_CACHE_SIZE	= "25";

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
// Mutex for thread safety for the static collection objects
CMutex PDFInputOutputMgr::ms_mutex;

// Collection of currently active files
map<string, CachedFileData *> PDFInputOutputMgr::ms_mapActiveFiles;

// Collection of inactive files to be deleted as queue reaches its capacity
deque<CachedFileData *> PDFInputOutputMgr::ms_quInactiveFiles;

// Maximum number of inactive files to be stored in deque
int PDFInputOutputMgr::ms_nMaxCacheSize = 0;

//-------------------------------------------------------------------------------------------------
// CachedFileData
//-------------------------------------------------------------------------------------------------
CachedFileData::CachedFileData(const string& strPDFName)
{
	try
	{
		try
		{
			// Create new Temporary File Name object
			m_apTFN = auto_ptr<TemporaryFileName>(new TemporaryFileName( NULL, ".tif", true ));
			ASSERT_RESOURCE_ALLOCATION( "ELI16138", m_apTFN.get() != __nullptr );

			// Store original filename and lower-case copy
			m_strOriginalFileName = m_strLCOriginalFileName = strPDFName;
			makeLowerCase( m_strLCOriginalFileName );

			// Store last modified time
			m_tmOriginalFileModificationTime = getFileModificationTimeStamp( strPDFName );

			// Set the conversion data members
			m_data.strPdfName = strPDFName;
			m_data.strTempName = m_apTFN->getName();

			// Start the conversion thread
			if(!AfxBeginThread(convertPdf, &m_data))
			{
				throw UCLIDException("ELI31873", "Unable to initialize pdf conversion thread.");
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25222");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("PDF File Name", strPDFName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
CachedFileData::~CachedFileData()
{
	try
	{
		// Wait for the pdf conversion to finish
		m_data.m_eventConverted.wait();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31871");
}
//-------------------------------------------------------------------------------------------------
void CachedFileData::waitForConversion()
{
	// Block until the conversion is complete
	m_data.m_eventConverted.wait();
}
//-------------------------------------------------------------------------------------------------
UINT CachedFileData::convertPdf(void* pData)
{
	ConversionData* pCData = __nullptr;
	try
	{
		pCData = (ConversionData*) pData;
		ASSERT_RESOURCE_ALLOCATION("ELI25209", pCData != __nullptr);

			// Convert PDF input file to temporary TIF
		convertPDFToTIF(pCData->strPdfName, pCData->strTempName);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31872");

	if (pCData != __nullptr)
	{
		pCData->m_eventConverted.signal();
	}

	return 0;
}

//-------------------------------------------------------------------------------------------------
// PDFInputOutputMgr
//-------------------------------------------------------------------------------------------------
PDFInputOutputMgr::PDFInputOutputMgr(const string& strOriginalName, bool bFileUsedAsInput,
									 const string& strUserPassword, const string& strOwnerPassword,
									 int nPermissions)
: m_apTFN(__nullptr),
  m_strFileNameInformationString(""),
  m_bOriginalUsedAsInput(bFileUsedAsInput),
  m_strUserPassword(strUserPassword),
  m_strOwnerPassword(strOwnerPassword),
  m_nPermissions(nPermissions)
{
	try
	{
		// Read cache size from registry once
		if (ms_nMaxCacheSize == 0)
		{
			// Create RPM to retrieve cache size from registry
			RegistryPersistenceMgr rpm( HKEY_LOCAL_MACHINE, gstrVENDORSPECIFIC_REG_PATH );

			// Check for cache size key existence
			if (!rpm.keyExists( gstrLEAD_UTILS_FOLDER, gstrCACHE_SIZE_KEY ))
			{
				// Create key with default value
				rpm.createKey( gstrLEAD_UTILS_FOLDER, gstrCACHE_SIZE_KEY, gstrDEFAULT_CACHE_SIZE );
			};

			// Retrieve the value
			ms_nMaxCacheSize = asLong( rpm.getKeyValue( gstrLEAD_UTILS_FOLDER, gstrCACHE_SIZE_KEY ) );
		}

		// Handle input PDF differently than output PDF
		if (bFileUsedAsInput)
		{
			// Check file existence
			validateFileOrFolderExistence( strOriginalName );

			// Provide image to cache
			m_strWorkingName = addActiveFile( strOriginalName );
		}
		else if (isPDFFile(strOriginalName))
		{
			// Create new Temporary File Name object
			m_apTFN = auto_ptr<TemporaryFileName>(new TemporaryFileName( NULL, ".tif", true ));
			ASSERT_RESOURCE_ALLOCATION( "ELI16180", m_apTFN.get() != __nullptr );

			// Working filename is the name of the temporary file
			m_strWorkingName = m_apTFN->getName();
		}
		// else this is an output non-PDF, just save the original name as the working name
		else
		{
			m_strWorkingName = strOriginalName;
		}

		// Save the filename
		m_strOriginalName = strOriginalName;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16162");
}
//-------------------------------------------------------------------------------------------------
PDFInputOutputMgr::~PDFInputOutputMgr()
{
	try
	{
		if (m_bOriginalUsedAsInput)
		{
			makeLowerCase( m_strOriginalName );

			// Protect collections against access by other threads
			CSingleLock lg( &ms_mutex, TRUE );

			// Move this file from active collection to inactive collection, if present
			removeActiveFile( m_strOriginalName );
		}
		else if (m_apTFN.get() != __nullptr)
		{
			// Ensure we can read the temporary file before attempting to convert it
			waitForFileToBeReadable(m_apTFN->getName());

			// Convert the temporary file into the output PDF
			// (retaining redaction annotations) [FIDSC #3131 - JDS - 12/17/2008]
			// Set the specified security settings
			convertTIFToPDF( m_apTFN->getName(), m_strOriginalName, true,
				m_strUserPassword, m_strOwnerPassword, m_nPermissions);
		}
		// else this was an output non-PDF and nothing needs to be done
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16470");
}
//-------------------------------------------------------------------------------------------------
const string& PDFInputOutputMgr::getFileName()
{
	return m_strWorkingName;
}
//-------------------------------------------------------------------------------------------------
const string& PDFInputOutputMgr::getFileNameInformationString()
{
	// if m_strFileNameInformationString is empty then build the string
	// this way the string only needs to be built once
	if (m_strFileNameInformationString.empty())
	{
		// if the working and original name are the same then just return the original
		if (m_strWorkingName == m_strOriginalName)
		{
			m_strFileNameInformationString = m_strOriginalName;
		}
		// working and original are different, build clever file information string
		else
		{
			m_strFileNameInformationString = "Original: " + m_strOriginalName + " As: " 
				+ m_strWorkingName;
		}
	}

	// return the file name information string
	return m_strFileNameInformationString;
}
//-------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::sFlushCache()
{
	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Remove any remaining active items
	map<string, CachedFileData *>::iterator iterMap;
	for (iterMap = ms_mapActiveFiles.begin(); iterMap != ms_mapActiveFiles.end(); iterMap++)
	{
		// No need to check for NULL, it is safe to delete on NULL (C++ standard && Stroustrup)
		delete iterMap->second;
	}

	// Clear the map
	ms_mapActiveFiles.clear();

	// Step through each item in collection of inactive files
	int nCurrentSize = ms_quInactiveFiles.size();
	for (int i = 0; i < nCurrentSize; i++)
	{
		// Remove the last item from the collection
		removeLastItem();
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
string PDFInputOutputMgr::addActiveFile(const string& strName)
{
	try
	{
		// No special handling for non-PDF images
		if (!isPDFFile( strName ))
		{
			// Just return the input name
			return strName;
		}

		// Convert filename to lower-case for comparison
		string strLCName = strName;
		makeLowerCase( strLCName );

		// Mutex over collection access
		CSingleLock lg( &ms_mutex, TRUE );

		// Attemp to get the file from the map
		CachedFileData* pCFD = getFileFromMap(strLCName);

		// File is not in map
		if (pCFD == __nullptr)
		{
			// Check for inactive file (if inactive, make it active again)
			if (isFileInactive(strLCName, true))
			{
				// Retrieve data structure
				pCFD = ms_mapActiveFiles[strLCName];
				ASSERT_RESOURCE_ALLOCATION( "ELI16140", pCFD != __nullptr );

				CTime tmFile = getFileModificationTimeStamp( strName );
				if (tmFile > pCFD->m_tmOriginalFileModificationTime)
				{
					// The file has been modified since being added to the cache
					// so a new cache entry must be created and the old entry replaced
					CachedFileData* pNewCFD = new CachedFileData( strName );
					ASSERT_RESOURCE_ALLOCATION( "ELI16175", pNewCFD != __nullptr );

					// Delete the previous CFD item
					delete pCFD;
					pCFD = pNewCFD;

					// Overwrite this item in the map
					ms_mapActiveFiles[strLCName] = pCFD;
				}
			}
			else // Not inactive, need to create a new cached file
			{
				// Create new data structure
				pCFD = new CachedFileData( strName );
				ASSERT_RESOURCE_ALLOCATION( "ELI16144", pCFD != __nullptr );

				// Add this data structure to the map
				ms_mapActiveFiles[strLCName] = pCFD;
			}
		}

		// Unlock the mutex
		lg.Unlock();

		// Get the name of the temporary file (this is the file name that
		// will be returned from this method)
		TemporaryFileName* pTempFile = pCFD->m_apTFN.get();
		ASSERT_RESOURCE_ALLOCATION("ELI25272", pTempFile != __nullptr);

		// Replace the LC name with the temporary file name (this is the
		// value that will be returned from the method)
		strLCName = pTempFile->getName();

		// Wait for the temporary file conversion to complete
		pCFD->waitForConversion();

		// Return the temporary name
		return strLCName;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31870");
}
//--------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::addItemToInactiveQueue(CachedFileData* pCFD)
{
	ASSERT_ARGUMENT( "ELI16169", pCFD != __nullptr );

	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Just return if file is already in the inactive queue
	if (isFileInactive(pCFD))
	{
		return;
	}

	// Check previous cache size
	if (ms_quInactiveFiles.size() == ms_nMaxCacheSize)
	{
		// Cache is already full, remove the oldest item
		removeLastItem();
	}

	// Add this item to the top of the queue
	ms_quInactiveFiles.push_front( pCFD );
}
//--------------------------------------------------------------------------------------------------
bool PDFInputOutputMgr::isFileInactive(CachedFileData* pCFD)
{
	deque<CachedFileData*>::iterator quIter;

	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );
	for(quIter = ms_quInactiveFiles.begin(); quIter != ms_quInactiveFiles.end(); quIter++)
	{
		if (*quIter == pCFD)
		{
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool PDFInputOutputMgr::isFileInactive(const string &strLCName, bool bMoveToActive)
{
	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Step through queue
	deque <CachedFileData *>::iterator quIter;
	for (quIter = ms_quInactiveFiles.begin(); quIter != ms_quInactiveFiles.end(); quIter++)
	{
		// Retrieve this data structure
		CachedFileData* pThisCFD = *quIter;
		ASSERT_RESOURCE_ALLOCATION( "ELI16146", pThisCFD != __nullptr );

		// Compare the filenames : case-insensitive
		if (strLCName == pThisCFD->m_strLCOriginalFileName)
		{
			// Filenames match, should data structure be moved?
			if (bMoveToActive)
			{
				// Add this item to the active map
				ms_mapActiveFiles[strLCName] = pThisCFD;

				// Remove this item from the inactive queue
				ms_quInactiveFiles.erase( quIter );
			}

			return true;
		}
	}

	// File not found in inactive queue
	return false;
}
//--------------------------------------------------------------------------------------------------
CachedFileData* PDFInputOutputMgr::getFileFromMap(const string& strLCName, bool bErase)
{
	CachedFileData* pReturn = __nullptr;

	map<string, CachedFileData*>::iterator it = ms_mapActiveFiles.find(strLCName);
	if (it != ms_mapActiveFiles.end())
	{
		pReturn = it->second;
		if (bErase)
		{
			ms_mapActiveFiles.erase(it);
		}
	}

	return pReturn;
}
//--------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::removeActiveFile(const string &strLCName)
{
	// Get the cached data for the file from the map, removing it from the
	// map if it is found.
	CachedFileData* pCFD = getFileFromMap(strLCName, true);
	if (pCFD != __nullptr)
	{
		// Add the cached data to the inactive queue
		addItemToInactiveQueue( pCFD );
	}
	// else not contained in the map so no movement is needed
}
//--------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::removeLastItem()
{
	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Retrieve last inactive file
	CachedFileData* pLastCFD = ms_quInactiveFiles.back();
	ASSERT_RESOURCE_ALLOCATION( "ELI16139", pLastCFD != __nullptr );

	// Delete the CachedFileData object, also deleting the temporary file
	delete pLastCFD;

	// Remove the last queue element
	ms_quInactiveFiles.pop_back();
}
//-------------------------------------------------------------------------------------------------
