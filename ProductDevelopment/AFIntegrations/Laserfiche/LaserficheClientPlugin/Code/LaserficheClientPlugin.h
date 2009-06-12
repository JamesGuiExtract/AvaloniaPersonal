//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LaserficheClientPlugin.h
//
// PURPOSE:	The entry point for most Laserfice intergration operations.  The ID Shield buttons
//			added to the client interface will call this application with command line switches
//			applicable to the button pressed.  Also, the admin console and service config
//			console will be launched via this application.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//==================================================================================================

#pragma once

#include "resource.h"		// main symbols

class CLaserficheClientPluginApp : public CWinApp
{
public:
	CLaserficheClientPluginApp();
	~CLaserficheClientPluginApp();

	//////////////////
	// Overrides
	//////////////////
	virtual BOOL InitInstance();
	BOOL ExitInstance(void);

	DECLARE_MESSAGE_MAP()
	
private:

	//////////////////
	// Variables
	//////////////////
	
	IIDShieldLFPtr m_ipIDShieldLFPtr;
};

extern CLaserficheClientPluginApp theApp;