//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AboutPP.h
//
// PURPOSE:	To display product information
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================
#pragma once

#include "stdafx.h"
#include "resource.h"

#include <string>
#include "afxwin.h"
using namespace std;

//--------------------------------------------------------------------------------------------------
// CAboutPP dialog
//--------------------------------------------------------------------------------------------------
class CAboutPP : public CPropertyPage
{
	DECLARE_DYNAMIC(CAboutPP)

public:
	CAboutPP();
	virtual ~CAboutPP();

	enum { IDD = IDD_TAB_ABOUT };

protected:

	//////////////////////
	// Overrides
	//////////////////////
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();

	//////////////////////
	// Control variables
	//////////////////////
	CStatic m_Icon;

private:

	//////////////////////
	// Methods
	//////////////////////

	// Retrieves the FKB Version number
	string getFKBVersion();

	DECLARE_MESSAGE_MAP()
};
