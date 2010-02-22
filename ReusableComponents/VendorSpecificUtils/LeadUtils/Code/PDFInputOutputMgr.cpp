
#include "stdafx.h"
#include "PDFInputOutputMgr.h"
#include "MiscLeadUtils.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>

// By default, logging can only be enabled in debug mode
#ifdef _DEBUG
//#define _CACHE_LOGGING
#endif

#ifdef _CACHE_LOGGING
#include <ThreadSafeLogFile.h>
#endif

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
			ASSERT_RESOURCE_ALLOCATION( "ELI16138", m_apTFN.get() != NULL );

			// Store original filename and lower-case copy
			m_strOriginalFileName = m_strLCOriginalFileName = strPDFName;
			makeLowerCase( m_strLCOriginalFileName );

			// Convert PDF input file to temporary TIF
			convertPDFToTIF( strPDFName, m_apTFN->getName() );

			// Store last modified time
			m_tmOriginalFileModificationTime = getFileModificationTimeStamp( strPDFName );
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
// PDFInputOutputMgr
//-------------------------------------------------------------------------------------------------
PDFInputOutputMgr::PDFInputOutputMgr(const string& strOriginalName, bool bFileUsedAsInput,
									 const string& strUserPassword, const string& strOwnerPassword,
									 int nPermissions)
