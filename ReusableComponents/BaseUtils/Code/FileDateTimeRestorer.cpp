#include "stdafx.h"
#include "FileDateTimeRestorer.h"
#include "cpputil.h"
#include "UCLIDException.h"

#include <SYS/UTIME.H>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// FileDateTimeRestorer
//-------------------------------------------------------------------------------------------------
FileDateTimeRestorer::FileDateTimeRestorer(const string strFileName)
{
	try
	{
		m_bFileFound = false;

		CFileFind	ffSource;
		if (ffSource.FindFile( strFileName.c_str() ))
		{
			// Find the "next" file and update source 
			// information for this one
//			BOOL	bMoreFiles = ffSource.FindNextFile();
			ffSource.FindNextFile();

			// Get file creation time
			if (ffSource.GetCreationTime( &m_ftCreationTime ) == 0)
			{
				// Unexpected error, create and throw exception
				UCLIDException ue( "ELI19338", "Unable to get file creation time." );
				throw ue;
			}

			// Get file access time
			if (ffSource.GetLastAccessTime( &m_ftLastAccessTime ) == 0)
			{
				// Unexpected error, create and throw exception
				UCLIDException ue( "ELI07377", "Unable to get file access time." );
				throw ue;
			}
			
			// Get file write time
			if (ffSource.GetLastWriteTime( &m_ftLastWriteTime ) == 0)
			{
				// Unexpected error, create and throw exception
				UCLIDException ue( "ELI07378", "Unable to get file write time." );
				throw ue;
			}

			// Save the filename and set flag
			m_strFileName = strFileName;
			m_bFileFound = true;
		}
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07375")
}
//-------------------------------------------------------------------------------------------------
FileDateTimeRestorer::~FileDateTimeRestorer()
{
	try
	{
		// Reset times only if file already existed
		if (m_bFileFound)
		{
			// Convert original time settings into time_t objects
			CTime	ctmAccess( m_ftLastAccessTime );
			CTime	ctmWrite( m_ftLastWriteTime );
			time_t	tmAccess = ctmAccess.GetTime();
			time_t	tmWrite = ctmWrite.GetTime();

			// Set file modification time
			struct _utimbuf	tmSettings;
			tmSettings.actime = tmAccess;
			tmSettings.modtime = tmWrite;

			// Reset file modification time
			long lResult = _utime( m_strFileName.c_str(), &tmSettings );
			if (lResult != 0)
			{
				UCLIDException ue( "ELI07386", "Unable to update file settings." );
				throw ue;
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI07387")
}
//-------------------------------------------------------------------------------------------------
