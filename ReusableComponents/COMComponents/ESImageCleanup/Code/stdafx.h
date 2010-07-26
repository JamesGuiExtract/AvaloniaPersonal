// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#define _ATL_APARTMENT_THREADED

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdlgs.h>

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdisp.h>        // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT

#include <atlbase.h>
extern CComModule _Module;

#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#import "..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\APIs\Inlite_5_7\bin\ClearImage.dll" no_namespace named_guids

#import "ESImageCleanup.tlb"