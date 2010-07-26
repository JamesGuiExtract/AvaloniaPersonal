// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef STRICT
#define STRICT
#endif

#include <CommonToExtractProducts.h>

#define _ATL_APARTMENT_THREADED
#define _ATL_NO_AUTOMATIC_NAMESPACE

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

#include <afxwin.h>
#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdisp.h>        // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT

//#include <comsvcs.h>

#include "resource.h"
#include <atlbase.h>

#include <atlcom.h>
#include <atlctl.h>
#include <atlwin.h>

using namespace ATL;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDTestingFramework\Interfaces\Code\UCLIDTestingFramework.tlb" named_guids 
using namespace UCLID_TESTINGFRAMEWORKINTERFACESLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\..\AFCore\Code\AFCore.tlb" named_guids
using namespace UCLID_AFCORELib; 

#import "..\..\..\..\AFUtils\Code\AFUtils.tlb" named_guids
using namespace UCLID_AFUTILSLib;

#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#import "..\..\..\..\..\..\RC.Net\Imaging\Core\Code\Extract.Imaging.tlb" named_guids
using namespace Extract_Imaging;

#import "..\..\RedactionCustomComponents\Code\RedactionCustomComponents.tlb" named_guids
using namespace UCLID_REDACTIONCUSTOMCOMPONENTSLib;