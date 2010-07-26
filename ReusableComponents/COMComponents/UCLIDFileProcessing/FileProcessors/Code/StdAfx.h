// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxdlgs.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#import "..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\UCLIDImageUtils\Code\UCLIDImageUtils.tlb" named_guids
using namespace UCLID_IMAGEUTILSLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\UCLIDFileProcessing\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#import "..\..\..\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR2\Code\SSOCR2.tlb" named_guids 
using namespace UCLID_SSOCR2Lib;

#import "..\..\..\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids 
using namespace UCLID_SSOCRLib;

#import "..\..\..\ESImageCleanup\Code\ESImageCleanup.tlb" named_guids
using namespace ESImageCleanupLib;

#import "..\..\..\..\..\RC.Net\Imaging\Core\Code\Extract.Imaging.tlb" named_guids
using namespace Extract_Imaging;

#import "FileProcessors.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
