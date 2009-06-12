//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ProgressDlg.h
//
// PURPOSE:	To display an IProgressStatusDialog in a seperate thread so that the calling thread
//			can work and still have a responsive progress status dialog.
//
// NOTES:	This class uses 2 threads: The callers thread which will continue running after the 
//			dialog is displayed, and a UI thread which displays the progress status dialog.
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "stdafx.h"

#include <Win32Event.h>

//--------------------------------------------------------------------------------------------------
// CProgressDlg
//--------------------------------------------------------------------------------------------------
class CProgressDlg
{
public:
	// Constructs and displays a progress status box using the provided ipProgressStatus.  If
	// hwndParent is non-NULL, the window will be created as a child of the provided window.  If
	// hStopEvent is non-NULL, a stop button will be displayed and the provided event handle will
	// be signaled if the stop button is pressed.
	CProgressDlg(IProgressStatusPtr ipProgressStatus, HWND hwndParent = NULL, 
				 HANDLE hStopEvent = NULL);
	virtual ~CProgressDlg(void);

	// Closes the progress status dialog
	void Close();

private:

	////////////////
	// Varibles
	////////////////

	// Events to track the progress/state of the dialog
	Win32Event m_eventInitialized;
	Win32Event m_eventDestroy;
	Win32Event m_eventUIThreadFinished;
	HANDLE m_hStopEvent;

	// Progress status and parent window objects
	IProgressStatusDialogPtr m_ipProgressStatusDlg;
	IProgressStatusPtr m_ipProgressStatus;
	HWND m_hwndParent;

	////////////////
	// Methods
	////////////////

	// A new thread entry point in which the progress status dialog is constructed and displayed.
	static UINT showProgressStatus(LPVOID pParam);
};
