#include "stdafx.h"
#include "FolderEventsListener.h"
#include "cpputil.h"
#include "RegConstants.h"

#include <iostream>

// globals
const unsigned long gulFOLDER_LISTENER_BUF_SIZE	 = 65536;
const string gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY = "FEL_TimeBetweenListeningRestarts";
const unsigned long gulDEFAULT_TIME_BETWEEN_RESTARTS = 60000; // 60 seconds
const unsigned long gulTIME_TO_WAIT_FOR_THREAD_EXIT = 10000; // 10 seconds

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
{
	ma_pCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE, gstrBASEUTILS_REG_PATH ));

}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::startListening(const std::string &strFolder, bool bRecursive,
										  BYTE eventTypeFlags/* = 0xFF*/)
{
	try
	{
		// stop listening
		stopListening();

		m_eventTypeFlags = eventTypeFlags;

		// Reset the events for the Listening thread
		m_eventKillThreads.reset();
		m_eventListeningExited.reset();
		m_eventListeningStarted.reset();

		m_strFolderToListenTo = strFolder;
		m_bRecursive = bRecursive;
		AfxBeginThread(threadFuncListen, this);

		// Wait for listening thread to start
		if (m_eventListeningStarted.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT)
		{
			// signal the kill event so threads will be in known state
			// need to stop the listening thread
			m_eventKillThreads.signal();

			// There was a problem starting the thread
			UCLIDException ue("ELI30297", "Listening thread did not start.");
			ue.addDebugInfo("Folder", strFolder);
			throw ue;
		}

		// make sure the events are reset for the dispatch thread
		m_eventDispatchThreadExited.reset();
		m_eventDispatchThreadStarted.reset();
		AfxBeginThread(threadDispatchEvents, this);

		// Wait for thread to start
		if (m_eventDispatchThreadStarted.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT)
		{
			// need to stop the listening thread
			m_eventKillThreads.signal();

			// There was a problem starting the dispatch thread
			UCLIDException ue("ELI30298", "Listening dispatch thread did not start.");
			ue.addDebugInfo("Folder", strFolder);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30299");
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::stopListening()
{
	try
	{
		// Signal the kill threads event
		m_eventKillThreads.signal();

		bool bWaitTimeout = false;

		// Check if Listening was started
		if (m_eventListeningStarted.isSignaled())
		{
			// Wait for listening to exit
			if (m_eventListeningExited.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT )
			{
				UCLIDException ue("ELI30307", "Application Trace: Listening thread did not exit properly.");
				ue.addDebugInfo("Folder", m_strFolderToListenTo);
				ue.log();
			}
		}

		// Check if Dispatch thread was started
		if (m_eventDispatchThreadStarted.isSignaled())
		{
			// Wait for dispatch thread to stop
			if (m_eventDispatchThreadExited.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT)
			{
				UCLIDException ue("ELI30300", "Application Trace: Listening dispatch thread did not exit properly.");
				ue.addDebugInfo("Folder", m_strFolderToListenTo);
				ue.log();
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27786");
}
//-------------------------------------------------------------------------------------------------
UINT FolderEventsListener::threadFuncListen(LPVOID pParam)
{
	INIT_EXCEPTION_AND_TRACING("MLI03277");

	FolderEventsListener* fl = NULL;
	LPBYTE lpFileBuffer = NULL;
	try
	{
		_lastCodePos = "10";
		
		// the Listener is the caller and will still be a valid pointer after the try...catch
		// this will be used to set the thread exit event
		fl = (FolderEventsListener *)pParam;
		ASSERT_RESOURCE_ALLOCATION("ELI25256", fl != __nullptr);

		_lastCodePos = "20";

		bool bRecursive = fl->m_bRecursive;

		// let the the creating thread know it is ok to continue
		fl->m_eventListeningStarted.signal();
		
		unsigned long ulTimeBetweenRestarts = gulDEFAULT_TIME_BETWEEN_RESTARTS;
		_lastCodePos = "30";

		// Variable to track the number of times listening is started
		// This will be used to track the number of calls to getListeningHandle
		int nListeningStartCount = 0;
		
		// Allocate buffers for the change operations
		lpFileBuffer = new BYTE[gulFOLDER_LISTENER_BUF_SIZE];
		ASSERT_RESOURCE_ALLOCATION("ELI30306", lpFileBuffer != __nullptr);
		_lastCodePos = "70";

		do
		{
			// Setup events for changes
			Win32Event eventFileChangesReady;

			// Overlapped structures to contain the event and buffer info for each 
			// change process
			OVERLAPPED oFileChanges;
			_lastCodePos = "40";

			// initialize to zero
			memset( &oFileChanges, 0, sizeof(oFileChanges));
			_lastCodePos = "50";

			// set the event to signal completion of the overlapped IO
			oFileChanges.hEvent = eventFileChangesReady.getHandle();
			_lastCodePos = "60";

			// set up handle array for the handles to wait for
			HANDLE handles[2];
			handles[0] = oFileChanges.hEvent;
			handles[1] = fl->m_eventKillThreads.getHandle();
			_lastCodePos = "80";

			// Listening handle for changes in the files
			HANDLE hFile = NULL;

			try
			{
				// Handle for file changes
				hFile = fl->getListeningHandle(fl->m_strFolderToListenTo);
				_lastCodePos = "90";

				nListeningStartCount++;

				// Log an exception each time listening starts 
				UCLIDException ueStart("ELI29123", "Application trace: Listening started.");
				ueStart.addDebugInfo("Number times started", nListeningStartCount);
				ueStart.addDebugInfo("Folder",  fl->m_strFolderToListenTo);
				ueStart.log();

				bool bDone = false;
				do
				// There are many flags that can be specified other than FILE_NOTIFY_CHANGE_FILE_NAME
				{
					// Start listening for file changes
					fl->beginChangeListen(hFile, 
						FILE_NOTIFY_CHANGE_FILE_NAME | FILE_NOTIFY_CHANGE_SIZE,
						lpFileBuffer, oFileChanges,  bRecursive);
					_lastCodePos = "100";

					// Wait for changes or a kill signal
					DWORD dwWaitResult = WaitForMultipleObjects( 2, (HANDLE *)&handles, FALSE, INFINITE );
					_lastCodePos = "110";

					if ( dwWaitResult == WAIT_OBJECT_0 )
					{
						// File changes were detected
						_lastCodePos = "120";

						DWORD dwBytesTransfered;

						// This call will wait for the event in the oChanges to be signalled so don't 
						// reset it until after this call
						int iResult = GetOverlappedResult( hFile, &oFileChanges, &dwBytesTransfered, TRUE );
						_lastCodePos = "130";
						if ( iResult == FALSE )
						{
							// Error geting the results
							UCLIDException ue ("ELI13017", "Error getting file changes.");
							ue.addWin32ErrorInfo();
							ue.addDebugInfo("Folder", fl->m_strFolderToListenTo);
							throw ue;
						}

						// reset the event
						eventFileChangesReady.reset();
						_lastCodePos = "140";

						_lastCodePos = "150";

						// Process the changes
						fl->processChanges(fl->m_strFolderToListenTo, fl->m_eventKillThreads, lpFileBuffer, fl->m_queEvents, true );
						_lastCodePos = "160";

						// Check to see if Kill thread event was signaled
						bDone = fl->m_eventKillThreads.isSignaled();
						_lastCodePos = "170";
					}
					else
					{
						// The kill event was signaled
						bDone = true;
						_lastCodePos = "180";
					}
				}
				while (!bDone);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI13161");

			_lastCodePos = "190";

			// Clean up
			//Check to make sure the hFile was opened before trying to close it
			// Cancel any pending overlapped operations
			if (hFile != __nullptr)
			{
				CancelIo( hFile );
				_lastCodePos = "200";
				CloseHandle( hFile );
				_lastCodePos = "210";
			}
			
			_lastCodePos = "230";
			ulTimeBetweenRestarts = fl->getTimeBetweenListeningRestarts();
			_lastCodePos = "240";

			// Log Application trace exception so there is an indication when listening has stopped
			UCLIDException ueStop("ELI30303", "Application trace: Listening stopped.");
			ueStop.addDebugInfo("Folder",  fl->m_strFolderToListenTo);
			ueStop.log();
		}
		while (fl->m_eventKillThreads.wait( ulTimeBetweenRestarts ) == WAIT_TIMEOUT );
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12780");
	

	// deallocate the file buffer
	try
	{
		if (lpFileBuffer != __nullptr)
		{
			// Delete the file buffer
			delete [] lpFileBuffer;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI30302");

	// Signal that the thread has exited this always needs to be done
	try
	{
		// this could be NULL if it was not set before starting the thread 
		if ( fl != __nullptr )
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
		ASSERT_RESOURCE_ALLOCATION("ELI30301", fl != __nullptr);

		fl->m_eventDispatchThreadStarted.signal();
		try
		{
			vector<FolderEvent> vecEvents;

			while ( fl->m_eventKillThreads.wait(1000) == WAIT_TIMEOUT )
			{
				while(fl->m_queEvents.getSize() > 0)
				{
					if(fl->m_eventKillThreads.isSignaled())
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
					if(fl->m_eventKillThreads.isSignaled())
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
			fl->m_eventDispatchThreadExited.signal();
			throw;
		}
		fl->m_eventDispatchThreadExited.signal();
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
void FolderEventsListener::dispatchEvent(const FolderEvent& folderEvent)
{
	try
	{
		switch(folderEvent.m_nEvent)
		{
		case kFileAdded:
			onFileAdded(folderEvent.m_strFileNameNew);
			break;
		case kFileRemoved:
			onFileRemoved(folderEvent.m_strFileNameOld);
			break;
		case kFileModified:
			onFileModified(folderEvent.m_strFileNameNew);
			break;
		case kFileRenamed:
			onFileRenamed(folderEvent.m_strFileNameOld, folderEvent.m_strFileNameNew);
			break;
		case kFolderAdded:
			onFolderAdded(folderEvent.m_strFileNameNew );
			break;
		case kFolderRemoved:
			onFolderRemoved(folderEvent.m_strFileNameOld );
			break;
		case kFolderRenamed:
			onFolderRenamed(folderEvent.m_strFileNameOld, folderEvent.m_strFileNameNew);
			break;
		default:
			break;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29912");
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
	try
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
			ASSERT_RESOURCE_ALLOCATION("ELI12935", result != __nullptr);

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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29911");
}
//-------------------------------------------------------------------------------------------------
HANDLE FolderEventsListener::getListeningHandle (std::string strDir )
{
	try
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29909");
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::beginChangeListen( HANDLE hDir, DWORD dwNotifyFilter, LPBYTE &lpBuffer, OVERLAPPED &oOverLapped, bool bRecursive)
{
	try
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29910");
}
//-------------------------------------------------------------------------------------------------

