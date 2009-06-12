// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#pragma once

#pragma warning(disable:4786)

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <CommonToExtractProducts.h>

// DAO support conflicts with ADO support, so remove it
#define _AFX_NO_DAO_SUPPORT

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids 
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_") \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids, raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\AFCore\Code\AFCore.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_AFCORELib;

#import "..\..\FeedbackManager\Code\FeedbackManager.tlb" named_guids raw_property_prefixes("raw_get_", "raw_put_", "raw_putref_")
using namespace UCLID_FEEDBACKMANAGERLib;

// Required for ADO support
#import "c:\program files\common files\system\ado\msado27.tlb" \
	no_namespace \
	rename ("EOF", "adoEOF")


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
