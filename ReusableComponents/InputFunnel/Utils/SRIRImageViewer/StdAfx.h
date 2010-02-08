// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#pragma once

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <CommonToExtractProducts.h>

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#include <atlbase.h>
extern CComModule _Module;
#include <atlcom.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDTestingFramework\Interfaces\Code\UCLIDTestingFramework.tlb" named_guids
using namespace UCLID_TESTINGFRAMEWORKINTERFACESLib;

#import "..\..\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\InputReceivers\SpotRecognitionIR\Code\SubImageHandlers\Code\SubImageHandlers.tlb" named_guids
using namespace UCLID_SUBIMAGEHANDLERSLib;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
