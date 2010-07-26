// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define STRICT 1
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxext.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

//#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\AbbyyOCR\Code\AbbyyOCR.tlb" named_guids
//using namespace UCLID_ABBYYOCRLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDTestingFramework\Interfaces\Code\UCLIDTestingFramework.tlb" named_guids 
using namespace UCLID_TESTINGFRAMEWORKINTERFACESLib;

#import "IFCore.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

