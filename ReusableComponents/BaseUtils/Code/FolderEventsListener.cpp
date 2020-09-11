#include "stdafx.h"
#include "FolderEventsListener.h"
#include "cpputil.h"
#include "RegConstants.h"

#include <iostream>

// globals
const unsigned long gulFOLDER_LISTENER_BUF_SIZE	 = 65536;
const string gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY = "FEL_TimeBetweenListeningRestarts";
const unsigned long gulDEFAULT_TIME_BETWEEN_RESTARTS = 5000; // 5 seconds
const unsigned long gulTIME_TO_WAIT_FOR_THREAD_EXIT = 10000; // 10 seconds
const unsigned long gulTIME_TO_WAIT_BETWEEN_ACCESS_CHECKS = 30000; // 30 seconds

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
: m_strLastAddedFilename("")
, m_dwAddFileTickTime(0)
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
		
		// Reset the monitor event before starting the main listening thread
		m_eventFolderInaccessable.reset();
		
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

		// Make sure the events are reset for the monitor folder access is start
		m_eventMonitorFolderThreadStarted.reset();
		m_eventMonitorFolderThreadExited.reset();
		AfxBeginThread(threadMonitorFolderAccess, this);

		if (m_eventMonitorFolderThreadStarted.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT)
		{
			m_eventKillThreads.signal();

			// There was a problem starting the Monitor thread
			UCLIDException ue("ELI38237", "Listening monitor folder access thread did not start.");
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
		
		// Check if the monitoring thread was started
		if (m_eventMonitorFolderThreadStarted.isSignaled())
		{
			// Wait for monitoring to exit
			if (m_eventListeningExited.wait(gulTIME_TO_WAIT_FOR_THREAD_EXIT) == WAIT_TIMEOUT )
			{
				UCLIDException ue("ELI38238", "Application Trace: Listening monitoring thread did not exit properly.");
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

		// let the creating thread know it is OK to continue
		fl->m_eventListeningStarted.signal();
		
		unsigned long ulTimeBetweenRestarts = fl->getTimeBetweenListeningRestarts();
		_lastCodePos = "30";

		// Variable to track the number of times listening is started
		// This will be used to track the number of calls to getListeningHandle
		int nListeningStartCount = 0;

		// Tracks the number of consecutive times we attempt to start listening.
		int nListeningStartAttemptCount = 0;
		
		// Allocate buffers for the change operations
		lpFileBuffer = new BYTE[gulFOLDER_LISTENER_BUF_SIZE];
		ASSERT_RESOURCE_ALLOCATION("ELI30306", lpFileBuffer != __nullptr);
		_lastCodePos = "70";

		bool bDone = false;

		do
		{
			// Update the number of start attempts
			nListeningStartAttemptCount++;

			// make sure the folder is valid , this is faster than
			// going through all the setup and then it doesn't 
			if (!isValidFolder(fl->m_strFolderToListenTo))
			{
				if (nListeningStartAttemptCount <=1)
				{
					UCLIDException ue("ELI36810", "Listening folder is not accessible. Retrying...");
					ue.addDebugInfo("Folder", fl->m_strFolderToListenTo);
					ue.log();
				}
				continue;
			}

			// Folder was accessible to get here so reset the folderInaccessable event
			fl->m_eventFolderInaccessable.reset();

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
			HANDLE handles[3];
			handles[0] = oFileChanges.hEvent;
			handles[1] = fl->m_eventFolderInaccessable.getHandle();
			handles[2] = fl->m_eventKillThreads.getHandle();
			_lastCodePos = "80";

			// Listening handle for changes in the files
			HANDLE hFile = NULL;

			// Indicates whether listening started correctly this iteration.
			bool bListeningStarted = false;

			try
			{
				try
				{
					// Handle for file changes
					hFile = fl->getListeningHandle(fl->m_strFolderToListenTo);
					_lastCodePos = "90";

					bListeningStarted = true;
					nListeningStartCount++;

					// Log an exception each time listening starts 
					UCLIDException ueStart("ELI29123", "Application trace: Listening started.");
					ueStart.addDebugInfo("Number times started", nListeningStartCount);
					ueStart.addDebugInfo("Folder",  fl->m_strFolderToListenTo);
					if (nListeningStartAttemptCount > 1)
					{
						ueStart.addDebugInfo("Attempts required",  nListeningStartAttemptCount);
					}
					ueStart.log();

					// Reset the number of start attempts
					nListeningStartAttemptCount = 0;

					do
					// There are many flags that can be specified other than FILE_NOTIFY_CHANGE_FILE_NAME
					{
						// Start listening for file changes
						fl->beginChangeListen(hFile, 
							FILE_NOTIFY_CHANGE_FILE_NAME | FILE_NOTIFY_CHANGE_SIZE,
							lpFileBuffer, oFileChanges,  bRecursive);
						_lastCodePos = "100";

						// Wait for changes or a kill signal
						DWORD dwWaitResult = WaitForMultipleObjects( 3, (HANDLE *)&handles, FALSE, INFINITE );
						_lastCodePos = "110";

						if ( dwWaitResult == WAIT_OBJECT_0 )
						{
							// File changes were detected
							_lastCodePos = "120";

							DWORD dwBytesTransfered;

							// This call will wait for the event in the oChanges to be signaled so don't 
							// reset it until after this call
							int iResult = GetOverlappedResult( hFile, &oFileChanges, &dwBytesTransfered, TRUE );
							_lastCodePos = "130";
							if ( iResult == FALSE )
							{
								// Error getting the results
								UCLIDException ue ("ELI13017", "Error getting file changes.");
								ue.addWin32ErrorInfo();
								ue.addDebugInfo("Folder", fl->m_strFolderToListenTo);
								ue.addDebugInfo("Bytes in buffer", dwBytesTransfered);
								throw ue;
							}
							if (dwBytesTransfered == 0)
							{
								UCLIDException oue("ELI50366", "Buffer overflow getting directory changes.");
								oue.addDebugInfo("Folder", fl->m_strFolderToListenTo);
								throw oue;
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
						else if ( dwWaitResult == WAIT_OBJECT_0 + 1)
						{
							_lastCodePos = "175";

							// Check that the folder is valid (if the network connection is 
							// no longer valid this will return false)
							if (!isValidFolder(fl->m_strFolderToListenTo))
							{
								UCLIDException ue("ELI36799", "Listening folder is not accessible.");
								ue.addDebugInfo("Folder", fl->m_strFolderToListenTo);
								throw ue;
							}
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
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI13161");
			}
			catch (UCLIDException &ue)
			{
				// Only log exceptions when the first attempt to connect fails, or we had previously
				// been connected. Don't continue to log exceptions with each repeated failed
				// connection.
				if (nListeningStartAttemptCount <= 1)
				{
					if (!bListeningStarted)
					{
						ue = UCLIDException("ELI34047",
							"Failed to access folder, starting retry attempts...", ue);
					}

					ue.log();
				}
			}

			_lastCodePos = "190";

			try
			{
				// Clean up
				// Check to make sure the hFile was opened before trying to close it
				// Cancel any pending overlapped operations
				if (hFile != __nullptr)
				{
					CancelIo( hFile );
					_lastCodePos = "200";
					CloseHandle( hFile );
					_lastCodePos = "210";
				}
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34040");

			if (bListeningStarted)
			{
				_lastCodePos = "240";

				// Log Application trace exception so there is an indication when listening has stopped
				UCLIDException ueStop("ELI30303", "Application trace: Listening stopped.");
				ueStop.addDebugInfo("Folder",  fl->m_strFolderToListenTo);
				ueStop.log();
			}
		}
		while ((!bDone && (nListeningStartAttemptCount <= 1)) || 
			fl->m_eventKillThreads.wait( ulTimeBetweenRestarts ) == WAIT_TIMEOUT );
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

					if((event.m_nEvent == kFileAdded || event.m_nEvent == kFileModified || event.m_nEvent == kFileRenamed) &&
						fl->fileMatchPattern(event.m_strFileNameNew))
					{
						if(!fl->fileReadyForAccess(event.m_strFileNameNew))
						{
							// Before putting the file back on the queue make sure it still exists
							// https://extract.atlassian.net/browse/ISSUE-13789
							if (isFileOrFolderValid(event.m_strFileNameNew))
							{
								fl->m_queEvents.push(event);
							}
							continue;	
						}
					}

					fl->dispatchEvent(event);
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
UINT FolderEventsListener::threadMonitorFolderAccess(LPVOID pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		FolderEventsListener* fl = (FolderEventsListener*)pParam;
		ASSERT_RESOURCE_ALLOCATION("ELI38235", fl != __nullptr);

		// Signal that the thread started
		fl->m_eventMonitorFolderThreadStarted.signal();
		try
		{
			// Wait on the kill event
			while ( fl->m_eventKillThreads.wait(gulTIME_TO_WAIT_BETWEEN_ACCESS_CHECKS) == WAIT_TIMEOUT )
			{
				// Only check the file if the event is not already signaled
				if (!fl->m_eventFolderInaccessable.isSignaled())
				{
					// Check that the folder is valid (if the network connection is 
					// no longer valid this will return false)
					if (!isValidFolder(fl->m_strFolderToListenTo))
					{
						// Signal event that indicates that the folder is inaccessible
						fl->m_eventFolderInaccessable.signal();
					}
				}
			}
		}
		catch(...)
		{
			fl->m_eventMonitorFolderThreadExited.signal();
			throw;
		}
		fl->m_eventMonitorFolderThreadExited.signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI38236");
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
	return asUnsignedLong( ma_pCfgMgr->getKeyValue("", gstrTIME_BETWEEN_LISTENING_RESTARTS_KEY,
		asString(gulDEFAULT_TIME_BETWEEN_RESTARTS)));
}
//-------------------------------------------------------------------------------------------------
void FolderEventsListener::processChanges(string strBaseDir, Win32Event &eventKill, LPBYTE & rlpBuffer, MTSafeQueue<FolderEvent> & queEvents, bool bFileChange )
{
	try
	{
		LPBYTE pCurrByte = rlpBuffer;
		PFILE_NOTIFY_INFORMATION pFileInfo = NULL;

		// rename events happen in two pieces, rename old and rename new
		// when a rename old event happens we keep track of it so that we 
		// can have one event handler for rename that takes the old and new
		// names
		string strOldFilename = "";

		// iterate through the buffer extracting the information that we need
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
				m_strLastAddedFilename == strFilename && (GetTickCount() - m_dwAddFileTickTime) < 1000)
			{
				eFilteredEventType = (EFileEventType)0;
			}
			else if (eFilteredEventType == kFileAdded)
			{
				// If this is a new add event, keep track of the filename
				m_strLastAddedFilename = strFilename;
				m_dwAddFileTickTime = GetTickCount();
			}
			else
			{
				m_strLastAddedFilename = "";
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

