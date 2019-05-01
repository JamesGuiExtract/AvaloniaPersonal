// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0600
#endif
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\LineTextCorrectors\Code\LineTextCorrectors.tlb"  named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_LINETEXTCORRECTORSLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\LineTextEvaluators\Code\LineTextEvaluators.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_LINETEXTEVALUATORSLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\ParagraphTextCorrectors\Code\ParagraphTextCorrectors.tlb"  named_guids //raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_PARAGRAPHTEXTCORRECTORSLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\ParagraphTextHandlers\Code\ParagraphTextHandlers.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_PARAGRAPHTEXTHANDLERSLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\SubImageHandlers\Code\SubImageHandlers.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_SUBIMAGEHANDLERSLib;

#import "..\..\InputReceivers\HighlightedTextIR\Code\Core\HighlightedTextIR.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_HIGHLIGHTEDTEXTIRLib;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
