// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT 1
#define _ATL_APARTMENT_THREADED

// DAO support conflicts with ADO support, so remove it
#define _AFX_NO_DAO_SUPPORT

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxcmn.h>			// For CListCtrl class
#include <afxext.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\AFCore\Code\AFCore.tlb" named_guids
using namespace UCLID_AFCORELib;

//#import "..\..\..\AFUtils\Code\AFUtils.tlb" named_guids
//using namespace UCLID_AFUTILSLib;

//#import "FeedbackMgr.tlb"

// Required for ADO support
#import "c:\program files\common files\system\ado\msado27.tlb" \
	no_namespace \
	rename ("EOF", "adoEOF")
