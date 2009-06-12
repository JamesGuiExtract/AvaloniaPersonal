//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	WaitDlg.h
//
// PURPOSE:	Similar to the hourglass mouse cursor in that it displays a message that remains
//			until it the object is destroyed (goes out of scope)
//
// NOTES:	This class uses 2 threads: The callers thread which will continue running after the 
//			dialog is displayed, and a UI thread which displays the toolbar.
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "stdafx.h"
#include "resource.h"

#include <Win32Event.h>

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CWaitDlg
//--------------------------------------------------------------------------------------------------
class CWaitDlg : public CDialog
{
	DECLARE_DYNAMIC(CWaitDlg)

public:
	CWaitDlg(const string &strMessage, CWnd* pParent = NULL);
	CWaitDlg(CWnd* pParent = NULL);
	virtual ~CWaitDlg();
	
	enum { IDD = IDD_WAIT };

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Show the strMessage in a dialog modal to the parent window
	void showMessage(const string &strMessage);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: If the dialog is hidden, redisplays the dialog after asserting that the dialog exists.
	void show();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Hides the dialog without closing it.  
	// NOTE: Using this call instead of close can help ensure other dialogs that ID Shield display
	// show up as the foreground window.
	void hide();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Closes the dialog completely.
	// NOTE: Call hide instead of close unless you have a specific reason.  If the dialog is
	// closed and it is the only open ID Shield dialog at the time, it will relinquish foreground
	// status to another process (probably Laserfiche), and prevent the next ID Shield dialog to 
	// display from being shown in the foreground.
	void close();

protected:

	/////////////////////
	// Overrides
	/////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	/////////////////////
	// Message handlers
	/////////////////////
	LRESULT OnCloseDialog(WPARAM wParam, LPARAM lParam);
	LRESULT OnUpdateMessage(WPARAM wParam, LPARAM lParam);

	/////////////////////
	// Control Variables
	/////////////////////
	CString m_zMessage;

private:

	//////////////////
	// Variables
	//////////////////
	Win32Event m_eventIsInitialized;
	Win32Event m_eventUIThreadFinished;

	//////////////////
	// Methods
	//////////////////
	static UINT showDialog(LPVOID pData);

	DECLARE_MESSAGE_MAP()
};
