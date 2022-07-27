// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxmt.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>

#include <MemLeakDetection.h>

#import "..\..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\UCLIDImageUtils\Code\UCLIDImageUtils.tlb" named_guids
using namespace UCLID_IMAGEUTILSLib;

#import "..\..\SSOCR2\Code\SSOCR2.tlb" named_guids
using namespace UCLID_SSOCR2Lib;

#import "..\..\..\..\..\..\RC.Net\Interfaces\Core\Code\Extract.Interfaces.tlb" named_guids
using namespace Extract_Interfaces;

#import "SSOCR.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
