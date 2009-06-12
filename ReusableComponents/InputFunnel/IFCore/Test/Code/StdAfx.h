// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#import "..\..\..\..\..\ReusableComponents\COMComponents\VariantCollection\Code\VariantCollection.dll"
using namespace UCLID_VarCollection;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDIUnknownVector\Code\UCLIDIUnknownVector.tlb"
using namespace UCLIDIUNKNOWNVECTORLib;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentCategoryMgmt\Code\UCLIDComponentCategoryMgmt.tlb"
using namespace UCLIDCOMPONENTCATEGORYMGMTLib;

#import "..\..\Code\IFCore.tlb"
using namespace UCLID_INPUTFUNNELLib;

#import "..\TestComponents\Code\TestComponents.tlb"
using namespace TESTCOMPONENTSLib;

#include <atlbase.h>

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

