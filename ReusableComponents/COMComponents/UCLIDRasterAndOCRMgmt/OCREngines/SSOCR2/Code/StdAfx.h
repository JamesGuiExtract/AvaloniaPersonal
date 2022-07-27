// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>   // MFC core and standard components
#include <afxext.h>   // MFC extensions
#include <afxdisp.h>  // MFC Automation extensions

#include <atlbase.h>

#include <MemLeakDetection.h>

// [Added 11/04/2009 - JDS] NOTE: This is an older way of working around
// the issue related to including both afxwin.h and atlbase.h (including
// both restricts access to some of the ATL template classes, in particular
// CAtlExeModuleT).  For a newer implementation example see the FAMProcess.cpp
// in the FAMProcess project.
//
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
class CExeModule : public CComModule
{
public:
	LONG Unlock();
	DWORD dwThreadID;
	HANDLE hEventShutdown;
	void MonitorShutdown();
	bool StartMonitor();
	bool bActivity;
};
extern CExeModule _Module;
#include <atlcom.h>

#include <atlctl.h>

#import "..\..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\..\..\RC.Net\Interfaces\Core\Code\Extract.Interfaces.tlb" named_guids
using namespace Extract_Interfaces;

#import "SSOCR2.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
