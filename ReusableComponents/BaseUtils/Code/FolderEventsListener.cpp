#define _WIN32_WINNT 0x0400 
#include "stdafx.h"
#include "FolderEventsListener.h"
#include "cpputil.h"
#include "RegConstants.h"

#include <iostream>

// globals
const unsigned long gulFOLDER_LISTENER_BUF_SIZE	 = 65536;
const string gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY = "FEL_TimeBetweenListeningRestarts";
const unsigned long gulDEFAULT_TIME_BETWEEN_RESTARTS = 60000; // 60 seconds

//-------------------------------------------------------------------------------------------------
FolderEventsListener::FolderEvent::FolderEvent(EFileEventType nEvent, string strFileNameNew, string strFileNameOld)
:
m_nEvent(nEvent),
m_strFileNameNew(strFileNameNew),
m_strFileNameOld(strFileNameOld)
{
}
//-------------------------------------------------------------------------------------------------
FolderEventsListener::FolderEventsListener() 
: 
m_pCurrThreadData(NULL),
m_pthreadDispatch(NULL)
{
	ma_pCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, gstrBASEUTILS_REG_PATH ));

}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::startListening(const std::string strFolder, bool bRecursive,
										  BYTE eventTypeFlags/* = 0xFF*/)
{
	
	// Currently Folder Events listener supports listening on 
	// only one folder at a time so if we are to start listening on 
	// and new folder we need to any folder we are currently listening on
	if(m_pCurrThreadData)
	{
		stopListening();
	}

	m_eventTypeFlags = eventTypeFlags;

	// Create a new thread Data structure that will be used for communication between
	// this thread and the one we are about to create
	// the thread function will delete the ThreadData before exiting
	m_pCurrThreadData = new ThreadData();
	m_pCurrThreadData->m_eventKillThread.reset();
	m_eventListeningExited.reset();

	m_pCurrThreadData->m_strFilename = strFolder;
	m_pCurrThreadData->m_bRecursive = bRecursive;
	m_pCurrThreadData->m_pListener = this;
	m_pCurrThreadData->m_pmutexFolderListen = &m_mutexFolderListen;
	CSingleLock lg( &m_mutexFolderListen, TRUE );
	m_pCurrThreadData->m_pThread = AfxBeginThread(threadFuncListen, m_pCurrThreadData);

	// make sure the events are reset for the dispatch thread
	m_eventKillDispatchThread.reset();
	m_eventDispatchThreadExit.reset();
	m_pthreadDispatch = AfxBeginThread(threadDispatchEvents, this);

	// Wait for the thread to begin executing before returning 
	m_eventFolderThreadBegin.wait(10000);
	m_eventFolderThreadBegin.reset();
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::stopListening()
{
	try
	{
		CSingleLock lg( &m_mutexFolderListen, TRUE );
		if ( m_eventListeningExited.isSignaled() ) 
		{
			// Thread has stopped and so this has already been deleted
			m_pCurrThreadData = NULL;
		}
		if(m_pCurrThreadData)
		{
			{
				m_pCurrThreadData->m_eventKillThread.signal();
			}
			m_pCurrThreadData = NULL;
		}

		if(m_pthreadDispatch)
		{
			m_eventKillDispatchThread.signal();
			m_eventDispatchThreadExit.wait(2000);
			m_pthreadDispatch = NULL;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27786");
}
//-------------------------------------------------------------------------------------------------
UINT FolderEventsListener::threadFuncListen(LPVOID pParam)
{
	FolderEventsListener* fl = NULL;
	try
	{
		// this is an autoptr to delete the data so that the caller doesn't have to
		auto_ptr<ThreadData> pTD((ThreadData*)pParam);
		ASSERT_RESOURCE_ALLOCATION("ELI25255", pTD.get() != NULL);
		
		// the Listener is the caller and will still be a valid pointer after the try...catch
		// this will be used to set the thread exit event
		fl = pTD->m_pListener;
		ASSERT_RESOURCE_ALLOCATION("ELI25256", fl != NULL);

		bool bRecursive = pTD->m_bRecursive;
		
		// let the the creating thread know it is ok to continue
		fl->m_eventFolderThreadBegin.signal();
		unsigned long ulTimeBetweenRestarts = gulDEFAULT_TIME_BETWEEN_RESTARTS;

		// Variable to track the number of times listening is started
		// This will be used to track the number of calls to getListeningHandle
		int nListeningStartCount = 0;

		do
		{
			// Setup events for changes
			Win32Event eventFileChangesReady;

			// Overlapped structures to contain the event and buffer info for each 
			// change process
			OVERLAPPED oFileChanges;
			
			// initialize to zero
			memset( &oFileChanges, 0, sizeof(oFileChanges));

			// set the event to signal completion of the overlapped IO
			oFileChanges.hEvent = eventFileChangesReady.getHandle();

			// Allocate buffers for the change operations
			LPBYTE lpFileBuffer = new BYTE[gulFOLDER_LISTENER_BUF_SIZE];

			// set up handle array for the handles to wait for
			HANDLE handles[2];
			handles[0] = oFileChanges.hEvent;
			handles[1] = pTD->m_eventKillThread.getHandle();

			// Listening handle for changes in the files
			HANDLE hFile;

			try
			{
				// Handle for file changes
				hFile = fl->getListeningHandle( pTD->m_strFilename );

				nListeningStartCount++;

				// Log an exception each time listening starts 
				UCLIDException ueStart("ELI29123", "Application trace: Listening started.");
				ueStart.addDebugInfo("Number times started", nListeningStartCount);
				ueStart.addDebugInfo("Folder",  pTD->m_strFilename);
				ueStart.log();

				bool bDone = false;
				do
				// There are many flags that can be specified other than FILE_NOTIFY_CHANGE_FILE_NAME
				{
					// Start listening for file changes
					fl->beginChangeListen(hFile, 
						FILE_NOTIFY_CHANGE_FILE_NAME | FILE_NOTIFY_CHANGE_SIZE,
						lpFileBuffer, oFileChanges,  bRecursive);

					// Wait for changes or a kill signal
					DWORD dwWaitResult = WaitForMultipleObjects( 2, (HANDLE *)&handles, FALSE, INFINITE );

					if ( dwWaitResult == WAIT_OBJECT_0 )
					{
						// File changes were detected

						DWORD dwBytesTransfered;

						// This call will wait for the event in the oChanges to be signalled so don't 
						// reset it until after this call
						int iResult = GetOverlappedResult( hFile, &oFileChanges, &dwBytesTransfered, TRUE );
						if ( iResult == FALSE )
						{
							// Error geting the results
							UCLIDException ue ("ELI13017", "Error getting file changes.");
							ue.addDebugInfo("Directory Name",
								(pTD.get() != NULL ? pTD->m_strFilename : "<Empty>"));
							ue.addWin32ErrorInfo();
							ue.addDebugInfo("Folder",  pTD->m_strFilename);
							throw ue;
						}

						// reset the event
						eventFileChangesReady.reset();

						CSingleLock lg ( pTD->m_pmutexFolderListen, TRUE );

						// Process the changes
						fl->processChanges(pTD->m_strFilename, pTD->m_eventKillThread, lpFileBuffer, fl->m_queEvents, true );

						// Check to see if Kill thread event was signaled
						bDone = pTD->m_eventKillThread.isSignaled();
					}
					else
					{
						// The kill event was signaled
						bDone = true;
					}
				}
				while (!bDone);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13161");

			// Clean up
			// Cancel any pending overlapped operations
			CancelIo( hFile );
			CloseHandle( hFile );
			delete [] lpFileBuffer;
			
			// Get the time to wait before restarting
			FolderEventsListener* pListener = pTD->m_pListener;
			ASSERT_RESOURCE_ALLOCATION("ELI25237", pListener != NULL);
			ulTimeBetweenRestarts = pListener->getTimeBetweenListeningRestarts();
		}
		while ( pTD->m_eventKillThread.wait( ulTimeBetweenRestarts ) == WAIT_TIMEOUT );

		CSingleLock lg ( pTD->m_pmutexFolderListen, TRUE );
		FolderEventsListener* pListener = pTD->m_pListener;
		ASSERT_RESOURCE_ALLOCATION("ELI25238", pListener != NULL);
		pListener->m_pCurrThreadData = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12780");
	
	// Signal that the thread has exited this always needs to be done
	try
	{
		// this could be NULL if it was not set before starting the thread 
		if ( fl != NULL )
		{
			fl->m_eventListeningExited.signal();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13091");

	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT FolderEventsListener::threadDispatchEvents(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{

		FolderEventsListener* fl = (FolderEventsListener*)pParam;
		try
		{
			vector<FolderEvent> vecEvents;

			while ( fl->m_eventKillDispatchThread.wait(1000) == WAIT_TIMEOUT )
			{
				while(fl->m_queEvents.getSize() > 0)
				{
					if(fl->m_eventKillDispatchThread.isSignaled())
					{
						break;
					}
					FolderEvent event;
					fl->m_queEvents.getTopAndPop(event);
					vecEvents.push_back(event);
				}

				unsigned int i;
				for(i = 0; i < vecEvents.size(); i++)
				{
					if(fl->m_eventKillDispatchThread.isSignaled())
					{
						break;
					}

					FolderEvent event = vecEvents[i];

					if(event.m_nEvent == kFileAdded || event.m_nEvent == kFileModified || event.m_nEvent == kFileRenamed)
					{
						if(!fl->fileReadyForAccess(event.m_strFileNameNew))
						{
							continue;	
						}
					}

					fl->dispatchEvent(event);
					vecEvents.erase(vecEvents.begin() + i);
					i--;
				}

				// [LegacyRCAndUtils:5258]
				// Call swap to free excess capacity so that after processing a large number
				// of events, the capacity allocated for FolderEvents in the vector is released.
				if (vecEvents.empty() && vecEvents.capacity() > 1000)
				{
					vecEvents.swap(vector<FolderEvent>());
				}
			}
		}
		catch(...)
		{
			fl->m_eventDispatchThreadExit.signal();
			throw;
		}
		fl->m_eventDispatchThreadExit.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI10197");
	CoUninitialize();
	return 0;
			
}
//-------------------------------------------------------------------------------------------------
bool FolderEventsListener::fileReadyForAccess(std::string strFileName)
{
	// if we cannot read the file we will try to run it later
	bool bReadable = true;
	try
	{
		CFile file(strFileName.c_str(), CFile::modeRead );
	}
	catch(CException* pEx)
	{
		bReadable = false;
		pEx->Delete();
	}
	return bReadable;
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::dispatchEvent(const FolderEvent& event)
{
	switch(event.m_nEvent)
	{
	case kFileAdded:
		onFileAdded(event.m_strFileNameNew);
		break;
	case kFileRemoved:
		onFileRemoved(event.m_strFileNameOld);
		break;
	case kFileModified:
		onFileModified(event.m_strFileNameNew);
		break;
	case kFileRenamed:
		onFileRenamed(event.m_strFileNameOld, event.m_strFileNameNew);
		break;
	case kFolderAdded:
		onFolderAdded(event.m_strFileNameNew );
		break;
	case kFolderRemoved:
		onFolderRemoved(event.m_strFileNameOld );
		break;
	case kFolderRenamed:
		onFolderRenamed(event.m_strFileNameOld, event.m_strFileNameNew);
		break;
	default:
		break;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long FolderEventsListener::getTimeBetweenListeningRestarts()
{
	if ( !ma_pCfgMgr->keyExists("", gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY ))
	{
		ma_pCfgMgr->createKey ("", gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY, asString(gulDEFAULT_TIME_BETWEEN_RESTARTS));
	}
	return asUnsignedLong( ma_pCfgMgr->getKeyValue("", gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY ));
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::processChanges(string strBaseDir, Win32Event &eventKill, LPBYTE & rlpBuffer, MTSafeQueue<FolderEvent> & queEvents, bool bFileChange )
{
	LPBYTE pCurrByte = rlpBuffer;
	PFILE_NOTIFY_INFORMATION pFileInfo = NULL;

	// rename events happen in two peices, rename old and rename new
	// when a rename old event happens we keep track of it so that we 
	// can have one event handler for rename that takes the old and new
	// names
	string strOldFilename = "";

	// Add file events will be immediately followed by a modify event for the same file. Keep track
	// of add file events so that if both events are being monitored, the modify event is considered
	// part of the add event rather than a stand-alone modify event.
	static string strLastAddedFilename = "";
	static DWORD dwAddFileTickTime = 0;

	// ierate through the buffer extracting the information that we need
	do
	{
		// has this thread been instructed to terminate ?? 
		if(eventKill.isSignaled())
		{
			// exit 
			return;
		}
		pFileInfo = (PFILE_NOTIFY_INFORMATION)pCurrByte;
		string strFilename;
		// get the name of the affected file
		int len = WideCharToMultiByte(CP_ACP, 0, pFileInfo->FileName, pFileInfo->FileNameLength / sizeof(WCHAR), 0, 0, 0, 0);
		LPSTR result = new char[len+1];
		ASSERT_RESOURCE_ALLOCATION("ELI12935", result != NULL);

		try
		{
			WideCharToMultiByte(CP_ACP, 0, pFileInfo->FileName, pFileInfo->FileNameLength / sizeof(WCHAR), result, len, 0, 0);
			result[len] = '\0';
			strFilename = removeLastSlashFromPath(strBaseDir)+ "\\" + result;
		}
		catch (...)
		{
			// delete result array if exception is thrown
			delete[] result;
			throw;
		}
		// delete result array
		delete[] result;

		EFileEventType eEventType = (EFileEventType)0;

		// call the appropriate virtual handler based on which action has taken place on the file
		switch(pFileInfo->Action)
		{
		case FILE_ACTION_ADDED:
			eEventType = bFileChange ? kFileAdded : kFolderAdded;
			break;
		case FILE_ACTION_REMOVED:
			{
				strOldFilename = strFilename;
				strFilename = "";
				eEventType = bFileChange ? kFileRemoved : kFolderRemoved;
			}
			break;
		case FILE_ACTION_MODIFIED:
			eEventType = bFileChange ? kFileModified : kFolderModified;
			break;
		case FILE_ACTION_RENAMED_OLD_NAME:
			strOldFilename = strFilename;
			break;
		case FILE_ACTION_RENAMED_NEW_NAME:
			eEventType = bFileChange ? kFileRenamed : kFolderRenamed;
			break;
		default:
			break;
		}

		// [LegacyRCAndUtils:5258]
		// Only process the event types being monitored.
		EFileEventType eFilteredEventType = (EFileEventType)(m_eventTypeFlags & eEventType);

		// [FlexIDSCore:3737]
		// Any modify/add events that immediately follow an add event on the same file
		// (and occurs within a second of the original add event) should be ignored.
		if ((eFilteredEventType == kFileModified  || eFilteredEventType == kFileAdded) &&
			strLastAddedFilename == strFilename && (GetTickCount() - dwAddFileTickTime) < 1000)
		{
			eFilteredEventType = (EFileEventType)0;
		}
		else if (eFilteredEventType == kFileAdded)
		{
			// If this is a new add event, keep track of the filename
			strLastAddedFilename = strFilename;
			dwAddFileTickTime = GetTickCount();
		}
		else
		{
			strLastAddedFilename = "";
		}

		if (eFilteredEventType != 0)
		{
			FolderEvent event(eEventType, strFilename, strOldFilename);
			queEvents.push(event);
		}

		// Reset strOldFilename after an event it which it was used.
		if ((int)eEventType != 0 && !strOldFilename.empty())
		{
			strOldFilename = "";
		}

		pCurrByte += pFileInfo->NextEntryOffset;
	}
	while(pFileInfo->NextEntryOffset != 0);
}
//-------------------------------------------------------------------------------------------------
HANDLE FolderEventsListener::getListeningHandle (std::string strDir )
{
	HANDLE hDir = CreateFile(	strDir.c_str(),								
		FILE_LIST_DIRECTORY,
		FILE_SHARE_READ|FILE_SHARE_DELETE|FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_BACKUP_SEMANTICS|FILE_FLAG_OVERLAPPED,
		NULL );

	if ( hDir == INVALID_HANDLE_VALUE )
	{
		UCLIDException ue("ELI13086", "Invalid Directory Handle!");
		ue.addDebugInfo("Directory Name", strDir);
		ue.addWin32ErrorInfo();
		throw ue;
	}
	return hDir;
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::beginChangeListen( HANDLE hDir, DWORD dwNotifyFilter, LPBYTE &lpBuffer, OVERLAPPED &oOverLapped, bool bRecursive)
{
	int iResult;
	DWORD dwBytesReturned;
	
	// Start listening for changes, the dwBytesReturned value can be
	// ignored because the changes will be returned asynchronously
	iResult = ::ReadDirectoryChangesW(	hDir, 
		(LPVOID)lpBuffer, 
		gulFOLDER_LISTENER_BUF_SIZE, 
		bRecursive, 
		dwNotifyFilter,
		&dwBytesReturned, &oOverLapped, NULL);

	if ( iResult == 0 )
	{
		DWORD dwErrCode = GetLastError();
		UCLIDException ue ( "ELI13016", "Error Listening to directory.");
		ue.addDebugInfo( "ErrorCode", dwErrCode );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------

