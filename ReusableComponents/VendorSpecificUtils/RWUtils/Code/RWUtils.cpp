// RWUtils.cpp : Defines the entry point for the DLL application.
//
#include "stdafx.h"
#include <afxdllx.h>

#include "RWUtils.h"
#include <Grid\gxall.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


AFX_EXTENSION_MODULE RWUtilsDLL = { NULL, NULL };

//-------------------------------------------------------------------------------------------------
extern "C" int APIENTRY
DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	// Remove this if you use lpReserved
	UNREFERENCED_PARAMETER(lpReserved);

	if (dwReason == DLL_PROCESS_ATTACH)
	{
		TRACE0("RWUTILS.DLL Initializing!\n");

		// Extension DLL one-time initialization
		if (!AfxInitExtensionModule(RWUtilsDLL, hInstance))
			return 0;	

		// Insert this DLL into the resource chain
		// NOTE: If this Extension DLL is being implicitly linked to by
		//  an MFC Regular DLL (such as an ActiveX Control)
		//  instead of an MFC application, then you will want to
		//  remove this line from DllMain and put it in a separate
		//  function exported from this Extension DLL.  The Regular DLL
		//  that uses this Extension DLL should then explicitly call that
		//  function to initialize this Extension DLL.  Otherwise,
		//  the CDynLinkLibrary object will not be attached to the
		//  Regular DLL's resource chain, and serious problems will
		//  result.

		new CDynLinkLibrary(RWUtilsDLL);
	}
	else if (dwReason == DLL_PROCESS_DETACH)
	{
		TRACE0("RWUTILS.DLL Terminating!\n");

		// Terminate the library before destructors are called
		AfxTermExtensionModule(RWUtilsDLL);
	}
	return 1;   // ok
}

//-------------------------------------------------------------------------------------------------
// RWInitializer
//-------------------------------------------------------------------------------------------------
// This is the constructor of a class that has been exported.
// see RWUtils.h for the class definition
RWInitializer::RWInitializer()
{
	// Initialize the Objective Grid
	GXInit();
}

//-------------------------------------------------------------------------------------------------
// RWCleanup
//-------------------------------------------------------------------------------------------------
// This is the constructor of a class that has been exported.
// see RWUtils.h for the class definition
RWCleanup::RWCleanup()
{
	// PVCS P16 #332
	// per http://www.roguewave.com/support/private/ts/viewsrchdoc.cfm?issn=991029-003, 
	// call GXForceTerminate() instead of GXTerminate() to cleanup the Objective Grid
	GXForceTerminate();
}
//-------------------------------------------------------------------------------------------------
