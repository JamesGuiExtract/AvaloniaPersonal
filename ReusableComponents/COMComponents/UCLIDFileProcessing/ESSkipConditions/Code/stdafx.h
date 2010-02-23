// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxdlgs.h>
#include <afxcmn.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

//using namespace ATL;
#import "..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\Code\UCLIDFileProcessing.tlb" named_guids \
	rename("DeleteFile", "DeleteFPMFile")
using namespace UCLID_FILEPROCESSINGLib;

#import "ESSkipConditions.tlb"
