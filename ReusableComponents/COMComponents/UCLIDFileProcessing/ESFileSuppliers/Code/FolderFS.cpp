// FolderFS.cpp : Implementation of CFolderFS

#include "stdafx.h"
#include "FolderFS.h"
#include "FileSupplierUtils.h"
#include "ESFileSuppliers.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CFolderFS
//-------------------------------------------------------------------------------------------------
CFolderFS::CFolderFS()
: m_bDirty(false),
	m_strFolderName(""),
	m_bRecurseFolders(false),
	m_bAddedFiles(false),
	m_bModifiedFiles(false),
	m_bTargetOfMoveOrRename(false),
	m_bNoExistingFiles(false),
	m_StopEvent(),
	m_eventSearchingExited(),
	m_ipTarget(NULL),
	m_eventSupplyingStarted(),
	m_strExpandFolderName(""),
	m_eventResume(),
	m_pSearchThread(NULL),
	m_nCurrentSearchThreadID(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFolderFS,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFileSupplier
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19634", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Files from folder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13744")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if(pObject != __nullptr)
		{
			// Validate license first
			validateLicense();

			ICopyableObjectPtr ipObjCopy;
			ipObjCopy.CreateInstance(CLSID_FolderFS);
			ASSERT_RESOURCE_ALLOCATION("ELI13746", ipObjCopy != __nullptr);

			IUnknownPtr ipUnk = this;
			ipObjCopy->CopyFrom(ipUnk);

			// Return the new object to the caller
			*pObject = ipObjCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13745")

	return S_OK;	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFolderFS(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13769", ipFolderFS != __nullptr );

		m_strFolderName = ipFolderFS->FolderName;

		// parse the string into individual extension
		string strFileExtensions = asString(ipFolderFS->FileExtensions);

		StringTokenizer::sGetTokens(strFileExtensions, ";", m_vecFileExtensions);

		m_bRecurseFolders = ipFolderFS->RecurseFolders == VARIANT_TRUE;
		m_bAddedFiles = ipFolderFS->AddedFiles == VARIANT_TRUE;
		m_bModifiedFiles = ipFolderFS->ModifiedFiles == VARIANT_TRUE;
		m_bTargetOfMoveOrRename = ipFolderFS->TargetOfMoveOrRename == VARIANT_TRUE;
		m_bNoExistingFiles = ipFolderFS->NoExistingFiles == VARIANT_TRUE;
		
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19418")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileSupplier Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_Start(IFileSupplierTarget * pTarget, IFAMTagManager *pFAMTM,
	IFileProcessingDB* pDB, long nActionID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI20453", pTarget != __nullptr);
		ASSERT_ARGUMENT("ELI20454", pFAMTM != __nullptr);

		try
		{
			try
			{
				// Pass an empty string as the second parameter because file supplier doesn't support <SourceDocName> tag 
				// and we also don't have a source doc name to expand it [P13: 3901]
				m_strExpandFolderName = CFileSupplierUtils::ExpandTagsAndTFE(pFAMTM, m_strFolderName, "");

				// Check if the folder exists [P13: 4121, 4132]
				if (!isFileOrFolderValid(m_strExpandFolderName))
				{
					// Prompt information for the user that the folder does not exist
					string strInfo = "\"" + m_strExpandFolderName + "\"" + " does not exist!";

					// Notify the supplying target that the supplying failed
					pTarget->NotifyFileSupplyingFailed(this, strInfo.c_str());

					return S_OK;
				}

				// Check to see if suppling has already been started and is not done
				if ( m_eventSupplyingStarted.isSignaled() && !m_eventSearchingExited.isSignaled())
				{
					return S_OK;
				}

				// Signal the Resume Event(not paused)
				m_eventResume.signal();

				// Signal that the supplying has started
				m_eventSupplyingStarted.signal();

				m_ipTarget = pTarget;
				m_StopEvent.reset();
				m_eventSearchingExited.reset();

				if ( !m_bNoExistingFiles )
				{
					// start search thread
					m_pSearchThread = AfxBeginThread(searchFileThread, this, 0, CREATE_SUSPENDED );

					// [LegacyRCAndUtils:6294]
					// Since we can't count on being able to check m_pSearchThread->m_nThreadID
					// later, make a note of what it is now.
					m_nCurrentSearchThreadID = m_pSearchThread->m_nThreadID;

					m_pSearchThread->ResumeThread();
				}

				if ( m_bAddedFiles || m_bModifiedFiles || m_bTargetOfMoveOrRename )
				{
					// Renames and deletes always need to be monitored to maintain the database, but
					// only monitor file/folder add and modify events as required.
					BYTE byteEventTypes =
						kFileRemoved | kFileRenamed | kFolderRemoved | kFolderRenamed;
					byteEventTypes |= m_bAddedFiles ? kFileAdded : 0;
					byteEventTypes |= m_bModifiedFiles ? kFileModified : 0;

					// start listening
					startListening(m_strExpandFolderName, m_bRecurseFolders, byteEventTypes);
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15298");
		}
		catch (UCLIDException ue)
		{
			// Need to stop everything that could have started
			// Signal stop event to stop the search for files if adding existing files
			if (!m_bNoExistingFiles)
			{
				m_StopEvent.signal();
			}

			// Stop listening for files if listening
			if ( m_bAddedFiles || m_bModifiedFiles || m_bTargetOfMoveOrRename )
			{
				// stopListening call can throw an exception so it needs to be handled
				try
				{
					stopListening();
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI30031");
			}

			// Notify the supplying target that the supplying failed
			pTarget->NotifyFileSupplyingFailed(this, ue.asStringizedByteStream().c_str());
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13749")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_Stop()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI03269");

	try
	{
		// NOTE: Do not call validateLicense here because we want to be able 
		//   to gracefully stop processing even if the license state is corrupted.
		// validateLicense();

		// If this is the search thread, run this method in a new thread to avoid deadlock.
		// [FlexIDSCore #3463]
		_lastCodePos = "10";
		if (m_nCurrentSearchThreadID == GetCurrentThreadId())
		{
			if (m_eventSearchingExited.isSignaled())
			{
				UCLIDException ue("ELI34359",
					"Unexpected situation encountered stopping file searching thread.");
				ue.log();
			}
			else
			{
				AfxBeginThread(stopSearchFileThread, this);
			}

			return S_OK;
		}

		// Signal Search threads to stop
		_lastCodePos = "20";
		m_StopEvent.signal();

		// Stop listening if it is enabled
		if ( m_bAddedFiles || m_bModifiedFiles || m_bTargetOfMoveOrRename )
		{
			_lastCodePos = "30";
			stopListening();
		}
		_lastCodePos = "40";

		if ( !m_bNoExistingFiles )
		{
			_lastCodePos = "50";
			// Wait for search thread to exit
			m_eventSearchingExited.wait();
		}
		_lastCodePos = "60";

		// reset the Suppling started event
		m_eventSupplyingStarted.reset();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13750")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_Pause()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		if ( !m_eventResume.isSignaled() )
		{
			return S_OK;
		}
		if ( !m_bNoExistingFiles && !m_eventSearchingExited.isSignaled() )
		{
			// Reset the resume event (Pause)
			m_eventResume.reset();
		}
		// Stop listening if it is enabled
		if ( m_bAddedFiles || m_bModifiedFiles || m_bTargetOfMoveOrRename )
		{
			stopListening();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13751")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_Resume()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		if ( !m_eventResume.isSignaled() )
		{
			if ( !m_bNoExistingFiles && !m_eventSearchingExited.isSignaled() )
			{
				// resume search thread
				// Signal Resume event
				m_eventResume.signal();
			}
		}
		// Restart listening if it is enabled
		if ( m_bAddedFiles || m_bModifiedFiles || m_bTargetOfMoveOrRename )
		{
			// start listening
			startListening( m_strExpandFolderName, m_bRecurseFolders);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13752")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::raw_IsConfigured(VARIANT_BOOL * bConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
		
		*bConfigured = VARIANT_TRUE; 
		
		// Must have a folder name
		if ( m_strFolderName.empty() )
		{
			*bConfigured = VARIANT_FALSE;
		}
		// Must specify an extension
		if ( m_vecFileExtensions.empty() )
		{
			*bConfigured = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13748");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_FolderFS;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		dataReader >> m_strFolderName;

		// get flags
		dataReader >> m_bRecurseFolders;
		dataReader >> m_bAddedFiles;
		dataReader >> m_bModifiedFiles;
		dataReader >> m_bTargetOfMoveOrRename;
		dataReader >> m_bNoExistingFiles;
		
		// get File extensions
		m_vecFileExtensions.clear();
		long nSize;
		dataReader >> nSize;
		for ( long n = 0; n < nSize; n++ )
		{
			string str;
			dataReader >> str;
			m_vecFileExtensions.push_back(str);
		}
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13932");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::Save(IStream *pStream, BOOL fClearDirty)
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

		dataWriter << m_strFolderName;

		// Save flags
		dataWriter << m_bRecurseFolders;
		dataWriter << m_bAddedFiles;
		dataWriter << m_bModifiedFiles;
		dataWriter << m_bTargetOfMoveOrRename;
		dataWriter << m_bNoExistingFiles;

		// Save File extensions
		long nSize =  (long)m_vecFileExtensions.size();
		dataWriter << nSize;
		for each ( string str in m_vecFileExtensions )
		{
			dataWriter << str;
		}

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13933");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
// IFolderFS
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_FolderName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFolder = asString(newVal);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14442", ipFAMTagManager != __nullptr);

		// make sure the file exists
		// or that it contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFolder.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14443", "The folder string contains invalid tags!");
			ue.addDebugInfo("Folder string", strFolder);
			throw ue;
		}

		m_strFolderName = strFolder;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13763");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_FolderName(BSTR *pVal )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13765", pVal );
		
		*pVal = _bstr_t(m_strFolderName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13764");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_FileExtensions(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// parse the string into individual extension
		string strFileExtensions = asString(newVal);
		if (strFileExtensions.size() <= 0)
		{
			UCLIDException ue("ELI13894", "Invalid blank file extension specified.");
			throw ue;
		}

		// Separate string into multiple file extensions
		StringTokenizer::sGetTokens(strFileExtensions, ";", m_vecFileExtensions);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13771");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_FileExtensions(BSTR *pVal )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI13772", pVal );

		// form a string that contains user defined extensions 
		// separated by ;
		string strFileExtensions("");
		for (unsigned int n = 0; n < m_vecFileExtensions.size(); n++)
		{
			if (n > 0)
			{
				strFileExtensions += ";";
			}
			// add file extension to string
			strFileExtensions += m_vecFileExtensions[n];
		}

		*pVal = _bstr_t(strFileExtensions.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13773");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_RecurseFolders( VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_bRecurseFolders = bVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13776");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_RecurseFolders(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13777", pbVal );
		
		*pbVal = m_bRecurseFolders ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13778");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_AddedFiles( VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_bAddedFiles = bVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13779");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_AddedFiles(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13780", pbVal );
		
		*pbVal = m_bAddedFiles ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13781");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_ModifiedFiles( VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_bModifiedFiles = bVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13782");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_ModifiedFiles(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13783", pbVal );
		
		*pbVal = m_bModifiedFiles ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13784");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_TargetOfMoveOrRename( VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_bTargetOfMoveOrRename = bVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13785");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_TargetOfMoveOrRename(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13786", pbVal );
		
		*pbVal = m_bTargetOfMoveOrRename ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13787");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::put_NoExistingFiles( VARIANT_BOOL bVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_bNoExistingFiles = bVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13789");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFolderFS::get_NoExistingFiles(VARIANT_BOOL *pbVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();
		
		ASSERT_ARGUMENT("ELI13790", pbVal );
		
		*pbVal = m_bNoExistingFiles ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13791");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// FileDirectorySearcherBase overridden functions
//-------------------------------------------------------------------------------------------------
void CFolderFS::addFile( const std::string &strFile )
{
	try
	{
		// Wait for Resume event to be signaled
		m_eventResume.wait();
		if ( m_ipTarget != __nullptr )
		{
			m_ipTarget->NotifyFileAdded( strFile.c_str(), this );
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15022");
}
//-------------------------------------------------------------------------------------------------
bool CFolderFS::shouldStop()
{
	return m_StopEvent.isSignaled();
}

//-------------------------------------------------------------------------------------------------
// FolderEventsListener overridden functions
//-------------------------------------------------------------------------------------------------
bool CFolderFS::fileMatchPattern(const std::string& strFileName)
{
	try
	{
		string strTmp = getFileNameFromFullPath(strFileName);
		return doesFileMatchPatterns(m_vecFileExtensions, strTmp);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI49660");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFileAdded(const std::string& strFileName)
{
	try
	{
		// if processing added files
		if ( m_ipTarget != __nullptr && m_bAddedFiles)
		{
			string strTmp = getFileNameFromFullPath(strFileName);
			// Only Notify target if the file has an extension should be processed
			if (doesFileMatchPatterns(m_vecFileExtensions, strTmp))
			{
				m_ipTarget->NotifyFileAdded( strFileName.c_str(), this );
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13928");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFileRemoved(const std::string& strFileName)
{
	try
	{
		if ( m_ipTarget != __nullptr  )
		{
			string strTmp = getFileNameFromFullPath(strFileName);
			// Only Notify target if the file has an extension should be processed
			if (doesFileMatchPatterns(m_vecFileExtensions, strTmp))
			{
				m_ipTarget->NotifyFileRemoved ( strFileName.c_str(), this );
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13929");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFileRenamed(const std::string& strOldName, const std::string strNewName)
{
	try
	{
		if ( m_ipTarget != __nullptr )
		{
			// determine if old file has extension should be processed
			string strTmp = getFileNameFromFullPath(strOldName);
			bool bOldFileMatches = doesFileMatchPatterns(m_vecFileExtensions, strTmp);
			
			// determine if old file has extension should be processed
			strTmp = getFileNameFromFullPath(strNewName);
			bool bNewFileMatches = doesFileMatchPatterns(m_vecFileExtensions, strTmp);

			if ( bOldFileMatches && bNewFileMatches && m_bTargetOfMoveOrRename )
			{
				// if processing target of Move or rename and old and new file have extension should be processed notify target
				m_ipTarget->NotifyFileRenamed ( strOldName.c_str(), strNewName.c_str(), this );
			}
			else if ( bOldFileMatches )
			{
				// if only old file should be processed remove notify target that the file has been removed
				m_ipTarget->NotifyFileRemoved ( strOldName.c_str(), this );
			}
			else if ( bNewFileMatches && m_bTargetOfMoveOrRename )
			{
				// if only new file is being processed notify target 
				m_ipTarget->NotifyFileAdded ( strNewName.c_str(), this );
			}			
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13930");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFileModified(const std::string& strFileName)
{
	try
	{
		// Only process if looking for modified files
		if ( m_ipTarget != __nullptr && m_bModifiedFiles )
		{
			string strTmp = getFileNameFromFullPath(strFileName);
			// Only Notify target if the file has an extension should be processed
			if (doesFileMatchPatterns(m_vecFileExtensions, strTmp))
			{
				m_ipTarget->NotifyFileModified ( strFileName.c_str(), this );
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13931");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFolderRemoved(const std::string& strFolderName)
{
	try
	{
		if (m_ipTarget != __nullptr)
		{
			m_ipTarget->NotifyFolderDeleted( strFolderName.c_str(), this);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14132");
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::onFolderRenamed(const std::string& strOldName, const std::string& strNewName)
{
	try
	{
		if (m_ipTarget != __nullptr)
		{
			m_ipTarget->NotifyFolderRenamed(strOldName.c_str(), strNewName.c_str(), this);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI14133");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFolderFS::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13747", "Folder File Supplier");
}
//-------------------------------------------------------------------------------------------------
UINT CFolderFS::searchFileThread(LPVOID pParam )
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	
	try
	{
		CFolderFS *pFolderFS = static_cast<CFolderFS *> (pParam);
		ASSERT_RESOURCE_ALLOCATION("ELI25250", pFolderFS != __nullptr);

		IFileSupplierTargetPtr ipTarget = pFolderFS->m_ipTarget;
		ASSERT_RESOURCE_ALLOCATION("ELI25252", ipTarget != __nullptr);

		try
		{
			try
			{
				// Call the search function
				pFolderFS->searchForFiles();
				if (!pFolderFS->m_bAddedFiles && !pFolderFS->m_bModifiedFiles && !pFolderFS->m_bTargetOfMoveOrRename)
				{
					// If not listening, Notify target supplying is done if not a requested stop
					if ( !pFolderFS->shouldStop() )
					{
						ipTarget->NotifyFileSupplyingDone(pFolderFS);
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14118");
		}
		catch (UCLIDException ue)
		{
			// Notify the target that the supplying failed
			ipTarget->NotifyFileSupplyingFailed(pFolderFS, ue.asStringizedByteStream().c_str());
		}

		// Signal that the searching thread has exited
		pFolderFS->m_eventSearchingExited.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25251")
	CoUninitialize();
	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT CFolderFS::stopSearchFileThread(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	
	try
	{
		IFileSupplierPtr ipSupplier = static_cast<CFolderFS *>(pParam);
		ASSERT_RESOURCE_ALLOCATION("ELI25499", ipSupplier != __nullptr);

		// Stop searching for files
		ipSupplier->Stop();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25500")

	CoUninitialize();
	return 0;
}
//-------------------------------------------------------------------------------------------------
void CFolderFS::searchForFiles()
{
	// Loop through all extensions to search for
	for (unsigned int n = 0; n < m_vecFileExtensions.size(); n++)
	{
		string strFileExtension = m_vecFileExtensions[n];
		string strFileSpec = m_strExpandFolderName + "\\" + strFileExtension;
		findFiles ( strFileSpec, m_bRecurseFolders );
		if ( shouldStop() )
		{
			return;
		}
	}
}
//-------------------------------------------------------------------------------------------------
