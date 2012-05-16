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

#include <afxwin.h>
#include <afxdisp.h>
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

#include <afxmt.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCurveParameter\Code\UCLIDCurveParameter.tlb" named_guids 
using namespace UCLID_CURVEPARAMETERLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDMeasurements\Code\UCLIDMeasurements.tlb" named_guids 
using namespace UCLID_MEASUREMENTSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDFeatureMgmt\Code\UCLIDFeatureMgmt.tlb"  named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_FEATUREMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\InputFunnel\IFCore\Code\IFCore.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDDistanceConverter\Code\UCLIDDistanceConverter.tlb"
using namespace UCLID_DISTANCECONVERTERLib;

#import "..\..\..\InputFunnel\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "..\..\..\InputFunnel\InputReceivers\SpotRecognitionIR\Code\LineTextCorrectors\Code\LineTextCorrectors.tlb"  named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_LINETEXTCORRECTORSLib;

#import "..\..\..\InputFunnel\InputReceivers\SpotRecognitionIR\Code\LineTextEvaluators\Code\LineTextEvaluators.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_LINETEXTEVALUATORSLib;

#import "..\..\..\InputFunnel\InputValidators\LandRecordsIV\Code\LandRecordsIV.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")

#import "..\..\..\PlatformSpecificUtils\GISPlatInterfaces\Code\GISPlatInterfaces.tlb" named_guids
using namespace UCLID_GISPLATINTERFACESLib;

#import "..\..\..\InputFunnel\InputContexts\Code\InputContexts.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_InputContextsLib;

#import "..\..\..\InputFunnel\InputTargetFramework\Code\InputTargetFramework.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_INPUTTARGETFRAMEWORKLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDHighlightWindow\Code\UCLIDHighlightWindow.tlb" named_guids
using namespace UCLID_HIGHLIGHTWINDOWLib;

#import "..\..\..\..\ReusableComponents\COMComponents\ESMessageUtils\Code\ESMessageUtils.tlb" named_guids
using namespace ESMESSAGEUTILSLib;

#import "IcoMapApp.tlb"

#include <afxext.h>
#include <afxdlgs.h>

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