: m_apTFN(NULL),
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

			// Protect collections against access by other threads
			CSingleLock lg( &ms_mutex, TRUE );

			// Provide image to cache
			m_strWorkingName = addActiveFile( strOriginalName );
		}
		else if (isPDFFile(strOriginalName))
		{
			// Create new Temporary File Name object
			m_apTFN = auto_ptr<TemporaryFileName>(new TemporaryFileName( NULL, ".tif", true ));
			ASSERT_RESOURCE_ALLOCATION( "ELI16180", m_apTFN.get() != NULL );

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
			// Protect collections against access by other threads
			CSingleLock lg( &ms_mutex, TRUE );

			// Move this file from active collection to inactive collection, if present
			makeLowerCase( m_strOriginalName );
			removeActiveFile( m_strOriginalName );
		}
		else if (m_apTFN.get() != NULL)
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
		// Retrieve and delete this item
		CachedFileData* pCFD = (*iterMap).second;
		ASSERT_RESOURCE_ALLOCATION( "ELI16168", pCFD != NULL );
		delete pCFD;
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
string PDFInputOutputMgr::addActiveFile(string strName)
{
	// No special handling for non-PDF images
	if (!isPDFFile( strName ))
	{
		// Just return the input name
		return strName;
	}

#ifdef _CACHE_LOGGING
	// Prepare log file
	ThreadSafeLogFile tslf;
#endif

	// Convert filename to lower-case for comparison
	string strLCName = strName;
	makeLowerCase( strLCName );

	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Check the map of active files
	if (isFileInMap( strLCName ))
	{
		// Retrieve data structure
		CachedFileData* pCFD = ms_mapActiveFiles[strLCName];
		ASSERT_RESOURCE_ALLOCATION( "ELI16142", pCFD != NULL );

#ifdef _CACHE_LOGGING
		// Add entry to default log file
		string strText = "Found file in map: ";
		strText += strLCName;
		tslf.writeLine( strText.c_str() );
#endif

		// Return name of working file
		// (use the working file from the cached file data) [LRCAU #5264]
		TemporaryFileName* pTempFile = pCFD->m_apTFN.get();
		ASSERT_RESOURCE_ALLOCATION("ELI16143", pTempFile != NULL);
		return pTempFile->getName();
	}
	// File must be moved from the inactive list back to the map of active files
	else if (isFileInactive( strLCName, true ))
	{
		// Retrieve data structure
		CachedFileData* pCFD = ms_mapActiveFiles[strLCName];
		ASSERT_RESOURCE_ALLOCATION( "ELI16140", pCFD != NULL );

#ifdef _CACHE_LOGGING
		// Add entry to default log file
		string strText = "Found file in inactive queue: ";
		strText += strLCName;
		tslf.writeLine( strText.c_str() );
#endif

		// Get timestamp of this file and compare against stored time
		CTime tmFile = getFileModificationTimeStamp( strName );
		if (tmFile > pCFD->m_tmOriginalFileModificationTime)
		{
			// The file has been modified since being added to the cache
			// so a new cache entry must be created and the old entry replaced
			CachedFileData* pNewCFD = new CachedFileData( strName );
			ASSERT_RESOURCE_ALLOCATION( "ELI16175", pNewCFD != NULL );

			// Delete the previous CFD item
			CachedFileData* pOldCFD = ms_mapActiveFiles[strLCName];
			ASSERT_RESOURCE_ALLOCATION( "ELI16179", pOldCFD != NULL );
			delete pOldCFD;

			// Overwrite this item in the map
			ms_mapActiveFiles[strLCName] = pNewCFD;

#ifdef _CACHE_LOGGING
			// Add entry to default log file
			string strText = "Overwrote existing entry in map: ";
			strText += strLCName;
			tslf.writeLine( strText.c_str() );
#endif

			// Return name of new temporary file
			TemporaryFileName* pTempFile = pNewCFD->m_apTFN.get();
			ASSERT_RESOURCE_ALLOCATION("ELI25271", pTempFile != NULL);
			return pTempFile->getName();
		}
		else
		{
			// Return name of working file
			TemporaryFileName* pTempFile = pCFD->m_apTFN.get();
			ASSERT_RESOURCE_ALLOCATION("ELI16141", pTempFile != NULL);
			return pTempFile->getName();
		}
	}
	// File must be added to the map of active files
	else
	{
		// Create new data structure
		CachedFileData* pCFD = new CachedFileData( strName );
		ASSERT_RESOURCE_ALLOCATION( "ELI16144", pCFD != NULL );

		// Add this file and data structure to the map
		ms_mapActiveFiles[strLCName] = pCFD;

#ifdef _CACHE_LOGGING
		// Add entry to default log file
		string strText = "Added new entry to map: ";
		strText += strLCName;
		tslf.writeLine( strText.c_str() );
#endif

		// Return name of working file
		TemporaryFileName* pTempFile = pCFD->m_apTFN.get();
		ASSERT_RESOURCE_ALLOCATION("ELI25272", pTempFile != NULL);
		return pTempFile->getName();
	}
}
//--------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::addItemToInactiveQueue(CachedFileData* pCFD)
{
	// Retrieve lower-case copy of original filename
	ASSERT_ARGUMENT( "ELI16169", pCFD != NULL );
	string strLCName = pCFD->m_strLCOriginalFileName;

	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Just return if file is already in the inactive queue
	if (isFileInactive( strLCName, false ))
	{
		return;
	}

	// Check previous cache size
	if (ms_quInactiveFiles.size() == ms_nMaxCacheSize)
	{
#ifdef _CACHE_LOGGING
		// Add entry to default log file
		ThreadSafeLogFile tslf;
		string strText = "Queue is too full for: ";
		strText += strLCName;
		tslf.writeLine( strText.c_str() );
#endif

		// Cache is already full, remove the oldest item
		removeLastItem();
	}

	// Add this item to the top of the queue
	ms_quInactiveFiles.push_front( pCFD );
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
		ASSERT_RESOURCE_ALLOCATION( "ELI16146", pThisCFD != NULL );

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
bool PDFInputOutputMgr::isFileInMap(const string &strLCName)
{
	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	return (ms_mapActiveFiles.count(strLCName) > 0);
}
//--------------------------------------------------------------------------------------------------
void PDFInputOutputMgr::removeActiveFile(const string &strLCName)
{
	// Protect collections against access by other threads
	CSingleLock lg( &ms_mutex, TRUE );

	// Check the map of active files
	if (isFileInMap( strLCName ))
	{
		// Retrieve the data structure
		CachedFileData* pCFD = ms_mapActiveFiles[strLCName];
		ASSERT_RESOURCE_ALLOCATION( "ELI16148", pCFD != NULL );

		// Add the data structure to the top of the inactive files queue
		addItemToInactiveQueue( pCFD );

#ifdef _CACHE_LOGGING
		// Add log file entry
		ThreadSafeLogFile tslf;
		string strText = "Moved file to inactive queue: ";
		strText += pCFD->m_strLCOriginalFileName;
		tslf.writeLine( strText.c_str() );
#endif

		// Remove this map entry
		map<string, CachedFileData *>::iterator mapIter = ms_mapActiveFiles.find( strLCName );
		if (mapIter != ms_mapActiveFiles.end())
		{
			ms_mapActiveFiles.erase( mapIter );
		}
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
	ASSERT_RESOURCE_ALLOCATION( "ELI16139", pLastCFD != NULL );

#ifdef _CACHE_LOGGING
	// Add log file entry
	ThreadSafeLogFile tslf;
	string strText = "Removing last item from queue: ";
	strText += pLastCFD->strLCOriginalFileName;
	tslf.writeLine( strText.c_str() );
#endif

	// Delete the CachedFileData object, also deleting the temporary file
	delete pLastCFD;

	// Remove the last queue element
	ms_quInactiveFiles.pop_back();
}
//-------------------------------------------------------------------------------------------------
