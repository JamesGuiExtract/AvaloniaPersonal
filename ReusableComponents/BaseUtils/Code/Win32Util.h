//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Win32Util.h
//
// PURPOSE:	Definition of some reusable utility functions that can be used in the Win32 environment.
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (September 2000 - present)
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>

//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine if one window is the ancestor window of another window.
// REQUIRE: The specified window handles (hParent & hPotentialChild) are valid.
// PROMISE: To return true if hParent is the handle of a window that is the eventual parent
//			of the window who's handle is hPotentialChild.  Promise to return false otherwise.
// ARGS:	hParent: the handle of the parent window, of which it is to be determined whether
//				hPotentialChild is an eventual child.
//			hPotentialChild: the handle of the child window, of which it is to be determined whether
//				hParent is an eventual parent.
EXPORT_BaseUtils bool windowIsAncestorWindowOf(HWND hParent, HWND hPotentialChild);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine if a window has an ancestor window of a given class.
// REQUIRE: The specified window handle (hChild) is valid.
// PROMISE: To return the handle of the window that is the most recent ancestor of the window
//			whose handle is hChild where the class name of the most recent ancestor window is
//			pszWindowClassName.  If no ancestor window is found who's class name is 
//			pszWindowClassName, then NULL will be returned.
// ARGS:	hChild: the handle of the child window, of which it is to be determined whether
//				an eventual parent window exists who's class name is pszWindowClassName.
//			pszWindowClassName: the name of the class of the ancestor window we are searching for
EXPORT_BaseUtils HWND windowHasAncestorOfClass(HWND hChild, const char *pszWindowClassName);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To return the class name for dialogs.
// REQUIRE: Nothing.
// PROMISE: To return the string that represents the class name for Windows(tm) dialogs.  This
//			returned value must not be deleted by caller.
// ARGS:	Not Applicable.
EXPORT_BaseUtils const char* getDefaultWindowsDialogClassName();
//--------------------------------------------------------------------------------------------------
// PURPOSE: To determine if a given window is of the default windows dialog class.
// REQUIRE: Nothing.
// PROMISE: To return true if the class name associated with hWnd is the same as the class name 
//			returned by getDefaultWindowsDialogClassName(), otherwise false will be returned.
// ARGS:	hWnd: The handle of the window for which it is to be determined whether the window
//			is of the default windows dialog class.
EXPORT_BaseUtils bool windowIsOfDefaultWindowsDialogClass(HWND hWnd);
//--------------------------------------------------------------------------------------------------
// PURPOSE: Kills each instance of a particular named process running on 
//			this machine.
// REQUIRE: None.
// PROMISE: All threads will be terminated.  Child processes will not be 
//			notified.  DLLs attached to the process will also not be 
//			notified.  The number of processes killed will be returned.
// ARGS:	pszProcessName: the Process Name of the process
// AUTHOR:	Wayne Lenius
EXPORT_BaseUtils unsigned long killNamedProcess(const char *pszProcessName);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To create registry entries associating a file extension with
//			an application, an icon, and a file type description
// REQUIRE: strFileExtension should have the leading period in it (e.g. ".tif")
//			strFileTypeDescription should be non-empty and unique (with respect
//			to other file types current stored in the registry)
//			nDefaultIconIndex is a valid index to an icon within strFullPathToEXE
// PROMISE: To create registry entries associating the file extension 
//			strFileExtension with the application strFullPathToEXE, the
//			an icon stored in the EXE at position nDefaultIconIndex,
//			and the file type description strFileTypeDescription
//			If the main folder for the extension and filetype description
//			already exist in the registy, and if bSkipIfKeysExist == true,
//			then no modifications will be made to the registry.
// AUTHOR:	Arvind Ganesan
EXPORT_BaseUtils void registerFileAssociations(const std::string& strFileExtension,
											const std::string& strFileTypeDescription,
											const std::string& strFullPathToEXE,
											bool bSkipIfKeysExist,
											unsigned long nDefaultIconIndex = 0);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To delete registry entries associating a file extension with
//			an application, an icon, and a file type description
// REQUIRE: strFileExtension should have the leading period in it (e.g. ".tif")
//			strFileTypeDescription should be non-empty and unique (with respect
//			to other file types current stored in the registry)
// PROMISE: To delete registry entries associated with the specified file 
//			extension and filetype
// AUTHOR:	Arvind Ganesan
EXPORT_BaseUtils void unregisterFileAssociations(const std::string& strFileExtension,
											  const std::string& strFileTypeDescription);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To return the full path to the currently executing EXE
// AUTHOR:	Arvind Ganesan
EXPORT_BaseUtils std::string getAppFullPath();
//--------------------------------------------------------------------------------------------------
// Flashes the window represented by hWnd to bring the user's attention to it.
// If bSetFocus == true, then focus will be set to the window
EXPORT_BaseUtils void flashWindow(HWND hWnd, bool bSetFocus);

