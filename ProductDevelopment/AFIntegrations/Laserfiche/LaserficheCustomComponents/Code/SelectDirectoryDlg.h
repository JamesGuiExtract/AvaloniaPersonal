//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SelectDirectoryDlg.h
//
// PURPOSE:	A simple prompt to receive a Laserfiche directory
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "afxwin.h"
#include "resource.h"

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSelectDirectoryDlg
//--------------------------------------------------------------------------------------------------
class CSelectDirectoryDlg : public CDialog
{
	DECLARE_DYNAMIC(CSelectDirectoryDlg)

public:
	CSelectDirectoryDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~CSelectDirectoryDlg();

	enum { IDD = IDD_SELECT_DIRECTORY };

	// Returns a CDialog return result (IDOK, IDCANCEL).  If IDOK, rstrDirectory
	// will contain the specified directory.  The dialog is initialized with the
	// value passed in.
	int prompt(string &rstrDirectory);

protected:

	//////////////////////
	// Overrides
	//////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	//////////////////////
	// Control Variables
	//////////////////////
	CString m_zDirectory;
	
	DECLARE_MESSAGE_MAP()
};
