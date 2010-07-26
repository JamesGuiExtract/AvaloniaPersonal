// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include <CommonToExtractProducts.h>

#define _ATL_APARTMENT_THREADED
#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <afxwin.h>
#include <afxext.h>   // MFC extensions

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdisp.h>        // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT

#include "resource.h"
#include <atlbase.h>
#include <atlcom.h>

using namespace ATL;

#import "..\..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;