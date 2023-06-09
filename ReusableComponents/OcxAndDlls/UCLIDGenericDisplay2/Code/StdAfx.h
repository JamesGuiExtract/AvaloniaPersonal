#pragma once

// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#include <CommonToExtractProducts.h>

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxctl.h>         // MFC support for ActiveX Controls
#include <afxext.h>         // MFC extensions
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Comon Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

// Delete the two includes below if you do not wish to use the MFC
//  database classes
#include <afxdb.h>			// MFC database classes
#include <afxdao.h>			// MFC DAO database classes

#include <MemLeakDetection.h>

// application specific includes

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#import "..\..\..\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;
