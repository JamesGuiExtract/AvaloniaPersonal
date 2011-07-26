// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxole.h>  // Needed for UltimateGrid
					 // (see http://www.codeproject.com/Messages/2869965/Re-Run-Time-Check-Failure-sharp0.aspx)
#include <afxdisp.h>
#include <afxcmn.h>			// For CListCtrl class
#include <afxext.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\ReusableComponents\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#include <afxmt.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\..\..\RC.Net\Utilities\Email\Core\Code\Extract.Utilities.Email.tlb" named_guids
using namespace Extract_Utilities_Email;


#import "..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\..\ReusableComponents\InputFunnel\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "AFCore.tlb"