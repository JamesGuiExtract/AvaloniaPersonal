// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#pragma warning(disable:4786)

#define STRICT 1
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxcmn.h>			// For CListCtrl class
#include <afxext.h>

// Use this #define to compile in logging code currently available in Redaction Verification
#define _VERIFICATION_LOGGING

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\..\..\ReusableComponents\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"
#include <afxmt.h>

#import <msxml.dll> named_guids

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\..\..\..\ReusableComponents\OcxAndDlls\UCLIDGenericDisplay2\Code\UCLIDGenericDisplay.tlb" named_guids
using namespace UCLIDGENERICDISPLAYLib;

#import "..\..\..\..\..\..\ReusableComponents\InputFunnel\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\..\AFCore\Code\AFCore.tlb" named_guids
using namespace UCLID_AFCORELib;

#import "..\..\..\..\AFUtils\Code\AFUtils.tlb" named_guids
using namespace UCLID_AFUTILSLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#import "RedactionCustomComponents.tlb"  

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
