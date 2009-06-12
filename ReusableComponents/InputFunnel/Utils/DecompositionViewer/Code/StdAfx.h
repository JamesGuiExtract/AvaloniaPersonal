// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if !defined(AFX_STDAFX_H__8F2A9A35_C92C_44FF_84EB_4AB8E3D1BBE8__INCLUDED_)
#define AFX_STDAFX_H__8F2A9A35_C92C_44FF_84EB_4AB8E3D1BBE8__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls

#import "D:\\Engineering\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "D:\\Engineering\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "D:\\Engineering\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "D:\\Engineering\ReusableComponents\COMComponents\UCLIDTestingFramework\Interfaces\Code\UCLIDTestingFramework.tlb" named_guids
using namespace UCLID_TESTINGFRAMEWORKINTERFACESLib;

#import "D:\Engineering\ProductDevelopment\InputFunnel\\IFCore\\Code\\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "D:\Engineering\ProductDevelopment\InputFunnel\\InputReceivers\\SpotRecognitionIR\\Code\\Core\\SpotRecognitionIR.tlb" named_guids
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "D:\Engineering\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__8F2A9A35_C92C_44FF_84EB_4AB8E3D1BBE8__INCLUDED_)
