//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LevenshteinDistanceDlg.h
//
// PURPOSE:	Declaration of CLevenshteinDistanceApp class
//
// NOTES:	
//
// AUTHORS:	Ryan Mulder
//
//============================================================================
#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols
#include <string>


// CLevenshteinDistanceApp:
// See LevenshteinDistance.cpp for the implementation of this class
//

class CLevenshteinDistanceApp : public CWinApp
{
public:
	CLevenshteinDistanceApp();
	
	
// Overrides
	public:
	virtual BOOL InitInstance();

// Implementation

	DECLARE_MESSAGE_MAP()
};

extern CLevenshteinDistanceApp theApp;