// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#pragma once
#ifndef PCH_H
#define PCH_H

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers
#endif

#include <CommonToExtractProducts.h>

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <afxwin.h>         // MFC core and standard components
#include <afxmt.h>

#import "..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "C:\Program Files (x86)\Common Files\System\ADO\msado28.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\..\..\RC.Net\Interfaces\Core\Code\Extract.Interfaces.tlb" named_guids
using namespace Extract_Interfaces;

#import "..\..\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#endif //PCH_H
