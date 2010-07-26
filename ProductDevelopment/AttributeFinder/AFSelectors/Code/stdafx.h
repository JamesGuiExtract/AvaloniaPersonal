// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include <CommonToExtractProducts.h>

#define _ATL_APARTMENT_THREADED
#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <afxwin.h>
#include <afxdisp.h>
#include <afxdlgs.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\ReusableComponents\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

using namespace ATL;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDImageUtils\Code\UCLIDImageUtils.tlb" named_guids 
using namespace UCLID_IMAGEUTILSLib;

#import "..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\AFCore\Code\AFCore.tlb" named_guids
#include <atlctl.h>
using namespace UCLID_AFCORELib; 

#import "..\..\AFUtils\Code\AFUtils.tlb" named_guids
using namespace UCLID_AFUTILSLib;

#import "AFSelectors.tlb"

