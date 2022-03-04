// FileProcessingManager.cpp : Implementation of CFileProcessingManager
#include "stdafx.h"
#include "UCLIDFileProcessing.h"
#include "FileProcessingManager.h"
#include "HelperFunctions.h"
#include "CommonConstants.h"

#include <UCLIDException.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 19;
const int gnOLD_CONVERT_VERSION = 10;
//-------------------------------------------------------------------------------------------------
// Version 7:
//   Added Skip Condition persistence
// Version 8:
//   Added File Suppliers persistence
// Version 9:
//   Removed nMaxAttempts, Folder Scope, Individual Files Scope, Filter Patterns
// Version 10:
//   Add Action tab persistence
// Version 11:
//   Moved persistence of Supplying and Processing items to ...MgmtRole objects
// Version 12:
//	 Added DBConfig file persistence
// Version 13:
//	 Removed the DBConfig file name
//	 Added the Server and Database
// Version 14:
//	 Added Max files from DB into FPS file settings as opposed to registry setting
// Version 15:
//   Added AdvancedConnectionStringProperties
// Version 16:
//   Added Active workflow
// Version 17:
//	 Added require admin edit setting
// Version 18:
//	 Added use random queue order setting
// Version 19:
//	 Added ability to process files queued for a specific user

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FileProcessingManager;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if the directly held data is dirty, then indicate to the caller that
		// this object is dirty
		if (m_bDirty)
		{
			return S_OK;
		}

		// check if the file supplying role object is dirty
		if (m_ipFSMgmtRole != __nullptr)
		{
			IPersistStreamPtr ipFSStream = m_ipFSMgmtRole;
			ASSERT_RESOURCE_ALLOCATION("ELI14197", ipFSStream != __nullptr);
			if (ipFSStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// if the file processors container is dirty, then indicate to the caller that
		// this object is dirty
		if (m_ipFPMgmtRole != __nullptr)
		{
			IPersistStreamPtr ipFPStream = m_ipFPMgmtRole;
			ASSERT_RESOURCE_ALLOCATION("ELI14159", ipFPStream != __nullptr);
			if (ipFPStream->IsDirty() == S_OK)
			{
				return S_OK;
			}
		}

		// indicate to the caller that this object is not dirty
		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30414");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		clear();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI10967", "Unable to load newer File Action Manager." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}
		// Also check for version too old
		else if (nDataVersion <= gnOLD_CONVERT_VERSION)
		{
			// Throw exception
			UCLIDException ue( "ELI14280", "Unable to load older File Action Manager.\r\n\r\nPlease run the ConvertFPSFile utility first." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		////////////////////
		// nDataVersion >= 11 at this point
		// Streamed information no longer includes File or Folder Scope details
		////////////////////

		// Read the action name
		dataReader >> m_strAction;

		// Read statistics check box status
		dataReader >> m_bDisplayOfStatisticsEnabled;

		// For version 12 need to read the config file name
		if ( nDataVersion == 12 )
		{
			string strDBConfigFile;
			dataReader >> strDBConfigFile;
			AfxMessageBox("This file is obsolete. Please select a Server and a Database.",
				MB_OK | MB_ICONEXCLAMATION);
		}

		// Get the Server and Database
		if ( nDataVersion >= 13 )
		{
			// Read the server and database
			string strServer;
			dataReader >> strServer;
			setDBServer(strServer);
			string strDatabase;
			dataReader >> strDatabase;
			setDBName(strDatabase);
		}

		// Get the max files from db value
		if (nDataVersion >= 14)
		{
			dataReader >> m_nMaxFilesFromDB;
			if (m_nMaxFilesFromDB < gnNUM_FILES_LOWER_RANGE
				|| m_nMaxFilesFromDB > gnNUM_FILES_UPPER_RANGE)
			{
				long nLoaded = m_nMaxFilesFromDB;
				m_nMaxFilesFromDB = min(max(m_nMaxFilesFromDB, gnNUM_FILES_LOWER_RANGE),
					gnNUM_FILES_UPPER_RANGE);
				UCLIDException ue("ELI32147",
					"Application Trace: Persisted value for max files from database was out of range. Reset to closest value.");
				ue.addDebugInfo("Lower Bound", gnNUM_FILES_LOWER_RANGE);
				ue.addDebugInfo("Upper Bound", gnNUM_FILES_UPPER_RANGE);
				ue.addDebugInfo("Loaded Value", nLoaded);
				ue.addDebugInfo("Closest Value", m_nMaxFilesFromDB);
				ue.log();
			}
		}

		// Get advanced connection string properties
		if (nDataVersion >= 15)
		{
			string strAdvConnStrProperties;
			dataReader >> strAdvConnStrProperties;
			setAdvConnString(strAdvConnStrProperties);
		}

		if (nDataVersion >= 16)
		{
			dataReader >> m_strActiveWorkflow;
		}

		if (nDataVersion >= 17)
		{
			dataReader >> m_bRequireAdminEdit;
		}

		if (nDataVersion >= 18)
		{
			dataReader >> m_bUseRandomIDForQueueOrder;
		}

		if (nDataVersion >= 19)
		{
			dataReader >> m_bLimitToUserQueue;
		}

		// Read in the collected File Supplying Management Role
		IPersistStreamPtr ipFSObj;
		readObjectFromStream( ipFSObj, pStream, "ELI14399" );
		ASSERT_RESOURCE_ALLOCATION( "ELI14400", ipFSObj != __nullptr );
		m_ipFSMgmtRole = ipFSObj;

		// Read in the collected File Processing Management Role
		IPersistStreamPtr ipFPObj;
		readObjectFromStream( ipFPObj, pStream, "ELI17362" );
		ASSERT_RESOURCE_ALLOCATION( "ELI17363", ipFPObj != __nullptr );
		m_ipFPMgmtRole = ipFPObj;

		// Check the Action Name for result of recent File Conversion
		if (m_strAction == gstrCONVERTED_FPS_ACTION_NAME.c_str())
		{
			// Create and log an exception
			UCLIDException ue( "ELI14962", 
				"Application trace: The FPS file has been recently converted from a previous version. "
				"An Action must be selected.");
			ue.log();
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10968");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

		dataWriter << gnCurrentVersion;

		// Save the current action name
		dataWriter << m_strAction;

		// Save the status of the statistics check box
		dataWriter << m_bDisplayOfStatisticsEnabled;

		// Save the Database server
		dataWriter << m_strDBServer;
		
		// Save the database name
		dataWriter << m_strDBName;

		dataWriter << m_nMaxFilesFromDB;

		// Save the advanced connection string properties
		dataWriter << m_strAdvConnString;

		// Save the current workflow
		dataWriter << m_strActiveWorkflow;

		// Save the require admin edit flag
		dataWriter << m_bRequireAdminEdit;

		dataWriter << m_bUseRandomIDForQueueOrder;

		dataWriter << m_bLimitToUserQueue;
		
		// Flush the stream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Write out the collected File Supplying Management Role
		IPersistStreamPtr ipFSObj = m_ipFSMgmtRole;
		ASSERT_RESOURCE_ALLOCATION( "ELI14426", ipFSObj != __nullptr );
		writeObjectToStream( ipFSObj, pStream, "ELI14427", fClearDirty );

		// Write out the collected File Processing Management Role
		IPersistStreamPtr ipFPObj = m_ipFPMgmtRole;
		ASSERT_RESOURCE_ALLOCATION( "ELI14428", ipFPObj != __nullptr );
		writeObjectToStream( ipFPObj, pStream, "ELI14429", fClearDirty );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10969");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileProcessingManager::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