//-----------------------------------------------------------------------------------------
// PURPOSE:	To return a string for the running windows platform
EXPORT_BaseUtils std::string getPlatformAsString();
//-----------------------------------------------------------------------------------------
class EXPORT_BaseUtils HandleCloser
{
public:
	// REQUIRE:	hHandle is the object handle to automatically close when
	//			this object is destructed
	HandleCloser(HANDLE hHandle);

	// PURPOSE: Automatically close the object handle if it hasn't 
	//			already been closed
	~HandleCloser();
	
	// PURPOSE:	Explicity close the object handle
	void close();

private:
	HANDLE m_hHandle;
};
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils ClipboardOpenerCloser
{
public:
	// REQUIRE:	pWnd is the window that should open the clipboard
	//			and for which the clipboard should be closed when
	//			this object goes out of scope
	ClipboardOpenerCloser::ClipboardOpenerCloser(CWnd *pWnd);
	
	// PURPOSE: Automatically close the clipboard associated with m_pWnd
	~ClipboardOpenerCloser();

	// PURPOSE: To explicity close the clipboard that was opened in the constructor
	void close();

private:
	CWnd *m_pWnd;
};
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils GlobalMemoryHandler
{
public:
	// REQUIRE:	hData is the handle to global memory which must now
	//			be locked, and unlocked at the time this object goes out of scope
	// PROMISE: To call lock(hData)
	//			Subsequent calls to lock(), unlock(), getData(), and 
	//			operator (HGLOBAL) will operate on hData
	GlobalMemoryHandler::GlobalMemoryHandler(HGLOBAL hData);
	
	// PURPOSE: Automatically close the clipboard associated with m_pWnd
	// PROMISE: To call unlock()
	~GlobalMemoryHandler();

	// PURPOSE: To explicity lock the specified global memory handle
	// PROMISE: Any global memory handle previously locked by this
	//			object will automatically be unlocked.
	//			The specified global memory handle will automatically be
	//			unlocked when this object goes out of scope, or when unlock()
	//			is called.
	void lock(HGLOBAL hData);
	
	// PURPOSE: To require the lock associated with the handle m_hData
	// REQUIRE: A valid HBLOBAL handle is currently associated with this
	//			object via a direct or indirect call to lock(HGLOBAL hData)
	void lock();

	// PURPOSE: To explicity unlock the global memory handle that was
	//			attached to this object at time of construction
	void unlock();

	// REQUIRE: a global memory object must have successfully been locked
	//			by a call to lock() directly, or by an indirect call to lock()
	//			from the constructor or assignment operator
	void* getData();

	// PURPOSE: Same effect as lock()
	GlobalMemoryHandler& operator=(HGLOBAL hData);

	// PURPOSE: To return the underlying m_hData HGLOBAL variable;
	operator HGLOBAL();

private:
	HGLOBAL m_hData;
	void *m_pData;
};
//--------------------------------------------------------------------------------------------------
// NOTE:	The following structures and function return information about the
//			the list of processes that are currently executing.
//			Note that some fields from the original MS-supplied structure 
//			are commented out because they are not used.
// AUTHOR:	Wayne Lenius
// REQUIRE:	pTaskList is the address of the first entry in a task-list array
//			dwMaxTasks is the maximum number of processes about which information
//			should be retrieved. dwMaxTasks is expected to be the size of the 
//			pTastList array.
typedef struct _TASK_LIST 
{ 
	DWORD		dwProcessId; 
//	DWORD		dwInheritedFromProcessId; 
//	BOOL		flags; 
//	HWND		hwnd; 
	CHAR		ProcessName[MAX_PATH]; 
//	CHAR		WindowTitle[TITLE_SIZE]; 
} TASK_LIST, *PTASK_LIST; 

EXPORT_BaseUtils DWORD getTaskList(TASK_LIST arrTaskList[], DWORD dwMaxTasks);
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils ForegroundWindowRestorer
{
public:
	ForegroundWindowRestorer();

	// PURPOSE: Automatically restore the window that was in the foreground
	// upon construction
	~ForegroundWindowRestorer();
	
	// PURPOSE:	Explicity restore the window
	void restore();

private:
	// PURPOSE: handle of the active window stored upon creation
	HWND m_hwndActive;
};
//--------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils DragDropFinisher
{
public:
	DragDropFinisher(HDROP hDrop);

	// PURPOSE: Call DragFinish to release memory associated with
	// the drag operation represented by m_hDrop;
	~DragDropFinisher();
	
	// PURPOSE:	Explicity finish the drag operation represented by m_hDrop
	void finish();

private:
	// PURPOSE: handle of the active window stored upon creation
	HDROP m_hDrop;
};
//--------------------------------------------------------------------------------------------------
// PURPOSE: To disable the specified window and then re-enable it when the class
// goes out of scope
class EXPORT_BaseUtils WindowDisabler
{
public:
	// Disables the specified window
	WindowDisabler(HWND hWnd);

	~WindowDisabler();

private:
	HWND m_hWnd;
};
//--------------------------------------------------------------------------------------------------
