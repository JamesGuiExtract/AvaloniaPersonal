// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef _SECURE_ATL
#define _SECURE_ATL 1
#endif

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers
#endif

#include<CommonToExtractProducts.h>

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS	// some CString constructors will be explicit

// turns off MFC's hiding of some common and often safely ignored warning messages
#define _AFX_ALL_WARNINGS

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#endif
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#include <MemLeakDetection.h>

#import <COMLM.tlb> named_guids 
using namespace UCLID_COMLMLib;

#import <UCLIDCOMUtils.tlb> named_guids 
using namespace UCLID_COMUTILSLib;

#import <UCLIDRasterAndOCRMgmt.tlb> named_guids \
	raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import <IFCore.tlb> named_guids, raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_INPUTFUNNELLib;

#import <AFCore.tlb> named_guids
using namespace UCLID_AFCORELib;
