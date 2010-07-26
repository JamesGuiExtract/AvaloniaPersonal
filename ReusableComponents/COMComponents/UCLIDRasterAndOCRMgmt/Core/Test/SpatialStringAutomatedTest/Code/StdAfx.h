// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

#import "..\..\..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\..\UCLIDTestingFramework\Interfaces\Code\UCLIDTestingFramework.tlb" named_guids 
using namespace UCLID_TESTINGFRAMEWORKINTERFACESLib;

#import "..\..\..\..\..\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
