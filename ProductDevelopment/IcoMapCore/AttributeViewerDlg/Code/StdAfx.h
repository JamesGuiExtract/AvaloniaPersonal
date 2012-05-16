// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#pragma once

#pragma warning(disable:4786)

#include <CommonToExtractProducts.h>

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions

#ifndef _AFX_NO_OLE_SUPPORT
#include <afxole.h>         // MFC OLE classes
#include <afxodlgs.h>       // MFC OLE dialog classes
#include <afxdisp.h>        // MFC Automation classes
#endif // _AFX_NO_OLE_SUPPORT

#include <atlbase.h>
extern CComModule _Module;
#include <atlcom.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCurveParameter\Code\UCLIDCurveParameter.tlb" named_guids 
using namespace UCLID_CURVEPARAMETERLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDMeasurements\Code\UCLIDMeasurements.tlb" named_guids 
using namespace UCLID_MEASUREMENTSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDFeatureMgmt\Code\UCLIDFeatureMgmt.tlb" named_guids
using namespace UCLID_FEATUREMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDDistanceConverter\Code\UCLIDDistanceConverter.tlb"
using namespace UCLID_DISTANCECONVERTERLib;

#ifndef _AFX_NO_DB_SUPPORT
#include <afxdb.h>			// MFC ODBC database classes
#endif // _AFX_NO_DB_SUPPORT

#ifndef _AFX_NO_DAO_SUPPORT
#include <afxdao.h>			// MFC DAO database classes
#endif // _AFX_NO_DAO_SUPPORT

#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
