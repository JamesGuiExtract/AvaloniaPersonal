// FeedbackMgr.cpp : Implementation of CFeedbackMgr
#include "stdafx.h"
#include "FeedbackManager.h"
#include "FeedbackMgr.h"
#include "..\\..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <TemporaryResourceOverride.h>
#include <RegistryPersistenceMgr.h>
#include <comutils.h>
#include <DateUtil.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>

#include <Time.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Static variable used by openDBConnection() to disable Feedback collection for 
// this session because connection failed
bool CFeedbackMgr::ms_bDisableSessionFeedback = false;
CMutex CFeedbackMgr::m_sMutex;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CFeedbackMgr
//-------------------------------------------------------------------------------------------------
CFeedbackMgr::CFeedbackMgr()
: m_bConnectionOpen(false),
  m_ipConnection(NULL)
{
	try
	{
		// Get Persistence Manager for dialog
		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_REG_UTILS_FOLDER_PATH ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09154", ma_pUserCfgMgr.get() != __nullptr );

		ma_pCfgFeedbackMgr = unique_ptr<PersistenceMgr>(new PersistenceMgr( 
			ma_pUserCfgMgr.get(), gstrAF_REG_FEEDBACK_FOLDER ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09155", ma_pCfgFeedbackMgr.get() != __nullptr );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08476")
}
//-------------------------------------------------------------------------------------------------
CFeedbackMgr::~CFeedbackMgr()
{
	try
	{
		// Close the DB Connection
		if (m_bConnectionOpen)
		{
			// Clear the ADO connection
			m_ipConnection->Close();
			m_ipConnection = __nullptr;
			m_bConnectionOpen = false;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16438");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFeedbackMgr,
		&IID_ILicensedComponent,
		&IID_IFeedbackMgrInternals
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFeedbackMgr
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_RecordCorrectData(BSTR bstrRuleExecutionID, IIUnknownVector* pData)
{
	try
	{
		// Check licensing
		validateLicense();

		// Convert RuleID to number
		string	strID = asString( bstrRuleExecutionID );

		// Just return if Rule ID is empty string
		if (strID.empty())
		{
			return S_OK;
		}

		// Check for empty or invalid feedback folder
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (strFeedbackFolder.empty() || !isFileOrFolderValid( strFeedbackFolder ))
		{
			// Just return
			return S_FALSE;
		}

		long lRuleID = asLong( strID );

		// Retrieve CorrectData submit time
		SYSTEMTIME	st;
		GetSystemTime( &st );
		CTime	t( st );
		__time64_t t64Time = t.GetTime();

		// Update this record
		writeCorrectTime( lRuleID, t64Time );

		///////////////////////////////////
		// Store specified data in VOA file
		///////////////////////////////////
		CString	zFilename;
		zFilename.Format( "%08d.correct.voa", lRuleID );

		// Get full path for output file
		string strFile = strFeedbackFolder + "\\" + LPCTSTR(zFilename);

		// Create smart pointer for Data
		IIUnknownVectorPtr	ipData( pData );
		ASSERT_RESOURCE_ALLOCATION( "ELI08785", ipData != __nullptr );

		// Write the vector of results to the file
		ipData->SaveTo(strFile.c_str(), VARIANT_TRUE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07942")
}

//-------------------------------------------------------------------------------------------------
// IFeedbackMgrInternals
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_RecordFoundData(BSTR bstrRuleExecutionID, IIUnknownVector * pData)
{
	try
	{
		// Check licensing
		validateLicense();

		string strRuleExecutionID = asString(bstrRuleExecutionID);

		// Just return if Rule ID is empty string
		if (strRuleExecutionID.empty())
		{
			return S_OK;
		}

		// Check for empty or invalid feedback folder
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (strFeedbackFolder.empty() || !isFileOrFolderValid( strFeedbackFolder ))
		{
			// Just return
			return S_FALSE;
		}

		// Get numeric Rule ID
		long lRuleID = asLong(strRuleExecutionID);

		// Get associated Duration timer
		StopWatch Watch;
		map<long, StopWatch>::iterator iterMap = m_mapDurationTimers.find( lRuleID );
		if (iterMap != m_mapDurationTimers.end())
		{
			// Retrieve the StopWatch
			Watch = iterMap->second;

			// Stop the timer and store the elapsed time in database
			Watch.stop();
			double dTime = Watch.getElapsedTime();
			writeDuration( lRuleID, dTime );

			// Clear the map entry
			m_mapDurationTimers.erase( iterMap );
		}

		///////////////////////////////////
		// Store specified data in VOA file
		///////////////////////////////////

		// Construct filename for data
		CString	zFilename;
		zFilename.Format( "%08d.found.voa", lRuleID );

		// Get full path for output file
		string strFile = strFeedbackFolder + "\\" + LPCTSTR(zFilename);

		// Create smart pointer for Data
		IIUnknownVectorPtr	ipData( pData );
		ASSERT_RESOURCE_ALLOCATION( "ELI08786", ipData != __nullptr );

		// Write the vector of results to the file
		ipData->SaveTo(strFile.c_str(), VARIANT_FALSE);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08787")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_RecordException(BSTR bstrRuleExecutionID, BSTR bstrException)
{
	try
	{
		// Check licensing
		validateLicense();

		string strRuleID = asString(bstrRuleExecutionID);

		// Just return if Rule ID is empty string
		if (strRuleID.empty())
		{
			return S_OK;
		}

		// Check for empty or invalid feedback folder
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (strFeedbackFolder.empty() || !isFileOrFolderValid( strFeedbackFolder ))
		{
			// Just return
			return S_FALSE;
		}

		// Get numeric Rule ID
		long lRuleID = asLong(strRuleID);

		///////////////////////////////////
		// Store specified data in UEX file
		///////////////////////////////////

		// Construct filename for UCLID Exception
		CString	zFilename;
		zFilename.Format( "%08d.uex", lRuleID );

		// Get full path for output file
		string strFile = strFeedbackFolder + "\\" + LPCTSTR(zFilename);

		// Create UCLID Exception object
		UCLIDException ue;
		ue.createFromString( "ELI08789", asString( bstrException ) );

		// Write the UCLID Exception to the file
		ue.saveTo( strFile );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19357")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_RecordRuleExecution(IAFDocument *pAFDoc, BSTR bstrRSDFileName, 
												   BSTR* pbstrRuleExecutionID)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI28091", pbstrRuleExecutionID != __nullptr);

		////////////////////////////////////////////
		// Default Rule Execution ID to empty string
		////////////////////////////////////////////
		_bstr_t	bstrID( "" );

		CSingleLock lg(&m_sMutex, TRUE);

		// Check for feedback collection disabled for this session
		if (ms_bDisableSessionFeedback)
		{
			// Just return the empty string
			*pbstrRuleExecutionID = bstrID.Detach();
			return S_OK;
		}

		// Check for feedback collection disabled
		if (!ma_pCfgFeedbackMgr->getFeedbackEnabled())
		{
			// Just return the empty string
			*pbstrRuleExecutionID = bstrID.Detach();
			return S_OK;
		}

		// Check for empty or invalid feedback folder
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (strFeedbackFolder.empty() || !isFileOrFolderValid( strFeedbackFolder ))
		{
			// Just return the empty string
			*pbstrRuleExecutionID = bstrID.Detach();
			return S_OK;
		}

		///////////////////////////////////////
		// Check for automatic turn-off enabled
		///////////////////////////////////////
		bool bCheckCount = false;
		long lTurnOffCount = -1;
		if (ma_pCfgFeedbackMgr->getAutoTurnOffEnabled())
		{
			// Check for turn-off via Date
			lTurnOffCount = ma_pCfgFeedbackMgr->getTurnOffCount();
			if (lTurnOffCount == 0)
			{
				string strDate = ma_pCfgFeedbackMgr->getTurnOffDate();
				long lMonth = -1;
				long lDay = -1;
				long lYear = -1;
				if (isValidDate( strDate, &lMonth, &lDay, &lYear ))
				{
					// Convert Date string to Date object
					COleDateTime tmStop;
					tmStop.SetDate( lYear, lMonth, lDay );

					// Get current Date as Date object
					SYSTEMTIME	time;
					GetSystemTime( &time );
					COleDateTime tmNow( time );

					// Compare times
					if (tmNow >= tmStop)
					{
						// Just return the empty string
						*pbstrRuleExecutionID = bstrID.Detach();
						return S_OK;
					}
				}
				else
				{
					// Throw exception
					UCLIDException ue( "ELI09082", "Invalid date to turn off Feedback collection!" );
					ue.addDebugInfo( "Date", strDate );
					throw ue;
				}
			}
			else
			{
				// Set flag to check count after Index file is evaluated
				bCheckCount = true;
			}
		}

		//////////////////////////////////////
		// Open Index file if not already open
		//////////////////////////////////////
		if (!openDBConnection( strFeedbackFolder ))
		{
			// Set flag to disable Feedback collection for this session
			ms_bDisableSessionFeedback = true;

			// Inform caller that Feedback will not be collected for this session
			MessageBox( NULL, "Feedback will not be collected for this session",
				"Warning", MB_OK|MB_ICONINFORMATION );

			// Just return the empty string
			*pbstrRuleExecutionID = bstrID.Detach();

			return S_OK;
		}

		// Check collection count
		if (bCheckCount)
		{
			// Retrieve rule execution records from database
			_variant_t vtQuery = "SELECT * FROM RuleExecution";

			// Retrieve recordset
			_RecordsetPtr	ipRS( __uuidof(Recordset) );
			ipRS->Open( vtQuery , 
				_variant_t((IDispatch *)m_ipConnection),
				adOpenKeyset,
				adLockPessimistic,
				adCmdText);

			long lCollectionCount = 0;
			if (ipRS->adoEOF == VARIANT_FALSE)
			{
				// Retrieve counter value
				lCollectionCount = ipRS->GetRecordCount();
			}
			ipRS->Close();

			// Compare turn-off count against current count
			if (lCollectionCount >= lTurnOffCount)
			{
				// Just return the empty string
				*pbstrRuleExecutionID = bstrID.Detach();
				return S_OK;
			}
		}

		// Check configured skip count
		long lConfigSkipCount = ma_pCfgFeedbackMgr->getSkipCount();
		if (lConfigSkipCount > 0)
		{
			// Retrieve pending skip count from database
			_variant_t vtQuery("SELECT * FROM Counter WHERE CounterName = 'SkipCount'");

			// Retrieve recordset
			_RecordsetPtr	ipRS( __uuidof(Recordset) );
			ipRS->Open( vtQuery , 
				_variant_t((IDispatch *)m_ipConnection),
				adOpenKeyset,
				adLockPessimistic,
				adCmdText);

			if (ipRS->adoEOF == VARIANT_FALSE)
			{
				// Retrieve counter value
				long lSkipCount = 0;
				_variant_t	vtValue;
				vtValue.vt = VT_I4;
				vtValue = ipRS->GetCollect( gstrCounterValueField );
				if (vtValue.vt == VT_I4)
				{
					lSkipCount = vtValue.lVal;
				}

				// Update local skip counter
				lSkipCount++;

				// Reset counter if multiple of skip count
				if (lSkipCount % (lConfigSkipCount + 1) == 0)
				{
					lSkipCount = 0;
				}

				// Update skip count in database
				vtValue.lVal = lSkipCount;
				ipRS->PutCollect( gstrCounterValueField, vtValue );
				ipRS->Update();

				// Skip this item unless local counter is multiple of skip count
				if (lSkipCount != 0)
				{
					// Just return the empty string
					*pbstrRuleExecutionID = bstrID.Detach();
					return S_OK;
				}
			}
		}

		// Determine Start Time for new record
		SYSTEMTIME	st;
		GetSystemTime( &st );
		CTime	t( st );
		__time64_t t64Time = t.GetTime();

		// Add the new record and retrieve associated Rule ID
		long lNewRuleID = writeNewStartTime( t64Time );

		// Handle Source Document and database field
		handleSourceDoc( lNewRuleID, pAFDoc );

		// Write RSD File name
		string	strRSDFile = asString( bstrRSDFileName );
		writeRSDFileName( lNewRuleID, strRSDFile );

		// Write Computer name
		string strComputer = getComputerName();
		writeComputerName( lNewRuleID, strComputer );

		// Create a StopWatch object to track Duration
		StopWatch	Watch;
		Watch.start();
		m_mapDurationTimers[lNewRuleID] = Watch;

		// Return Rule ID to caller
		CString zID;
		zID.Format( "%ld", lNewRuleID );
		*pbstrRuleExecutionID = _bstr_t( LPCTSTR(zID) ).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08477")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_ClearFeedbackData(VARIANT_BOOL bShowPrompt)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		// Get and validate Feedback folder
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (isFileOrFolderValid( strFeedbackFolder ))
		{
			int iResult = IDYES;

			// Present MessageBox if needed
			if (bShowPrompt == VARIANT_TRUE)
			{
				iResult = MessageBox( NULL, 
					"Are you sure all files in the Feedback folder should be deleted?", 
					"Confirm Clear Feedback", MB_YESNO | MB_ICONQUESTION );
			}

			// Act on response
			if (iResult == IDYES)
			{
				////////////////////////////////////////
				// Delete records in RuleExecution table
				// Reset SkipCount in Counter table
				////////////////////////////////////////
				if (openDBConnection( strFeedbackFolder ))
				{
					// Remove all files from the Feedback folder
					vector<string> vecSubDirs;
					getAllSubDirsAndDeleteAllFiles( strFeedbackFolder, vecSubDirs );

					// Clear RuleExecution
					_variant_t vtQuery( "SELECT * FROM RuleExecution" );

					// Retrieve recordset
					_RecordsetPtr	ipRS( __uuidof(Recordset) );
					ipRS->Open( vtQuery , 
						_variant_t((IDispatch *)m_ipConnection),
						adOpenDynamic,
						adLockPessimistic,
						adCmdText);

					while (ipRS->adoEOF == VARIANT_FALSE)
					{
						// Delete this record
						ipRS->Delete( adAffectCurrent );

						// Move to the next record
						ipRS->MoveNext();
					}

					ipRS->Close();

					// Define query
					vtQuery.SetString("SELECT * FROM Counter WHERE CounterName = 'SkipCount'");

					// Retrieve recordset
					ipRS->Open( vtQuery , 
						_variant_t((IDispatch *)m_ipConnection),
						adOpenDynamic,
						adLockPessimistic,
						adCmdText);

					// Reset SkipCount
					if (ipRS->adoEOF == VARIANT_FALSE)
					{
						_variant_t	vtValue;
						vtValue.vt = VT_I4;
						vtValue.lVal = 0;
						ipRS->PutCollect( gstrCounterValueField, vtValue );
						ipRS->Update();
					}
				}
				else
				{
					MessageBox( NULL, 
						"A connection to the Feedback database could not be made, files and records will not be deleted.", 
						"Error", MB_OK | MB_ICONSTOP );
				}
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09121")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_GetFeedbackRecords(IUnknown** ppFeedbackRecords)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		ASSERT_ARGUMENT("ELI28092", ppFeedbackRecords != __nullptr);

		// Default to NULL Recordset
		*ppFeedbackRecords = NULL;

		//////////////////////////////////////////////
		// Retrieve all records in RuleExecution table
		//////////////////////////////////////////////
		string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
		if (isFileOrFolderValid( strFeedbackFolder ))
		{
			if (openDBConnection( strFeedbackFolder ))
			{
				// Construct query to retrieve specified records
				_variant_t vtQuery( "SELECT * FROM RuleExecution" );

				// Retrieve recordset
				_RecordsetPtr	ipRS( __uuidof(Recordset) );
				ipRS->Open( vtQuery , 
					_variant_t((IDispatch *)m_ipConnection),
					adOpenStatic,
					adLockPessimistic,
					adCmdText);

				if (ipRS->adoEOF == VARIANT_FALSE)
				{
					// Provide this Recordset to the caller
					*ppFeedbackRecords = ipRS.Detach();
				}
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10185")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_CloseConnection()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		// Continue only if connection is already open
		if (m_bConnectionOpen)
		{
			// Close the connection
			m_ipConnection->Close();
			m_bConnectionOpen = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10240")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFeedbackMgr::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::handleSourceDoc(long lRuleID, IAFDocument *pAFDoc)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return;
	}

	// Get local AFDocument object pointer
	IAFDocumentPtr	ipDoc( pAFDoc );
	ASSERT_RESOURCE_ALLOCATION( "ELI10175", ipDoc != __nullptr );

	// Get document text
	ISpatialStringPtr ipText = ipDoc->Text;
	ASSERT_RESOURCE_ALLOCATION("ELI15607", ipText != __nullptr);

	// Source Document Name
	string strSourceDocName = ipText->SourceDocName;

	// Check for source document to be collected at rule execution time
	long lSource = ma_pCfgFeedbackMgr->getDocumentCollection();
	if (lSource == 1)
	{
		// Validate file name
		if (isFileOrFolderValid( strSourceDocName ))
		{
			// Get plain file name
			string	strName = getFileNameFromFullPath( strSourceDocName );

			// Prepend feedback folder
			string	strFeedbackFolder = ma_pCfgFeedbackMgr->getFeedbackFolder();
			string	strFullPath = strFeedbackFolder + "\\" + strName;

			// Get file extension
			string	strExt = getExtensionFromFullPath( strFullPath );

			// Construct new filename for feedback folder
			CString	zID;
			zID.Format( "%08d", lRuleID );
			string	strNewPath = strFeedbackFolder + "\\" + LPCTSTR(zID) + strExt;

			// Check for conversion of source document to text
			if (ma_pCfgFeedbackMgr->getDocumentConversion())
			{
				// Append ".uss" to paths
				strSourceDocName += ".uss";
				strNewPath += ".uss";

				// If text file exists
				if (isFileOrFolderValid( strSourceDocName ))
				{
					copyFile(strSourceDocName, strNewPath, true);
				}
				// Otherwise, save the Spatial String to the feedback folder
				else
				{
					ipText->SaveTo( strNewPath.c_str(), VARIANT_TRUE, 
						VARIANT_TRUE );
				}
			}
			// No, just retain the original source document
			else
			{
				// Copy file to feedback folder
				copyFile(strSourceDocName, strNewPath, true);
			}
		}		// end if Source Doc file found
	}			// end if Source Doc collection at rule execution time

	///////////////////////////////
	// Always store Source Doc name
	// if available
	///////////////////////////////
	if (strSourceDocName.length() == 0)
	{
		return;
	}

	// Construct query to retrieve specified record
	CString	zQuery;
	zQuery.Format( "SELECT * FROM RuleExecution WHERE RuleID = %ld", lRuleID );

	// Retrieve single-item recordset with this Rule ID
	_RecordsetPtr	ipRS( __uuidof(Recordset) );
	ipRS->Open( LPCTSTR(zQuery) , 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset,
		adLockPessimistic,
		adCmdText);

	// Update the Source Doc fields in this record
	if (ipRS->adoEOF == VARIANT_FALSE)
	{
		// Set the Source Doc
		_variant_t	vtValue;
		vtValue.vt = VT_BSTR;

		vtValue.bstrVal = _bstr_t( strSourceDocName.c_str() ).copy();
		ipRS->PutCollect( gstrSourceDocField, vtValue );

		// Set the Package Source Doc
		_variant_t	vtPackage;
		vtPackage.vt = VT_I1;
		if (lSource == 0)
		{
			vtPackage.bVal = 0;
		}
		else
		{
			vtPackage.bVal = 1;
		}
		ipRS->PutCollect( gstrPackageSrcDocField, vtPackage );

		// Apply the updated record to the database
		ipRS->Update();
	}
}
//-------------------------------------------------------------------------------------------------
bool CFeedbackMgr::openDBConnection(const string& strFeedbackFolder)
{
	// Just return if connection is already open
	if (m_bConnectionOpen)
	{
		return true;
	}

	// Validate feedback folder
	if (!isFileOrFolderValid( strFeedbackFolder ))
	{
		// Throw exception
		UCLIDException ue( "ELI08767", "Invalid folder for UCLID Feedback!" );
		ue.addDebugInfo( "Folder", strFeedbackFolder );
		throw ue;
	}

	//////////////////////////////
	// Construct database filename
	//////////////////////////////
	string	strDir = getModuleDirectory( _Module.m_hInst );
	string	strFileName = strDir + "\\" + gstrINDEXFILE;

	// Confirm presence of database
	if (!isFileOrFolderValid( strFileName ))
	{
		// Throw exception
		UCLIDException ue( "ELI10148", "Unable to locate Feedback database!" );
		ue.addDebugInfo( "Filename", strFileName );
		throw ue;
	}
	else
	{
		// Get current file attributes
		DWORD	dwAttributes = GetFileAttributes( strFileName.c_str() );

		// Prompt user for permission to remove read-only flag
		if (dwAttributes & FILE_ATTRIBUTE_READONLY)
		{
			CString zPrompt;
			zPrompt.Format( "The database file must be writable to collect Feedback.  Modify file attributes?" );
			int iResult = MessageBox( NULL, zPrompt, "Warning", MB_YESNO | MB_ICONQUESTION );
			if (iResult == IDNO)
			{
				return false;
			}
			else
			{
				// Remove read-only attribute from database file
				dwAttributes &= ~FILE_ATTRIBUTE_READONLY;
				SetFileAttributes( strFileName.c_str(), dwAttributes );
			}
		}
	}

	/////////////////////////
	// Construct DSN filename
	/////////////////////////
	string	strDSNFile = strDir + "\\" + gstrDSNFILE;

	// Confirm presence of DSN file
	if (!isFileOrFolderValid( strDSNFile ))
	{
		// Throw exception
		UCLIDException ue( "ELI10182", "Unable to locate DSN file!" );
		ue.addDebugInfo( "Filename", strDSNFile );
		throw ue;
	}
	else
	{
		// Get current file attributes
		DWORD	dwAttributes = GetFileAttributes( strDSNFile.c_str() );

		// Prompt user for permission to remove read-only flag
		if (dwAttributes & FILE_ATTRIBUTE_READONLY)
		{
			CString zPrompt;
			zPrompt.Format( "The DSN file must be writable to collect Feedback.  Modify file attributes?" );
			int iResult = MessageBox( NULL, zPrompt, "Warning", MB_YESNO | MB_ICONQUESTION );
			if (iResult == IDNO)
			{
				return false;
			}
			else
			{
				// Remove read-only attribute from DSN file
				dwAttributes &= ~FILE_ATTRIBUTE_READONLY;
				SetFileAttributes( strDSNFile.c_str(), dwAttributes );
			}
		}

		// Update DBQ setting in DSN file to reflect database location
		::WritePrivateProfileString( "ODBC", "DBQ", strFileName.c_str(), strDSNFile.c_str() );
	}

	/////////////////////////////////////////////
	// Open a connection to the Feedback database
	/////////////////////////////////////////////
	try
	{
		try
		{
			CString zDSNPath;
			zDSNPath.Format( "FILEDSN=%s\\%s", strDir.c_str(), gstrDSNFILE.c_str() );

			// Open the ADO connection
			m_ipConnection.CreateInstance( __uuidof(Connection) );
			m_ipConnection->Open( LPCTSTR(zDSNPath), "", "", -1 );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15379")
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI15380", "Unable to open connection to feedback database!", ue);
		uexOuter.log();
		return false;
	}

	// Set connection flag
	m_bConnectionOpen = true;

	return true;
}
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::writeComputerName(long lRuleID, const string& strName)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return;
	}

	// Construct query to retrieve specified record
	CString	zQuery;
	zQuery.Format( "SELECT * FROM RuleExecution WHERE RuleID = %ld", lRuleID );

	// Retrieve single-item recordset with this Rule ID
	_RecordsetPtr	ipRS( __uuidof(Recordset) );
	ipRS->Open( LPCTSTR(zQuery) , 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset,
		adLockPessimistic,
		adCmdText);

	// Update the fields in this record
	if (ipRS->adoEOF == VARIANT_FALSE)
	{
		_variant_t	vtValue(strName.c_str());
		ipRS->PutCollect( gstrComputerField, vtValue );

		// Apply the updated record to the database
		ipRS->Update();
	}
}
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::writeCorrectTime(long lRuleID, __time64_t t64Time)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return;
	}

	// Construct query to retrieve specified record
	CString	zQuery;
	zQuery.Format( "SELECT * FROM RuleExecution WHERE RuleID = %ld", lRuleID );


	// Retrieve single-item recordset with this Rule ID
	_RecordsetPtr	ipRS( __uuidof(Recordset) );
	ipRS->Open( LPCTSTR(zQuery), 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset,
		adLockPessimistic,
		adCmdText);

	// Update the fields in this record
	if (ipRS->adoEOF == VARIANT_FALSE)
	{
		// Set the CorrectTime
		_variant_t	vtValue;
		vtValue.vt = VT_I8;
		vtValue.llVal = t64Time;
		ipRS->PutCollect( gstrCorrectTimeField, vtValue );

		// Apply the updated record to the database
		ipRS->Update();
	}
}
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::writeDuration(long lRuleID, double dSeconds)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return;
	}

	// Construct query to retrieve specified record
	CString	zQuery;
	zQuery.Format( "SELECT * FROM RuleExecution WHERE RuleID = %ld", lRuleID );

	// Retrieve single-item recordset with this Rule ID
	_RecordsetPtr	ipRS( __uuidof(Recordset) );
	ipRS->Open( LPCTSTR(zQuery) , 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset,
		adLockPessimistic,
		adCmdText);

	// Update the fields in this record
	if (ipRS->adoEOF == VARIANT_FALSE)
	{
		// Set the Duration
		_variant_t vtValue(dSeconds);
		ipRS->PutCollect( gstrDurationField, vtValue );

		// Apply the updated record to the database
		ipRS->Update();
	}
}
//-------------------------------------------------------------------------------------------------
//long CFeedbackMgr::writeNewStartTime(DWORD dwTime)
long CFeedbackMgr::writeNewStartTime(__time64_t t64Time)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return 0;
	}

	// Create empty recordset object
	_RecordsetPtr	ipRS( __uuidof(Recordset) );

	// Open the recordset as an Table operation
	// NOTE: Must use adOpenKeyset to have RuleID available after ipRS->Update()
	ipRS->Open( "RuleExecution", 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset, 
		adLockPessimistic,
		adCmdTable );

	// Add a new record to the table
	ipRS->AddNew();

	// Set the Start Time field
	_variant_t	vtValue;
	vtValue.vt = VT_I8;
	vtValue.llVal = t64Time;
	ipRS->PutCollect( gstrStartTimeField, vtValue );

	// Update the record
	ipRS->Update();

	// Retrieve Rule ID for return to caller
	long lNewRuleID = 0;
	vtValue = ipRS->GetCollect( gstrRuleIDField );
	if (vtValue.vt == VT_I4)
	{
		lNewRuleID = vtValue.lVal;
	}

	return lNewRuleID;
}
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::writeRSDFileName(long lRuleID, const string& strRSDFileName)
{
	// Just return if connection is not already open
	if (!m_bConnectionOpen || (m_ipConnection == __nullptr))
	{
		return;
	}

	// Construct query to retrieve specified record
	CString	zQuery;
	zQuery.Format( "SELECT * FROM RuleExecution WHERE RuleID = %ld", lRuleID );

	// Retrieve single-item recordset with this Rule ID
	_RecordsetPtr	ipRS( __uuidof(Recordset) );
	ipRS->Open( LPCTSTR(zQuery) , 
		_variant_t((IDispatch *)m_ipConnection),
		adOpenKeyset,
		adLockPessimistic,
		adCmdText);

	// Update the fields in this record
	if (ipRS->adoEOF == VARIANT_FALSE)
	{
		// Set the RSD File Name
		_variant_t	vtValue(strRSDFileName.c_str());
		ipRS->PutCollect( gstrRSDFileField, vtValue );

		// Apply the updated record to the database
		ipRS->Update();
	}
}
//-------------------------------------------------------------------------------------------------
void CFeedbackMgr::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnSIMPLE_RULE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI07917", 
					"Feedback Manager" );
}
//-------------------------------------------------------------------------------------------------
