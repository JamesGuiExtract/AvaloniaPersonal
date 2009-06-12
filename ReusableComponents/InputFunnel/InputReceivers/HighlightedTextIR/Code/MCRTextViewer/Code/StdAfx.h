// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#if defined( _DEBUG) && defined(NDEBUG_MFC)
    // Building debug version to be used with non-debug/Prod AutoCAD"
    #define _DEBUG_WAS_DEFINED
    #undef _DEBUG
#endif

#pragma warning (disable: 4786)

#include <CommonToExtractProducts.h>

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxctl.h>         // MFC support for ActiveX Controls
#include <afxext.h>         // MFC extensions
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Comon Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#include <afxrich.h>
#endif // _AFX_NO_AFXCMN_SUPPORT

// Delete the two includes below if you do not wish to use the MFC
//  database classes
#include <afxdb.h>			// MFC database classes
#include <afxdao.h>			// MFC DAO database classes

#ifdef _DEBUG_WAS_DEFINED
    #define _DEBUG
    #undef _DEBUG_WAS_DEFINED
#endif

#import "..\..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\InputFinders\Code\InputFinders.tlb"
using namespace UCLID_INPUTFINDERSLib;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
