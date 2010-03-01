#pragma once

#include "BaseUtils.h"
#include "Win32Event.h"
#include "MTSafeQueue.h"
#include "RegistryPersistenceMgr.h"

#include <afxmt.h>

#include <memory>
#include <string>

// This class allows listening to the events of only one folder at a time.
// Can easily be expanded to allow listening to events for multiple folders
// when necessary.
class EXPORT_BaseUtils FolderEventsListener
{
protected:
	// Protected ctor to prevent direct instantiation
	// This class must be derived from.
	FolderEventsListener();

	// virtual destructor - can be overridden in the derived class
	virtual ~FolderEventsListener() {}

	// the method to start listening will start a new worker thread to 
	// listen to change events in the specified folder.  This method
	// will immediately return and events will be dispatched onFileXXXXX() methods
	// will be called from the worker thread when the events take place.
	// Currenlty this will stop listing to previous folders and 
	// start listening on the given folder
	// If eventTypeFlags is specified, the method will only listen for the the OR'd
	// EFileEventTypes provided.
	void startListening(const std::string strFolder, bool bRecursive,
		BYTE eventTypeFlags = 0xFF);

	// stop listening to the folder events.
	void stopListening();

protected:
	// following methods can be overridden in the derived class
	// REQUIRE: These handlers MAY NOT throw exceptions 
	// REQUIRE: These handlers must be written THREAD SAFE
	//			as they will be called from a different thread
	//			than the one that calls start listening
	virtual void onFileAdded(const std::string& /*strFileName*/) {}
	virtual void onFileRemoved(const std::string& /*strFileName*/) {}
	virtual void onFileRenamed(const std::string& /*strOldName*/, const std::string /*strNewName*/) {}
	virtual void onFileModified(const std::string& /*strFileName*/) {}
	// Folder methods
	virtual void onFolderAdded(const std::string& strFolderName) {};
	virtual void onFolderRemoved(const std::string& strFolderName) {};
	virtual void onFolderModified(const std::string& strFolderName ) {};
	virtual void onFolderRenamed(const std::string& strOldName, const std::string strNewName) {};

	// This event is signaled when the Listening thread exits
	// This is to be used as an indicator listening exited and the 
	// listening thread is no longer running
	// Derived classes can use this to make sure the listing hasn't exited
	Win32Event m_eventListeningExited;

	CMutex m_mutexFolderListen;
	enum EFileEventType
	{
		kFileAdded		= 0x01,
		kFileRemoved	= 0x02,
		kFileModified	= 0x04,
		kFileRenamed	= 0x08,
		kFolderAdded	= 0x10,
		kFolderRemoved  = 0x20,
		kFolderModified = 0x40,
		kFolderRenamed  = 0x80
	};

private:

	// The event types that should be monitored.
	BYTE m_eventTypeFlags;

	class ThreadData
	{
	public:
		CMutex * m_pmutexFolderListen;
		FolderEventsListener* m_pListener;

		CWinThread* m_pThread;
		Win32Event m_eventKillThread;
		string m_strFilename;
		volatile bool m_bRecursive;
	};

	class FolderEvent
	{
	public:
		FolderEvent(EFileEventType nEvent = kFileAdded, string strFileNameNew = "", string strFileNameOld = "");
		string m_strFileNameNew;
		string m_strFileNameOld;
		EFileEventType m_nEvent;
	};


	// used to get the time between restarts of the listening if an exception
	// is thrown in the listening thread
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pCfgMgr;

	// The listen thread adds events to this queue and the 
	// and the dispatch queue removes them and sends them to the 
	// handlers
	MTSafeQueue<FolderEvent> m_queEvents;

	///////////////////////
	// the listen thread
	///////////////////////
	// The listen thread listens for events on a particular thread
	// when an event occurs a new FolderEvent is created and added to
	// the dispatch queue (m_queEvents) for processing
	// If this class is expanded to listen on multiple folders it
	// will do so by creating multiple listen threads
	ThreadData* m_pCurrThreadData;
	static UINT threadFuncListen(LPVOID pParam);
	Win32Event	m_eventFolderThreadBegin;

	///////////////////////
	// the dispatch thread
	///////////////////////
	// The dispatch thread picks FolderEvents off of the dispatch 
	// queue(m_queEvents) has them processed by calling the virtual methods
	// like onFileAdded() and so on.  In addition the dispatch thread 
	// ensures that when a file added event occurs that the file
	// is accessible (READABLE) before calling onFileAdded
	CWinThread* m_pthreadDispatch;
	Win32Event m_eventKillDispatchThread;
	Win32Event m_eventDispatchThreadExit;
	static UINT threadDispatchEvents(LPVOID pParam);
	void dispatchEvent(const FolderEvent& event);

	// Utility functions
	bool fileReadyForAccess(std::string strFileName);

	// This method processes the changes that are in the rlpBuffer argument and will put them
	// on the queEvents argument 
	// if the bFileChange 
	//		true it is for file changes (kFileAdded, kFileRemoved, kFileModified, kFileRenamed)
	//		false is is for directory changes (	kFolderAdded, kFolderRemoved, kFolderRenamed)
	void processChanges(std::string strBaseDir, Win32Event &eventKill, LPBYTE & rlpBuffer, MTSafeQueue<FolderEvent> & queEvents, bool bFileChange );

	// This method gets a handle opened for listening on the given directory
	HANDLE getListeningHandle (std::string strDir );

	// This method will begin the read for changes on the given handle
	void beginChangeListen( HANDLE hDir, DWORD dwNotifyFilter, LPBYTE &lpBuffer, OVERLAPPED &oOverLapped, bool bRecursive);

	unsigned long getTimeBetweenListeningRestarts();
};
